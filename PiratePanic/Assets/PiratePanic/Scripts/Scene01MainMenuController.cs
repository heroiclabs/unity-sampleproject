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
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
	/// <summary>
	/// Handles user authentication with Nakama server,
	/// and adds all panels to the scene and handles navigation in menu.
	///
	/// See <a href="https://heroiclabs.com/docs/unity-client-guide/#authenticate">Nakama Docs</a> for more info.
	///
	/// </summary>
	public class Scene01MainMenuController : MonoBehaviour
	{
		[SerializeField] BattleMenuUI _battleMenuUI;
		[SerializeField] ClansMenuUI _clansMenuUI;
		[SerializeField] FriendsMenuUI _friendsMenuUI;
		[SerializeField] LeaderboardsMenuUI _leaderboardMenuUI;
		[SerializeField] ProfileMenuUI _profileMenuUI;
		[SerializeField] CardsMenuUI _cardsMenuUI;
		[SerializeField] ClanCreationPanel _clanCreationPanel;
		[SerializeField] LoadingMenu _loadingMenu;
		[SerializeField] ProfilePopup _profilePopup;
		[SerializeField] ProfileUpdatePanel _profileUpdatePanel;
		[SerializeField] NotificationPopup _notificationPopup;

		[SerializeField] Button _battleButton;
		[SerializeField] Button _cardsButton;
		[SerializeField] Button _clansButton;
		[SerializeField] Button _friendsButton;
		[SerializeField] Button _leaderboardsButton;
		[SerializeField] Button _profileButton;
		[SerializeField] private GameConnection _connection;

		private void Awake()
		{
			_battleButton.onClick.AddListener(() => _battleMenuUI.Show());
			_cardsButton.onClick.AddListener(() => _cardsMenuUI.Show());
			_clansButton.onClick.AddListener(() => _clansMenuUI.Show());
			_friendsButton.onClick.AddListener(() => _friendsMenuUI.Show());
			_leaderboardsButton.onClick.AddListener(() => _leaderboardMenuUI.Show());
			_profileButton.onClick.AddListener(() => _profileMenuUI.Show());
		}

		private async void Start()
		{

			_loadingMenu.Show(true);

			if (_connection.Session == null)
			{
				string deviceId = GetDeviceId();

				if (!string.IsNullOrEmpty(deviceId))
				{
					PlayerPrefs.SetString(GameConstants.DeviceIdKey, deviceId);
				}

				await InitializeGame(deviceId);
			}

			// Provide Nakama connection to UI elements that need it.
			_battleMenuUI.Init(_connection);
			_loadingMenu.Init(_connection);
			_notificationPopup.Init(_connection);
			_cardsMenuUI.Init(_connection);
			_clanCreationPanel.Init(_connection);
			_profilePopup.Init(_connection, _profileUpdatePanel);
			_profileUpdatePanel.Init(_connection, GetDeviceId());
			_clansMenuUI.Init(_connection, _profilePopup);
			_friendsMenuUI.Init(_connection);
			_leaderboardMenuUI.Init(_connection, _profilePopup);
			_profileMenuUI.Init(_connection, _profileUpdatePanel);

			_loadingMenu.Hide(true);
		}

		private async Task InitializeGame(string deviceId)
		{
			var client = new Client("http", "localhost", 7350, "defaultkey", UnityWebRequestAdapter.Instance);
			client.Timeout = 5;

			var socket = client.NewSocket(useMainThread: true);

			string authToken = PlayerPrefs.GetString(GameConstants.AuthTokenKey, null);
			bool isAuthToken = !string.IsNullOrEmpty(authToken);

			string refreshToken = PlayerPrefs.GetString(GameConstants.RefreshTokenKey, null);

			ISession session = null;

			// refresh token can be null/empty for initial migration of client to using refresh tokens.
			if (isAuthToken)
			{
				session = Session.Restore(authToken, refreshToken);

				// Check whether a session is close to expiry.
				if (session.HasExpired(DateTime.UtcNow.AddDays(1)))
				{
					try
					{
						// get a new access token
						session = await client.SessionRefreshAsync(session);
					}
					catch (ApiResponseException)
					{
						// get a new refresh token
						session = await client.AuthenticateDeviceAsync(deviceId);
						PlayerPrefs.SetString(GameConstants.RefreshTokenKey, session.RefreshToken);
					}

					PlayerPrefs.SetString(GameConstants.AuthTokenKey, session.AuthToken);
				}
			}
			else
			{
				session = await client.AuthenticateDeviceAsync(deviceId);
				PlayerPrefs.SetString(GameConstants.AuthTokenKey, session.AuthToken);
				PlayerPrefs.SetString(GameConstants.RefreshTokenKey, session.RefreshToken);
			}

			socket.Closed += () => Connect(socket, session);
			
			Connect(socket, session);

			IApiAccount account = null;

			try
			{
				account = await client.GetAccountAsync(session);
			}
			catch (ApiResponseException e)
			{
				Debug.LogError("Error getting user account: " + e.Message);
			}

			_connection.Init(client, socket, account, session);
		}

		private string GetDeviceId()
		{
			string deviceId = "";

			deviceId = PlayerPrefs.GetString(GameConstants.DeviceIdKey);

			if (string.IsNullOrWhiteSpace(deviceId))
			{
				// Ordinarily, we would use SystemInfo.deviceUniqueIdentifier but for the purposes
				// of this demo we use Guid.NewGuid() so that developers can test against themselves locally.
				// Also note: SystemInfo.deviceUniqueIdentifier is not supported in WebGL.
				deviceId = Guid.NewGuid().ToString();
			}

			return deviceId;
		}

		private async void OnApplicationQuit()
		{
			await _connection.Socket.CloseAsync();
		}

		private async void Connect(ISocket socket, ISession session)
		{
			try
			{
				if (!socket.IsConnected)
				{
					await socket.ConnectAsync(session);
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("Error connecting socket: " + e.Message);
			}
		}
	}
}
