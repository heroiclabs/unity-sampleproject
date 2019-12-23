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

using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DemoGame.Scripts.Utils;
using DemoGame.Scripts.Session;

namespace DemoGame.Scripts.Chat
{
    /// <summary>
    /// Used for communication between nakama server and local chat channels
    /// </summary>
    public class ChatManager : Singleton<ChatManager>
    {
        #region private variables

        /// <summary>
        /// Chat channels sorted by id
        /// </summary>
        private Dictionary<string, ChatChannel> _chatChannels = new Dictionary<string, ChatChannel>();

        #endregion

        #region mono and initialization

        private void Start()
        {
            //If NakamaSessionManager singleton is already connected to Nakama server - initialize, else - wait for connection
            if (NakamaSessionManager.Instance.IsConnected == true)
            {
                Init();
            }
            else
            {
                NakamaSessionManager.Instance.OnConnectionSuccess += Init;
            }
        }

        /// <summary>
        /// Initializes manager
        /// </summary>
        private void Init()
        {
            //Register methods to Socket events
            NakamaSessionManager.Instance.OnConnectionSuccess -= Init;
            NakamaSessionManager.Instance.Socket.ReceivedChannelMessage += ReceiveMessage;
            NakamaSessionManager.Instance.Socket.ReceivedChannelPresence += ReceiveChannelPresence;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Join chat with given user, returns ChatChannel object of chat, creates new ChatChannel and ads to dictionary if needed
        /// </summary>
        public async Task<ChatChannel> JoinChatWithUserAsync(string userId)
        {
            try
            {
                IChannel channel = await NakamaSessionManager.Instance.Socket.JoinChatAsync(userId, ChannelType.DirectMessage, true);

                //checks if channel is already created if yes - get it from dict and return, else create its and return 
                ChatChannel chatChannel = _chatChannels.ContainsKey(channel.Id) ? _chatChannels[channel.Id] : CreateChannel(channel.Id);

                Debug.Log("Joined direct chat: " + channel.Id);

                return chatChannel;
            }
            catch (Exception e)
            {
                Debug.LogError("Joining chat with user error: " + e);
                return null;
            }
        }

        /// <summary>
        /// Join chat with given group(clan), returns ChatChannel object of chat, creates new ChatChannel and ads to dictionary if needed
        /// </summary>
        public async Task<ChatChannel> JoinChatWithGroupAsync(string groupId)
        {
            try
            {
                IChannel channel = await NakamaSessionManager.Instance.Socket.JoinChatAsync(groupId, ChannelType.Group, true);

                //checks if channel is already created if yes - get it from dict and return, else create its and return 
                ChatChannel chatChannel = _chatChannels.ContainsKey(channel.Id) ? _chatChannels[channel.Id] : CreateChannel(channel.Id);

                //receive initial user presences
                ReceiveChannelPresence(channel.Id, channel.Presences, null);

                Debug.Log("Joined group chat: " + channel.Id);

                return chatChannel;
            }
            catch (Exception e)
            {
                Debug.LogError("Joining chat with group error: " + e);
                return null;
            }
        }

        /// <summary>
        /// Leaves chat with given id and removes it from _chatChannels dictionary
        /// </summary>
        public async Task<bool> LeaveChatChannelAsync(string channelId)
        {
            try
            {
                await NakamaSessionManager.Instance.Socket.LeaveChatAsync(channelId);

                _chatChannels.Remove(channelId);

                Debug.Log("Leaved chat: " + channelId);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't leave chat: " + e);
                return false;
            }
        }
        /// <summary>
        /// Sends new message to channel with given id
        /// </summary>
        public async void SendNewMessage(string channelId, string message)
        {
            try
            {
                //Pack message content to json
                string jsonMessage = (new Dictionary<string, string> { { "message", message } }).ToJson();

                IChannelMessageAck response = await NakamaSessionManager.Instance.Socket.WriteChatMessageAsync(channelId, jsonMessage);

                Debug.Log("Message created by username " + response.Username + ", on channel " + response.ChannelId + ", on date " + response.CreateTime);
            }
            catch (Exception e)
            {
                Debug.LogError("Message sending error: " + e);
            }
        }

        /// <summary>
        /// Updates message with given id on channel with given id
        /// </summary>
        public async Task<bool> EditMessageAsync(string channelId, string messageId, string message)
        {
            try
            {
                //Pack message content to json
                string jsonMessage = (new Dictionary<string, string> { { "message", message } }).ToJson();

                IChannelMessageAck response = await NakamaSessionManager.Instance.Socket.UpdateChatMessageAsync(channelId, messageId, jsonMessage);

                Debug.Log("Message edited by username " + response.Username + ", on channel " + response.ChannelId + ", on date " + response.CreateTime);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Message edit error: " + e);
                return false;
            }
        }

        /// <summary>
        /// Removes message with given id on channel with given id
        /// </summary>
        public async Task<bool> RemoveMessageAsync(string channelId, string messageId)
        {
            try
            {
                IChannelMessageAck response = await NakamaSessionManager.Instance.Socket.RemoveChatMessageAsync(channelId, messageId);

                Debug.Log("Message removed by username " + response.Username + ", on channel " + response.ChannelId + ", on date " + response.CreateTime);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Message removing error: " + e);
                return false;
            }
        }

        /// <summary>
        /// Loads historical messages to chat channel, returns if channel history next cursor isn't null
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<bool> LoadChannelHistoryAsync(ChatChannel channel)
        {
            try
            {
                Client client = NakamaSessionManager.Instance.Client;
                ISession session = NakamaSessionManager.Instance.Session;

                IApiChannelMessageList loadedMessages = await client.ListChannelMessagesAsync(session, channel.Id, 10, false, channel.NextCursor);

                foreach (IApiChannelMessage message in loadedMessages.Messages)
                {
                    ReceiveMessage(message, true);
                }

                channel.NextCursor = loadedMessages.NextCursor;

                //if next cursor is empty - no more messages could be loaded - return false, else return true
                if (string.IsNullOrEmpty(channel.NextCursor))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error while loading channel history: " + e);
                return true;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Dispatches <see cref="ReceiveMessage(IApiChannelMessage, bool)"/> to be runned in main thread
        /// </summary>
        private void ReceiveMessage(IApiChannelMessage message)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => ReceiveMessage(message));
        }

        /// <summary>
        /// Receives and translates incoming messages
        /// </summary>
        private void ReceiveMessage(IApiChannelMessage message, bool historical = false)
        {
            Debug.Log("Received message: " + message);

            ChatChannel channel;
            string messageContent = "";

            //Search for chat channel which should receive message, if it's not created - creates it
            if (_chatChannels.ContainsKey(message.ChannelId))
            {
                channel = _chatChannels[message.ChannelId];
            }
            else
            {
                channel = CreateChannel(message.ChannelId);
            }

            //Translate message basing on message code
            switch (message.Code)
            {
                //Receiving chat message
                case 0:
                    if (!string.IsNullOrEmpty(message.Content) && message.Content != "{}")
                    {
                        messageContent = JsonParser.FromJson<Dictionary<string, string>>(message.Content)["message"];
                    }

                    channel.ChatMessage(message.MessageId, message.SenderId, message.Username, messageContent, message.CreateTime, historical);

                    break;

                //Receiving chat message update
                case 1:
                    if (!string.IsNullOrEmpty(message.Content) && message.Content != "{}")
                    {
                        messageContent = JsonParser.FromJson<Dictionary<string, string>>(message.Content)["message"];
                    }

                    channel.ChatUpdate(message.MessageId, messageContent, message.CreateTime);

                    break;

                //Receiving chat message remove
                case 2:
                    channel.ChatRemove(message.MessageId);

                    break;

                //Receiving information about user joined group
                case 3:
                    channel.JoinedGroup(message.MessageId, message.Username, historical);

                    break;

                //Receiving information about user added to group
                case 4:
                    channel.AddedToGroup(message.MessageId, message.Username, historical);

                    break;

                //Receiving information about user left group
                case 5:
                    channel.LeftGroup(message.MessageId, message.Username, historical);

                    break;

                //Receiving information about user kicked group
                case 6:
                    channel.KickedFromGroup(message.MessageId, message.Username, historical);

                    break;

                //Receiving information about user promoted in group
                case 7:
                    channel.PromotedInGroup(message.MessageId, message.Username, historical);

                    break;
            }
        }

        /// <summary>
        /// Dispatches <see cref="ReceiveChannelPresence(string, IEnumerable{IUserPresence}, IEnumerable{IUserPresence})"/> to be runned in main thread
        /// </summary>
        private void ReceiveChannelPresence(IChannelPresenceEvent presenceEvent)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => ReceiveChannelPresence(presenceEvent.ChannelId, presenceEvent.Joins, presenceEvent.Leaves));
        }


        /// <summary>
        /// Receives information about users presence on channel, manages users joining and leaving channel
        /// </summary>
        private void ReceiveChannelPresence(string channelId, IEnumerable<IUserPresence> joins, IEnumerable<IUserPresence> leaves = null)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            ChatChannel channel;

            //Receiving chat message
            if (_chatChannels.ContainsKey(channelId))
            {
                channel = _chatChannels[channelId];
            }
            else
            {
                channel = CreateChannel(channelId);
            }

            //Managing joining users
            foreach (IUserPresence presence in joins)
            {
                channel.JoinedChannel(presence.UserId, presence.Username);
            }

            //Managing leaving users
            if (leaves != null)
            {
                foreach (IUserPresence presence in leaves)
                {
                    channel.LeftChannel(presence.UserId);
                }
            }
        }

        /// <summary>
        /// Creates new <see cref="ChatChannel"/> with given id and returns it
        /// </summary>
        private ChatChannel CreateChannel(string channelId)
        {
            ChatChannel channel = new ChatChannel(channelId);
            _chatChannels.Add(channelId, channel);
            return channel;
        }

        #endregion
    }
}
