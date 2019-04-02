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

local us = {}

function us.search_username(context, payload)
    nk.logger_info("Search username " .. payload)
    local query_result = nk.sql_query([[
    SELECT username FROM users WHERE username LIKE '%
    ]] .. payload .. [[%';]])

    local users = {
        ""
    }

    local count = 0
    for _, tab in pairs(query_result) do
        for k, v in pairs(tab) do
            count = count + 1
            users[count] = v
        end
    end

    return nk.json_encode({ usernames = users })
end

return us
