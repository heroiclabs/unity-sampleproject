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

using DemoGame.Scripts.Profile;
using DemoGame.Scripts.Session;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Menus
{

    /// <summary>
    /// Menu displayed to the user at the beginning of the game.
    /// Blocks any action before connection with the server can be established.
    /// </summary>
    public class LoadingMenu : SingletonMenu<LoadingMenu>
    {
        #region Fields

        #region Connecting

        [Space]
        /// <summary>
        /// Panel containing UI showing connection progress.
        /// </summary>
        [SerializeField] private GameObject _connectingPanel = null;

        /// <summary>
        /// Spinning connection icon.
        /// </summary>
        [SerializeField] private GameObject _loadingIcon = null;

        /// <summary>
        /// Speed at which <see cref="_loadingIcon"/> is spinning in degrees per second.
        /// </summary>
        [SerializeField] private float rotationSpeed = -90;

        #endregion

        #region Retry

        [Space]
        /// <summary>
        /// Panel containing reconnect UI.
        /// </summary>
        [SerializeField] private GameObject _retryPanel = null;

        /// <summary>
        /// Reconnect with server button.
        /// </summary>
        [SerializeField] private Button _retryButton = null;

        #endregion

        #endregion

        #region Mono

        /// <summary>
        /// Rotates <see cref="_loadingIcon"/> around Z-axis.
        /// </summary>
        private void Update()
        {
            if (base.IsShown == true)
            {
                _loadingIcon.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets up Nakama session success and failure handlers.
        /// Adds listener to <see cref="_retryButton"/> and shows this menu.
        /// </summary>
        public override void Show()
        {
            base.Show();
            _retryButton.onClick.AddListener(Retry);
            AwaitConnection();
        }

        /// <summary>
        /// Shows connection awaiting panel.
        /// Subscribes to <see cref="NakamaSessionManager.OnConnectionSuccess"/>
        /// and <see cref="NakamaSessionManager.OnConnectionFailure"/> events.
        /// </summary>
        public void AwaitConnection()
        {
            _connectingPanel.SetActive(true);
            _retryPanel.SetActive(false);
            NakamaSessionManager.Instance.OnConnectionSuccess += ConnectionSuccess;
            NakamaSessionManager.Instance.OnNewAccountCreated += NewAccountCreated;
            NakamaSessionManager.Instance.OnConnectionFailure += ConnectionFailed;
        }

        /// <summary>
        /// Hides this panel.
        /// Unsubscribes from <see cref="NakamaSessionManager.OnConnectionSuccess"/>
        /// and <see cref="NakamaSessionManager.OnConnectionFailure"/> events.
        /// </summary>
        private void ConnectionSuccess()
        {
            NakamaSessionManager.Instance.OnConnectionSuccess -= ConnectionSuccess;
            NakamaSessionManager.Instance.OnNewAccountCreated -= NewAccountCreated;
            NakamaSessionManager.Instance.OnConnectionFailure -= ConnectionFailed;
            MenuManager.Instance.HideTopMenu();
        }

        /// <summary>
        /// Opens <see cref="ProfileUpdatePanel"/>
        /// </summary>
        private void NewAccountCreated()
        {
            ConnectionSuccess();
            ProfileUpdatePanel.Instance.ShowUpdatePanel(null, false);
        }

        /// <summary>
        /// Shows <see cref="_retryPanel"/> on connection failure.
        /// </summary>
        private void ConnectionFailed()
        {
            _connectingPanel.SetActive(false);
            _retryPanel.SetActive(true);
        }

        /// <summary>
        /// Tries to reconnect with Nakama server.
        /// </summary>
        private async void Retry()
        {
            _connectingPanel.SetActive(true);
            _retryPanel.SetActive(false);
            await NakamaSessionManager.Instance.ConnectAsync();
        }

        #endregion

    }

}