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

using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Used to show a stack of copies of a card.
    /// </summary>
    public class CardSlotStackUI : CardSlotUI
    {
        #region Fields

        /// <summary>
        /// Image displaying the card beneath the top card.
        /// Used to visualize the stack of cards.
        /// </summary>
        [SerializeField] private Image _firstStackedCard = null;

        /// <summary>
        /// Image displaying the card beneath the <see cref="_firstStackedCard"/> card.
        /// Used to visualize the stack of cards.
        /// </summary>
        [SerializeField] private Image _secondStackedCard = null;

        /// <summary>
        /// Displays the number of cards in the stack.
        /// </summary>
        [SerializeField] private Text _countText = null;

        /// <summary>
        /// Backgroung image of cost textfield.
        /// This image is hidden if card stack has no cards inside.
        /// </summary>
        [SerializeField] private Image _costBackground = null;

        #endregion

        #region Properties

        /// <summary>
        /// The number of cards in the stack.
        /// </summary>
        public int Count { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the card displayed in this slot and sets its count.
        /// </summary>
        public override void SetCard(Card card = null)
        {
            base.SetCard(card);
            if (card != null)
            {
                SetCount(1);
            }
            else
            {
                SetCount(0);
            }
        }

        /// <summary>
        /// Sets the card displayed in this slot and sets its count.
        /// </summary>
        public void SetCard(Card card, int count)
        {
            base.SetCard(card);
            SetCount(count);
        }

        /// <summary>
        /// Sets the count texftield for this card stack.
        /// If there are more than one cards on the stack,
        /// <see cref="_firstStackedCard"/> and <see cref="_secondStackedCard"/>
        /// will be shown.
        /// </summary>
        public void SetCount(int count)
        {
            Count = count;
            if (count == 0)
            {
                _costBackground.enabled = false;
            }
            else
            {
                _costBackground.enabled = true;
            }

            if (count <= 1)
            {
                _countText.text = "";
            }
            else
            {
                _countText.text = count.ToString();
            }

            if (count <= 1)
            {
                _firstStackedCard.gameObject.SetActive(false);
                _secondStackedCard.gameObject.SetActive(false);
            }
            else if (count == 2)
            {
                _firstStackedCard.gameObject.SetActive(true);
                _secondStackedCard.gameObject.SetActive(false);
            }
            else
            {
                _firstStackedCard.gameObject.SetActive(true);
                _secondStackedCard.gameObject.SetActive(true);
            }
        }
        #endregion
    }
}