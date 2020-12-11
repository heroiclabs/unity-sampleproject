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
    /// Menu responsible for creating a new clan.
    /// </summary>
    public class ClanCreationPanel : Menu
	{
		public event Action<IApiGroup> OnClanCreated;

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
		/// All available avatar icons to choose from.
		/// </summary>
		[SerializeField] private AvatarSprites _avatarSprites = null;

		/// <summary>
		/// The index of the currently shown avatar.
		/// </summary>
		private int _currentAvatarIndex;

		private GameConnection _connection;

		public void Init(GameConnection connection)
		{
			_connection = connection;
			_doneButton.onClick.AddListener(() =>
			{
				Hide();
			});
		}

		/// <summary>
		/// Adds listeners to buttons.
		/// </summary>
		private void Awake()
		{
			base.Hide(true);
			_avatarButton.onClick.AddListener(ChangeAvatar);
			ChangeAvatar();
		}

		/// <summary>
		/// Changes currently displayed avatar.
		/// </summary>
		private void ChangeAvatar()
		{
			int nextIndex = _currentAvatarIndex = (_currentAvatarIndex + 1) % _avatarSprites.Sprites.Length;
			_avatarImage.sprite = _avatarSprites.Sprites[nextIndex];
		}

		/// <summary>
		/// Sends clan creation request to Nakama server.
		/// Does nothing if user already belongs to a clan.
		/// </summary>
		private async void CreateClan()
		{
			string name = _clanName.text;

			try
			{
				IApiGroup group = await _connection.Client.CreateGroupAsync(_connection.Session, name, "A super great clan.", _avatarImage.name);
				if (OnClanCreated != null)
				{
					OnClanCreated(group);
				}
			}
			catch (ApiResponseException e)
			{
				Debug.LogError("Error creating clan: " + e.Message);
			}
		}

		public override void Hide (bool isMuteSoundManager = false)
		{
			CreateClan();
			base.Hide(isMuteSoundManager);

		}
	}
}