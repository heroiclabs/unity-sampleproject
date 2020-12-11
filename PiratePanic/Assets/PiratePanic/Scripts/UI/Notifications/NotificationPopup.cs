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
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
	/// <summary>
	/// Popup menu showed in the top right corner of the screen.
	/// Shown upon finishing a quest.
	/// </summary>
	/// <remarks>
	/// One of the quests prepared for this demo is "new friend quest". To finish it,
	/// add a dummy account named "Richard" to friends.
	/// </remarks>
	public class NotificationPopup : Menu
	{
		/// <summary>
		/// Title of the notification.
		/// </summary>
		[SerializeField] private Text _titleText = null;

		/// <summary>
		/// Description of the notification.
		/// </summary>
		[SerializeField] private Text _descriptionText = null;

		/// <summary>
		/// Button to hide the notification.
		/// </summary>
		[SerializeField] private Button _dismissButton = null;

		/// <summary>
		/// Serializable class used to retrieve the reward gained from completing a quest.
		/// </summary>
		[Serializable]
		private class Reward
		{
			public int reward = 0;
		}

		/// <summary>
		/// Initialize the popup with a connection to the Nakama server.
		/// </summary>
		public void Init(GameConnection connection)
		{
			connection.Socket.ReceivedNotification += NotificationReceived;
		}

		/// <summary>
		/// Sets up dismiss button handlers and hides the popup.
		/// </summary>
		private void Awake()
		{
			_dismissButton.onClick.AddListener(() => base.Hide());
		}

		/// <summary>
		/// Handles incomming notification messages.
		/// </summary>
		private void NotificationReceived(IApiNotification e)
		{
			if (e.Code == (int)NotificationCode.Quest_NewFriend)
			{
				Reward reward = JsonUtility.FromJson<Reward>(e.Content);
				base.Show();
				_titleText.text = e.Subject;
				_descriptionText.text = "Received reward: " + reward.reward;
			}
		}

		/// <summary>
		/// Shows a notification panel
		/// </summary>
		private void NotifyQuestComplete(IApiNotification e)
		{
			Reward reward = JsonUtility.FromJson<Reward>(e.Content);
			base.Show();
			_titleText.text = e.Subject;
			_descriptionText.text = "Received reward: " + reward.reward;
		}
	}
}