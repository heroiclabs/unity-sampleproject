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

using System.Collections.Generic;

namespace DemoGame.Scripts.Matchmaking
{

    /// <summary>
    /// Parameters supplied to <see cref="Nakama.ISocket.AddMatchmakerAsync()"/>.
    /// </summary>
    public class MatchmakingParams
    {
        /// <summary>
        /// Defines how the user wants to find their opponents.
        /// More information here: https://heroiclabs.com/docs/gameplay-matchmaker/#query
        /// </summary>
        public string query = "*";

        /// <summary>
        /// The minimum number of players for a match to start.
        /// </summary>
        public int minUserCount = 2;

        /// <summary>
        /// The maximum number of players for a match to start.
        /// </summary>
        public int maxUserCount = 2;

        /// <summary>
        /// String properties used by <see cref="query"/> for better matchmaking experience.
        /// </summary>
        public Dictionary<string, string> stringProperties = null;

        /// <summary>
        /// Numeric properties used by <see cref="query"/> for better matchmaking experience.
        /// </summary>
        public Dictionary<string, double> numericProperties = null;
    }

}