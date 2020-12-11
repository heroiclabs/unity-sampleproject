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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PiratePanic
{
	/// <summary>
	/// Contains a list of all used and unused cards owned by a user.
	/// </summary>
	[DataContract]
	public class CardCollection
	{
		/// <summary>
		/// List of all cards owned by the deck owner that are currently in the deck.
		/// </summary>
		[DataMember(Name="deckCards")]
	    public readonly Dictionary<string, CardData> deckCards = new Dictionary<string, CardData>();

		/// <summary>
		/// List of all cards owned by the deck owner that are currently not in the deck.
		/// </summary>
		[DataMember(Name="storedCards")]
		public readonly Dictionary<string, CardData> storedCards = new Dictionary<string, CardData>();

		private List<string> _sortedDeckCards;
		private List<string> _sortedStoredCards;

		public bool DeckContains(string cardId)
		{
			return deckCards.ContainsKey(cardId);
		}

		public bool StoredContains(string cardId)
		{
			return storedCards.ContainsKey(cardId);
		}

		public void AddStored(Card card)
		{
			storedCards.Add(card.Id, card.CardData);
			GetStoredList().Add(card.Id);
		}

		public Card GetDeckCard(string id)
		{
			return new Card(id, deckCards[id]);
		}

		public Card GetStoredCard(string id)
		{
			return new Card(id, storedCards[id]);
		}

		public List<string> GetDeckList()
		{
			return _sortedDeckCards ?? (_sortedDeckCards = CreateSortedList(deckCards));
		}

		public List<string> GetStoredList()
		{
			return _sortedStoredCards ?? (_sortedStoredCards = CreateSortedList(storedCards));
		}

		public void UpgradeCard(string id, CardData upgradedCardData)
		{
			if (deckCards.ContainsKey(id))
			{
				deckCards[id] = upgradedCardData;
			}
			else
			{
				storedCards[id] = upgradedCardData;
			}
		}

		private List<string> CreateSortedList(Dictionary<string, CardData> cardsDict)
		{
			return cardsDict
			.OrderBy(keyVal => keyVal.Value.type)
			.Select(keyVal => keyVal.Key)
			.ToList();
		}
	}
}
