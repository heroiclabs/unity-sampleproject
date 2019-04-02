/**
 * Copyright 2019 Heroic Labs and contributors
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

using System.Collections.Generic;
using UnityEngine;
using System;

namespace DemoGame.Scripts.Chat
{
    /// <summary>
    /// Bypass between ChatManager and ChatChannel UI.
    /// </summary>
    public class ChatChannel
    {
        #region public variables

        /// <summary>
        /// Id of chat channel used to communicate with channel instance on server
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// Name of channel, could be nickname of user in 1-on-1 chat or name of clan in group chat
        /// </summary>
        public string ChannelName;

        /// <summary>
        /// Cursor for next message in channel history. Used for loading more channel history.
        /// </summary>
        public string NextCursor;

        #endregion

        #region public properties

        /// <summary>
        /// List of active users usernames
        /// </summary>
        public List<string> ActiveUsersUsernames
        {
            get
            {
                List<string> usernames = new List<string>();
                foreach (var user in _users)
                {
                    usernames.Add(user.Value.Username);
                }
                return usernames;
            }
        }

        #endregion

        #region public events

        /// <summary>
        /// An event fired when new message was added by any user. 
        /// <para>Params: messageId, userid, username, content, create time, is loaded from history</para>
        /// </summary>
        public event Action<string, string, string, string, string, bool> OnChatMessage = delegate { };

        /// <summary>
        /// An event fired when new message was edited by any user. 
        /// <para>Params: messageId, content, create time</para>
        /// </summary>
        public event Action<string, string> OnChatUpdate = delegate { };

        /// <summary>
        /// An event fired when new message was removed by any user.
        /// <para>Params: messageIds</para>
        /// </summary>
        public event Action<string> OnChatRemove = delegate { };

        /// <summary>
        /// An event fired when received new message from server. 
        /// Used when any player joined group, left group, was added to group, was kicked from group and was promoted in group
        /// <para>Params: messageId, content</para>
        /// </summary>
        public event Action<string, string, bool> OnServerMessage = delegate { };

        /// <summary>
        /// An event fired when any player joined the channel.
        /// </summary>
        public event Action<ChatUser> OnJoinedChannel = delegate { };

        /// <summary>
        /// An event fired when any player left the channel.
        /// </summary>
        public event Action<ChatUser> OnLeftChannel = delegate { };

        #endregion

        #region private variables

        /// <summary>
        /// List of active channel users
        /// </summary>
        private Dictionary<string, ChatUser> _users = new Dictionary<string, ChatUser>();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates new ChatChannel with given Id
        /// </summary>
        /// <param name="channelId"></param>
        public ChatChannel(string channelId)
        {
            Id = channelId;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Fires OnChatMessage event, when new message comes
        /// </summary>
        public void ChatMessage(string messageId, string userId, string username, string content, string createTime, bool historical = false)
        {
            OnChatMessage(messageId, userId, username, content, createTime, historical);
        }

        /// <summary>
        /// Fires OnChatUpdate event when update for message comes
        /// </summary>
        public void ChatUpdate(string messageId, string content, string createTime)
        {
            OnChatUpdate(messageId, content);
        }

        /// <summary>
        /// Fires OnChatRemove event when remove message information comes
        /// </summary>
        public void ChatRemove(string messageId)
        {
            OnChatRemove(messageId);
        }

        /// <summary>
        /// Fires OnServerMessage event with username and information about joining
        /// </summary>
        public void JoinedGroup(string messageId, string username, bool historical = false)
        {
            OnServerMessage(messageId, username + " has joined", historical);
        }

        /// <summary>
        /// Fires OnServerMessage event with username and infrmation about adding
        /// </summary>
        public void AddedToGroup(string messageId, string username, bool historical = false)
        {
            OnServerMessage(messageId, username + " was added", historical);
        }

        /// <summary>
        /// Fires OnServerMessage event with username and information about leaving
        /// </summary>
        public void LeftGroup(string messageId, string username, bool historical = false)
        {
            OnServerMessage(messageId, username + " has left", historical);
        }

        /// <summary>
        /// Fires OnServerMessage event with username and information about kicking
        /// </summary>
        public void KickedFromGroup(string messageId, string username, bool historical = false)
        {
            OnServerMessage(messageId, username + " was kicked", historical);
        }

        /// <summary>
        /// Fires OnServerMessage event with username and information about promoting
        /// </summary>
        public void PromotedInGroup(string messageId, string username, bool historical = false)
        {
            OnServerMessage(messageId, username + " was promoted", historical);
        }


        public void JoinedChannel(string userId, string username, string avatar = null)
        {
            Debug.Log("User joined: " + username);

            ChatUser newUser = new ChatUser(userId, username, avatar);
            _users.Add(userId, newUser);
            OnJoinedChannel(newUser);
        }

        /// <summary>
        /// Fires OnLeftChannel event with ChatUser and then removes this user from _users dictionary
        /// </summary>
        /// <param name="userId"></param>
        public void LeftChannel(string userId)
        {
            Debug.Log("User left: " + _users[userId].Username);

            OnLeftChannel(_users[userId]);
            _users.Remove(userId);
        }

        #endregion
    }
}
