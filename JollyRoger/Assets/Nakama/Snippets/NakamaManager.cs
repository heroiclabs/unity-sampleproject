/**
 * Copyright 2019 The Nakama Authors
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
using System.Threading.Tasks;
using UnityEngine;

namespace Nakama.Snippets
{
    /// <summary>
    /// Manages a Nakama client and optional socket connection.
    /// </summary>
    /// <seealso cref="NakamaManagerUsage"/>
    public class NakamaManager : MonoBehaviour
    {
        private const string SessionPrefName = "nakama.session";
        private const string SingletonName = "/[NakamaManager]";

        private static readonly object Lock = new object();
        private static NakamaManager _instance;

        /// <summary>
        /// The singleton instance of the Nakama sdk manager.
        /// </summary>
        public static NakamaManager Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance != null) return _instance;
                    var go = GameObject.Find(SingletonName);
                    if (go == null)
                    {
                        go = new GameObject(SingletonName);
                    }

                    if (go.GetComponent<NakamaManager>() == null)
                    {
                        go.AddComponent<NakamaManager>();
                    }
                    DontDestroyOnLoad(go);
                    _instance = go.GetComponent<NakamaManager>();
                    return _instance;
                }
            }
        }

        public IClient Client { get; }
        public ISocket Socket { get; }

        public Task<ISession> Session { get; private set; }
        public Dictionary<string, Tuple<List<IUserPresence>, List<IApiChannelMessage>>> ChannelCache { get; }

        private NakamaManager()
        {
            Client = new Client("http", "127.0.0.1", 7350, "defaultkey")
            {
#if UNITY_EDITOR
                Logger = new UnityLogger()
#endif
            };
            ChannelCache = new Dictionary<string, Tuple<List<IUserPresence>, List<IApiChannelMessage>>>();
            Socket = Client.NewSocket();
            Socket.Closed += () => ChannelCache.Clear();
            Socket.ReceivedChannelMessage += msg =>
            {
                Tuple<List<IUserPresence>, List<IApiChannelMessage>> channel;
                var exists = ChannelCache.TryGetValue(msg.ChannelId, out channel);
                if (exists)
                {
                    channel.Item2.Add(msg);
                }
                else
                {
                    Debug.LogErrorFormat("Msg on channel id '{0}' but not in cache.", msg.ChannelId);
                }
            };
            Socket.ReceivedChannelPresence += presenceEvent =>
            {
                Tuple<List<IUserPresence>, List<IApiChannelMessage>> channel;
                var exists = ChannelCache.TryGetValue(presenceEvent.ChannelId, out channel);
                if (exists)
                {
                    foreach (var presence in presenceEvent.Leaves)
                    {
                        channel.Item1.Remove(presence);
                    }
                    channel.Item1.AddRange(presenceEvent.Joins);
                }
                else
                {
                    Debug.LogErrorFormat("Presence event on channel id '{0}' but not in cache.", presenceEvent.ChannelId);
                }
            };
        }

        public async Task<IChannel> JoinChatAsync(string target, ChannelType type, bool persistence = false, bool hidden = false)
        {
            var channel = await Socket.JoinChatAsync(target, type, persistence, hidden);
            var presences = new List<IUserPresence>(10);
            presences.AddRange(channel.Presences);
            var messages = new List<IApiChannelMessage>(0);
            ChannelCache.Add(channel.Id, new Tuple<List<IUserPresence>, List<IApiChannelMessage>>(presences, messages));
            return channel;
        }

        public Task LeaveChatAsync(IChannel channel) => LeaveChatAsync(channel.Id);

        public async Task LeaveChatAsync(string channelId)
        {
            await Socket.LeaveChatAsync(channelId);
            ChannelCache.Remove(channelId);
        }

        private Task<ISession> AuthenticateAsync()
        {
            // Modify to fit the authentication strategy you want within your game.
            // EXAMPLE:
            const string deviceIdPrefName = "deviceid";
            var deviceId = PlayerPrefs.GetString(deviceIdPrefName, SystemInfo.deviceUniqueIdentifier);
#if UNITY_EDITOR
            Debug.LogFormat("Device id: {0}", deviceId);
#endif
            // With device IDs save it locally in case of OS updates which can change the value on device.
            PlayerPrefs.SetString(deviceIdPrefName, deviceId);
            return Client.AuthenticateDeviceAsync(deviceId);
        }

        private void Awake()
        {
            // Restore session or create a new one.
            var authToken = PlayerPrefs.GetString(SessionPrefName);
            var session = Nakama.Session.Restore(authToken);
            var expiredDate = DateTime.UtcNow.AddDays(-1);
            if (session == null || session.HasExpired(expiredDate))
            {
                var sessionTask = AuthenticateAsync();
                Session = sessionTask;
                sessionTask.ContinueWith(t =>
                {
                    if (t.IsCompleted)
                    {
                        PlayerPrefs.SetString(SessionPrefName, t.Result.AuthToken);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                Session = Task.FromResult(session);
            }
        }

        private void OnApplicationQuit() => Socket?.CloseAsync();
    }
}
