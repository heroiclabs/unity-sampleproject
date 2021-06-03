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

namespace PiratePanic
{
	/// <summary>
 	/// A card that has been played by a user within a match.
 	/// </summary>
	public class PlayedCard
	{
		[DataMember("player_id")]
		public string PlayerId { get; }

		[DataMember("card")]
		public Card Card { get; }

		[DataMember("node_x")]
		public int NodeX { get; }

		[DataMember("node_y")]
		public int NodeY { get; }

		[DataMember("hand_index")]
		public int HandIndex { get; }

		public PlayedCard(string playerId, Card card, int nodeX, int nodeY, int handIndex)
		{
			PlayerId = playerId;
			Card = card;
			NodeX = nodeX;
			NodeY = nodeY;
			HandIndex = handIndex;
		}
	}
}
