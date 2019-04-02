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
using System.Threading.Tasks;
using Nakama;
using UnityEngine;

namespace DemoGame.Scripts.Matchmaking
{

    /// <summary>
    /// Handles entering and exiting matchmaking queue.
    /// </summary>
    public static class MatchmakingManager
    {

        /// <summary>
        /// Adds user to matchmaking queue with given params.
        /// Returns matchmaker ticket on succes or null on failure.
        /// </summary>
        public static async Task<IMatchmakerTicket> EnterQueueAsync(ISocket socket, MatchmakingParams matchmakingParams)
        {
            if (matchmakingParams == null)
            {
                Debug.LogError("Matchmaking params cannot be null");
                return null;
            }

            try
            {
                // Acquires matchmaking ticket used to join a match
                IMatchmakerTicket ticket = await socket.AddMatchmakerAsync(
                    matchmakingParams.query,
                    matchmakingParams.minUserCount,
                    matchmakingParams.maxUserCount,
                    matchmakingParams.stringProperties,
                    matchmakingParams.numericProperties);

                return ticket;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An error has occured while joining the matchmaker: " + e);
                return null;
            }
        }

        /// <summary>
        /// Removes user from a queue.
        /// Returns matchmaker true on succes.
        /// </summary>
        public static async Task<bool> LeaveQueueAsync(ISocket socket, IMatchmakerTicket ticket)
        {
            try
            {
                await socket.RemoveMatchmakerAsync(ticket);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("An exception has occured while leaving matchamaker: " + e);
                return false;
            }
        }
    }

}