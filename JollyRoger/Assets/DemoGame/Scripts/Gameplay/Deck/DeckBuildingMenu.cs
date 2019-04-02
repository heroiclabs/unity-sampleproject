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
using System.Collections.Generic;
using System.Threading.Tasks;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Menus;
using DemoGame.Scripts.Session;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Gameplay.Decks
{
    /// <summary>
    /// Menu responsible for building users deck.
    /// Players can view their cards and upgrade them to next level.
    /// </summary>
    public class DeckBuildingMenu : Menu
    {
        #region Fields

        #region Card Lists

        /// <summary>
        /// Reference to the starting deck card list.
        /// This deck is given to the played upon first authentication.
        /// </summary>
        [SerializeField] private CardList _defaultDeck = null;

        #endregion

        #region Card Prefabs

        /// <summary>
        /// Prefab of a single card holder.
        /// </summary>
        [SerializeField] private CardSlotUI _slotPrefab = null;

        /// <summary>
        /// Prefab of card row.
        /// </summary>
        [SerializeField] private GameObject _slotRowPrefab = null;

        #endregion

        #region Visuals

        /// <summary>
        /// The number of cards in a deck.
        /// </summary>
        [SerializeField] private int _deckSize = 6;

        /// <summary>
        /// The number of cards displayed per row.
        /// </summary>
        [SerializeField] private int _cardsPerRow = 6;

        /// <summary>
        /// Panel containing all cards currently added to user's deck.
        /// </summary>
        [SerializeField] private GridLayoutGroup _deckPanel = null;

        /// <summary>
        /// When user is trying to swap cards, this backround image will flicker to visualize
        /// awailable options.
        /// </summary>
        [SerializeField] private Image _deckPanelBackground = null;

        /// <summary>
        /// Panel containing all unused cards owned by the user.
        /// </summary>
        [SerializeField] private VerticalLayoutGroup _ownedPanel = null;

        /// <summary>
        /// Textfield displaying current funds.
        /// </summary>
        [SerializeField] private Text _fundsText = null;

        #endregion

        #region Card Management

        /// <summary>
        /// Buy a random card with gems.
        /// </summary>
        [SerializeField] private Button _buyRandomCardButton = null;

        /// <summary>
        /// Instantly get free gems.
        /// </summary>
        /// <remarks>
        /// This button is for demo purposes only.
        /// </remarks>
        [SerializeField] private Button _getFreeGemsButton = null;

        /// <summary>
        /// Debug button used to remove all owned cards and replace this
        /// deck with <see cref="_defaultDeck"/>. 
        /// </summary>
        [SerializeField] private Button _clearCardsButton = null;

        #endregion

        /// <summary>
        /// Storage used to write and read the deck content from Nakama server.
        /// </summary>
        [SerializeField] private DeckStorage _deckStorage = null;
        /// <summary>
        /// Panel displaying the info of selected card.
        /// </summary>
        [SerializeField] private CardInfoSidePanel _cardInfoSidePanel = null;

        /// <summary>
        /// List of all card holders in deck.
        /// </summary>
        private List<CardSlotUI> _deckCardsDisplays = new List<CardSlotUI>();

        /// <summary>
        /// List of all card stacks in unused panel.
        /// </summary>
        private List<CardSlotStackUI> _ownedCardsDisplays = new List<CardSlotStackUI>();

        /// <summary>
        /// Reference to users deck.
        /// </summary>
        private Deck _deck = null;

        /// <summary>
        /// Card slot being currently selected.
        /// </summary>
        private CardSlotUI _selectedSlot = null;

        /// <summary>
        /// If <see cref="_usedCard"/> value is not null, user has selected this card to be put
        /// in the deck and is choosing a card from the deck to replace it with.
        /// </summary>
        private Card _usedCard = null;

        /// <summary>
        /// The ammount of currency user own.
        /// </summary>
        private int _funds = 0;

        #endregion

        #region Mono

        /// <summary>
        /// Sets buttons listeners and invokes <see cref="Init"/> on connection with Nakama.
        /// </summary>
        private void Awake()
        {
            _buyRandomCardButton.onClick.AddListener(GetRandomCard);
            _clearCardsButton.onClick.AddListener(ClearCards);
            _getFreeGemsButton.onClick.AddListener(AddFreeGems);
            base.SetBackButtonHandler(MenuManager.Instance.HideTopMenu);

            if (NakamaSessionManager.Instance.IsConnected == false)
            {
                NakamaSessionManager.Instance.OnConnectionSuccess += Init;
            }
            else
            {
                Init();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Fills the deck panel and unused cards panel with cards retrieved from the server.
        /// </summary>
        private async void Init()
        {
            NakamaSessionManager.Instance.OnConnectionSuccess -= Init;
            _deck = await _deckStorage.LoadDataAsync("deck");
            if (_deck == null)
            {
                _deck = GenerateDefaultDeck(_defaultDeck);
                await _deckStorage.StoreDataAsync(_deck);
            }
            _cardInfoSidePanel.SetDeck(_deck);

            // Create a number of CardMenuUI equal to _deckSize and initialize them with user cards.
            for (int i = _deckSize - 1; i >= 0; i--)
            {
                CardSlotUI cardDisplay = Instantiate(_slotPrefab, _deckPanel.transform, false);
                cardDisplay.Init(OnCardSelected);
                _deckCardsDisplays.Add(cardDisplay);
                if (i < _deck.usedCards.Count)
                {
                    cardDisplay.SetCard(_deck.usedCards[i]);
                }
                else
                {
                    cardDisplay.SetCard(null);
                }
            }

            // Update the unused cards panel UI
            RefreshUnusedCards(_deck.unusedCards);
            // Refresh gems counter
            await UpdateFundsCounterAsync();

            // Select first slot if no card is selected
            if (_selectedSlot == null)
            {
                OnCardSelected(_deckCardsDisplays[0]);
            }
        }

        /// <summary>
        /// Generates a default deck based on the list of cards stored in <see cref="_defaultDeck"/>.
        /// </summary>
        private Deck GenerateDefaultDeck(CardList defaultDeck)
        {
            Deck deck = new Deck();
            deck.deckName = "Default Deck";
            deck.unusedCards = new List<Card>();
            deck.usedCards = new List<Card>();
            foreach (CardInfo info in defaultDeck.CardInfos)
            {
                Card card = new Card();
                card.cardType = info.CardType;
                card.level = 1;

                if (deck.usedCards.Count < _deckSize)
                {
                    deck.usedCards.Add(card);
                    card.isUsed = true;
                }
                else
                {
                    deck.unusedCards.Add(card);
                    card.isUsed = false;
                }
            }
            return deck;
        }

        /// <summary>
        /// Updates the cards displayed by each <see cref="CardSlotUI"/> in user's deck.
        /// </summary>
        private void RefreshUsedCards(List<Card> deckCards)
        {
            for (int i = _deckSize - 1; i >= 0; --i)
            {
                CardSlotUI cardDisplay = _deckCardsDisplays[i];
                if (i > _deckSize - deckCards.Count - 1)
                {
                    cardDisplay.SetCard(deckCards[_deckSize - i - 1]);
                }
                else
                {
                    cardDisplay.SetCard(null);
                }
            }
        }

        /// <summary>
        /// Updates the cards displayed by each <see cref="CardSlotStackUI"/>.
        /// </summary>
        private void RefreshUnusedCards(List<Card> cards)
        {
            List<Card> unusedCards = new List<Card>();
            List<int> unusedCardCounts = new List<int>();

            // Sort the list of cards by their type and level
            Comparison<Card> comparison = new Comparison<Card>(CompareCard);
            cards.Sort(comparison);

            // Group all cards by their type and level
            foreach (Card card in cards)
            {
                int index = unusedCards.FindIndex(x => x.IsCopy(card));
                if (index == -1)
                {
                    unusedCards.Add(card);
                    unusedCardCounts.Add(1);
                }
                else
                {
                    unusedCardCounts[index] += 1;
                }
            }

            // Determine the row count
            int rowCount = 1;
            if (unusedCards.Count > 0)
            {
                rowCount = (int)Mathf.Ceil((float)unusedCards.Count / (float)_cardsPerRow);
            }

            // Add a number of rows to fit all owned cards
            while (rowCount * _cardsPerRow > _ownedCardsDisplays.Count)
            {
                GameObject cardsRow = Instantiate(_slotRowPrefab, _ownedPanel.transform, false);
                for (int i = 0; i < _cardsPerRow; i++)
                {
                    CardSlotStackUI cardDisplay = cardsRow.transform.GetChild(i).GetComponent<CardSlotStackUI>();
                    cardDisplay.SetCard(null);
                    cardDisplay.Init(OnCardSelected);
                    _ownedCardsDisplays.Add(cardDisplay);
                }
            }
            // Remove empty rows
            while (rowCount * _cardsPerRow < _ownedCardsDisplays.Count)
            {
                for (int i = 0; i < _cardsPerRow; i++)
                {
                    CardSlotStackUI cardDisplay = _ownedCardsDisplays[_ownedCardsDisplays.Count - 1];
                    _ownedCardsDisplays.RemoveAt(_ownedCardsDisplays.Count - 1);
                    Destroy(cardDisplay.gameObject);
                }
                Transform lastRow = _ownedPanel.transform.GetChild(_ownedPanel.transform.childCount - 1);
                Destroy(lastRow.gameObject);
            }

            // If there are less cards than the maximum card count in all existing rows,
            // set the cards of all unused slots to null
            if (rowCount * _cardsPerRow > unusedCards.Count)
            {
                for (int i = rowCount * _cardsPerRow - 1; i >= unusedCards.Count; i--)
                {
                    _ownedCardsDisplays[i].SetCard(null);
                }
            }

            // Set the card references
            for (int i = 0; i < unusedCards.Count; i++)
            {
                Card card = unusedCards[i];
                int count = unusedCardCounts[i];
                _ownedCardsDisplays[i].SetCard(card, count);
            }
        }

        /// <summary>
        /// Method used to sort a list of cards by their type and level.
        /// Initially, cards are sorted by <see cref="CardType"/> enum.
        /// If two cards have the same card type, they are sorted by their level.
        /// </summary>
        private int CompareCard(Card card1, Card card2)
        {
            // Sorting by the type
            if (card1.cardType < card2.cardType)
            {
                return -1;
            }
            else if (card1.cardType > card2.cardType)
            {
                return 1;
            }
            else
            {
                // Both cards have the same type
                // Sorting by the level
                if (card1.level < card2.level)
                {
                    return -1;
                }
                else if (card1.level > card2.level)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// A card has been selected by the user.
        /// Shows the <see cref="_cardInfoSidePanel"/> displaying selected card's info.
        /// </summary>
        private async void OnCardSelected(CardSlotUI slot)
        {
            if (slot == null)
            {
                // Cannot unselect card
                return;
            }
            else
            {
                ChangeSelection(slot);

                if (slot.Card != null && slot.Card.isUsed == true)
                {
                    // Selected card from deck

                    if (_usedCard != null)
                    {
                        // Card replacement has been already started
                        // Replacing _usedCard with selected card
                        bool good = await ReplaceCardAsync(slot.Card, _usedCard);
                        if (good == true)
                        {
                            EndReplaceCard();
                            OnCardSelected(slot);
                        }
                    }
                    else
                    {
                        _cardInfoSidePanel.SetUsedCard(slot.Card, OnMerge, _funds >= 50);
                    }
                }
                else
                {
                    // Selected card from unused card list

                    _cardInfoSidePanel.SetUnusedCard(slot.Card, BegniReplaceCard, OnMerge);
                    if (_usedCard != null)
                    {
                        // Card replacement has been started but user didn't select any card
                        // from deck. Terminating card replacement
                        EndReplaceCard();
                    }
                }
            }
        }

        /// <summary>
        /// Changes color of selected slot background.
        /// </summary>
        /// <param name="slot"></param>
        private void ChangeSelection(CardSlotUI slot)
        {
            if (_selectedSlot != null)
            {
                _selectedSlot.Unselect();
            }
            _selectedSlot = slot;
            if (_selectedSlot != null)
            {
                _selectedSlot.Select();
            }
        }

        /// <summary>
        /// Starts the card replacement process.
        /// </summary>
        private void BegniReplaceCard(Card card)
        {
            _usedCard = card;
            _deckPanelBackground.color = Color.yellow;
        }

        /// <summary>
        /// Ends the card replacement process.
        /// </summary>
        private void EndReplaceCard()
        {
            _usedCard = null;
            _deckPanelBackground.color = Color.white;
        }

        /// <summary>
        /// Replaces a card from the list of unused cards with a card from the deck.
        /// Sends the request to the server.
        /// </summary>
        private async Task<bool> ReplaceCardAsync(Card removedCard, Card usedCard)
        {
            // Perform card swap on server
            CardOperationResponse canReplace = await DeckBuildingManager.SwapAsync(removedCard, usedCard);
            if (canReplace.response == false)
            {
                Debug.LogWarning("Couldn't swap cards: " + canReplace.message);
                return false;
            }

            // Get indices of supplied cards
            int usedIndex = _deck.unusedCards.IndexOf(usedCard);
            int removedIndex = _deck.usedCards.IndexOf(removedCard);

            // Replace cards
            _deck.usedCards.RemoveAt(removedIndex);
            _deck.unusedCards.RemoveAt(usedIndex);
            _deck.usedCards.Insert(removedIndex, usedCard);
            _deck.unusedCards.Insert(usedIndex, removedCard);

            // Store changes locally
            usedCard.isUsed = true;
            removedCard.isUsed = false;

            // Update UI
            RefreshUsedCards(_deck.usedCards);
            RefreshUnusedCards(_deck.unusedCards);

            // Change selected card's background color
            CardSlotUI slot = _deckCardsDisplays.Find(x => x.Card == usedCard);
            return true;
        }

        /// <summary>
        /// Upgrades a card to the next level and removes the second.
        /// </summary>
        private async void OnMerge(Card upgraded, Card removed)
        {
            // Perform card upgrade on server
            CardOperationResponse canMerge = await DeckBuildingManager.MergeAsync(upgraded, removed);
            if (canMerge.response == false)
            {
                Debug.LogWarning("Couldn't merge cards: " + canMerge.message);
                return;
            }

            _deck.unusedCards.Remove(removed);
            // Create a new card based on the upgraded card 
            Card card = new Card();
            card.cardType = upgraded.cardType;
            card.level = upgraded.level + 1;
            card.isUsed = upgraded.isUsed;

            // Replace the upgraded card with newly created copy
            if (upgraded.isUsed == true)
            {
                int index = _deck.usedCards.IndexOf(upgraded);
                _deck.usedCards.RemoveAt(index);
                _deck.usedCards.Insert(index, card);
            }
            else
            {
                _deck.unusedCards.Remove(upgraded);
                _deck.unusedCards.Add(card);
            }

            // Update UI
            await UpdateFundsCounterAsync();
            RefreshUsedCards(_deck.usedCards);
            RefreshUnusedCards(_deck.unusedCards);

            CardSlotUI slot = null;
            if (card.isUsed == true)
            {
                slot = _deckCardsDisplays.Find(x => x.Card.IsCopy(card));
            }
            else
            {
                slot = _ownedCardsDisplays.Find(x => x.Card.IsCopy(card));
            }
            OnCardSelected(slot);
        }


        /// <summary>
        /// Debug method used to add a random card to the owned card list.
        /// </summary>
        private async void GetRandomCard()
        {
            CardOperationResponse response = await DeckBuildingManager.DebugAddRandomCardAsync();
            if (response.response == false)
            {
                Debug.Log("Couldn't receive random card: " + response.message);
                return;
            }

            Deck deck = await _deckStorage.LoadDataAsync("deck");
            if (deck != null)
            {
                await UpdateFundsCounterAsync();
                _deck.usedCards = deck.usedCards;
                _deck.unusedCards = deck.unusedCards;
                RefreshUsedCards(_deck.usedCards);
                RefreshUnusedCards(_deck.unusedCards);
            }
        }

        /// <summary>
        /// Generates a new deck based on <see cref="_defaultDeck"/>.
        /// Sends the deck to Nakama server and then stores it locally.
        /// </summary>
        private async void ClearCards()
        {
            CardOperationResponse response = await DeckBuildingManager.DebugClearDeckAsync();
            if (response.response == false)
            {
                Debug.Log("Couldn't clear deck: " + response.message);
                return;
            }

            Deck deck = GenerateDefaultDeck(_defaultDeck);
            deck.deckName = _deck.deckName;

            bool good = await _deckStorage.StoreDataAsync(deck);
            if (good == true)
            {
                _deck.usedCards = deck.usedCards;
                _deck.unusedCards = deck.unusedCards;
                RefreshUsedCards(_deck.usedCards);
                RefreshUnusedCards(_deck.unusedCards);
            }
        }

        /// <summary>
        /// Adds free gems to user's wallet.
        /// </summary>
        /// <remarks>
        /// This method is created for demo purpose only.
        /// </remarks>
        private async void AddFreeGems()
        {
            CardOperationResponse response = await DeckBuildingManager.DebugAddGemsAsync();
            if (response.response == false)
            {
                Debug.Log("Couldn't add gems: " + response.message);
                return;
            }

            await UpdateFundsCounterAsync();
        }

        /// <summary>
        /// Updates gold counter.
        /// </summary>
        public async override void Show()
        {
            await UpdateFundsCounterAsync();
            base.Show();
        }

        /// <summary>
        /// Retrieves gold count owned by the user and sets the value in UI.
        /// </summary>
        private async Task UpdateFundsCounterAsync()
        {
            IApiAccount account = await NakamaSessionManager.Instance.GetAccountAsync();
            string wallet = account.Wallet;
            try
            {
                Dictionary<string, int> currency = wallet.FromJson<Dictionary<string, int>>();
                _funds = currency["gold"];
            }
            catch (Exception)
            {
            }
            _fundsText.text = _funds.ToString();

            _buyRandomCardButton.interactable = _funds >= 50;
        }

        #endregion
    }
}
