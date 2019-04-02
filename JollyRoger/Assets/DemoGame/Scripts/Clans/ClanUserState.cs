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

namespace DemoGame.Scripts.Clans
{

    /// <summary>
    /// Enum containing all states a user can be assigned in a clan.
    /// </summary>
    public enum ClanUserState
    {
        /// <summary>
        /// There must at least be 1 superadmin in any group.
        /// User with Superadmin status can promote <see cref="JoinRequest"/>, <see cref="Member"/> and <see cref="Admin"/> users.
        /// User with Superadmin status can kick users with any state.
        /// </summary>
        Superadmin = 0,

        /// <summary>
        /// There can be one of more admins. Admins can update groups as well as accept, kick, promote or add members.
        /// User with Admin can status promote <see cref="JoinRequest"/> and <see cref="Member"/> users.
        /// User with Admin can status kick <see cref="JoinRequest"/> and <see cref="Member"/> users.
        /// </summary>
        Admin = 1,

        /// <summary>
        /// Regular group member. They cannot accept join request from new users.
        /// User with Member status cannot promote nor kick any users.
        /// </summary>
        Member = 2,

        /// <summary>
        /// A new join request from a new user. This does not count towards the maximum group member count.
        /// User with JoinRequest state cannot promote nor kick any users.
        /// </summary>
        JoinRequest = 3,

        /// <summary>
        /// The user is not a part of current clan.
        /// </summary>
        None = -1
    }

}