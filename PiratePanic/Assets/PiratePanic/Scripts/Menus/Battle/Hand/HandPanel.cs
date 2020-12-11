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
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
    /// <summary>
    /// Panel displaying all cards currently held by the local user.
    /// </summary>
    public class HandPanel : MonoBehaviour
	{

		/// <summary>
		/// Parent transform all dragged cards are attached to during drag.
		/// This will ensure they will be always on top of other cards.
		/// </summary>
		[SerializeField] private Transform _dragParent = null;

		/// <summary>
		/// Point from which cards are being drawn.
		/// </summary>
		[SerializeField] private Transform _drawPosition = null;

		/// <summary>
		/// Reference to canvas scaler, used to calculate the real mouse position on canvas.
		/// </summary>
		[SerializeField] private CanvasScaler _canvasScaler = null;

		/// <summary>
		/// Card prefab displayed in user's hand.
		/// </summary>
		[SerializeField] private CardGrabber _inGameCardPrefab = null;

		/// <summary>
		/// List of available card slots in hand.
		/// </summary>
		[SerializeField] private List<Transform> _cardSlots = null;


		[SerializeField] private RectTransform _handRegion = null;
		[SerializeField] private RectTransform _allyHalf = null;
		[SerializeField] private RectTransform _allySpawn = null;
		[SerializeField] private RectTransform _enemyHalf = null;
		[SerializeField] private RectTransform _enemySpawn = null;


		/// <summary>
		/// List of cards being currently in user's hand.
		/// </summary>
		private List<CardGrabber> _cardsInHand = new List<CardGrabber>();

		/// <summary>
		/// Invoked whenever user plays a card.
		/// </summary>
		public event Action<MatchMessageCardPlayRequest> OnCardPlayed;

		/// <summary>
		/// Invoked whenever user clicks on a card in hand.
		/// </summary>
		public event Action<CardGrabber> OnCardGrabbed;

		/// <summary>
		/// Invoked whenever user returns grabbed card to their hand.
		/// </summary>
		public event Action<CardGrabber> OnCardReturned;

		/// <summary>
		/// Reference to card being currently grabbed.
		/// </summary>
		public CardGrabber CurrentlyGrabbedCard { get; private set; }

		private GameStateManager _stateManager;
		private GameConnection _connection;

		/// <summary>
		/// Subscribes to <see cref="GameStateManager.OnCardCancelled"/> event.
		/// </summary>
		public void Init(GameConnection connection, GameStateManager stateManager)
		{
			this._stateManager = stateManager;
			this._connection = connection;
			_stateManager.OnCardCancelled += CancelPlay;
		}

		/// <summary>
		/// Creates <see cref="CardGrabber"/> instance and initializes it.
		/// </summary>
		public CardGrabber DrawCard(Card card, int slotId)
		{
			Transform slot = _cardSlots[slotId];
			RectTransform region = SelectRegion(card);
			CardGrabber cardGrabber = Instantiate(_inGameCardPrefab, slot, false);
			cardGrabber.Init(_stateManager, _connection.BattleConnection.HostId == _connection.Session.UserId);

			cardGrabber.transform.position = _drawPosition.position;
			cardGrabber.Initialize(card, slot, _handRegion, region, _canvasScaler);
			cardGrabber.OnCardPlayed += PlayCard;
			cardGrabber.OnDragStarted += StartCardDrag;
			cardGrabber.OnCardReturned += ReturnCard;
			_cardsInHand.Insert(slotId, cardGrabber);
			return cardGrabber;
		}

		/// <summary>
		/// Returns a region based on card info.
		/// </summary>
		private RectTransform SelectRegion(Card card)
		{
			DropRegion dropRegion = card.CardData.GetCardInfo().DropRegion;
			switch (dropRegion)
			{
				case DropRegion.WholeMap:
					return null;
				case DropRegion.EnemyHalf:
					return _enemyHalf;
				case DropRegion.EnemySpawn:
					return _enemySpawn;
				case DropRegion.AllyHalf:
					return _allyHalf;
				case DropRegion.AllySpawn:
					return _allySpawn;
				default:
					return null;
			}
		}

		/// <summary>
		/// Invoked on card grab.
		/// </summary>
		private void StartCardDrag(CardGrabber grabber)
		{
			grabber.transform.SetParent(_dragParent);
			CurrentlyGrabbedCard = grabber;
			OnCardGrabbed?.Invoke(grabber);
		}

		/// <summary>
		/// Invoked on card returning to hand.
		/// </summary>
		private void ReturnCard(CardGrabber grabber)
		{
			grabber.transform.SetParent(grabber.Slot);
			OnCardReturned?.Invoke(grabber);
			CurrentlyGrabbedCard = null;
		}

		/// <summary>
		/// Invoked on card played.
		/// Sends <see cref="MatchMessageCardPlayRequest"/> to host.
		/// </summary>
		private void PlayCard(CardGrabber grabber, Vector3 dropPosition)
		{
			grabber.OnDragStarted -= StartCardDrag;
			grabber.OnCardReturned -= ReturnCard;
			grabber.OnCardPlayed -= PlayCard;

			string id = _connection.Session.UserId;
			int index = _cardsInHand.IndexOf(grabber);
			//_cardsInHand.RemoveAt(index);
			MatchMessageCardPlayRequest message = new MatchMessageCardPlayRequest(
				id, grabber.Card, index, dropPosition.x, dropPosition.y, dropPosition.z);

			OnCardPlayed?.Invoke(message);
			CurrentlyGrabbedCard = null;
			//Destroy(grabber.gameObject);
		}

		/// <summary>
		/// Prevents given card from being played.
		/// Returns it to user's hand.
		/// </summary>
		private void CancelPlay(MatchMessageCardCanceled message)
		{
			CardGrabber grabber = _cardsInHand[message.CardSlotIndex];
			grabber.OnDragStarted += StartCardDrag;
			grabber.OnCardReturned += ReturnCard;
			grabber.OnCardPlayed += PlayCard;
			grabber.CancelPlay();
		}

		/// <summary>
		/// Removes played card grabber from the game.
		/// </summary>
		public void ResolveCardPlay(MatchMessageCardPlayed message)
		{
			CardGrabber grabber = _cardsInHand[message.CardSlotIndex];
			grabber.Resolve(message);
			_cardsInHand.RemoveAt(message.CardSlotIndex);
			Destroy(grabber.gameObject);
		}
	}
}
