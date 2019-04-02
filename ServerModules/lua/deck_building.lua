--[[
 Copyright 2019 The Nakama Authors

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
--]]

local nk = require("nakama")
local w = require("wallet")

local db = {
    merge_cost = 50,
    buy_cost = 50,
    storage = "deck",
    card_type_first = 1,
    card_type_count = 4,
    permission_read = 2,
    permission_write = 0
}

local function list_tab(tab)
    for k, v in pairs(tab) do
        print(k)
        print(v)
    end
end

local function remove_empty_cards(deck, type)
    local size = 0
    for level, count in pairs(deck[type]) do
        if count == 0 then
            deck[type][level] = nil;
        else
            size = size + 1
        end
    end
    if size == 0 then
        deck[type] = nil
    end
end


local function store_deck(user_id, key, deck)
    local object = {
        {
            collection = db.storage,
            key = key,
            user_id = user_id,
            value = deck,
            permission_read = db.permission_read,
            permission_write = db.permission_write
        }
    }
    nk.storage_write(object)
end

local function remove_from_deck(card, deck)
    for type, levels in pairs(deck) do
        if card.card_type == type then
            for level, count in pairs(levels) do
                if card.level == level then
                    if count > 0 then
                        levels[level] = levels[level] - 1
                        remove_empty_cards(deck, type)
                        return true
                    end
                end
            end
        end
    end
    return false
end

local function add_card(deck, card_type, level)
    if deck[card_type] == nil then
        deck[card_type] = {}
    end
    if deck[card_type][level] == nil then
        deck[card_type][level] = 1
    else
        deck[card_type][level] = deck[card_type][level] + 1
    end
end

local function get_cards(user_id, key)
    local ids = {
        { collection = db.storage, key = key, user_id = user_id },
    }
    local cards = nk.storage_read(ids)[1].value
    return cards
end

local function remove_cards(first, second, used_cards, unused_cards)
    if first.is_used == "True" then
        if remove_from_deck(first, used_cards) == false then
            return false, "first card not found in used card list"
        end
    else
        if remove_from_deck(first, unused_cards) == false then
            return false, "first card not found in unused card list"
        end
    end

    if second.is_used == "True" then
        if remove_from_deck(second, used_cards) == false then
            return false, "second card not found in unused card list"
        end
    else
        if remove_from_deck(second, unused_cards) == false then
            return false, "second card not found in unused card list"
        end
    end
    return true
end

local function swap(user_id, first, second)
    local used_cards = get_cards(user_id, "used_cards")
    local unused_cards = get_cards(user_id, "unused_cards")

    local result, msg = remove_cards(first, second, used_cards, unused_cards)
    if result == false then
        return false, msg
    end

    if first.is_used == "True" then
        add_card(unused_cards, first.card_type, first.level)
        add_card(used_cards, second.card_type, second.level)
    else
        add_card(used_cards, first.card_type, first.level)
        add_card(unused_cards, second.card_type, second.level)
    end

    store_deck(user_id, "used_cards", used_cards)
    store_deck(user_id, "unused_cards", unused_cards)

    return true
end

local function merge(user_id, first, second)
    local used_cards = get_cards(user_id, "used_cards")
    local unused_cards = get_cards(user_id, "unused_cards")

    local result, msg = remove_cards(first, second, used_cards, unused_cards)
    if result == false then
        return false, msg
    end

    if first.is_used == "True" or second.is_used == "True" then
        add_card(used_cards, first.card_type, tostring(first.level + 1))
    else
        add_card(unused_cards, first.card_type, tostring(first.level + 1))
    end

    local funds = w.get_funds(user_id, w.currency_name)
    if funds < db.merge_cost then
        return false, "insufficient funds"
    else
        if w.update_wallet(user_id, -db.merge_cost) == true then
            store_deck(user_id, "used_cards", used_cards)
            store_deck(user_id, "unused_cards", unused_cards)
            return true
        else
            return false, "error updating wallet"
        end
    end
end

function db.merge_cards(context, payload)

    if payload == nil then
        return nk.json_encode({ response = false, message = "payload is nil" })
    end

    data = nk.json_decode(payload)

    if data.first == nil or data.second == nil then
        return nk.json_encode({ response = false, message = "payload has no \"first\" or \"second\" fields" })
    end

    if data.first.is_used == "True" and data.second.is_used == "True" then
        return nk.json_encode({ response = false, message = "both cards are in use" })
    end

    if data.first.card_type ~= data.second.card_type then
        local msg = "sent cards have different card type (" .. data.first.card_type .. " and " .. data.second.card_type .. ")"
        return nk.json_encode({ response = false, message = msg })
    end

    if data.first.level ~= data.second.level then
        local msg = "sent cards are on different level (" .. data.first.level .. " and " .. data.second.level .. ")"
        return nk.json_encode({ response = false, message = msg })
    end


    local result, msg = merge(context.user_id, data.first, data.second)
    if result == true then
        return nk.json_encode({ response = true, message = "" })
    else
        return nk.json_encode({ response = false, message = msg })
    end
end



function db.swap_cards(context, payload)
    if payload == nil then
        return nk.json_encode({ response = false, message = "payload is nil" })
    end

    local data = nk.json_decode(payload)

    if data.first == nil or data.second == nil then
        return nk.json_encode({ response = false, message = "payload has no \"first\" or \"second\" fields" })
    end

    if data.first.is_used == data.second.is_used then
        return nk.json_encode({ response = false, message = "both cards are used/unused" })
    end


    local result, msg = swap(context.user_id, data.first, data.second)
    if result == true then
        return nk.json_encode({ response = true, message = "" })
    else
        return nk.json_encode({ response = false, message = msg })
    end
end


function db.debug_add_random_card(context, payload)
    if w.update_wallet(context.user_id, -db.buy_cost, metadata, true) == false then
        return nk.json_encode({ response = false, message = "insufficient funds" })
    end
    local random_type = math.random(db.card_type_first, db.card_type_count - 1)
    local unused_cards = get_cards(context.user_id, "unused_cards")
    add_card(unused_cards, tostring(random_type), tostring(1))
    store_deck(context.user_id, "unused_cards", unused_cards)
    local metadata =
    {
        source = "random_card_bought"
    }
    return nk.json_encode({ response = true, message = "" })
end

function db.debug_clear_deck(context, payload)
    local objects = {
        { collection = db.storage, key = "unused_cards", user_id = context.user_id, },
        { collection = db.storage, key = "used_cards", user_id = context.user_id, },
        { collection = db.storage, key = "name", user_id = context.user_id, }
    }
    nk.storage_delete(objects)
    return nk.json_encode({ response = true, message = "" })
end

function db.debug_add_gems(context, payload)
    if w.update_wallet(context.user_id, 100, metadata) == false then
        return nk.json_encode({ response = false, message = "failed to update wallet" })
    else
        return nk.json_encode({ response = true, message = "" })
    end
end

return db
