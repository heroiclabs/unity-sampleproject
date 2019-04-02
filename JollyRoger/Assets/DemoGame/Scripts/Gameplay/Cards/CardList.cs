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
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Scriptable object containing a list of <see cref="CardInfo"/>.
    /// Used for easy card management and distinction.
    /// </summary>
    [CreateAssetMenu(fileName = "CardList", menuName = "Deck/Card List")]
    public class CardList : ScriptableObject
    {

        /// <summary>
        /// List of card infos stored by this object.
        /// </summary>
        [SerializeField] private List<CardInfo> cardInfos = null;

        /// <summary>
        /// Returns the list of card infos stored in this scriptable object.
        /// </summary>
        public List<CardInfo> CardInfos => cardInfos;

    }
}
