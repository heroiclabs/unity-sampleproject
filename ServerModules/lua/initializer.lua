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

local fq = require("friend_quest")
local us = require("user_search")
local ud = require("user_data")
local lb = require("leaderboards")
local db = require("deck_building")
local n = require("notifications")
local m = require("match")
local nk = require("nakama")

local function run_once()
    fq.init_friend_quest()
    lb.init_global_leaderboards()
end

local function authenticate(context, payload)
    fq.start_friend_quest(context, payload)
    ud.initialize_data(context)
end

nk.run_once(run_once)
nk.register_req_after(authenticate, "AuthenticateDevice")
nk.register_req_after(authenticate, "AuthenticateFacebook")
nk.register_req_after(n.update_clan_notification, "JoinGroup")
nk.register_req_after(n.update_clan_notification, "LeaveGroup")
nk.register_req_after(n.update_clan_notification, "DeleteGroup")
nk.register_req_after(n.update_clan_notification, "PromoteGroupUsers")
nk.register_rt_before(m.data_sent, "MatchDataSend")
nk.register_req_before(fq.check_friend_quest, "AddFriends")
nk.register_req_before(n.kick_clan_notification, "KickGroupUsers")
nk.register_req_before(n.delete_clan_notification, "DeleteGroup")
nk.register_rpc(us.search_username, "search_username")
nk.register_rpc(lb.list_group_leaderboard, "list_group_leaderboards")
nk.register_rpc(db.merge_cards, "merge_cards")
nk.register_rpc(db.swap_cards, "swap_cards")
nk.register_rpc(db.debug_add_random_card, "debug_add_random_card")
nk.register_rpc(db.debug_clear_deck, "debug_clear_deck")
nk.register_rpc(db.debug_add_gems, "debug_add_gems")
nk.register_rpc(m.last_match_reward, "last_match_reward")
