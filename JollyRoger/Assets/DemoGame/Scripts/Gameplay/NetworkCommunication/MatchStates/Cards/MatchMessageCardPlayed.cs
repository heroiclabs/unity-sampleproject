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
    /// Contains data of currently played card.
    /// </summary>
    public class MatchMessageCardPlayed : MatchMessage<MatchMessageCardPlayed>
    {
        #region Fields

        /// <summary>
        /// The id of card owner.
        /// </summary>
        public readonly string PlayerId;

        /// <summary>
        /// Card that is being played.
        /// </summary>
        public readonly Card Card;

        /// <summary>
        /// Index of the slot the played card is used from.
        /// </summary>
        public readonly int CardSlotIndex;

        /// <summary>
        /// Card to replace the played card.
        /// </summary>
        public readonly Card NewCard;

        /// <summary>
        /// X index of the node this card is being played to.
        /// </summary>
        public readonly int NodeX;

        /// <summary>
        /// Y index of the node this card is being played to.
        /// </summary>
        public readonly int NodeY;

        #endregion

        public MatchMessageCardPlayed(string playerId, Card card, int cardSlotIndex, Card newCard, int nodeX, int nodeY)
        {
            PlayerId = playerId;
            Card = card;
            CardSlotIndex = cardSlotIndex;
            NewCard = newCard;
            NodeX = nodeX;
            NodeY = nodeY;
        }
    }

}