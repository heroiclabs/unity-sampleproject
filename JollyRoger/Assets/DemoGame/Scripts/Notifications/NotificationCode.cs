/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch, Pawel Stolarczyk 
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

namespace DemoGame.Scripts.Notifications
{

    /// <summary>
    /// Code received from socket notification.
    /// </summary>
    public enum NotificationCode
    {
        FriendJoinedGame = -6,
        FriendWantToJoinGroup = -5,
        GroupJoinRequestAccepted = -4,
        UserAcceptedFriendInvite = -3,
        UserSentFriendInvite = -2,
        UserWantsToChat = -1,

        /// <summary>
        /// Quest "New Friend" has been completed - user added dummy account named 
        /// "richard" to their friend list.
        /// </summary>
        Quest_NewFriend = 1,

        Clan_RefreshMembers = 2,
        Clan_Delete = 3,
    }

}