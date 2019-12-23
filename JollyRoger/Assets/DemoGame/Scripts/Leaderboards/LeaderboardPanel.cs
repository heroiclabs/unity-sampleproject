/**
 * Copyright 2019 The Knights Of Unity, created by Pawel Stolarczyk
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
using DemoGame.Scripts.Clans;
using DemoGame.Scripts.Friends;
using DemoGame.Scripts.Menus;
using DemoGame.Scripts.Profile;
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Leaderboards
{

    /// <summary>
    /// Manages leaderboards UI.
    /// </summary>
    public class LeaderboardPanel : Menu
    {
        #region Fields

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

        #region Buttons

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

        #endregion


        /// <summary>
        /// Invoked upon clicking <see cref="_nextPageButton"/>.
        /// </summary>
        private Action _nextPage;

        /// <summary>
        /// Invoked upon clicking <see cref="_prevPageButton"/>.
        /// </summary>
        private Action _prevPage;

        #endregion

        #region Properties

        /// <summary>
        /// Returns <see cref="Nakama.Client"/> from <see cref="NakamaSessionManager"/> for easier access.
        /// </summary>
        private Client Client { get { return NakamaSessionManager.Instance.Client; } }

        /// <summary>
        /// Returns <see cref="ISession"/> from <see cref="NakamaSessionManager"/> for easier access.
        /// </summary>
        private ISession Session { get { return NakamaSessionManager.Instance.Session; } }

        #endregion

        #region Mono

        /// <summary>
        /// Calls <see cref="Init"/> method upon connecting to Nakama.
        /// </summary>
        private void Awake()
        {
            base.SetBackButtonHandler(MenuManager.Instance.HideTopMenu);
            if (NakamaSessionManager.Instance.IsConnected == false)
            {
                NakamaSessionManager.Instance.OnConnectionSuccess += Init;
            }
            else
            {
                Init();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds handlers to buttons.
        /// </summary>
        private void Init()
        {
            NakamaSessionManager.Instance.OnConnectionSuccess -= Init;
            _showClan.onClick.AddListener(() => ShowClanLeaderboards(null));
            _showGlobal.onClick.AddListener(() => ShowGlobalLeaderboards(null));
            _showFriends.onClick.AddListener(() => ShowFriendsLeaderboards(null));
            ShowGlobalLeaderboards();
        }

        #region RecordListFilling

        /// <summary>
        /// Fills <see cref="_userList"/> with user records sorted by the score.
        /// </summary>
        public async void ShowGlobalLeaderboards(string cursor = null)
        {
            IApiLeaderboardRecordList records = await LeaderboardManager.GetGlobalLeaderboarsAsync(Client, Session, _recordsPerPage, cursor);
            if (records != null)
            {
                SetLeaderboardsCursor(records, ShowGlobalLeaderboards);
                FillLeaderboard(records.Records);

                _showFriends.interactable = true;
                _showClan.interactable = true;
                _showGlobal.interactable = false;
            }
        }

        /// <summary>
        /// Fills <see cref="_userList"/> with records of all members of the clan local user belongs to.
        /// </summary>
        public async void ShowClanLeaderboards(string cursor)
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan == null)
            {
                return;
            }
            IApiLeaderboardRecordList records = await LeaderboardManager.GetClanLeaderboarsAsync(Client, Session, clan.Group, _recordsPerPage, cursor);
            if (records != null)
            {
                SetLeaderboardsCursor(records, ShowClanLeaderboards);
                FillLeaderboard(records.OwnerRecords);

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
            var friends = await FriendsManager.LoadFriendsAsync(Client, Session);
            if (friends == null)
            {
                return;
            }
            IApiLeaderboardRecordList records = await LeaderboardManager.GetFriendsLeaderboarsAsync(Client, Session, friends.Friends, _recordsPerPage, cursor);
            if (records != null)
            {
                SetLeaderboardsCursor(records, ShowFriendsLeaderboards);
                FillLeaderboard(records.OwnerRecords);

                _showFriends.interactable = false;
                _showClan.interactable = true;
                _showGlobal.interactable = true;
            }
        }

        #endregion

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
            string localId = NakamaSessionManager.Instance.Account.User.Id;

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
        private async void OnProfileClicked(string userId)
        {

            IApiUser user = await NakamaSessionManager.Instance.GetUserInfoAsync(userId, null);
            if (user != null)
            {
                ProfilePopup.Instance.Show(userId);
            }
        }

        /// <summary>
        /// Checks whether local user is a member of a clan, then disables or enables <see cref="_showClan"/>
        /// button accordingly. Shows this menu.
        /// </summary>
        public async override void Show()
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            // If local user is not a member of a clan, disable clan leaderboards
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(client, session);
            if (clan != null)
            {
                _showClan.gameObject.SetActive(true);
            }
            else
            {
                // User is not a member of any clan
                // Hiding clan score tab

                if (_showClan.interactable == true)
                {
                    // Last showed tab is clan tab
                    // Switching to other tab
                    ShowGlobalLeaderboards();
                }
                _showClan.gameObject.SetActive(false);
            }


            base.Show();
        }

        #endregion

    }

}