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
using System.Linq;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{

    /// <summary>
    /// Manages leaderboards UI.
    /// </summary>
    public class LeaderboardsMenuUI : Menu
	{
		/// <summary>
		/// Parent for all leaderboards records.
		/// </summary>
		[SerializeField] private RectTransform _userList = null;

		/// <summary>
		/// Prefab of a single leaderboard record.
		/// </summary>
		[SerializeField] private LeaderboardEntry _leaderboardEntryPrefab = null;

		/// <summary>
		/// Maximum number of records displayed on a single page.
		/// </summary>
		[SerializeField] private int _recordsPerPage = 100;

		/// <summary>
		/// Shows global leaderboard.
		/// </summary>
		[SerializeField] private Button _showGlobal = null;

		/// <summary>
		/// Shows clan leaderboards.
		/// </summary>
		[SerializeField] private Button _showClan = null;

		/// <summary>
		/// Shows friends leaderboards.
		/// </summary>
		[SerializeField] private Button _showFriends = null;


		/// <summary>
		/// Shows next page of leaderboard results.
		/// </summary>
		[SerializeField] private Button _nextPageButton = null;

		/// <summary>
		/// Shows previous page of leaderboard results.
		/// </summary>
		[SerializeField] private Button _prevPageButton = null;

		/// <summary>
		/// Invoked upon clicking <see cref="_nextPageButton"/>.
		/// </summary>
		private Action _nextPage;

		/// <summary>
		/// Invoked upon clicking <see cref="_prevPageButton"/>.
		/// </summary>
		private Action _prevPage;

		private GameConnection _connection;
		private ProfilePopup _profilePopup;
		private IUserGroupListUserGroup _userClan;

		private void Awake()
		{
			_showClan.onClick.AddListener(() => ShowClanLeaderboards(null));
			_showGlobal.onClick.AddListener(() => ShowGlobalLeaderboards(null));
			_showFriends.onClick.AddListener(() => ShowFriendsLeaderboards(null));
			_backButton.onClick.AddListener(() => Hide());
		}

		/// <summary>
		/// Adds handlers to buttons.
		/// </summary>
		public void Init(GameConnection connection, ProfilePopup profilePopup)
		{
			_connection = connection;
			_profilePopup = profilePopup;
		}

		/// <summary>
		/// Fills <see cref="_userList"/> with user records sorted by the score.
		/// </summary>
		public async void ShowGlobalLeaderboards(string cursor = null)
		{
			IApiLeaderboardRecordList records = await _connection.Client.ListLeaderboardRecordsAsync(_connection.Session, "global", ownerIds: null, expiry: null, _recordsPerPage, cursor);

			SetLeaderboardsCursor(records, ShowGlobalLeaderboards);
			FillLeaderboard(records.Records);

			_showFriends.interactable = true;
			_showClan.interactable = true;
			_showGlobal.interactable = false;
		}

		/// <summary>
		/// Fills <see cref="_userList"/> with records of all members of the clan local user belongs to.
		/// </summary>
		public async void ShowClanLeaderboards(string cursor)
		{
			if (_userClan == null)
			{
				return;
			}

			var users = await _connection.Client.ListGroupUsersAsync(_connection.Session, _userClan.Group.Id, null, 1, null);
			IEnumerable<string> ids = users.GroupUsers.Select(x => x.User.Id);

			IApiLeaderboardRecordList list = await _connection.Client.ListLeaderboardRecordsAsync(_connection.Session, "global", ids, null, 1, cursor);

			if (list.Records != null)
			{
				SetLeaderboardsCursor(list, ShowClanLeaderboards);
				FillLeaderboard(list.OwnerRecords);

				_showFriends.interactable = true;
				_showClan.interactable = false;
				_showGlobal.interactable = true;
			}
		}

		/// <summary>
		/// Gets ids of all friends of local user and displays their records.
		/// This also includes local user.
		/// </summary>
		public async void ShowFriendsLeaderboards(string cursor)
		{
			try
			{
				var friends = await _connection.Client.ListFriendsAsync(_connection.Session);
				List<string> ids = friends.Friends.Select(x => x.User.Id).ToList();
				ids.Add(_connection.Session.UserId);

				IApiLeaderboardRecordList records = await _connection.Client.ListLeaderboardRecordsAsync(_connection.Session, "global", ids, null, 1, cursor);

				SetLeaderboardsCursor(records, ShowFriendsLeaderboards);
				FillLeaderboard(records.OwnerRecords);

				_showFriends.interactable = false;
				_showClan.interactable = true;
				_showGlobal.interactable = true;
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Error showing friends leaderboards: " + e.Message);
			}
		}

		/// <summary>
		/// Sets <see cref="_nextPageButton"/> and <see cref="_prevPageButton"/> onClick events.
		/// If there is no next or previous page, disables interactions with these buttons.
		/// </summary>
		/// <param name="records">
		/// Contains <see cref="IApiLeaderboardRecordList.NextCursor"/> and
		/// <see cref="IApiLeaderboardRecordList.PrevCursor"/> responsible for iterating through pages.
		/// </param>
		/// <param name="caller">
		/// Method used to receive <paramref name="records"/>. This method will be called to iterate
		/// through pages using cursors.
		/// </param>
		private void SetLeaderboardsCursor(IApiLeaderboardRecordList records, Action<string> caller)
		{
			if (records.PrevCursor != null)
			{
				_prevPageButton.interactable = true;
				_prevPageButton.onClick.RemoveAllListeners();
				_prevPageButton.onClick.AddListener(() => caller(records.PrevCursor));
			}
			else
			{
				_prevPageButton.interactable = false;
			}

			if (records.NextCursor != null)
			{
				_nextPageButton.interactable = true;
				_nextPageButton.onClick.RemoveAllListeners();
				_nextPageButton.onClick.AddListener(() => caller(records.NextCursor));
			}
			else
			{
				_nextPageButton.interactable = false;
			}
		}

		/// <summary>
		/// Destroys every child of <see cref="_userList"/>, then
		/// creates an instance of <see cref="_leaderboardEntryPrefab"/> for each record.
		/// </summary>
		private void FillLeaderboard(IEnumerable<IApiLeaderboardRecord> recordList)
		{
			foreach (Transform entry in _userList)
			{
				Destroy(entry.gameObject);
			}

			int rank = 1;
			string localId = _connection.Account.User.Id;

			foreach (IApiLeaderboardRecord record in recordList)
			{
				LeaderboardEntry entry = Instantiate(_leaderboardEntryPrefab, _userList);
				string username = record.Username;
				if (localId == record.OwnerId)
				{
					username += " (You)";
				}
				entry.SetPlayer(username, rank, record.Score, () => OnProfileClicked(record.OwnerId));
				rank += 1;
			}
		}

		/// <summary>
		/// Invoked whenever user clicks <see cref="LeaderboardEntry._profile"/> button of a record.
		/// </summary>
		private void OnProfileClicked(string userId)
		{
			_profilePopup.Show(userId);
		}

		/// <summary>
		/// Checks whether local user is a member of a clan, then disables or enables <see cref="_showClan"/>
		/// button accordingly. Shows this menu.
		/// </summary>
		public async override void Show(bool isMuteButtonClick = false)
		{
			IApiUserGroupList clanList = null;

			try
			{
				clanList = await _connection.Client.ListUserGroupsAsync(_connection.Session);
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Error showing clan leaderboards: " + e.Message);
				return;
			}

			_userClan = clanList.UserGroups.FirstOrDefault();

			if (_userClan != null)
			{
				_showClan.gameObject.SetActive(true);
			}
			else
			{
				// User is not a member of any clan
				// Hiding clan score tab
				if (_showClan.interactable)
				{
					// Last showed tab is clan tab
					// Switching to other tab
					ShowGlobalLeaderboards();
				}

				_showClan.gameObject.SetActive(false);
			}

			base.Show(isMuteButtonClick);
		}
	}
}