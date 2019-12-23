/**
 * Copyright 2019 The Knights Of Unity, created by Pawel Stolarczyk
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;

namespace DemoGame.Scripts.Leaderboards
{

    /// <summary>
    /// Static class responsible for managing leaderboards requests.
    /// </summary>
    public static class LeaderboardManager
    {
        #region Methods

        /// <summary>
        /// Retrieves top best records of all time from the server.
        /// </summary>
        public static async Task<IApiLeaderboardRecordList> GetGlobalLeaderboarsAsync(Client client, ISession session, int limit = 1, string cursor = null)
        {
            try
            {
                IApiLeaderboardRecordList list = await client.ListLeaderboardRecordsAsync(session, "global", null, null, limit, cursor);
                return list;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An error has occured while showing global leaderboards: " + e);
                return null;
            }
        }

        /// <summary>
        /// Retrieves all user ids from <paramref name="clan"/> and filters all records from global leaderboard to show only filtered users.
        /// </summary>
        public static async Task<IApiLeaderboardRecordList> GetClanLeaderboarsAsync(Client client, ISession session, IApiGroup clan, int limit = 1, string cursor = null)
        {
            try
            {
                var users = await client.ListGroupUsersAsync(session, clan.Id, null, 1, null);
                IEnumerable<string> ids = users.GroupUsers.Select(x => x.User.Id);
                IApiLeaderboardRecordList list = await client.ListLeaderboardRecordsAsync(session, "global", ids, null, limit, cursor);
                return list;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An error has occured while showing clan leaderboards: " + e);
                return null;
            }
        }

        /// <summary>
        /// Retrieves all user ids from <paramref name="friends"/> and filters all records from global leaderboard to show only filtered users.
        /// </summary>
        public static async Task<IApiLeaderboardRecordList> GetFriendsLeaderboarsAsync(Client client, ISession session, IEnumerable<IApiFriend> friends, int limit = 1, string cursor = null)
        {
            try
            {
                List<string> ids = friends.Select(x => x.User.Id).ToList();
                ids.Add(NakamaSessionManager.Instance.Session.UserId);
                IApiLeaderboardRecordList list = await client.ListLeaderboardRecordsAsync(session, "global", ids, null, limit, cursor);
                return list;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An error has occured while showing friends leaderboards: " + e);
                return null;
            }
        }

        #endregion
    }

}