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

using System.Runtime.Serialization;
using UnityEngine;

namespace PiratePanic
{
	/// <summary>
	/// Contains all the information of a single instance of playable card.
	/// </summary>
	[DataContract]
	public class CardData
	{
		/// <summary>
		/// Determines the underlying <see cref="GetCardInfo"/> scriptable object.
		/// </summary>
		[DataMember] public CardType type;

		/// <summary>
		/// The level of this card instance.
		/// Cards gain additional benefits with every level.
		/// </summary>
		[DataMember] public int level;

		/// <summary>
		/// Contains all stats and in-game object prefab of this card.
		/// </summary>
		private CardInfo _cardInfo;

		/// <summary>
		/// Returns <see cref="CardInfo"/> containing all stats and in-game object prefab
		/// reference of this card.
		/// </summary>
		public CardInfo GetCardInfo()
		{
			if (_cardInfo == null)
			{
				SetCardInfo(CardListManager.Instance.AllCards);
				if (_cardInfo == null)
				{
					SetCardInfo(CardListManager.Instance.StartingTowers);
					if (_cardInfo == null)
					{
						Debug.LogError("No card with type " + type + " found in Card List");
					}
				}
			}
			return _cardInfo;
		}

		/// <summary>
		/// Sets the <see cref="GetCardInfo"/> property based on
		/// this instance's <see cref="type"/>.
		/// </summary>
		/// <param name="cardList">
		/// Scriptable object containing a list of all awailable cards.
		/// </param>
		private void SetCardInfo(CardList cardList)
		{
			foreach (CardInfo cardInfo in cardList.CardInfos)
			{
				if (cardInfo.CardType == type)
				{
					this._cardInfo = cardInfo;
					return;
				}
			}
		}
	}
}
