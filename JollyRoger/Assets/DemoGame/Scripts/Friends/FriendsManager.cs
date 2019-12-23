/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch
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
using System.Threading.Tasks;
using UnityEngine;
using Nakama;

namespace DemoGame.Scripts.Friends
{

    /// <summary>
    /// Contains methods for all types of requests changing user friends statuses in database
    /// </summary>
    public static class FriendsManager
    {
        #region public static methods

        /// <summary>
        /// This method is loading friends list from database. Return value indicates whether operation succeed or failed.
        /// </summary>
        public static async Task<IApiFriendList> LoadFriendsAsync(IClient client, ISession session)
        {
            try
            {
                var friends = await client.ListFriendsAsync(session);
                return friends;
            }
            catch (Exception e)  //catching exception, if program enters this code loading operation was not successfully completed
            {
                Debug.Log("Loading friends failed (" + e.Message + ")");
                return null;
            }
        }

        /// <summary>
        /// Adding new friend using information from previously loaded IApiFriend object. Return value indicates whether operation succeed or failed.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> AddFriendAsync(IApiFriend friend, IClient client, ISession session)
        {
            bool success = await AddFriendByIDAsync(friend.User.Id, client, session);
            return success;
        }


        /// <summary>
        /// This method is sending request to database for adding new friend using user id. Return value indicates whether operation succeed or failed.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> AddFriendByIDAsync(string id, IClient client, ISession session)
        {
            try
            {
                string[] ids = new[] { id };
                await client.AddFriendsAsync(session, ids);
                return true;
            }
            catch (Exception e) //catching exception, if program entered this code adding friend operation was not successfully completed
            {
                Debug.Log("Adding friend failed (" + e.Message + ")");
                return false;
            }
        }

        /// <summary>
        /// This method is sending request to database for adding new friend using username. Return value indicates whether operation succeed or failed.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> AddFriendByUsernameAsync(string username, IClient client, ISession session)
        {
            try
            {
                string[] usernames = new[] { username };
                await client.AddFriendsAsync(session, new string[] { }, usernames);
                return true;
            }
            catch (Exception e) //catching exception, if program entered this code adding friend operation was not successfully completed
            {
                Debug.Log("Adding friend failed (" + e.Message + ")");
                return false;
            }
        }

        /// <summary>
        /// This method is sending request to database for removing friend. Return value indicates whether operation succeed or failed.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> RemoveFriendAsync(IApiFriend friend, IClient client, ISession session)
        {
            try
            {
                string[] ids = new[] { friend.User.Id };
                await client.DeleteFriendsAsync(session, ids);
                return true;
            }
            catch (Exception e) //catching exception, if program entered this code removing friend operation was not successfully completed
            {
                Debug.Log("Removing friend failed (" + e.Message + ")");
                return false;
            }
        }

        /// <summary>
        /// This method is sending request to database for blocking a friend. Return value indicates whether operation succeed or failed.
        /// Blocking user who is on the friends list/sent friend requests list is automatically deleted from these lists and added to banned users list.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> BlockFriendAsync(IApiFriend friend, IClient client, ISession session)
        {
            try
            {
                string[] ids = new[] { friend.User.Id };
                await client.BlockFriendsAsync(session, ids);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Blocking friend failed (" + e.Message + ")");    //catching exception, if program entered this code blocking friend operation was not successfully completed
                return false;
            }
        }
        #endregion
    }

}
