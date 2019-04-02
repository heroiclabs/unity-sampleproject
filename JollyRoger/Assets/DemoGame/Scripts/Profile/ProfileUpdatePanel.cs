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
using Facebook.Unity;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Profile
{

    /// <summary>
    /// Panel used to change username and avatar of a user account.
    /// </summary>
    public class ProfileUpdatePanel : SingletonMenu<ProfileUpdatePanel>
    {
        #region Fields

        #region FacebookRestoreUI

        /// <summary>
        /// Begins account migration process by invoking
        /// <see cref="NakamaSessionManager.MigrateDeviceIdAsync(string)"/>.
        /// </summary>
        [SerializeField] private Button _facebookConflictConfirmButton = null;

        /// <summary>
        /// Panel with UI representing succesfull Facebook account linking.
        /// </summary>
        [SerializeField] private Menu _facebookSuccessPanel = null;

        /// <summary>
        /// Panel with UI representing failed Facebook account linking.
        /// </summary>
        [SerializeField] private Menu _facebookErrorPanel = null;

        /// <summary>
        /// Panel allowing user to chose whether to migrate current device to
        /// an already existing accoun linked to supplied Facebook account
        /// </summary>
        [SerializeField] private Menu _facebookConflictPanel = null;

        #endregion

        [Space]
        /// <summary>
        /// Sends account update request to Nakama server and closes the panel.
        /// </summary>
        [SerializeField] private Button _doneButton = null;

        /// <summary>
        /// Textbox containing account's username.
        /// </summary>
        [SerializeField] private InputField _usernameText = null;

        /// <summary>
        /// Restores account using Facebook.
        /// </summary>
        [SerializeField] private Button _linkFacebookButton = null;

        /// <summary>
        /// Image displying user's avatar.
        /// </summary>
        [SerializeField] private Image _avatarImage = null;

        /// <summary>
        /// Button used for changing avatars.
        /// </summary>
        [SerializeField] private Button _avatarButton = null;


        /// <summary>
        /// The path to currently displayed avatar in Resources folder.
        /// </summary>
        private string _avatarPath;

        #endregion

        #region Mono

        /// <summary>
        /// Sets button listeners.
        /// </summary>
        private void Start()
        {
            _avatarButton.onClick.AddListener(ChangeAvatar);
            _linkFacebookButton.onClick.AddListener(LinkFacebook);
            _facebookConflictConfirmButton.onClick.AddListener(MigrateAccount);

            _facebookConflictPanel.SetBackButtonHandler(_facebookConflictPanel.Hide);
            _facebookErrorPanel.SetBackButtonHandler(_facebookErrorPanel.Hide);
            _facebookSuccessPanel.SetBackButtonHandler(_facebookSuccessPanel.Hide);

            _facebookConflictPanel.Hide();
            _facebookErrorPanel.Hide();
            _facebookSuccessPanel.Hide();

            base.SetBackButtonHandler(MenuManager.Instance.HideTopMenu);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Makes this panel visible to the viewer.
        /// Fills fields with local user data.
        /// </summary>
        /// <param name="canTerminate">
        /// If false, user can't exit this panel before updating their account.
        /// </param>
        public void ShowUpdatePanel(Action onDone, bool canTerminate)
        {
            // Update done button listeners
            _doneButton.onClick.RemoveAllListeners();
            _doneButton.onClick.AddListener(() => Done(onDone));

            IApiUser user = NakamaSessionManager.Instance.Account.User;
            _usernameText.text = user.Username;
            _avatarPath = user.AvatarUrl;
            _avatarImage.sprite = AvatarManager.Instance.LoadAvatar(_avatarPath);
            MenuManager.Instance.ShowMenu(this, false);

            if (canTerminate == true)
            {
                _backButton.gameObject.SetActive(true);
            }
            else
            {
                _backButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Changes displayed avatar to next available.
        /// </summary>
        private void ChangeAvatar()
        {
            _avatarPath = AvatarManager.Instance.NextAvatar(_avatarPath);
            _avatarImage.sprite = AvatarManager.Instance.LoadAvatar(_avatarPath);
        }

        /// <summary>
        /// Links a Facebook account with Nakama user account.
        /// If given facebook account is already linked to other Nakama account, links a dummy device
        /// to current account, unlinks local device and migrates it to the Facebook linked account.
        /// </summary>
        private void LinkFacebook()
        {
            NakamaSessionManager.Instance.ConnectFacebook(OnFacebookResponded);
        }

        /// <summary>
        /// Invoked by <see cref="LinkFacebook"/> after successfull or unsuccessfull facebook linking.
        /// </summary>
        private void OnFacebookResponded(FacebookResponse response)
        {
            if (response == FacebookResponse.Conflict)
            {
                _facebookConflictPanel.Show();
            }
            else if (response == FacebookResponse.Error || response == FacebookResponse.NotInitialized)
            {
                _facebookErrorPanel.Show();
            }
            else if (response == FacebookResponse.Linked)
            {
                _facebookSuccessPanel.Show();
            }
        }

        /// <summary>
        /// Migrates current device to supplied Facebook account.
        /// </summary>
        private async void MigrateAccount()
        {
            _facebookConflictPanel.Hide();
            string token = AccessToken.CurrentAccessToken.TokenString;
            bool good = await NakamaSessionManager.Instance.MigrateDeviceIdAsync(token);
            if (good == false)
            {
                _facebookErrorPanel.Show();
            }
            else
            {
                _facebookSuccessPanel.Show();
            }
        }

        /// <summary>
        /// Sends account update request to server with new Username and AvatarUrl.
        /// </summary>
        private async void Done(Action onDone)
        {
            AuthenticationResponse response = await NakamaSessionManager.Instance.UpdateUserInfoAsync(_usernameText.text, _avatarPath);
            if (response != AuthenticationResponse.Error)
            {
                onDone?.Invoke();
                MenuManager.Instance.HideTopMenu();
            }
        }

        #endregion

    }

}