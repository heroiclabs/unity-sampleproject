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
using Nakama;
using UnityEngine;

namespace DemoGame.Scripts.Clans
{

    /// <summary>
    /// Static class containing methods used to create, delete, join, search and overall manage clans (groups) in Nakama.
    /// Each method is asychronous and contain at least two parameters: onSuccess and onFailure.
    /// Documentation of clans (groups) can be found here: https://heroiclabs.com/docs/social-groups-clans/
    /// </summary>
    public static class ClanManager
    {
        #region ClanManagement

        /// <summary>
        /// Creates clan on Nakama server with given <paramref name="name"/>.
        /// Fails when the name is already taken.
        /// Returns <see cref="IApiGroup"/> on success.
        /// </summary>
        public static async Task<IApiGroup> CreateClanAsync(Client client, ISession session, string name, string avatarUrl)
        {
            try
            {
                IApiGroup group = await client.CreateGroupAsync(session, name, "", avatarUrl);
                return group;
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == (long)System.Net.HttpStatusCode.InternalServerError)
                {
                    Debug.LogWarning("Clan name \"" + name + "\" already in use");
                    return null;
                }
                else
                {
                    Debug.LogWarning("An exception has occured when creating clan with code " + e.StatusCode + ": " + e);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("An internal exception has occured when creating clan: " + e);
                return null;
            }
        }

        /// <summary>
        /// Deletes supplied clan.
        /// Fails when user's status is not <see cref="ClanUserState.Superadmin"/>.
        /// </summary>
        public static async Task<bool> DeleteClanAsync(Client client, ISession session, IApiGroup clan)
        {
            try
            {
                await client.DeleteGroupAsync(session, clan.Id);
                return true;
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == (long)System.Net.HttpStatusCode.BadRequest)
                {
                    Debug.LogWarning("Unauthorized clan removal with code " + e.StatusCode + ": " + e);
                    return false;
                }
                else
                {
                    Debug.LogWarning("An exception has occured when deleting clan with code " + e.StatusCode + ": " + e);
                    return false;
                }

            }
            catch (Exception e)
            {
                Debug.LogWarning("An internal exception has occured when deleting clan: " + e);
                return false;
            }
        }

        /// <summary>
        /// Updates information about given clan.
        /// Fails if user has no premissions to update clan information.
        /// Returns the reference to newly updated clan's <see cref="IApiGroup"/>.
        /// If <paramref name="isPublic"/> is false, this clan will not be shown during clan search.
        /// </summary>
        public static async Task<IApiGroup> UpdateClanInfoAsync(Client client, ISession session, IApiGroup clan, string name, string description, string avatarUrl, bool isPublic)
        {
            try
            {
                await client.UpdateGroupAsync(session, clan.Id, name, isPublic, description, avatarUrl, null);
                // Getting list of all clans local user has joined.
                // In this demo a user can join only one clan at a time, so the first clan should always be the clan we updated.
                IApiGroupList clanList = await client.ListGroupsAsync(session, name, 1, null);
                IApiGroup updatedClan = clanList.Groups.First();

                if (updatedClan != null && updatedClan.Id == clan.Id)
                {
                    return updatedClan;
                }
                else
                {
                    Debug.LogWarning("An error has occured when retrieving updated clan data");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured when updating clan info: " + e);
                return null;
            }
        }

        #endregion

        #region ClanAssociation

        /// <summary>
        /// Sends request to join supplied clan.
        /// </summary>
        public static async Task<IApiGroup> JoinClanAsync(Client client, ISession session, IApiGroup clan)
        {
            try
            {
                await client.JoinGroupAsync(session, clan.Id);
                return clan;
            }
            catch (ApiResponseException e)
            {
                Debug.LogWarning("An exception has occured when joining clan with code " + e.StatusCode + ": " + e);
                return null;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An internal exception has occured when joining clan with name \"" + clan.Name + "\": " + e);
                return null;
            }
        }

        /// <summary>
        /// Sends request to leave supplied clan.
        /// </summary>
        public static async Task<bool> LeaveClanAsync(Client client, ISession session, IApiGroup clan)
        {
            try
            {
                await client.LeaveGroupAsync(session, clan.Id);
                return true;
            }
            catch (ApiResponseException e)
            {
                Debug.LogWarning("An exception has occured when leaving clan with code " + e.StatusCode + ": " + e);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An internal server exception has occured when leaving clan: " + e);
                return false;
            }
        }

        #endregion

        #region MemberManagement

