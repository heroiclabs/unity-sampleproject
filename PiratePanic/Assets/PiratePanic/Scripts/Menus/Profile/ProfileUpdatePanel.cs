/**
 * Copyright 2021 The Nakama Authors
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
using System.Collections.Generic;
using Facebook.Unity;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{

    /// <summary>
    /// Panel used to change username and avatar of a user account.
    /// </summary>
    public class ProfileUpdatePanel : Menu
	{
		/// <summary>
		/// Begins account migration process by invoking
		/// <see cref="GameConnection.MigrateDeviceIdAsync(string)"/>.
		/// </summary>
		[SerializeField] private Button _facebookConflictConfirmButton = null;

		/// <summary>
		/// Panel with UI representing succesfull Facebook account linking.
		/// </summary>
		[SerializeField] private Menu _facebookSuccessPanel = null;

		/// <summary>
		/// Panel with UI representing failed Facebook account linking.
		/// </summary>git sat
		[SerializeField] private Menu _facebookErrorPanel = null;

		/// <summary>
		/// Panel allowing user to chose whether to migrate current device to
		/// an already existing accoun linked to supplied Facebook account
		/// </summary>
		[SerializeField] private Menu _facebookConflictPanel = null;

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
		/// The available avatars to choose from.
		/// </summary>
		[SerializeField] private AvatarSprites _avatarSprites;

		/// <summary>
		/// The index of the currently selected avatar.
		/// </summary>
		private int _currentAvatarIndex;

		private GameConnection _connection;
		private string _deviceId;
		private string _facebookToken;

		public void Init(GameConnection connection, string deviceId)
		{
			_deviceId = deviceId;
			_connection = connection;

			_backButton.onClick.AddListener(() => Hide());
			_doneButton.onClick.AddListener(Done);
			_linkFacebookButton.gameObject.SetActive(FB.IsInitialized);

			IApiUser user = _connection.Account.User;
			_backButton.gameObject.SetActive(true);
		}

		/// <summary>
		/// Sets button listeners.
		/// </summary>
		private void Start()
		{
			_avatarButton.onClick.AddListener(ChangeAvatar);
			_linkFacebookButton.onClick.AddListener(LinkFacebook);
			_facebookConflictConfirmButton.onClick.AddListener(MigrateAccount);

			_facebookConflictPanel.SetBackButtonHandler(() => _facebookConflictPanel.Hide());
			_facebookErrorPanel.SetBackButtonHandler(() => _facebookErrorPanel.Hide());
			_facebookSuccessPanel.SetBackButtonHandler(() => _facebookSuccessPanel.Hide());

			_facebookConflictPanel.Hide(true);
			_facebookErrorPanel.Hide(true);
			_facebookSuccessPanel.Hide(true);
		}

        public override void Show(bool isMuteButtonClick = false)
        {
            base.Show(isMuteButtonClick);
            _usernameText.text = _connection.Account.User.Username;
			_avatarImage.sprite = _avatarSprites.GetSpriteByName(_connection.Account.User.AvatarUrl);
        }

		/// <summary>
		/// Changes displayed avatar to next available.
		/// </summary>
		private void ChangeAvatar()
		{
			int nextIndex = _currentAvatarIndex = (_currentAvatarIndex + 1) % _avatarSprites.Sprites.Length;
			_avatarImage.sprite = _avatarSprites.Sprites[nextIndex];
		}

		/// <summary>
		/// Links a Facebook account with Nakama user account.
		/// If given facebook account is already linked to other Nakama account, links a dummy device
		/// to current account, unlinks local device and migrates it to the Facebook linked account.
		/// </summary>
		private void LinkFacebook()
		{
			List<string> permissions = new List<string>();
			permissions.Add("public_profile");

			FB.LogInWithReadPermissions(permissions, async result =>
			{
				try
				{
					_facebookToken = result.AccessToken.TokenString;
					await _connection.Client.AuthenticateFacebookAsync(_facebookToken);
				}
				catch (Exception e)
				{
					Debug.LogWarning("Error linking to facebook: " + e.Message);
				}
			});
		}

		/// <summary>
		/// Invoked by <see cref="LinkFacebook"/> after successful or unsuccessfull facebook linking.
		/// </summary>
		private void OnFacebookResponded(long httpStatusCode)
		{
			if (httpStatusCode == 409)
			{
				_facebookConflictPanel.Show();
			}
			else if (httpStatusCode == 200)
			{
				_facebookSuccessPanel.Show();
			}
			else
			{
				_facebookErrorPanel.Show();
			}
		}

		/// <summary>
		/// Migrates current device to supplied Facebook account.
		/// </summary>
		private async void MigrateAccount()
		{
			_facebookConflictPanel.Hide();
			string token = AccessToken.CurrentAccessToken.TokenString;

			try
			{
				string dummyGuid = _deviceId + "-";
				await _connection.Client.LinkDeviceAsync(_connection.Session, dummyGuid);
				ISession activatedSession = await _connection.Client.AuthenticateFacebookAsync(_facebookToken, null, false);
				await _connection.Client.UnlinkDeviceAsync(_connection.Session, _deviceId);
				await _connection.Client.LinkDeviceAsync(activatedSession, _deviceId);
				_connection.Session = activatedSession;

				PlayerPrefs.SetString(GameConstants.AuthTokenKey, _connection.Session.AuthToken);

				_connection.Account = await _connection.Client.GetAccountAsync(_connection.Session);

				if (_connection.Account == null)
				{
					throw new Exception("Couldn't retrieve linked account data");
				}

				_facebookSuccessPanel.Show();
			}
			catch (Exception e)
			{
				Debug.LogWarning("An error has occured while linking dummy guid to local account: " + e);
				_facebookErrorPanel.Show();
			}
		}

		/// <summary>
		/// Sends account update request to server with new Username and AvatarUrl.
		/// </summary>
		private async void Done()
		{
			try
			{
				PlayerPrefs.SetString(GameConstants.AuthTokenKey, _connection.Session.AuthToken);

				await _connection.Client.UpdateAccountAsync(_connection.Session, _usernameText.text, null);
				var account = await _connection.Client.GetAccountAsync(_connection.Session);
				_connection.Account = account;
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Error updating username: " + e.Message);
			}

			Hide();
		}
	}
}
