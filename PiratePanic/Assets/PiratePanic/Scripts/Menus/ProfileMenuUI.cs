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
using UnityEngine;

namespace PiratePanic
{
    /// <summary>
    /// Menu showing <see cref="ProfilePanel"/> on enter filed with local user's data.
    /// </summary>
    public class ProfileMenuUI : Menu
	{
		/// <summary>
		/// Reference to panel containing profile information
		/// </summary>
		[SerializeField] private ProfilePanel _profilePanel = null;

		private GameConnection _connection;

		private void Awake()
		{
			_backButton.onClick.AddListener(() => Hide());
		}

		public void Init(GameConnection connection, ProfileUpdatePanel updatePanel)
		{
			_connection = connection;
			_profilePanel.Init(connection, updatePanel);
		}

		/// <summary>
		/// Shows this menu and <see cref="ProfilePanel"/> with local user data.
		/// </summary>
		public override void Show(bool isMuteButtonClick = false)
		{
			try
			{
				_profilePanel.Show(_connection.Account.User);
				base.Show(isMuteButtonClick);
			}
			catch (Exception e)
			{
				Debug.LogError("Could not show profile menu UI: " + e.Message);
			}
		}
	}
}
