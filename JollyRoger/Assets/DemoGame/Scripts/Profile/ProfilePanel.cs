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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoGame.Scripts.Clans;
using DemoGame.Scripts.DataStorage;
using DemoGame.Scripts.Friends;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Gameplay.Decks;
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Profile
{

    /// <summary>
    /// Panel displaying user's stats.
    /// </summary>
    public class ProfilePanel : MonoBehaviour
    {

        #region Fields

        #region UI

        /// <summary>
        /// Button sending friend request to currently shown user.
        /// </summary>
        [SerializeField] private Button _befriendButton = null;

        /// <summary>
        /// Textbox containing displayed user's username.
        /// </summary>
        [SerializeField] private Text _usernameText = null;

        /// <summary>
        /// Textbox containing displayed user's clan name.
        /// </summary>
        [SerializeField] private Text _clanNameText = null;

        /// <summary>
        /// List of card slots dipslaying user's deck.
        /// </summary>
        [SerializeField] private List<CardSlotUI> _cardSlots = null;

        /// <summary>
        /// Textbox containing displayed user's stats gathered using <see cref="_playerDataStorage"/>.
        /// </summary>
        [SerializeField] private Text _statsText = null;

        /// <summary>
        /// Image showing displayed user's avatar.
        /// </summary>
        [SerializeField] private Image _avatarImage = null;

        /// <summary>
        /// Shows <see cref="ProfileUpdatePanel"/>.
        /// </summary>
        [SerializeField] private Button _profileUpdateButton = null;

        #endregion

        /// <summary>
        /// Used to store and retrieve <see cref="PlayerData"/> used to populate this panel.
        /// </summary>
        [SerializeField] private PlayerDataStorage _playerDataStorage = null;

        /// <summary>
        /// Used to store and retrieve <see cref="Deck"/> used to populste this panel.
        /// </summary>
        [SerializeField] private DeckStorage _deckStorage = null;

        #endregion

        #region Mono

        /// <summary>
        /// Adds a listener to <see cref="_closeButton"/> and hides this panel.
        /// </summary>
        private void Awake()
        {
            _profileUpdateButton.onClick.AddListener(() => ProfileUpdatePanel.Instance.ShowUpdatePanel(OnAccoutUpdated, true));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Searches for a user with given <paramref name="userId"/>. If a user was found, populates this panel
        /// with retrieved user's stats. Makes this panel visible to the viewer.
        /// </summary>
        /// <param name="userId">Id of searched user.</param>
        public async Task<bool> ShowAsync(Client client, ISession session, string userId)
        {
            try
            {
                IApiUsers results = await client.GetUsersAsync(session, new string[] { userId });
                if (results.Users.Count() != 0)
                {
                    return await ShowAsync(results.Users.ElementAt(0));
                }
                else
                {
                    Debug.LogWarning("Couldn't find user with id: " + userId);
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("An error has occured while retrieving user info: " + e);
                return false;
            }
        }

        /// <summary>
        /// Populates fields of this panel using <see cref="PlayerData"/> gathered from <paramref name="user"/>.
        /// Makes this panel visible to the user.
        /// </summary>
        /// <param name="user">User to be displayed in this panel.</param>
        public async Task<bool> ShowAsync(IApiUser user)
        {
            bool populated = await PopulateDataAsync(user);
            if (populated == true)
            {
                // If asPopup is true, show raycast blocking background and close button.
                // Otherwise, caller of this method should take care of closing this panel.
                await SetUIAccessAsync(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Enables and disables UI.
        /// </summary>
        /// <remarks>
        /// Disables <see cref="_profileUpdateButton"/> if displayed user is not the local user.
        /// If displayed user is already a friend of local user (or is the local user), 
        /// <see cref="_befriendButton"/> is disabled.
        /// If asPopup is true, show raycast blocking background and close button, otherwise,
        /// caller of this method should take care of closing this panel.
        /// </remarks>
        private async Task SetUIAccessAsync(IApiUser user)
        {
            IApiUser localUser = NakamaSessionManager.Instance.Account.User;
            bool isFriend = await CanBeFriendAsync(user);

            _avatarImage.sprite = AvatarManager.Instance.LoadAvatar(user.AvatarUrl);
            _profileUpdateButton.gameObject.SetActive(user.Id == localUser.Id);
            _befriendButton.gameObject.SetActive(isFriend);
        }

        /// <summary>
        /// Returns true if
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<bool> CanBeFriendAsync(IApiUser user)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            if (user.Id == session.UserId)
            {
                return false;
            }

            var friends = await FriendsManager.LoadFriendsAsync(client, session);
            if (friends == null)
            {
                Debug.Log("Couldn't retrieve friends list");
                return false;
            }
            foreach (IApiFriend friend in friends.Friends)
            {
                if (friend.User.Id == user.Id)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Updates <see cref="_usernameText"/> and <see cref="_avatarImage"/>.
        /// Invoked after user successfully updates their account using <see cref="ProfileUpdatePanel"/>.
        /// </summary>
        private void OnAccoutUpdated()
        {
            IApiUser user = NakamaSessionManager.Instance.Account.User;
            _usernameText.text = user.Username;
            _avatarImage.sprite = AvatarManager.Instance.LoadAvatar(user.AvatarUrl);
        }

        /// <summary>
        /// Sets fields of this panel to show <see cref="PlayerData"/> gathered from <paramref name="user"/>.
        /// </summary>
        /// <param name="user">User to be displayed in this panel.</param>
        private async Task<bool> PopulateDataAsync(IApiUser user)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            PlayerData data = await _playerDataStorage.LoadDataAsync(user.Id, "player_data");
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(client, session, user.Id);
            Deck deck = await _deckStorage.LoadDataAsync(user.Id, "deck");

            if (data != null && deck != null)
            {
                _usernameText.text = user.Username;
                _statsText.text = GenerateStats(data).TrimEnd();

                if (clan != null)
                {
                    _clanNameText.text = clan.Group.Name;
                }
                else
                {
                    _clanNameText.text = "<i><color=#b0b0b0>[Not a clan member yet]</color></i>";
                }

                for (int i = 0; i < deck.usedCards.Count; i++)
                {
                    Card card = deck.usedCards[i];
                    _cardSlots[i].SetCard(card);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a formated string containing stats from <see cref="PlayerData"/>.
        /// </summary>
        private string GenerateStats(PlayerData data)
        {
            return
                "Level: \t" + data.level + System.Environment.NewLine +
                "Wins: \t" + data.wins + System.Environment.NewLine +
                "Games:\t " + data.gamesPlayed + System.Environment.NewLine;
        }

        #endregion

    }

}