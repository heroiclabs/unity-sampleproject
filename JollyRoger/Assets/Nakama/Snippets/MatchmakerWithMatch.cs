﻿/**
 * Copyright 2018 The Nakama Authors
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

using System.Collections;
using System.Collections.Generic;
using System.Net;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;

public class MatchmakerWithMatch : MonoBehaviour
{
    private IClient _client = new Client("http", "127.0.0.1", 7350, "defaultkey");
    private ISocket _socket;

    async void Start()
    {
        var deviceid = SystemInfo.deviceUniqueIdentifier;
        // NOTE should cache a user session.
        var session = await _client.AuthenticateDeviceAsync(deviceid);
        Debug.LogFormat("Session '{0}'", session);

        _socket = _client.NewSocket();

        IUserPresence self = null;
        var connectedOpponents = new List<IUserPresence>(0);
        _socket.ReceivedMatchmakerMatched += async (matched) =>
        {
            Debug.LogFormat("Matched '{0}'", matched);
            var match = await _socket.JoinMatchAsync(matched);
            self = match.Self;
            Debug.LogFormat("Self '{0}'", self);
            connectedOpponents.AddRange(match.Presences);

            // NOTE shows how to send match state messages.
            var newState = new Dictionary<string, string> {{"hello", "world"}}.ToJson();
            await _socket.SendMatchStateAsync(match.Id, 0, newState); // Send to all connected users.
        };
        _socket.ReceivedMatchPresence += (presenceChange) =>
        {
            connectedOpponents.AddRange(presenceChange.Joins);
            foreach (var leave in presenceChange.Leaves)
            {
                connectedOpponents.RemoveAll(item => item.SessionId.Equals(leave.SessionId));
            };
            // Remove yourself from connected opponents.
            connectedOpponents.RemoveAll(item => {
                return self != null && item.SessionId.Equals(self.SessionId);
            });
        };
        _socket.ReceivedMatchState += (message) =>
        {
            var enc = System.Text.Encoding.UTF8;
            Debug.LogFormat("Match state '{0}'", enc.GetString(message.State));
        };
        _socket.Connected += () => Debug.Log("Socket connected.");
        _socket.Closed += () => Debug.Log("Socket disconnected.");

        await _socket.ConnectAsync(session);

        int minCount = 2;
        int maxCount = 8;
        var matchmakerTicket = await _socket.AddMatchmakerAsync("*", minCount, maxCount);
        Debug.LogFormat("Matchmaker ticket '{0}'", matchmakerTicket);
    }

    async void OnApplicationQuit()
    {
        if (_socket != null)
        {
            await _socket.CloseAsync();
        }
    }
}
