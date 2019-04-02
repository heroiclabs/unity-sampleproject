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

local n = {
    quest_new_friend = 1,
    clan_refresh = 2,
    clan_delete = 3
}

function n.update_clan_notification(context, _, payload)
    local user_list = nk.group_users_list(payload.group_id)

    for i, user in ipairs(user_list) do
        local user_id = user.user.user_id
        local sender_id = nil
        local content = {}
        local subject = "Refresh Member List"
        local code = n.clan_refresh
        local persistent = false
        nk.notification_send(user_id, subject, content, code, sender_id, persistent)
    end
end

function n.delete_clan_notification(context, payload)

    local user_list = nk.group_users_list(payload.group_id)

    for i, user in ipairs(user_list) do
        local user_id = user.user.user_id
        local sender_id = nil
        local content = {}
        local subject = "Delete Clan"
        local code = n.clan_delete
        local persistent = false
        nk.notification_send(user_id, subject, content, code, sender_id, persistent)
    end

    return payload
end

function n.kick_clan_notification(context, payload)
    for i, id in ipairs(payload.user_ids) do
        local user_id = id
        local sender_id = nil
        local content = {}
        local subject = "Kicked from Clan"
        local code = n.clan_delete
        local persistent = false
        nk.notification_send(user_id, subject, content, code, sender_id, persistent)
    end

    return payload
end

function n.update_account_notification(context, _, payload)
    local user_id = context.user_id
    local sender_id = nil
    local content = {}
    local subject = "Update Account"
    local code = n.refresh_clan_list
    local persistent = false
    nk.notification_send(user_id, subject, content, code, sender_id, persistent)
end

return n