        /// <summary>
        /// Kicks supplied user out of given clan.
        /// Fails if kicking user has no permissions to kick the user.
        /// </summary>
        public static async Task<bool> KickUserAsync(Client client, ISession session, IApiUser kickedUser, IApiGroup clan)
        {
            try
            {
                await client.KickGroupUsersAsync(session, clan.Id, new string[] { kickedUser.Id });
                return true;
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == (long)System.Net.HttpStatusCode.NotFound)
                {
                    Debug.LogWarning("Insufficient permissions to kick " + kickedUser.Username
                        + " from clan " + clan.Name + " or clan not found: " + e);
                    return false;
                }
                else
                {
                    Debug.LogWarning("An internal exception has occured when kicking user " + kickedUser.Username
                        + " from clan " + clan.Name + " with code " + e.StatusCode + ": " + e);
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured when kicking user " + kickedUser.Username + " from clan " + clan.Name + ": " + e);
                return false;
            }

        }

        /// <summary>
        /// Rises the <see cref="ClanUserState"/> of supplied member in given clan.
        /// Fails if promoting user has no permissions to promote given user.
        /// </summary>
        public static async Task<bool> PromoteUserAsync(Client client, ISession session, IApiUser promotedUser, IApiGroup clan)
        {
            try
            {
                await client.PromoteGroupUsersAsync(session, clan.Id, new string[] { promotedUser.Id });
                return true;
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == (long)System.Net.HttpStatusCode.BadRequest)
                {
                    Debug.LogWarning("Insufficient permissions to promote user or clan not found");
                    return false;
                }
                else
                {
                    Debug.LogWarning("An internal exception has occured when promoting user " + promotedUser.Username
                        + " in clan " + clan.Name + ": " + e);
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured when promoting user " + promotedUser.Username + " in clan " + clan.Name + ": " + e);
                return false;
            }
        }

        /// <summary>
        /// Gathers the list of all users in given clan and their <see cref="ClanUserState"/> (<see cref="IGroupUserListGroupUser.State"/>).
        /// </summary>
        public static async Task<List<IGroupUserListGroupUser>> GetClanUsersAsync(Client client, ISession session, IApiGroup clan)
        {
            try
            {
                var userEnumeration = await client.ListGroupUsersAsync(session, clan.Id, null, 1, null);
                List<IGroupUserListGroupUser> userList = userEnumeration.GroupUsers.ToList();
                return userList;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An error has occured when getting clan user list: " + e);
                return null;
            }
        }

        /// <summary>
        /// Returns the clan this user has joined.
        /// If user hasn't joined any clan yet, null will be returned.
        /// In this demo users can join only one clan at a time.
        /// </summary>
        public static async Task<IUserGroupListUserGroup> GetUserClanAsync(Client client, ISession session)
        {
            try
            {
                IApiUserGroupList clans = await client.ListUserGroupsAsync(session, null, 1, null);
                if (clans.UserGroups.Count() > 0)
                {
                    IUserGroupListUserGroup userGroup = clans.UserGroups.ElementAt(0);
                    return userGroup;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured when listing user clans: " + e);
                return null;
            }
        }

        /// <summary>
        /// Returns the clan this user has joined.
        /// If user hasn't joined any clan yet, null will be returned.
        /// In this demo users can join only one clan at a time.
        /// </summary>
        public static async Task<IUserGroupListUserGroup> GetUserClanAsync(Client client, ISession session, string userId)
        {
            try
            {
                IApiUserGroupList clans = await client.ListUserGroupsAsync(session, userId, null, 1, null);
                if (clans.UserGroups.Count() > 0)
                {
                    IUserGroupListUserGroup userGroup = clans.UserGroups.ElementAt(0);
                    return userGroup;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured when listing user clans: " + e);
                return null;
            }
        }

        /// <summary>
        /// Returns a list of clans containing given <paramref name="keyword"/> in their name as well as current cursor pointing to the next page.
        /// Returned list will contain up to <paramref name="perPageLimit"/> entries and can be iterated using <paramref name="cursor"/>.
        /// Supplying null <paramref name="cursor"/> will return the first page of results, otherwise returned list will depend on where
        /// the cursor points.
        /// </summary>
        public static async Task<IApiGroupList> ListClansAsync(Client client, ISession session, string keyword, int perPageLimit, string cursor)
        {
            try
            {
                IApiGroupList clanList = await client.ListGroupsAsync(session, keyword, perPageLimit, cursor);
                return clanList;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured when listing clans: " + e);
                return null;
            }
        }

        #endregion
    }

}