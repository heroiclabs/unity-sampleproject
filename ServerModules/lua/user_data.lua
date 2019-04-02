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

local ud = {}

local function is_initialized(context)
    local data = {
        collection = "personal",
        key = "player_data",
        user_id = context.user_id,
    }
    local result = nk.storage_read({ data })

    if next(result) == nil then
        return false
    else
        return true
    end
end

local function initialize(context)
    local data = {
        level = math.random(100),
        wins = math.random(100),
        gamesPlayed = math.random(100)
    }
    local data = {
        collection = "personal",
        key = "player_data",
        value = data,
        user_id = context.user_id,
        permission_read = 2,
        permission_write = 1
    }
    nk.storage_write({ data })
end

function ud.initialize_data(context)
    if is_initialized(context) == false then
        initialize(context)
    end
end

return ud
