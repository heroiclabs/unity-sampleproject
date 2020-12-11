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
using Nakama.TinyJson;
using UnityEngine;

namespace PiratePanic
{

	/// <summary>
	/// Responsible for hand management, handles card play.
	/// Used only by host.
	/// </summary>
	public class Hand : MonoBehaviour
	{
				/// <summary>
		/// Maximum hand size for every player.
		/// </summary>
		[SerializeField] private int _handSize = 3;

		/// <summary>
		/// Deck handled by this object.
		/// </summary>
		private CardCollection _cardCollection;

		/// <summary>
		/// The user id of hand owner.
		/// </summary>
		private string _ownerId;

		private GameConnection _connection;

		public async void Init(GameConnection connection)
		{
			_connection = connection;

			try
			{
				var response =  await connection.Client.RpcAsync(connection.Session, "load_user_cards", "");
				_cardCollection = response.Payload.FromJson<CardCollection>();;
			}
			catch (Exception e)
			{
				Debug.Log("error loading user cards " + e.Message);
			}
		}

		/// <summary>
		/// Retrieves users deck from Nakama server, shuffles all cards and fills the
		/// hand with a number of cards equal to <see cref="_handSize"/>.
		/// Returns a list of all cards in hand.
		/// </summary>
		public List<Card> DrawInitialCards()
		{
			_ownerId = _connection.Session.UserId;

			var cards = new List<Card>();

			for (int i = 0; i < _handSize; i++)
			{
				cards.Add(DrawCard());
			}

			return cards;
		}

		/// <summary>
		/// Takes the first card from user's deck and places it in hand.
		/// If there are no cards left in deck, shuffles all already played
		/// cards and puts them in deck.
		/// </summary>
		public Card DrawCard()
		{
			int index = UnityEngine.Random.Range(0, _cardCollection.GetDeckList().Count);
			string randId = _cardCollection.GetDeckList()[index];
			return _cardCollection.GetDeckCard(randId);
		}
	}
}