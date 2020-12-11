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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
    /// <summary>
    /// Menu responsible for building users deck.
    /// Players can view their cards and upgrade them to next level.
    /// </summary>
    public class CardsMenuUI : Menu
	{
		/// <summary>
		/// Prefab of a single card holder.
		/// </summary>
		[SerializeField] private CardSlotUI _slotPrefab = null;

		/// <summary>
		/// Prefab of stacked card slot.
		/// </summary>
		[SerializeField] private GameObject _stackedSlotPrefab = null;

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
		[SerializeField] private GridLayoutGroup _ownedPanel = null;

		/// <summary>
		/// Textfield displaying current funds.
		/// </summary>
		[SerializeField] private Text _fundsText = null;

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

		/// <summary>
		/// Panel displaying the info of selected card.
		/// </summary>
		[SerializeField] private CardInfoSidePanel _cardInfoSidePanel = null;

		[SerializeField] private Image _storagePanelBackground = null;

		/// <summary>
		/// List of all card holders in deck.
		/// </summary>
		private List<CardSlotUI> _deckCardsDisplays = new List<CardSlotUI>();

		/// <summary>
		/// List of all card stacks in unused panel.
		/// </summary>
		private List<CardSlotStackUI> _storedCardDisplays = new List<CardSlotStackUI>();

		/// <summary>
		/// Reference to users deck.
		/// </summary>
		private CardCollection _cardCollection = null;

		private CardSlotUI _lastSelectedSlot = null;

		private bool _primedSwap;

		private GameConnection _connection;

		private void Awake()
		{
			_buyRandomCardButton.onClick.AddListener(HandleBuyRandomCard);
			_clearCardsButton.onClick.AddListener(HandleClearCards);
			_getFreeGemsButton.onClick.AddListener(HandleAddFreeGems);
			_backButton.onClick.AddListener(() => Hide());
		}

		/// <summary>
		/// Fills the deck panel and unused cards panel with cards retrieved from the server.
		/// </summary>
		public void Init(GameConnection connection)
		{
			_connection = connection;
			_cardInfoSidePanel.Init(connection);
			_cardInfoSidePanel.OnAfterCardUpgraded += OnAfterCardUpgraded;
		}

        private async void OnAfterCardUpgraded(Card oldCard, CardData upgradedCardData)
        {
			_lastSelectedSlot = null;

			try
			{
				_connection.Account = await _connection.Client.GetAccountAsync(_connection.Session);
				UpdateGemsUI();
				_cardInfoSidePanel.SetActiveCard(new Card(oldCard.Id, upgradedCardData), BeginSwapCard);
				_cardInfoSidePanel.EnableUpgradeButton(HasGemsForUpgrade());
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Could not upgrade card: " + e.Message);
			}

			if (_cardCollection.DeckContains(oldCard.Id))
			{
				_cardCollection.UpgradeCard(oldCard.Id, upgradedCardData);
				UpdateDeckCardsUI(_cardCollection.GetDeckList());
			}
			else if (_cardCollection.StoredContains(oldCard.Id))
			{
				_cardCollection.UpgradeCard(oldCard.Id, upgradedCardData);
				UpdateStoredCardsUI(_cardCollection.GetStoredList());
			}
        }

        /// <summary>
        /// Updates the cards displayed by each <see cref="CardSlotUI"/> in user's deck.
        /// </summary>
        private void UpdateDeckCardsUI(List<string> deckCards)
		{
			foreach (CardSlotUI cardSlot in _deckCardsDisplays)
			{
				GameObject.Destroy(cardSlot.gameObject);
			}

			_deckCardsDisplays.Clear();

			foreach (string id in _cardCollection.GetDeckList())
			{
				CardSlotUI cardDisplay = Instantiate(_slotPrefab, _deckPanel.transform, false);
				cardDisplay.Init(OnCardSelected);
				cardDisplay.SetCard(_cardCollection.GetDeckCard(id));
				_deckCardsDisplays.Add(cardDisplay);
			}
		}

		/// <summary>
		/// Updates the cards displayed by each <see cref="CardSlotStackUI"/>.
		/// </summary>
		private void UpdateStoredCardsUI(List<string> cards)
		{
			foreach (CardSlotStackUI cardSlot in _storedCardDisplays)
			{
				GameObject.Destroy(cardSlot.gameObject);
			}

			_storedCardDisplays.Clear();

			var cardsByType = new Dictionary<CardType, List<Card>>();

			for (int i = 0; i < cards.Count; i++)
			{
				CardType type = _cardCollection.GetStoredCard(cards[i]).CardData.type;

				if (!cardsByType.ContainsKey(type))
				{
					cardsByType[type] = new List<Card>();
				}

				cardsByType[type].Add(_cardCollection.GetStoredCard(cards[i]));
			}

			IEnumerable<CardType> sortedKeys = cardsByType.Keys.OrderBy(key => key);

			foreach (CardType key in sortedKeys)
			{
				GameObject stackedSlot = GameObject.Instantiate(_stackedSlotPrefab);
				var cardDisplay = stackedSlot.GetComponent<CardSlotStackUI>();
				cardDisplay.SetCard(cardsByType[key].First(), cardsByType[key].Count);
				cardDisplay.Init(OnCardSelected);
				_storedCardDisplays.Add(cardDisplay);
				cardDisplay.transform.SetParent(_ownedPanel.transform);
				cardDisplay.transform.localScale = Vector3.one;
			}
		}

		/// <summary>
		/// A card has been selected by the user.
		/// Shows the <see cref="_cardInfoSidePanel"/> displaying selected card's info.
		/// </summary>
		private async void OnCardSelected(CardSlotUI slot)
		{
			_lastSelectedSlot?.Unselect();

			// Selected card from deck
			if (_lastSelectedSlot == null || !_primedSwap)
			{
				_cardInfoSidePanel.SetActiveCard(slot.Card, BeginSwapCard);
				_cardInfoSidePanel.EnableUpgradeButton(HasGemsForUpgrade());
				_lastSelectedSlot = slot;
				slot.Select();
			}
			else if (_primedSwap)
			{
				string deckCard = _cardCollection.DeckContains(slot.Card.Id) ? slot.Card.Id : _lastSelectedSlot.Card.Id;
				string storedCard = _cardCollection.StoredContains(slot.Card.Id) ? slot.Card.Id : _lastSelectedSlot.Card.Id;

				_cardCollection = await SwapCards(deckCard, storedCard);
				_cardInfoSidePanel.SetCardCollection(_cardCollection);
				EndSwapCard();
				UpdateDeckCardsUI(_cardCollection.GetDeckList());
				UpdateStoredCardsUI(_cardCollection.GetStoredList());
			}
		}

		/// <summary>
		/// Starts the card replacement process.
		/// </summary>
		private void BeginSwapCard(Card card)
		{
			if (_primedSwap)
			{
				EndSwapCard();
				return;
			}

			_primedSwap = true;

			if (_cardCollection.StoredContains(card.Id))
			{
				_deckPanelBackground.color = Color.yellow;
			}
			else
			{
				_storagePanelBackground.color = Color.yellow;
			}
		}

		/// <summary>
		/// Ends the card replacement process.
		/// </summary>
		private void EndSwapCard()
		{
			if (_cardCollection.StoredContains(_lastSelectedSlot.Card.Id))
			{
				_storagePanelBackground.color = Color.white;
			}
			else
			{
				_deckPanelBackground.color = Color.white;
			}

			_primedSwap = false;
		}

		/// <summary>
		/// Replaces a card from the list of unused cards with a card from the deck.
		/// Sends the request to the server.
		/// </summary>
		private async Task<CardCollection> SwapCards(string deckCardId, string storedCardId)
		{
			try
			{
				// Perform card swap on server
				Dictionary<string, string> swapPayload = new Dictionary<string, string>
				{
					{ "cardOutId", deckCardId },
					{ "cardInId", storedCardId }
				};

				var response = await _connection.Client.RpcAsync(_connection.Session, "swap_deck_card", JsonWriter.ToJson(swapPayload));
				return response.Payload.FromJson<CardCollection>();
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Couldn't swap user cards: " + e.Message);
				return _cardCollection;
			}
		}

		/// <summary>
		/// Generates a new deck based on <see cref="_defaultDeck"/>.
		/// Sends the deck to Nakama server and then stores it locally.
		/// </summary>
		private async void HandleClearCards()
		{
			try
			{
				IApiRpc response = await _connection.Client.RpcAsync(_connection.Session, "reset_card_collection");
				_cardCollection = response.Payload.FromJson<CardCollection>();
				_cardInfoSidePanel.SetCardCollection(_cardCollection);
				_cardInfoSidePanel.EnableUpgradeButton(HasGemsForUpgrade());
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Error clearing cards: " + e.Message);
				return;
			}

			_lastSelectedSlot = null;
			UpdateDeckCardsUI(_cardCollection.GetDeckList());
			UpdateStoredCardsUI(_cardCollection.GetStoredList());
		}

		/// <summary>
		/// Adds free gems to user's wallet.
		/// </summary>
		/// <remarks>
		/// This method is created for demo purpose only.
		/// </remarks>
		private async void HandleAddFreeGems()
		{
			try
			{
				IApiRpc newGems = await _connection.Client.RpcAsync(_connection.Session, "add_user_gems");
				_connection.Account = await _connection.Client.GetAccountAsync(_connection.Session);
			}
			catch (ApiResponseException e)
			{
				Debug.LogError("Error adding user gems: " + e.Message);
			}

			UpdateGemsUI();
		}

		/// <summary>
		/// Updates gold counter.
		/// </summary>
		public override async void Show(bool isMuteButtonClick = false)
		{
			try
			{
				var response = await _connection.Client.RpcAsync(_connection.Session, "load_user_cards", "");
				_cardCollection = response.Payload.FromJson<CardCollection>();
				_cardInfoSidePanel.SetCardCollection(_cardCollection);
				_cardInfoSidePanel.EnableUpgradeButton(HasGemsForUpgrade());
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Could not load user cards: " + e.Message);
				return;
			}

			UpdateDeckCardsUI(_cardCollection.GetDeckList());
			UpdateStoredCardsUI(_cardCollection.GetStoredList());
			UpdateGemsUI();
			base.Show(isMuteButtonClick);
		}

        private int GetGems(string wallet)
        {
			Dictionary<string, int> currency = wallet.FromJson<Dictionary<string, int>>();

			if (currency.ContainsKey("gems"))
			{
				return currency["gems"];
			}

			return 0;
        }

        /// <summary>
        /// Retrieves gems count owned by the user and sets the value in UI.
        /// </summary>
        private void UpdateGemsUI()
		{
			_fundsText.text = GetGems(_connection.Account.Wallet).ToString();
			_buyRandomCardButton.interactable = HasGemsForUpgrade();
		}

		/// <summary>
		/// Add a random card to the owned card list.
		/// </summary>
		private async void HandleBuyRandomCard()
		{
			try
			{
				IApiRpc response = await _connection.Client.RpcAsync(_connection.Session, "add_random_card");
				var card = response.Payload.FromJson<Dictionary<string, CardData>>();
				string cardId = card.Keys.First();
				_cardCollection.AddStored(new Card(cardId, card[cardId]));
				_connection.Account = await _connection.Client.GetAccountAsync(_connection.Session);
				_lastSelectedSlot = null;
				UpdateGemsUI();
				UpdateDeckCardsUI(_cardCollection.GetDeckList());
				UpdateStoredCardsUI(_cardCollection.GetStoredList());
				_cardInfoSidePanel.EnableUpgradeButton(HasGemsForUpgrade());
			}
			catch (Exception e)
			{
				Debug.LogWarning("Couldn't handle buy random card: " + e.Message);
				return;
			}
		}

		private bool HasGemsForUpgrade()
		{
			return GetGems(_connection.Account.Wallet) >= 50;
		}
	}
}
