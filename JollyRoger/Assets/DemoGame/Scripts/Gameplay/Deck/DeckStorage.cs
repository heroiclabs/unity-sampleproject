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

using System.Collections.Generic;
using System.Threading.Tasks;
using DemoGame.Scripts.DataStorage;
using DemoGame.Scripts.Gameplay.Cards;

namespace DemoGame.Scripts.Gameplay.Decks
{
    /// <summary>
    /// Used to send and retrieve informations about user's deck.
    /// </summary>
    public class DeckStorage : DataStorage<Deck>
    {
        #region Properties

        /// <summary>
        /// Determines who can change the deck on the server. 
        /// </summary>
        public override StorageWritePermission WritePermission => StorageWritePermission.NoWrite;

        /// <summary>
        /// Determines who can read the deck. 
        /// </summary>
        public override StorageReadPermission ReadPermission => StorageReadPermission.PublicRead;

        /// <summary>
        /// The colection this data storage saves the deck info.
        /// </summary>
        public override string StorageCollection => "deck";

        #endregion

        #region Methods

        /// <summary>
        /// Returns the key under which user's deck can be found on the server.
        /// </summary>
        public override string StorageKey(Deck deck) => "deck";

        /// <summary>
        /// Retrieves deck data from the server.
        /// </summary>
        public async override Task<Deck> LoadDataAsync(string userId, string key)
        {
            // Get deck's name
            string deckNameJson = await base.LoadDataJsonAsync(userId, "name");
            if (string.IsNullOrEmpty(deckNameJson) == true)
            {
                return null;
            }

            // Get cards used in deck
            string usedCardsJson = await base.LoadDataJsonAsync(userId, "used_cards");
            // Get owned cards not used in deck
            string unusedCardsJson = await base.LoadDataJsonAsync(userId, "unused_cards");


            // Parse received data
            string deckName = Nakama.TinyJson.JsonParser.FromJson<string>(deckNameJson);
            List<Card> usedCards = DeserializeCards(usedCardsJson);
            List<Card> unusedCards = DeserializeCards(unusedCardsJson);

            // Create new deck instance
            Deck deck = new Deck();
            deck.deckName = deckName;
            deck.usedCards = usedCards;
            deck.unusedCards = unusedCards;

            // Initialize cards
            foreach (Card card in deck.unusedCards)
            {
                card.isUsed = false;
            }
            foreach (Card card in deck.usedCards)
            {
                card.isUsed = true;
            }

            return deck;
        }

        /// <summary>
        /// Saves supplied deck data on the server.
        /// </summary>
        public override async Task<bool> StoreDataAsync(Deck data)
        {
            string key = StorageKey(data);
            Dictionary<string, Dictionary<string, int>> usedCards = SerializeCards(data.usedCards);
            Dictionary<string, Dictionary<string, int>> unusedCards = SerializeCards(data.unusedCards);
            Dictionary<string, string> name = new Dictionary<string, string> { { "name", data.deckName } };

            bool good;
            good = await base.StoreDataAsync("used_cards", Nakama.TinyJson.JsonWriter.ToJson(usedCards));
            if (good == false)
            {
                return false;
            }
            good = await base.StoreDataAsync("unused_cards", Nakama.TinyJson.JsonWriter.ToJson(unusedCards));
            if (good == false)
            {
                return false;
            }
            good = await base.StoreDataAsync("name", Nakama.TinyJson.JsonWriter.ToJson(name));
            if (good == false)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Serialize a list of cards into a dictionary which can be easly parsed one Json string.
        /// By grouping cards by their type and level we can decrease the size of send package.
        /// </summary>
        private Dictionary<string, Dictionary<string, int>> SerializeCards(List<Card> cardList)
        {
            Dictionary<string, Dictionary<string, int>> cards = new Dictionary<string, Dictionary<string, int>>();

            foreach (Card card in cardList)
            {
                Dictionary<string, int> dictionary;
                if (cards.TryGetValue(((int)card.cardType).ToString(), out dictionary) == true)
                {
                    if (dictionary.ContainsKey(card.level.ToString()) == false)
                    {
                        dictionary.Add(card.level.ToString(), 1);
                    }
                    else
                    {
                        dictionary[card.level.ToString()] += 1;
                    }
                }
                else
                {
                    dictionary = new Dictionary<string, int>();
                    dictionary.Add(card.level.ToString(), 1);
                    cards.Add(((int)card.cardType).ToString(), dictionary);
                }
            }

            return cards;
        }

        /// <summary>
        /// Deserializes Json string into a list of cards.
        /// </summary>
        private List<Card> DeserializeCards(string json)
        {
            Dictionary<string, Dictionary<string, int>> dictionary;
            dictionary = Nakama.TinyJson.JsonParser.FromJson<Dictionary<string, Dictionary<string, int>>>(json);
            List<Card> cards = new List<Card>();

            if (dictionary == null)
            {
                return cards;
            }
            foreach (KeyValuePair<string, Dictionary<string, int>> pair in dictionary)
            {
                foreach (KeyValuePair<string, int> innerPair in pair.Value)
                {
                    for (int i = 0; i < innerPair.Value; i++)
                    {
                        Card card = new Card();
                        card.cardType = (CardType)System.Enum.Parse(typeof(CardType), pair.Key);
                        card.level = int.Parse(innerPair.Key);
                        cards.Add(card);
                    }
                }
            }

            return cards;
        }

        #endregion
    }
}
