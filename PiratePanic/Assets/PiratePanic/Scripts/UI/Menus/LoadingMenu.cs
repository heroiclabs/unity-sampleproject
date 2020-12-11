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

using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
    /// <summary>
    /// Menu displayed to the user at the beginning of the game.
    /// Blocks any action before connection with the server can be established.
    /// </summary>
    public class LoadingMenu : Menu
	{
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

		[Space]
		/// <summary>
		/// Panel containing reconnect UI.
		/// </summary>
		[SerializeField] private GameObject _retryPanel = null;

		/// <summary>
		/// Reconnect with server button.
		/// </summary>
		[SerializeField] private Button _retryButton = null;

		private GameConnection _connection;

		public void Init(GameConnection connection)
		{
			_connection = connection;
		}

		/// <summary>
		/// Rotates <see cref="_loadingIcon"/> around Z-axis.
		/// </summary>
		private void Update()
		{
			if (base.IsShown)
			{
				_loadingIcon.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
			}
		}

		/// <summary>
		/// Sets up Nakama session success and failure handlers.
		/// Adds listener to <see cref="_retryButton"/> and shows this menu.
		/// </summary>
		public override void Show(bool isMuteButtonClick = false)
		{
			base.Show(isMuteButtonClick);
			_retryButton.onClick.AddListener(Retry);
			AwaitConnection();
		}

		/// <summary>
		/// Shows connection awaiting panel.
		/// Subscribes to <see cref="GameConnection.OnConnectionSuccess"/>
		/// and <see cref="GameConnection.OnConnectionFailure"/> events.
		/// </summary>
		public void AwaitConnection()
		{
			_connectingPanel.SetActive(true);
			_retryPanel.SetActive(false);
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
		private void Retry()
		{
			_connectingPanel.SetActive(true);
			_retryPanel.SetActive(false);
		}
	}
}