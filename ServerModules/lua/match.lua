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
local lb = require("leaderboards")

local m = {
    win_reward = 180;
    lose_reward = 110;
}

function m.calculate_score(is_winner, towers_destroyed, match_duration)
    local score = is_winner == true and 10 or 0
    score = score + towers_destroyed * 5

    local time_score = 0
    local duration = tonumber(match_duration) / 60
    duration = math.max(0, math.floor(duration))

    if is_winner == true then
        time_score = math.max(5 - duration, 1)
    else
        time_score = math.max(1, math.min(duration, 5))
    end
    score = score + time_score
    return score
end

function m.last_match_reward(context, payload)
    local user_id = context.user_id
    local message = nk.json_decode(payload)
    local rewards = w.get_match_rewards(user_id, message.match_id)
    local last_reward = rewards[rewards.n]
    if last_reward ~= nil then
        return nk.json_encode(last_reward.changeset)
    end
end


function m.data_sent(context, payload)
    if payload.match_data_send.op_code == "1" then
        local message = nk.base64_decode(payload.match_data_send.data)
        message = nk.json_decode(message)

        local metadata =
        {
            source = "match_reward",
            match_id = message.matchId
        }

        w.update_wallet(message.winnerId, m.win_reward, metadata);
        w.update_wallet(message.loserId, m.lose_reward, metadata);

        local winner_score = m.calculate_score(true, message.winnerTowersDestroyed, message.time)
        local loser_score = m.calculate_score(false, message.loserTowersDestroyed, message.time)
        lb.add_score(message.winnerId, winner_score)
        lb.add_score(message.loserId, loser_score)
    end
    return payload
end


return m
