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
using DemoGame.Scripts.Gameplay.Cards;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Decks
{
    /// <summary>
    /// Contains a list of all used and unused cards owned by a user.
    /// </summary>
    public class Deck
    {
        /// <summary>
        /// The name of this deck.
        /// </summary>
        [SerializeField] public string deckName = "Deck Name";

        /// <summary>
        /// List of all cards owned by the deck owner that are currently not in the deck.
        /// </summary>
        [SerializeField] public List<Card> unusedCards = new List<Card>();

        /// <summary>
        /// List of all cards owned by the deck owner that are currently in the deck.
        /// </summary>
        [SerializeField] public List<Card> usedCards = new List<Card>();
    }
}
