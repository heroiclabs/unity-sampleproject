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
local n = require("notifications")

local fq = {
    nakama_dummy_username = "Richard",
    system_id = "00000000-0000-0000-0000-000000000001",
    reward = 1000,
}

function fq.init_friend_quest(context)
    nk.sql_exec([[
        insert into users (id, username)
        values ($1, $2)
        on conflict (id) do nothing
    ]], { fq.system_id, fq.nakama_dummy_username })

    nk.logger_info("friend quest initialized")
end

local function friend_quest_record_write(id, is_done)
    return {
        collection = "quests",
        key = "first_friend",
        user_id = id,
        value = { done = is_done },
        permission_read = 0,
        permission_write = 0,
    }
end

local function friend_quest_record_read(id)
    return {
        collection = "quests",
        key = "first_friend",
        user_id = id,
    }
end

local function add_quest(user_id, username)
    nk.logger_info("user " .. username .. " created account using DeviceId")

    local quest_query = friend_quest_record_write(user_id, false)
    nk.storage_write({ quest_query });
    local message = "user " .. username .. " created account using DeviceId";

    nk.logger_info("new friend storage created")
end

function fq.start_friend_quest(context, payload)
    if payload.created == true then
        add_quest(context.user_id, context.username)
    end
end

function fq.check_friend_quest(context, payload)
    if payload.usernames == nil then
        return payload
    end

    for _, username in pairs(payload.usernames) do

        local dummy_username = fq.nakama_dummy_username
        nk.logger_info("added friend: " .. username .. ", dummy username: " .. dummy_username)

        if (username == dummy_username) then
            local quest_record = friend_quest_record_read(context.user_id)
            local result = nk.storage_read({ quest_record })
            if result[1] == nil then
                nk.logger_info("user " .. context.username .. "' friend quest not initialized; initializing now")
                add_quest(context.user_id, context.username)
                result = nk.storage_read({ quest_record })
                nk.logger_info("success reading player storage")
            else
                nk.logger_info("success reading player storage")
            end

            if result[1].value.done == false then
                local quest_query = friend_quest_record_write(context.user_id, true)
                nk.storage_write({ quest_query });
                nk.logger_info("new friend quest completed")

                w.update_wallet(context.user_id, fq.reward)
                fq.send_notification(context, fq.reward)
            end
        end
    end
    return payload
end

function fq.send_notification(context, reward)
    local user_id = context.user_id
    local sender_id = nil
    local content = { reward = reward }
    local subject = "\"A new friend!\""
    local code = n.quest_new_friend
    local persistent = true
    nk.notification_send(user_id, subject, content, code, sender_id, persistent)
end

return fq
