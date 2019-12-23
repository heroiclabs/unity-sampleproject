/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch and Paweł Stolarczyk
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using DemoGame.Scripts.Utils;
using DemoGame.Scripts.Session;
using DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates;

namespace DemoGame.Scripts.Gameplay.NetworkCommunication
{

    /// <summary>
    /// Role of this manager is sending match information to other players and receiving match messages from them through Nakama Server.
    /// </summary>
    public class MatchCommunicationManager : Singleton<MatchCommunicationManager>
    {
        /// <summary>
        /// Number of players required to start match.
        /// </summary>
        [SerializeField] private int _playerCount = 2;

        //This region contains events for all type of match messages that could be send in the game.
        //Events are fired after getting message sent by other players from Nakama server
        #region PUBLIC EVENTS

        //GAME
        public event Action OnGameStarted;
        public event Action<MatchMessageGameEnded> OnGameEnded;

        //UNITS
        public event Action<MatchMessageUnitSpawned> OnUnitSpawned;
        public event Action<MatchMessageUnitMoved> OnUnitMoved;
        public event Action<MatchMessageUnitAttacked> OnUnitAttacked;

        //SPELLS
        public event Action<MatchMessageSpellActivated> OnSpellActivated;

        //CARDS
        public event Action<MatchMessageCardPlayRequest> OnCardRequested;
        public event Action<MatchMessageCardPlayed> OnCardPlayed;
        public event Action<MatchMessageCardCanceled> OnCardCancelled;
        public event Action<MatchMessageStartingHand> OnStartingHandReceived;

        #endregion

        #region PROPORTIES

        /// <summary>
        /// Id of current game host
        /// </summary>
        public string HostId { private set; get; }

        /// <summary>
        /// Returns true if local player is host
        /// </summary>
        public bool IsHost
        {
            get
            {
                return HostId == NakamaSessionManager.Instance.Session.UserId;
            }
        }

        /// <summary>
        /// Id of opponent of local player in current game
        /// </summary>
        public string OpponentId { get; private set; }

        /// <summary>
        /// List of IUserPresence of all players
        /// </summary>
        public List<IUserPresence> Players { get; private set; }

        /// <summary>
        /// Returns true if Players presences count is equal to required players number
        /// </summary>
        public bool AllPlayersJoined { get { return Players.Count == _playerCount; } }

        /// <summary>
        /// Returns true if game is already started
        /// </summary>
        public bool GameStarted { get; private set; }

        /// <summary>
        /// Id of current match
        /// </summary>
        public string MatchId
        {
            get;
            private set;
        }

        /// <summary>
        /// Current socket which connects client to Nakama server. Through this socket are sent match messages.
        /// </summary>
        private ISocket _socket { get { return NakamaSessionManager.Instance.Socket; } }

        #endregion

        #region PRIVATE FIELDS

        private bool _allPlayersAdded;

        /// <summary>
        /// Indicates if player already joined match
        /// </summary>
        private bool _matchJoined;

        /// <summary>
        /// Indicates if player is already leaving match
        /// </summary>
        private bool _isLeaving;

        /// <summary>
        /// Queue used for enquequing incoming match messages when match is locally not started yet to make sure that they will be runned
        /// in properly order after game start
        /// </summary>
        private Queue<IncommingMessageState> _incommingMessages = new Queue<IncommingMessageState>();

        #endregion

        #region MONO

        private void Start()
        {
            OnGameEnded += GameEnded;
        }

