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

local w = {
    currency_name = "gold",
}

function w.get_funds(user_id, currency)

    local account = nk.account_get_id(user_id)
    if account.wallet[currency] ~= nil then
        return account.wallet[currency]
    else
        return 0
    end
end

function w.update_wallet(user_id, count, metadata, must_suffice)
    if must_suffice ~= nil and must_suffice == true then
        local funds = w.get_funds(user_id, w.currency_name)
        if funds + count < 0 then
            return false
        end
    end

    local content = { [w.currency_name] = count }
    local status, err = pcall(nk.wallet_update, user_id, content, metadata)
    if not status then
        nk.logger_info("An error has occured when updating wallet: " .. err)
        return false
    else
        nk.logger_info("Succes updating wallet")
        return true
    end
end

function w.get_match_rewards(user_id, match_id)
    local updates = nk.wallet_ledger_list(user_id)
    local match_rewards = {}
    local index = 0
    for _, update in ipairs(updates) do
        if update.metadata.source == "match_reward" then
            if update.metadata.match_id == match_id then
                index = index + 1
                match_rewards[index] = update
            end
        end
    end
    print("Found match gold: " .. tostring(index))
    match_rewards.n = index
    return match_rewards
end

return w
