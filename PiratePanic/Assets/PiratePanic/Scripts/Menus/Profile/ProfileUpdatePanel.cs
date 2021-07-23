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

		public void Init(GameConnection connection, string deviceId)
		{
			_deviceId = deviceId;
			_connection = connection;

			_backButton.onClick.AddListener(() => Hide());
			_doneButton.onClick.AddListener(Done);

			IApiUser user = _connection.Account.User;
			_backButton.gameObject.SetActive(true);
		}

		/// <summary>
		/// Sets button listeners.
		/// </summary>
		private void Start()
		{
			_avatarButton.onClick.AddListener(ChangeAvatar);
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
