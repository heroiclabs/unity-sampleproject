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

using System;
using DemoGame.Scripts.Gameplay.Decks;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Side panel in <see cref="DeckBuildingMenu"/> displaying informations of
    /// supplied <see cref="Card"/>.
    /// </summary>
    public class CardInfoSidePanel : MonoBehaviour
    {

        #region Fields

        /// <summary>
        /// Textfield containing the name of <see cref="_displayedCard"/>.
        /// </summary>
        [SerializeField] private Text _cardName = null;

        /// <summary>
        /// Textfield containing the level of <see cref="_displayedCard"/>.
        /// </summary>
        [SerializeField] private Text _cardLevel = null;

        /// <summary>
        /// Textfield containing the description of <see cref="_displayedCard"/>.
        /// </summary>
        [SerializeField] private Text _cardDescription = null;

        /// <summary>
        /// Textfield containing the cost of <see cref="_displayedCard"/>.
        /// </summary>
        [SerializeField] private Text _cardCost = null;

        /// <summary>
        /// Image showing the visual representation of <see cref="_displayedCard"/>.
        /// </summary>
        [SerializeField] private Image _cardImage = null;


        /// <summary>
        /// Button responsible for adding selected card to the deck.
        /// </summary>
        [SerializeField] private Button _useButton = null;

        /// <summary>
        /// Button responsible for upgrading selected card to the next level.
        /// </summary>
        [SerializeField] private Button _upgradeButton = null;

        /// <summary>
        /// Card the info of which is currently being displayed by this <see cref="CardInfoSidePanel"/>.
        /// </summary>
        private Card _displayedCard;

        /// <summary>
        /// The deck reference owned by local user.
        /// </summary>
        private Deck _deck;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the deck reference.
        /// </summary>
        public void SetDeck(Deck deck)
        {
            this._deck = deck;
        }

        /// <summary>
        /// Sets the <see cref="_displayedCard"/> reference.
        /// Supplied <paramref name="card"/> must be one of cards not used in the deck.
        /// </summary>
        public void SetUnusedCard(Card card, Action<Card> onSwapBegin, Action<Card, Card> onMerge)
        {
            if (card != null)
            {
                SetCardInfo(card);
            }
            else
            {
                // Cannot unselect card
                return;
            }

            // Reset the listener of the _useButton
            _useButton.interactable = true;
            _useButton.onClick.RemoveAllListeners();
            _useButton.onClick.AddListener(() => onSwapBegin(card));

            // Search for another copy of selected card
            Card other = GetCardToMerge(card, _deck);
            if (other == null)
            {
                // This is the only copy of this card; this card cannot be upgraded
                _upgradeButton.interactable = false;
            }
            else
            {
                // A copy of selected card has been found; upgrade is possible
                // Reset the listener of the _upgradeButton
                _upgradeButton.interactable = true;
                _upgradeButton.onClick.RemoveAllListeners();

                // Upgrading removes one copy of upgraded card
                // Determine which card should be removed and which upgraded
                Card removed = other.isUsed == false ? other : card;
                Card merged = removed != other ? other : card;

                _upgradeButton.onClick.AddListener(() => onMerge(merged, removed));
            }
        }

        /// <summary>
        /// Sets the <see cref="_displayedCard"/> reference.
        /// Supplied <paramref name="card"/> must be one of cards already used in the deck.
        /// </summary>
        public void SetUsedCard(Card card, Action<Card, Card> onMerge, bool sufficientFunds)
        {
            if (card != null)
            {
                SetCardInfo(card);
            }
            else
            {
                // Cannot unselect card
                return;
            }

            // Cards already in the deck cannot be added to deck again
            _useButton.interactable = false;

            // Search for another copy of selected card
            Card mergeCard = GetCardToMerge(card, _deck);
            if (mergeCard == null)
            {
                // This is the only copy of this card; this card cannot be upgraded
                _upgradeButton.interactable = false;
            }
            else
            {
                // A copy of selected card has been found; upgrade is possible
                // Reset the listener of the _upgradeButton
                // User have to have enough gold to upgrade
                _upgradeButton.interactable = sufficientFunds;
                _upgradeButton.onClick.RemoveAllListeners();

                // Because currently displayed card is in the deck and users deck must
                // contain 6 cards at all times, the other copy of selected card will be removed
                _upgradeButton.onClick.AddListener(() => onMerge(card, mergeCard));
            }
        }

        /// <summary>
        /// Sets the UI to display celected <paramref name="card"/> info.
        /// </summary>
        private void SetCardInfo(Card card)
        {
            _displayedCard = card;
            _cardLevel.text = $"lvl {card.level.ToString()}";
            _cardDescription.text = card.GetCardInfo().Description;
            _cardCost.text = card.GetCardInfo().Cost.ToString();
            _cardName.text = card.GetCardInfo().Name;
            _cardImage.sprite = card.GetCardInfo().Sprite;
        }

        /// <summary>
        /// Searches the <paramref name="deck"/> for a copy of supplied card.
        /// Two cards are copies of each other if both have the same <see cref="Card.cardType"/>
        /// and <see cref="Card.level"/> and are not the same card.
        /// </summary>
        private Card GetCardToMerge(Card card, Deck deck)
        {
            if (card == null)
            {
                return null;
            }

            foreach (Card other in deck.unusedCards)
            {
                if (other == card)
                {
                    continue;
                }
                if (other.IsCopy(card) == false)
                {
                    continue;
                }
                return other;
            }

            if (card.isUsed == true)
            {
                return null;
            }
            else
            {
                foreach (Card other in deck.usedCards)
                {
                    if (other == card)
                    {
                        continue;
                    }
                    if (other.IsCopy(card) == false)
                    {
                        continue;
                    }
                    return other;
                }
            }

            return null;
        }
        #endregion
    }
}
