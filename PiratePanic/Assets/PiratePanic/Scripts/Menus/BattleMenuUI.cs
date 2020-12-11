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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nakama;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace PiratePanic
{

	/// <summary>
	/// Panel informing user about their matchmaking progress.
	/// </summary>
	public class BattleMenuUI : Menu
	{
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
		private IMatchmakerTicket _ticket;
		private GameConnection _connection;

		public void Init(GameConnection connection)
		{
			_connection = connection;
			_backButton.onClick.AddListener(() => Hide());
		}

		/// <summary>
		/// Rotates <see cref="_degreesPerSecond"/>.
		/// </summary>
		private void Update()
		{
			if (gameObject.activeInHierarchy)
			{
				_rotatingSymbol.Rotate(Vector3.forward, -_degreesPerSecond * Time.deltaTime);
			}
		}

		/// <summary>
		/// Joins matchmaker queue and shows this panel.
		/// </summary>
		public async override void Show(bool isMuteButtonClick = false)
		{
			_connection.Socket.ReceivedMatchmakerMatched += OnMatchmakerMatched;

			// Join the matchmaker
			try
			{
				// Acquires matchmaking ticket used to join a match
				_ticket = await _connection.Socket.AddMatchmakerAsync(
					query: "*",
					minCount: 2,
					maxCount: 2,
					stringProperties: null,
					numericProperties: null);

			}
			catch (Exception e)
			{
				Debug.LogWarning("An error has occured while joining the matchmaker: " + e);
			}

			base.Show(isMuteButtonClick);
		}

		/// <summary>
		/// Leaves matchmaker queue and hides this panel.
		/// </summary>
		public async override void Hide(bool isMuteSoundManager = false)
		{
			try
			{
				await _connection.Socket.RemoveMatchmakerAsync(_ticket);
			}
			catch (Exception e)
			{
				Debug.LogWarning("An error has occured while removing from matchmaker: " + e);
			}

			_connection.Socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched;
			_ticket = null;
			base.Hide(isMuteSoundManager);
		}

		/// <summary>
		/// Invoked whenever matchmaker finds an opponent.
		/// </summary>
		private void OnMatchmakerMatched(IMatchmakerMatched matched)
		{
			_connection.BattleConnection = new BattleConnection(matched);
			_connection.Socket.ReceivedMatchmakerMatched -= OnMatchmakerMatched;

			SceneManager.LoadScene(GameConfigurationManager.Instance.GameConfiguration.SceneNameBattle);
		}
	}
}