        protected override void OnDestroy()
        {
            _incommingMessages = new Queue<IncommingMessageState>();
            OnGameEnded -= GameEnded;
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Joins given match found by matchmaker
        /// </summary>
        /// <param name="matched"></param>
        public async void JoinMatchAsync(IMatchmakerMatched matched)
        {
            //Choosing host in deterministic way, with no need to exchange data between players
            ChooseHost(matched);

            //Filling list of match participants
            Players = new List<IUserPresence>();

            try
            {
                // Listen to incomming match messages and user connection changes
                _socket.ReceivedMatchPresence += OnMatchPresence;
                _socket.ReceivedMatchState += ReceiveMatchStateMessage;

                // Join the match
                IMatch match = await _socket.JoinMatchAsync(matched);
                // Set current match id
                // It will be used to leave the match later
                MatchId = match.Id;
                Debug.Log("Joined match with id: " + match.Id + "; presences count: " + match.Presences.Count());

                // Add all players already connected to the match
                // If both players uses the same account, exit the game
                bool noDuplicateUsers = AddConnectedPlayers(match);
                if (noDuplicateUsers == true)
                {
                    // Match joined successfully
                    // Setting gameplay
                    _matchJoined = true;
                    StartGame();
                }
                else
                {
                    LeaveGame();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't join match: " + e.Message);
            }
        }

        /// <summary>
        /// Starts procedure of leaving match by local player
        /// </summary>
        public void LeaveGame()
        {
            if (_isLeaving == true)
            {
                Debug.Log("Already leaving");
                return;
            }
            _isLeaving = true;
            _socket.ReceivedMatchPresence -= OnMatchPresence;
            _socket.ReceivedMatchState -= ReceiveMatchStateMessage;

            //Starts coroutine which is loading main menu and also disconnects player from match
            StartCoroutine(LoadMenuCoroutine());
        }

        /// <summary>
        /// This method sends match state message to other players through Nakama server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="opCode"></param>
        /// <param name="message"></param>
        public void SendMatchStateMessage<T>(MatchMessageType opCode, T message)
            where T : MatchMessage<T>
        {
            try
            {
                //Packing MatchMessage object to json
                string json = MatchMessage<T>.ToJson(message);

                //Sending match state json along with opCode needed for unpacking message to server.
                //Then server sends it to other players
                _socket.SendMatchStateAsync(MatchId, (long)opCode, json);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while sending match state: " + e.Message);
            }
        }


        /// <summary>
        /// This method is used by host to invoke locally event connected with match message which is sent to other players.
        /// Should be always runned on host client after sending any message, otherwise some of the game logic would not be runned on host game instance.
        /// Don't use this method when client is not a host!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="opCode"></param>
        /// <param name="message"></param>
        public void SendMatchStateMessageSelf<T>(MatchMessageType opCode, T message)
            where T : MatchMessage<T>
        {
            //Choosing which event should be invoked basing on opCode and firing event
            switch (opCode)
            {
                //GAME
                case MatchMessageType.MatchEnded:
                    OnGameEnded?.Invoke(message as MatchMessageGameEnded);
                    break;

                //UNITS
                case MatchMessageType.UnitSpawned:
                    OnUnitSpawned?.Invoke(message as MatchMessageUnitSpawned);
                    break;

                case MatchMessageType.UnitMoved:
                    OnUnitMoved?.Invoke(message as MatchMessageUnitMoved);
                    break;

                case MatchMessageType.UnitAttacked:
                    OnUnitAttacked?.Invoke(message as MatchMessageUnitAttacked);
                    break;

                //SPELLS
                case MatchMessageType.SpellActivated:
                    OnSpellActivated?.Invoke(message as MatchMessageSpellActivated);
                    break;

                //CARDS
                case MatchMessageType.CardPlayRequest:
                    OnCardRequested?.Invoke(message as MatchMessageCardPlayRequest);
                    break;

                case MatchMessageType.CardPlayed:
                    OnCardPlayed?.Invoke(message as MatchMessageCardPlayed);
                    break;

                case MatchMessageType.CardCanceled:
                    OnCardCancelled?.Invoke(message as MatchMessageCardCanceled);
                    break;

                case MatchMessageType.StartingHand:
                    OnStartingHandReceived?.Invoke(message as MatchMessageStartingHand);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Reads match messages sent by other players, and fires locally events basing on opCode.
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="messageJson"></param>
        public void ReceiveMatchStateHandle(long opCode, string messageJson)
        {
            if (GameStarted == false)
            {
                _incommingMessages.Enqueue(new IncommingMessageState(opCode, messageJson));
                return;
            }

            //Choosing which event should be invoked basing on opCode, then parsing json to MatchMessage class and firing event
            switch ((MatchMessageType)opCode)
            {
                //GAME
                case MatchMessageType.MatchEnded:
                    MatchMessageGameEnded matchMessageGameEnded = MatchMessageGameEnded.Parse(messageJson);
                    OnGameEnded?.Invoke(matchMessageGameEnded);
                    break;

                //UNITS
                case MatchMessageType.UnitSpawned:
                    MatchMessageUnitSpawned matchMessageUnitSpawned = MatchMessageUnitSpawned.Parse(messageJson);
                    OnUnitSpawned?.Invoke(matchMessageUnitSpawned);
                    break;

                case MatchMessageType.UnitMoved:
                    MatchMessageUnitMoved matchMessageUnitMoved = MatchMessageUnitMoved.Parse(messageJson);
                    OnUnitMoved?.Invoke(matchMessageUnitMoved);
                    break;

                case MatchMessageType.UnitAttacked:
                    MatchMessageUnitAttacked matchMessageUnitAttacked = MatchMessageUnitAttacked.Parse(messageJson);
                    OnUnitAttacked?.Invoke(matchMessageUnitAttacked);
                    break;

                //SPELLS
                case MatchMessageType.SpellActivated:
                    MatchMessageSpellActivated matchMessageSpellActivated = MatchMessageSpellActivated.Parse(messageJson);
                    OnSpellActivated?.Invoke(matchMessageSpellActivated);
                    break;

                //CARDS
                case MatchMessageType.CardPlayRequest:
                    if (IsHost == true)
                    {
                        MatchMessageCardPlayRequest matchMessageCardPlayRequest = MatchMessageCardPlayRequest.Parse(messageJson);
                        OnCardRequested?.Invoke(matchMessageCardPlayRequest);
                    }
                    break;

                case MatchMessageType.CardPlayed:
                    MatchMessageCardPlayed matchMessageCardPlayed = MatchMessageCardPlayed.Parse(messageJson);
                    OnCardPlayed?.Invoke(matchMessageCardPlayed);
                    break;


                case MatchMessageType.CardCanceled:
                    MatchMessageCardCanceled matchMessageCardCancelled = MatchMessageCardCanceled.Parse(messageJson);
                    OnCardCancelled?.Invoke(matchMessageCardCancelled);
                    break;

                case MatchMessageType.StartingHand:
                    MatchMessageStartingHand matchMessageStartingHand = MatchMessageStartingHand.Parse(messageJson);
                    OnStartingHandReceived?.Invoke(matchMessageStartingHand);
                    break;
            }
        }

        /// <summary>
        /// Retrive match reward from Nakama server.
        /// </summary>
        public async Task<int> GetMatchRewardAsync(string matchId)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            // Maximum number of attempts to receive match result
            // Sometimes host tries to receive match message before it is fully stored on the server
            int maxRetries = 10;

            // The message containing last match id we send to server in order to receive required match info
            Dictionary<string, string> payload = new Dictionary<string, string> { { "match_id", matchId } };
            string payloadJson = JsonWriter.ToJson(payload);

            while (maxRetries-- > 0)
            {
                try
                {
                    // Calling an rpc method which returns our reward
                    IApiRpc response = await client.RpcAsync(session, "last_match_reward", payloadJson);
                    Dictionary<string, int> changeset = response.Payload.FromJson<Dictionary<string, int>>();
                    return changeset["gold"];
                }
                catch (Exception)
                {
                    Debug.Log("Couldn't retrieve match reward, retrying");
                    await Task.Delay(500);
                }
            }
            Debug.LogError("Couldn't retrieve match reward; network error");
            return -1;
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Method fired when any user leaves or joins the match
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMatchPresence(IMatchPresenceEvent e)
        {
            foreach (IUserPresence user in e.Joins)
            {
                if (Players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    Debug.Log("User " + user.Username + " joined match");
                    Players.Add(user);
                    if (user.UserId != NakamaSessionManager.Instance.Session.UserId)
                    {
                        OpponentId = user.UserId;
                    }
                    if (AllPlayersJoined == true)
                    {
                        _allPlayersAdded = true;
                        StartGame();
                    }
                }
            }
            if (e.Leaves.Count() > 0)
            {
                Debug.LogWarning("User left the game. Exiting");
                UnityMainThreadDispatcher.Instance().Enqueue(LeaveGame);
            }
        }

        /// <summary>
        /// Adds all users from given match to <see cref="Players"/> list.
        /// If any user is already on the list, this means there are two devices
        /// playing on the same account, which is not allowed.
        /// </summary>
        /// <returns>True if there are no duplicate user id.</returns>
        private bool AddConnectedPlayers(IMatch match)
        {
            foreach (IUserPresence user in match.Presences)
            {
                // Check if user is already in the game
                if (Players.FindIndex(x => x.UserId == user.UserId) == -1)
                {
                    Debug.Log("User " + user.Username + " joined match");

                    // Add to player list
                    Players.Add(user);

                    // Set opponent id for better access
                    if (user.UserId != NakamaSessionManager.Instance.Session.UserId)
                    {
                        OpponentId = user.UserId;
                    }

                    // If the number of players is equal to _playerCount, no more players will come
                    // Set _allPlayersAdded to true
                    if (AllPlayersJoined == true)
                    {
                        _allPlayersAdded = true;
                    }
                }
                else
                {
                    // User is already present in the game
                    // Two devices use the same account, this is not allowed
                    Debug.LogError("Two devices uses the same account, this is not allowed");
                    return false;
                }
            }
            return true;
        }

        private void StartGame()
        {
            if (GameStarted == true)
            {
                return;
            }
            if (_allPlayersAdded == false || _matchJoined == false)
            {
                return;
            }
            GameStarted = true;

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("Starting game");
                OnGameStarted?.Invoke();
                while (_incommingMessages.Count > 0)
                {
                    IncommingMessageState incommingMessage = _incommingMessages.Dequeue();
                    ReceiveMatchStateHandle(incommingMessage.opCode, incommingMessage.message);
                }
            });
        }

        private void GameEnded(MatchMessageGameEnded obj)
        {
            _socket.ReceivedMatchPresence -= OnMatchPresence;
        }

        /// <summary>
        /// Loads main menu scene and then leaves match
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadMenuCoroutine()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
            while (operation.isDone == false)
            {
                yield return null;
            }
            LeaveMatch();
        }

        /// <summary>
        /// Disconnects local player from match
        /// </summary>
        private async void LeaveMatch()
        {
            try
            {
                Debug.Log("Leaving match with id: " + MatchId);
                //Sending request to Nakama server for leaving match
                await NakamaSessionManager.Instance.Socket.LeaveMatchAsync(MatchId);
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't leave game: " + e);
                Debug.Log("Reconnecting...");
                await NakamaSessionManager.Instance.ConnectAsync();
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                SceneManager.UnloadSceneAsync("BattleScene");
            });

        }

