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
using Nakama;
using Nakama.TinyJson;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
	/// <summary>
	/// Side panel in <see cref="CardsMenuUI"/> displaying informations of
	/// supplied <see cref="CardData"/>.
	/// </summary>
	public class CardInfoSidePanel : MonoBehaviour
	{
		public event Action<Card, CardData> OnAfterCardUpgraded = delegate { };

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
		private CardCollection _cardCollection;

		private GameConnection _connection;

		public void Init(GameConnection connection)
		{
			_connection = connection;
			_upgradeButton.onClick.AddListener(HandleCardUpgradeClicked);
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Sets the deck reference.
		/// </summary>
		public void SetCardCollection(CardCollection cardCollection)
		{
			this._cardCollection = cardCollection;
		}

		public void EnableUpgradeButton(bool hasFunds)
		{
			_upgradeButton.interactable = hasFunds;
		}

		/// <summary>
		/// Sets the <see cref="_displayedCard"/> reference.
		/// Supplied <paramref name="card"/> must be one of cards not used in the deck.
		/// </summary>
		public void SetActiveCard(Card card, Action<Card> beginReplaceCard)
		{
			gameObject.SetActive(card != null);

			SetCardInfo(card);

			// Reset the listener of the _useButton
			_useButton.onClick.RemoveAllListeners();
			_useButton.onClick.AddListener(() => beginReplaceCard(card));
		}

		/// <summary>
		/// Sets the UI to display celected <paramref name="card"/> info.
		/// </summary>
		private void SetCardInfo(Card card)
		{
			_displayedCard = card;
			_cardLevel.text = $"lvl {card.CardData.level.ToString()}";
			_cardDescription.text = card.CardData.GetCardInfo().Description;
			_cardCost.text = card.CardData.GetCardInfo().Cost.ToString();
			_cardName.text = card.CardData.GetCardInfo().Name;
			_cardImage.sprite = card.CardData.GetCardInfo().Sprite;
		}

		private async void HandleCardUpgradeClicked()
		{
			try
			{
				string requestPayload = new Dictionary<string, string>() { { "id", _displayedCard.Id } }.ToJson();
				var upgradeResponse = await _connection.Client.RpcAsync(_connection.Session, "upgrade_card", requestPayload);
				var upgradedCardData = upgradeResponse.Payload.FromJson<CardData>();
				OnAfterCardUpgraded(_displayedCard, upgradedCardData);
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Error upgrading card: " + e.Message);
			}
		}
	}
}
