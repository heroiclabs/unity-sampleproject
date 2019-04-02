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

using DemoGame.Scripts.Utils;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Singleton containing the list of all available cards in game.
    /// </summary>
    public class CardListSingleton : Singleton<CardListSingleton>
    {
        #region Fields

        /// <summary>
        /// Reference to list of all available cards.
        /// </summary>
        [SerializeField] private CardList _allCards = null;

        /// <summary>
        /// Reference to list of all starting towers.
        /// </summary>
        [SerializeField] private CardList _startingTowers = null;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the list of all available cards.
        /// </summary>
        public CardList AllCards => _allCards;

        /// <summary>
        /// Returns the list of all starting towers.
        /// </summary>
        public CardList StartingTowers => _startingTowers;

        #endregion

        #region Monobehaviour

        /// <summary>
        /// Makes this gameobject persistent throughout the game.
        /// </summary>
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        #endregion
    }
}
