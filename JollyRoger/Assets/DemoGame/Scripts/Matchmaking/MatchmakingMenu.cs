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

using System;
using System.Collections;
using System.Threading.Tasks;
using DemoGame.Scripts.Gameplay.NetworkCommunication;
using DemoGame.Scripts.Menus;
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;

namespace DemoGame.Scripts.Matchmaking
{

    /// <summary>
    /// Panel informing user about their matchmaking progress.
    /// </summary>
    public class MatchmakingMenu : Menu
    {
        #region Fields

        /// <summary>
        /// Sprite indicating match searching progress.
        /// </summary>
        [SerializeField] private RectTransform _rotatingSymbol = null;

        /// <summary>
        /// <see cref="_rotatingSymbol"/> rotation speed.
        /// </summary>
        [SerializeField] private float _degreesPerSecond = 90;

        /// <summary>
        /// Mathmaker ticker used to leave queue or join match.
        /// </summary>
        private IMatchmakerTicket ticket;

        #endregion

        #region Mono

        /// <summary>
        /// Sets the back button on click listener.
        /// </summary>
        private void Awake()
        {
            base.SetBackButtonHandler(MenuManager.Instance.HideTopMenu);
        }

        /// <summary>
        /// Rotates <see cref="_degreesPerSecond"/>.
        /// </summary>
        private void Update()
        {
            if (gameObject.activeInHierarchy == true)
            {
                _rotatingSymbol.Rotate(Vector3.forward, -_degreesPerSecond * Time.deltaTime);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Joins matchmaker queue and shows this panel.
        /// </summary>
        public async override void Show()
        {
            if (IsShown == false)
            {
                await StartMatchmakerAsync();
                base.Show();
            }
        }

        /// <summary>
        /// Leaves matchmaker queue and hides this panel.
        /// </summary>
        public async override void Hide()
        {
            if (IsShown == true)
            {
                await StopMatchmakerAsync(ticket);
                base.Hide();
            }
        }

        /// <summary>
        /// Joins matchmaker queue.
        /// </summary>
        /// <remarks>
        /// Nakama allows for joining multiple matchmakers, however for the purposes of this demo,
        /// we will allow only for joining a single matchmaking queue.
        /// </remarks>
        private async Task<bool> StartMatchmakerAsync()
        {
            if (ticket != null)
            {
                Debug.Log("Matchmaker already started");
                return false;
            }

            ISocket socket = NakamaSessionManager.Instance.Socket;

            // Create params object with default values
            MatchmakingParams param = new MatchmakingParams();

            socket.ReceivedMatchmakerMatched += OnMatchmakerMatched;
            // Join the matchmaker
            ticket = await MatchmakingManager.EnterQueueAsync(socket, param);
            if (ticket == null)
            {
                Debug.Log("Couldn't start matchmaker" + Environment.NewLine + "Try again later");
                socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched;
                return false;
            }
            else
            {
                // Matchmaker queue joined
                return true;
            }
        }

        /// <summary>
        /// Invoked whenever matchmaker finds an opponent.
        /// </summary>
        private void OnMatchmakerMatched(IMatchmakerMatched e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ISocket socket = NakamaSessionManager.Instance.Socket;
                socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched;

                StartCoroutine(LoadBattle(e));
            });
        }

        /// <summary>
        /// Leaves matchmaker queue.
        /// </summary>
        private async Task<bool> StopMatchmakerAsync(IMatchmakerTicket ticket)
        {
            if (ticket == null)
            {
                Debug.Log("Couldn't stop matchmaker; matchmaking hasn't been started yet");
                return false;
            }
            ISocket socket = NakamaSessionManager.Instance.Socket;
            bool good = await MatchmakingManager.LeaveQueueAsync(socket, ticket);

            this.ticket = null;
            socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched;
            return good;
        }

        /// <summary>
        /// Starts the game scene and joins the match
        /// </summary>
        private IEnumerator LoadBattle(IMatchmakerMatched matched)
        {
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("BattleScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("MainScene");
            MatchCommunicationManager.Instance.JoinMatchAsync(matched);
        }

        #endregion
    }

}