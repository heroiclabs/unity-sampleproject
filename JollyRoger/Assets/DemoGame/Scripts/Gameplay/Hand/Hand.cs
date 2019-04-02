/**
 * Copyright 2019 The Knights Of Unity, created by Pawel Stolarczyk
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
using System.Threading.Tasks;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Gameplay.Decks;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Hands
{

    /// <summary>
    /// Responsible for hand management, handles card play.
    /// Used only by host.
    /// </summary>
    public class Hand : MonoBehaviour
    {
        #region Fields
        /// <summary>
        /// Maximum hand size for every player.
        /// </summary>
        [SerializeField] private int _handSize = 3;

        /// <summary>
        /// Reference to deck storage system.
        /// </summary>
        [SerializeField] private DeckStorage _deckStorage = null;


        /// <summary>
        /// Deck handled by this object.
        /// </summary>
        private Deck _deck;

        /// <summary>
        /// Queue of cards left in deck.
        /// </summary>
        private Queue<Card> _cards;

        /// <summary>
        /// Queue of already played cards.
        /// </summary>
        private Queue<Card> _playedCards;

        /// <summary>
        /// List of all cards currenly in hand.
        /// </summary>
        private List<Card> _cardsInHand;

        /// <summary>
        /// The user id of hand owner.
        /// </summary>
        private string _ownerId;

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves users deck from Nakama server, shuffles all cards and fills the
        /// hand with a number of cards equal to <see cref="_handSize"/>.
        /// Returns a list of all cards in hand.
        /// </summary>
        public async Task<List<Card>> InitAsync(string userId)
        {
            _ownerId = userId;
            _deck = await _deckStorage.LoadDataAsync(userId, "deck");

            _cards = new Queue<Card>();
            _playedCards = new Queue<Card>(_deck.usedCards);
            _cardsInHand = new List<Card>();

            for (int i = 0; i < _handSize; i++)
            {
                DrawCard();
            }
            return _cardsInHand;
        }

        /// <summary>
        /// Takes the first card from user's deck and places it in hand.
        /// If there are no cards left in deck, shuffles all already played
        /// cards and puts them in deck.
        /// </summary>
        public Card DrawCard()
        {
            if (_cards.Count == 0)
            {
                Debug.Log("refilling");
                RefillDeck();
            }
            Card card = _cards.Dequeue();
            _cardsInHand.Add(card);
            return card;
        }

        /// <summary>
        /// Removes played card from hand and adds it into <see cref="_playedCards"/>.
        /// </summary>
        /// <param name="index"></param>
        public void CardPlayed(int index)
        {
            Card card = _cardsInHand[index];
            _cardsInHand.RemoveAt(index);
            _playedCards.Enqueue(card);
        }

        /// <summary>
        /// Shuffles all already played cards into user's deck.
        /// </summary>
        private void RefillDeck()
        {
            List<Card> cardPool = new List<Card>(_playedCards);
            _playedCards.Clear();
            while (cardPool.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, cardPool.Count);
                _cards.Enqueue(cardPool[index]);
                cardPool.RemoveAt(index);
            }
        }

        /// <summary>
        /// Returns true if user has a copy of given card in hand.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool HasCardInHand(Card card)
        {
            foreach (Card cardInHand in _cardsInHand)
            {
                if (card.IsCopy(cardInHand))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }

}