/**
 * Copyright 2021 The Nakama Authors
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

using System.Runtime.Serialization;
using Nakama;

namespace PiratePanic
{
    public class MatchEndRequest
    {
        [DataMember(Name = "matchId")]
        public string MatchId { get; private set; }

        [DataMember(Name = "placement")]
		public MatchEndPlacement Placement { get; private set; }

        [DataMember(Name = "time")]
        public float Time { get; private set; }

        [DataMember(Name = "towersDestroyed")]
        public int TowersDestroyed { get; private set; }

        public MatchEndRequest(string matchId, MatchEndPlacement placement, float time, int towersDestroyed)
        {
			MatchId = matchId;
            Placement = placement;
            Time = time;
            TowersDestroyed = towersDestroyed;
        }
    }
}