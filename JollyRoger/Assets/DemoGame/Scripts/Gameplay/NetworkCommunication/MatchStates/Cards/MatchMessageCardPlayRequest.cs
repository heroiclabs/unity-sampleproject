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

using DemoGame.Scripts.Gameplay.Cards;


namespace DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates
{

    /// <summary>
    /// Requests a card play.
    /// </summary>
    public class MatchMessageCardPlayRequest : MatchMessage<MatchMessageCardPlayRequest>
    {
        #region Fields
        
        /// <summary>
        /// The user id of the card owner.
        /// </summary>
        public readonly string PlayerId;

        /// <summary>
        /// Played card.
        /// </summary>
        public readonly Card Card;

        /// <summary>
        /// Index of the slot the played card is used from.
        /// </summary>
        public readonly int CardSlotIndex;

        /// <summary>
        /// X position in world where the card was played.
        /// </summary>
        public readonly float X;

        /// <summary>
        /// Y position in world where the card was played.
        /// </summary>
        public readonly float Y;

        /// <summary>
        /// Z position in world where the card was played.
        /// </summary>
        public readonly float Z;

        #endregion

        public MatchMessageCardPlayRequest(string playerId, Card card, int cardSlotIndex, float x, float y, float z)
        {
            PlayerId = playerId;
            Card = card;
            CardSlotIndex = cardSlotIndex;
            X = x;
            Y = y;
            Z = z;
        }
    }

}