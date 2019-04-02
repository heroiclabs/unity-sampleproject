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

local lb = {
    global = "global"
}


local function get_username(user_id)
    local result = nk.sql_query([[
        select username
        from users
        where id = $1;
    ]], { user_id })

    return result[1].username
end

local function group_users_list(group_id)
    local result = nk.sql_query([[
        select u.id
        from users u
        join group_edge e
        on e.destination_id = u.id
        where e.source_id = $1;
    ]], { group_id })

    return result
end

local function get_user_ids(members)
    local ids = {}

    for i, member in ipairs(members) do
        ids[i] = member.id
    end

    return ids
end

function lb.init_global_leaderboards()
    nk.logger_info("Initializing global leaderboards")
    local id = lb.global
    local authoritative = true
    local sort = "desc"
    local operator = "best"
    local reset = nil
    local metadata = nil

    nk.leaderboard_create(id, authoritative, sort, operator, reset, metadata)
    nk.logger_info("Global leaderboards initialized")
end

function lb.add_score(user_id, score)
    local username = get_username(user_id)
    local ids = { [1] = user_id }
    local _, records = nk.leaderboard_records_list(lb.global, ids)
    nk.leaderboard_record_write(lb.global, user_id, username, score)
end

function lb.write_score(user_id, score)
    local username = get_username(user_id)
    nk.leaderboard_record_write(lb.global, user_id, username, score)
end

function lb.list_group_leaderboard(context, group_id)
    local members = nk.group_users_list(group_id)
    local ids = get_user_ids(members)
    local _, records = nk.leaderboard_records_list(lb.global, ids)
    return nk.json_encode(records)
end

return lb
