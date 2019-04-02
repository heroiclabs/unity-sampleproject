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
using DemoGame.Scripts.DataStorage;
using DemoGame.Scripts.Menus;
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Clans
{

    /// <summary>
    /// Menu responsible for creating a new clan.
    /// </summary>
    public class ClanCreationPanel : Menu
    {
        #region Fields

        /// <summary>
        /// Textbox containing clan name.
        /// </summary>
        [SerializeField] private Text _clanName = null;

        /// <summary>
        /// Button displaing list of available avatars.
        /// </summary>
        [SerializeField] private Button _avatarButton = null;

        /// <summary>
        /// Currently selected avatar.
        /// </summary>
        [SerializeField] private Image _avatarImage = null;

        /// <summary>
        /// Button sending Clan creation request to Nakama server.
        /// </summary>
        [SerializeField] private Button _doneButton = null;

        /// <summary>
        /// The path of currently selected avatar.
        /// </summary>
        private string _currentAvatar;

        #endregion

        #region Mono

        /// <summary>
        /// Adds listeners to buttons.
        /// </summary>
        private void Awake()
        {
            base.Hide();
            base.SetBackButtonHandler(MenuManager.Instance.HideTopMenu);
            _avatarButton.onClick.AddListener(ChangeAvatar);
            ChangeAvatar();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows this panel and adds listener to <see cref="_doneButton"/>.
        /// </summary>
        /// <param name="onCreated"></param>
        public void ShowCreationPanel(Action<IApiGroup> onCreated)
        {
            _doneButton.onClick.RemoveAllListeners();
            _doneButton.onClick.AddListener(() => CreateClan(onCreated));
            MenuManager.Instance.ShowMenu(this, false);
        }

        /// <summary>
        /// Changes currently displayed avatar.
        /// </summary>
        private void ChangeAvatar()
        {
            _currentAvatar = AvatarManager.Instance.NextEmblem(_currentAvatar);
            _avatarImage.sprite = AvatarManager.Instance.LoadEmblem(_currentAvatar);
        }

        /// <summary>
        /// Sends clan creation request to Nakama server.
        /// Does nothing if user already belongs to a clan.
        /// </summary>
        private async void CreateClan(Action<IApiGroup> onCreated)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            string name = _clanName.text;
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(client, session);
            if (clan != null)
            {
                Debug.LogWarning("User is already a member of a clan. Leave current clan first.");
            }
            else
            {
                IApiGroup newClan = await ClanManager.CreateClanAsync(client, session, name, _currentAvatar);
                if (newClan != null)
                {
                    MenuManager.Instance.HideTopMenu();
                    onCreated(newClan);
                }
            }
        }

        #endregion

    }

}