        /// <summary>
        /// Chooses host in deterministic way
        /// </summary>
        private void ChooseHost(IMatchmakerMatched matched)
        {
            // Add the session id of all users connected to the match
            List<string> userSessionIds = new List<string>();
            foreach (IMatchmakerUser user in matched.Users)
            {
                userSessionIds.Add(user.Presence.SessionId);
            }

            // Perform a lexicographical sort on list of user session ids
            userSessionIds.Sort();

            // First user from the sorted list will be the host of current match
            string hostSessionId = userSessionIds.First();

            // Get the user id from session id
            IMatchmakerUser hostUser = matched.Users.First(x => x.Presence.SessionId == hostSessionId);
            HostId = hostUser.Presence.UserId;
        }

        /// <summary>
        /// Receives and dispatches match state message to be handled in ReceiveMatchStateMesage in main thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="matchState"></param>
        private void ReceiveMatchStateMessage(object sender, IMatchState matchState)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(
                delegate () { ReceiveMatchStateMessage(matchState); }
                );
        }

        /// <summary>
        /// Decodes match state message json from byte form of matchState.State and then sends it to ReceiveMatchStateHandle
        /// for further reading and handling
        /// </summary>
        /// <param name="matchState"></param>
        private void ReceiveMatchStateMessage(IMatchState matchState)
        {
            string messageJson = System.Text.Encoding.UTF8.GetString(matchState.State);

            if (string.IsNullOrEmpty(messageJson))
            {
                return;
            }

            ReceiveMatchStateHandle(matchState.OpCode, messageJson);
        }

        #endregion
    }

}