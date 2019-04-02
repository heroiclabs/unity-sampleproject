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
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Session;
using Nakama;

namespace DemoGame.Scripts.Gameplay.Decks
{
    /// <summary>
    /// Manages communication with Nakama server regarding deck management.
    /// </summary>
    public static class DeckBuildingManager
    {
        /// <summary>
        /// Checks on the server whether user is allowed to merge the two supplied cards
        /// and if they have enough gold, then performs the upgrade.
        /// </summary>
        public static async Task<CardOperationResponse> MergeAsync(Card card1, Card card2)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            Dictionary<string, Dictionary<string, string>> cardPayload = new Dictionary<string, Dictionary<string, string>>
        {
            { "first", card1.Serialize() },
            { "second", card2.Serialize() }
        };
            string payload = Nakama.TinyJson.JsonWriter.ToJson(cardPayload);

            IApiRpc responsePayload = await client.RpcAsync(session, "merge_cards", payload);
            CardOperationResponse response = Nakama.TinyJson.JsonParser.FromJson<CardOperationResponse>(responsePayload.Payload);

            return response;
        }

        /// <summary>
        /// Checks on the server whether user is allowed to replace the two supplied cards,
        /// then performs the swap.
        /// </summary>
        public static async Task<CardOperationResponse> SwapAsync(Card card1, Card card2)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            Dictionary<string, Dictionary<string, string>> cardPayload = new Dictionary<string, Dictionary<string, string>>
        {
            { "first", card1.Serialize() },
            { "second", card2.Serialize() }
        };
            string payload = Nakama.TinyJson.JsonWriter.ToJson(cardPayload);

            IApiRpc responsePayload = await client.RpcAsync(session, "swap_cards", payload);
            CardOperationResponse response = Nakama.TinyJson.JsonParser.FromJson<CardOperationResponse>(responsePayload.Payload);

            return response;
        }

        #region Debug

        /// <summary>
        /// Debug method which forces server to add a random card to user's deck.
        /// </summary>
        public static async Task<CardOperationResponse> DebugAddRandomCardAsync()
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            IApiRpc responsePayload = await client.RpcAsync(session, "debug_add_random_card");
            CardOperationResponse response = Nakama.TinyJson.JsonParser.FromJson<CardOperationResponse>(responsePayload.Payload);

            return response;
        }

        /// <summary>
        /// Debug method which forces server to remove cards owned by the user.
        /// </summary>
        public static async Task<CardOperationResponse> DebugClearDeckAsync()
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            IApiRpc responsePayload = await client.RpcAsync(session, "debug_clear_deck");
            CardOperationResponse response = Nakama.TinyJson.JsonParser.FromJson<CardOperationResponse>(responsePayload.Payload);

            return response;
        }

        /// <summary>
        /// Debug method which forces server to add 100 gems to their wallet.
        /// </summary>
        public static async Task<CardOperationResponse> DebugAddGemsAsync()
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            IApiRpc responsePayload = await client.RpcAsync(session, "debug_add_gems");
            CardOperationResponse response = Nakama.TinyJson.JsonParser.FromJson<CardOperationResponse>(responsePayload.Payload);

            return response;
        }

        #endregion
    }
}
