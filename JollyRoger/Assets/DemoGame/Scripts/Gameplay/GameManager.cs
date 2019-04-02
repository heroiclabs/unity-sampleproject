/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch, Pawel Stolarczyk
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
using System.Threading.Tasks;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Gameplay.Hands;
using DemoGame.Scripts.Gameplay.NetworkCommunication;
using DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates;
using DemoGame.Scripts.Gameplay.Nodes;
using DemoGame.Scripts.Gameplay.UI;
using DemoGame.Scripts.Gameplay.Units;
using DemoGame.Scripts.Session;
using DemoGame.Scripts.Utils;
using Nakama;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay
{

    /// <summary>
    /// Core gameplay manager. Handles card playing, game ending and initialization.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {

        #region Fields

        /// <summary>
        /// Reference to camera parent object.
        /// This transform will be rotated by 180 degrees if local user is not host.
        /// </summary>
        [SerializeField] private Transform _cameraHolder = null;

        /// <summary>
        /// Reference to summary menu shown at the end of the match.
        /// </summary>
        [SerializeField] private SummaryMenu _summary = null;

        /// <summary>
        /// In-game time of beginning of the match.
        /// </summary>
        private float _timerStart;

        #region Hands

        [Header("Hands")]
        /// <summary>
        /// Reference to the hand side bar with cards.
        /// </summary>
        [SerializeField] private HandPanel _localHandPanel = null;

        /// <summary>
        /// Local user hand manager.
        /// Used only by host.
        /// </summary>
        [SerializeField] private Hand _localHand = null;

        /// <summary>
        /// Opponent hand manager.
        /// Used only by host.
        /// </summary>
        [SerializeField] private Hand _opponentHand = null;

        #endregion

        #region Gold

        [Header("Gold")]
        /// <summary>
        /// Reference to the bottom panel displaying current gold.
        /// </summary>
        [SerializeField] private GoldPanel _localGoldPanel = null;

        /// <summary>
        /// Local user gold manager.
        /// Used only by host.
        /// </summary>
        [SerializeField] private Gold _localGold = null;

        /// <summary>
        /// Opponent golr manager
        /// Used only by host.
        /// </summary>
        [SerializeField] private Gold _opponentGold = null;

        #endregion

        #region Structures

        /// <summary>
        /// List of towers owned by the local user.
        /// </summary>
        private List<Unit> _allyTowers;

        /// <summary>
        /// List of towers owned by the opponent.
        /// </summary>
        private List<Unit> _enemyTowers;

        /// <summary>
        /// Local user castle reference.
        /// </summary>
        private Unit _allyCastle;

        /// <summary>
        /// Opponen castle reference.
        /// </summary>
        private Unit _enemyCastle;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// 2d array of nodes units can move to during a match.
        /// Initialized by <see cref="NodeMapGenerator"/>.
        /// </summary>
        public Node[,] Nodes { get; private set; }

        /// <summary>
        /// Map size in nodes.
        /// </summary>
        public Vector2Int MapSize { get; private set; }

        #endregion

        #region Monobehaviour

        /// <summary>
        /// Subscribes to message events and initializes the game.
        /// </summary>
        private void Start()
        {
            MatchCommunicationManager.Instance.OnCardRequested += OnCardRequested;
            MatchCommunicationManager.Instance.OnCardPlayed += OnCardPlayed;
            MatchCommunicationManager.Instance.OnStartingHandReceived += OnStartingHandReceived;
            MatchCommunicationManager.Instance.OnGameEnded += OnGameEnded;
            _localHandPanel.OnCardPlayed += OnCardRequested;

            if (MatchCommunicationManager.Instance.GameStarted == true)
            {
                OnMatchJoined();
            }
            else
            {
                MatchCommunicationManager.Instance.OnGameStarted += OnMatchJoined;
            }
        }

        /// <summary>
        /// Unsubscribes from all message events.
        /// </summary>
        protected override void OnDestroy()
        {
            MatchCommunicationManager.Instance.OnCardRequested -= OnCardRequested;
            MatchCommunicationManager.Instance.OnCardPlayed -= OnCardPlayed;
            MatchCommunicationManager.Instance.OnStartingHandReceived -= OnStartingHandReceived;
            MatchCommunicationManager.Instance.OnGameStarted -= OnMatchJoined;
            MatchCommunicationManager.Instance.OnGameEnded -= OnGameEnded;
            _localHandPanel.OnCardPlayed -= OnCardRequested;
        }

        /// <summary>
        /// Handles exitting the game.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) == true)
            {
                MatchCommunicationManager.Instance.LeaveGame();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes nodes in the map.
        /// Invoked by <see cref="NodeMapGenerator"/>.
        /// </summary>
        public void InitMap(Node[,] nodes, Vector2Int mapSize)
        {
            Nodes = nodes;
            MapSize = mapSize;
        }

        /// <summary>
        /// Invoked on successful match join.
        /// If local user is host, initializes structures and sends starting hands to all players.
        /// Otherwise, rotates camera by 180 degrees.
        /// </summary>
        private async void OnMatchJoined()
        {
            MatchCommunicationManager.Instance.OnGameStarted -= OnMatchJoined;
            if (MatchCommunicationManager.Instance.IsHost == true)
            {
                foreach (IUserPresence presence in MatchCommunicationManager.Instance.Players)
                {
                    UnitsManager.Instance.BuildStartingStructures(presence.UserId);
                    await SendStartingHandAsync(presence);
                }
                _timerStart = Time.unscaledTime;
            }
            else
            {
                _cameraHolder.Rotate(Vector3.up, 180);
            }
        }

        /// <summary>
        /// Invoked upon receiving end game message.
        /// Shows summary.
        /// </summary>
        private async void OnGameEnded(MatchMessageGameEnded message)
        {
            bool localWin = message.winnerId == NakamaSessionManager.Instance.Session.UserId;
            int reward = await MatchCommunicationManager.Instance.GetMatchRewardAsync(message.matchId);
            _summary.SetResult(localWin, reward);
            _summary.Show();
        }

        #region Starting Hand

        /// <summary>
        /// Selects a number of cards equal to <see cref="Hand._cardsInHand"/> from players deck
        /// and sends them to that player.
        /// </summary>
        private async Task SendStartingHandAsync(IUserPresence presence)
        {
            if (presence.UserId == MatchCommunicationManager.Instance.HostId)
            {
                List<Card> cards = await _localHand.InitAsync(presence.UserId);
                MatchMessageStartingHand message = new MatchMessageStartingHand(presence.UserId, cards);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.StartingHand, message);
            }
            else
            {
                List<Card> cards = await _opponentHand.InitAsync(presence.UserId);
                MatchMessageStartingHand message = new MatchMessageStartingHand(presence.UserId, cards);
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.StartingHand, message);
                _opponentGold.Restart();
            }
        }

        /// <summary>
        /// Adds cards to local user's hand.
        /// </summary>
        private void OnStartingHandReceived(MatchMessageStartingHand message)
        {
            if (message.PlayerId == NakamaSessionManager.Instance.Session.UserId)
            {
                Debug.Log("Starting hands received");
                for (int i = 0; i < message.Cards.Count; i++)
                {
                    _localHandPanel.DrawCard(message.Cards[i], i);
                }
                _localGold.Restart();
                _localGoldPanel.Init(_localGold);
            }
        }

        #endregion

        #region Cards

        /// <summary>
        /// User requested card play.
        /// Host will handle the request or cancel it if it was illegal.
        /// </summary>
        private void OnCardRequested(MatchMessageCardPlayRequest message)
        {
            if (message.PlayerId == NakamaSessionManager.Instance.Session.UserId)
            {
                if (MatchCommunicationManager.Instance.IsHost == true)
                {
                    HandleCardRequest(message, _localHand, _localGold);
                }
                else
                {
                    MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.CardPlayRequest, message);
                }
            }
            else
            {
                if (MatchCommunicationManager.Instance.IsHost == true)
                {
                    HandleCardRequest(message, _opponentHand, _opponentGold);
                }
                else
                {
                    // Only host can handle incomming card play requests; do nothing
                }
            }
        }

        /// <summary>
        /// Handles card play request.
        /// If user playing the card has insufficiend gold or used card can not be played on specified node,
        /// a cancel message is sent to that card owner and its effects don't resolve.
        /// </summary>
        private void HandleCardRequest(MatchMessageCardPlayRequest message, Hand hand, Gold gold)
        {
            if (gold.CurrentGold < message.Card.GetCardInfo().Cost)
            {
                SendCardCanceledMessage(message);
            }
            else
            {
                bool isHost = MatchCommunicationManager.Instance.HostId == message.PlayerId;
                Vector3 position = new Vector3(message.X, message.Y, message.Z);
                Vector2Int nodePosition = ScreenToNodePos(position, isHost, message.Card.GetCardInfo().DropRegion);
                Node node = Nodes[nodePosition.x, nodePosition.y];

                if (node != null && (node.Unit == null || message.Card.GetCardInfo().CanBeDroppedOverOtherUnits == true))
                {
                    SendCardPlayedMessage(message, hand, nodePosition);
                }
                else
                {
                    SendCardCanceledMessage(message);
                }
            }
        }

        /// <summary>
        /// Requests card play.
        /// </summary>
        private void SendCardPlayedMessage(MatchMessageCardPlayRequest message, Hand userHand, Vector2Int nodePosition)
        {
            Card newCard = userHand.DrawCard();

            MatchMessageCardPlayed matchMessageCardPlayed = new MatchMessageCardPlayed(
                message.PlayerId,
                message.Card,
                message.CardSlotIndex,
                newCard,
                nodePosition.x,
                nodePosition.y);

            MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.CardPlayed, matchMessageCardPlayed);
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.CardPlayed, matchMessageCardPlayed);
            userHand.CardPlayed(message.CardSlotIndex);
        }

        /// <summary>
        /// Cancels played card and returns it to its owner's hand.
        /// </summary>
        private void SendCardCanceledMessage(MatchMessageCardPlayRequest message)
        {
            MatchMessageCardCanceled cardCanceled = new MatchMessageCardCanceled(message.PlayerId, message.CardSlotIndex);
            if (message.PlayerId == NakamaSessionManager.Instance.Session.UserId)
            {
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.CardCanceled, cardCanceled);
            }
            else
            {
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.CardCanceled, cardCanceled);
            }
        }

        /// <summary>
        /// Invoked whenever a card is played and it wan't canceled.
        /// Removes that card from its owner's hand and reduces their gold count by <see cref="CardInfo.Cost"/>.
        /// Card owner draws a new card from their deck.
        /// </summary>
        /// <param name="message"></param>
        private void OnCardPlayed(MatchMessageCardPlayed message)
        {
            Debug.Log("playing " + message.Card.cardType);
            if (message.PlayerId == NakamaSessionManager.Instance.Session.UserId)
            {
                _localHandPanel.ResolveCardPlay(message);
                _localHandPanel.DrawCard(message.NewCard, message.CardSlotIndex);
                _localGold.ChangeGoldCount(-message.Card.GetCardInfo().Cost);
            }
            else if (MatchCommunicationManager.Instance.IsHost == true)
            {
                _opponentGold.ChangeGoldCount(-message.Card.GetCardInfo().Cost);
            }
        }

        #endregion

        #region Node Positions

        /// <summary>
        /// Given the <see cref="DropRegion"/>, returns a node closest to given world position.
        /// </summary>
        public Vector2Int ScreenToNodePos(Vector3 position, bool isHost, DropRegion dropRegion)
        {
            switch (dropRegion)
            {
                case DropRegion.WholeMap:
                    return GetClosestNode_WholeMap(position);
                case DropRegion.EnemyHalf:
                    return GetClosestNode_HalfMap(position, isHost ? false : true);
                case DropRegion.EnemySpawn:
                    return GetClosestNode_Spawn(position, isHost ? false : true);
                case DropRegion.AllyHalf:
                    return GetClosestNode_HalfMap(position, isHost ? true : false);
                case DropRegion.AllySpawn:
                    return GetClosestNode_Spawn(position, isHost ? true : false);
                default:
                    break;
            }

            Debug.LogError("Drop region " + dropRegion + " was not handled");
            return new Vector2Int();
        }

        /// <summary>
        /// Returns a node from the first (or last) column of the map (spawn region).
        /// </summary>
        private Vector2Int GetClosestNode_Spawn(Vector3 position, bool leftSide)
        {
            float minDistance = float.MaxValue;
            Node closestNode = null;
            int x = leftSide == true ? 0 : Nodes.GetLength(0) - 1;


            for (int y = 0; y < Nodes.GetLength(1); y += 2)
            {
                Node node = Nodes[x, y];
                if (node == null) continue;
                float distance = Vector3.Distance(position, node.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestNode = node;
                }
            }

            return closestNode.Position;
        }

        /// <summary>
        /// Returns a node cosest to the <paramref name="position"/> from a half of the map,
        /// depending on the value of <paramref name="leftSide"/>.
        /// </summary>
        private Vector2Int GetClosestNode_HalfMap(Vector3 position, bool leftSide)
        {
            float minDistance = float.MaxValue;
            Node closestNode = null;

            for (int y = 0; y < Nodes.GetLength(1); y++)
            {
                int width = y % 2 == 0 ? Nodes.GetLength(0) : Nodes.GetLength(0) - 1;
                int half = width / 2;

                int beg = leftSide == true ? 0 : width - half;
                int end = leftSide == true ? half : width;

                for (int x = beg; x < end; x++)
                {
                    Node node = Nodes[x, y];
                    if (node == null) continue;
                    float distance = Vector3.Distance(position, node.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestNode = node;
                    }
                }
            }

            return closestNode.Position;
        }

        /// <summary>
        /// Returns a node closest to <paramref name="position"/>.
        /// </summary>
        private Vector2Int GetClosestNode_WholeMap(Vector3 position)
        {
            float minDistance = float.MaxValue;
            Node closestNode = null;


            for (int x = 0; x < Nodes.GetLength(0); ++x)
            {
                for (int y = 0; y < Nodes.GetLength(1); ++y)
                {
                    Node node = Nodes[x, y];
                    if (node == null) continue;
                    float distance = Vector3.Distance(position, node.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestNode = node;
                    }
                }
            }

            return closestNode.Position;
        }

        #endregion

        #region Structures

        /// <summary>
        /// Sets towers references.
        /// </summary>
        public void SetTowers(PlayerColor player, Unit castle, Unit tower1, Unit tower2)
        {
            if (player == PlayerColor.Black)
            {
                _allyTowers = new List<Unit> { tower1, tower2 };
                _allyCastle = castle;
                _allyCastle.OnDestroy += OnCastleDestroyed;
            }
            else
            {
                _enemyTowers = new List<Unit> { tower1, tower2 };
                _enemyCastle = castle;
                _enemyCastle.OnDestroy += OnCastleDestroyed;
            }
        }

        /// <summary>
        /// Invoked upon destruction of player's main castle.
        /// Sends game ending message.
        /// </summary>
        private void OnCastleDestroyed(Unit destroyedCastle)
        {
            if (destroyedCastle == _allyCastle)
            {
                Debug.Log("Enemy win");
                string winner = MatchCommunicationManager.Instance.OpponentId;
                string loser = NakamaSessionManager.Instance.Session.UserId;
                string matchId = MatchCommunicationManager.Instance.MatchId;
                float matchDuration = Time.unscaledTime - _timerStart;
                int winnerTowersDestroyed = 1 + _allyTowers.Count(x => x.IsDestroyed == true);
                int loserTowersDestroyed = 0 + _enemyTowers.Count(x => x.IsDestroyed == true);

                MatchMessageGameEnded message = new MatchMessageGameEnded(winner, loser, matchId, winnerTowersDestroyed, loserTowersDestroyed, matchDuration);
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.MatchEnded, message);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.MatchEnded, message);
            }
            else
            {
                Debug.Log("Ally win");
                string winner = NakamaSessionManager.Instance.Session.UserId;
                string loser = MatchCommunicationManager.Instance.OpponentId;
                string matchId = MatchCommunicationManager.Instance.MatchId;
                float matchDuration = Time.unscaledTime - _timerStart;
                int winnerTowersDestroyed = 1 + _enemyTowers.Count(x => x.IsDestroyed == true);
                int loserTowersDestroyed = 0 + _allyTowers.Count(x => x.IsDestroyed == true);

                MatchMessageGameEnded message = new MatchMessageGameEnded(winner, loser, matchId, winnerTowersDestroyed, loserTowersDestroyed, matchDuration);
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.MatchEnded, message);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.MatchEnded, message);
            }
        }

        #endregion

        #endregion

    }

}