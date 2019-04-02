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

using DemoGame.Scripts.Session;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Menus
{

    /// <summary>
    /// Menu visible at the start of the game.
    /// Allows the user to set the IP of the server they want to connect to.
    /// </summary>
    /// <remarks>
    /// In a fully released game, users should not be able to type in the IP by themselves,
    /// this is only for demo purposes to make it easier for developers to test their local server.
    /// </remarks>
    public class ServerIpMenu : Menu
    {
        #region Fields

        /// <summary>
        /// Server IP address input field.
        /// </summary>
        [SerializeField] private InputField _ip = null;

        /// <summary>
        /// On click clears player prefs. This will enable logging into multiple accounts
        /// on the same device.
        /// </summary>
        [SerializeField] private Button _clearPlayerPrefs = null;

        #endregion

        #region Methods

        /// <summary>
        /// Makes this menu visible to the user.
        /// If this client is already connected to the server, immediately closes itself.
        /// </summary>
        public override void Show()
        {
            base.Show();
            if (NakamaSessionManager.Instance.IsConnected == true)
            {
                MenuManager.Instance.HideTopMenu();
            }
            else
            {
                string ip = PlayerPrefs.GetString("server_ip");
                _ip.text = ip;
                SetBackButtonHandler(MenuManager.Instance.HideTopMenu);
                _ip.onValueChanged.AddListener(IpChanged);
                IpChanged(ip);
                _clearPlayerPrefs.onClick.AddListener(ClearPlayerPrefs);
            }
        }

        /// <summary>
        /// Clears prevous session auth token and device id from player prefs.
        /// </summary>
        private void ClearPlayerPrefs()
        {
            PlayerPrefs.SetString("nakama.authToken", "");
            PlayerPrefs.SetString("nakama.deviceId", "");
        }

        /// <summary>
        /// Invoked on entering characters into <see cref="_ip"/>.
        /// Sets the interactability of <see cref="Menu._backButton"/>.
        /// </summary>
        private void IpChanged(string ip)
        {
            ip = ip.Trim();
            if (string.IsNullOrWhiteSpace(ip) == true)
            {
                _backButton.interactable = false;
            }
            else
            {
                _backButton.interactable = true;
            }
        }

        /// <summary>
        /// Tries to connect to Nakama server using given ip address.
        /// </summary>
        public async override void Hide()
        {
            base.Hide();
            if (NakamaSessionManager.Instance.IsConnected == false)
            {
                NakamaSessionManager.Instance.SetIp(_ip.text);
                PlayerPrefs.SetString("server_ip", _ip.text);
                MenuManager.Instance.ShowMenu(LoadingMenu.Instance, false);
                await NakamaSessionManager.Instance.ConnectAsync();
            }
        }

        #endregion

    }

}