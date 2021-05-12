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

using System.Collections.Generic;
using System.Linq;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
	/// <summary>
	/// Manages user interface for group chat
	/// </summary>
	public class ChatChannelClanUI : ChatChannelUI
	{

		[Space()]
		[Header("Prefabs")]
		[Space()]
		[Header("ChatChannelClanUI")]

		/// <summary>
		/// Prefab for instantiating username in active users list
		/// </summary>
		[SerializeField] private GameObject _activeUserTextPrefab = null;

		[Space()]
		[Header("UI elements")]

		/// <summary>
		/// Rect Transform of active users list, parent of active users usernames Texts
		/// </summary>
		[SerializeField] private RectTransform _activeUsersListPanel = null;

		/// <summary>
		/// Actual list of spawned Texts with usernames on active users list
		/// </summary>
		private Dictionary<string, Text> _userTexts = new Dictionary<string, Text>();

		/// <summary>
		/// <see cref="ChatChannelUI.SetChatChannel"/>
		/// </summary>
		/// <param name="chatChannel"></param>
		public override void SetChatChannel(IChannel chatChannel)
		{
			//Checking if there was already connected ChatChannel to this UI
			if (_chatChannel != null && _chatChannel == chatChannel)
			{
				return;
			}

			//Base connecting ChatChannel to UI
			base.SetChatChannel(chatChannel);

			//Clear active users list
			ClearAllActiveUsersTexts();

			//Fill in active users list with initial users
			PopulateActiveUsersList(chatChannel.Presences.Select(presence => presence.Username));
		}

		/// <summary>
		/// Adds user to active users list
		/// </summary>
		/// <param name="user"></param>
		private void AddUserToList(string username)
		{
			_userTexts.Add(username, InstantiateUsernameText(username));
		}

		/// <summary>
		/// Removes user from active users list
		/// </summary>
		/// <param name="user"></param>
		private void DeleteUserFromList(string username)
		{
			Destroy(_userTexts[username].gameObject);
			_userTexts.Remove(username);
		}

		/// <summary>
		/// Fills in active users list with given usernames
		/// </summary>
		private void PopulateActiveUsersList(IEnumerable<string> usernames)
		{
			//Add every username from given list to active users list
			foreach (string username in usernames)
			{
				_userTexts.Add(username, InstantiateUsernameText(username));
			}
		}

		/// <summary>
		/// Clears active users list
		/// </summary>
		private void ClearAllActiveUsersTexts()
		{
			//Destroying every previously added text
			List<Text> texts = new List<Text>(_userTexts.Values);

			foreach (Text text in texts)
			{
				Destroy(text.gameObject);
			}

			//Clearing _userTexts dictionary
			_userTexts.Clear();
		}

		/// <summary>
		/// Instantiate new user Text on active users list
		/// </summary>
		private Text InstantiateUsernameText(string username)
		{
			GameObject textGO = Instantiate(_activeUserTextPrefab, _activeUsersListPanel) as GameObject;
			Text text = textGO.GetComponent<Text>();
			text.text = username;
			return text;
		}
	}
}
