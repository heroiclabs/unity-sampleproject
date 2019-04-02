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
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Clans
{

    /// <summary>
    /// Single user entry in <see cref="ClanPanel"/>'s member list.
    /// Contains UI needed to display basic user information aswell as allow for
    /// multiple clan related operations, such as kicking and promoting users.
    /// </summary>
    public class ClanUserEntry : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Text field containing username.
        /// </summary>
        [SerializeField] private Text _usernameText = null;

        /// <summary>
        /// Image representing user's rank.
        /// </summary>
        [SerializeField] private Image _rankImage = null;

        #region Interaction Panel

        [Space]
        /// <summary>
        /// Animator used to hide and show interaction panel.
        /// </summary>
        [SerializeField] private Animator _animator = null;

        /// <summary>
        /// Button used to open and close user interaction panel.
        /// </summary>
        [SerializeField] private Button _panelButton = null;

        /// <summary>
        /// Button responsible for kicking current user.
        /// </summary>
        [SerializeField] private Button _kickButton = null;

        /// <summary>
        /// Button responsible for promoting current user.
        /// </summary>
        [SerializeField] private Button _promoteButton = null;

        /// <summary>
        /// Button responsible for showing <see cref="ProfilePanel"/> of current user.
        /// </summary>
        [SerializeField] private Button _profileButton = null;

        /// <summary>
        /// If true, interaction panel is currently showm.
        /// </summary>
        private bool _isShown;

        #endregion

        #region Rank Sprites

        [Space]
        /// <summary>
        /// Sprite representing admin rank.
        /// </summary>
        [SerializeField] private Sprite _adminRankSprite = null;
        /// <summary>
        /// Sprite representing superadmin rank.
        /// </summary>
        [SerializeField] private Sprite _superadminRankSprite = null;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Initializes this <see cref="ClanUserEntry"/> object.
        /// </summary>
        /// <param name="user">The user all associated with this <see cref="ClanUserEntry"/> object.</param>
        /// <param name="userState">The state of <paramref name="user"/> in currently shown Clan.</param>
        /// <param name="localState">The state of local user in currently shown Clan.</param>
        /// <param name="onKick">Method called after clicking <see cref="_kickButton"/>.</param>
        /// <param name="onPromote">Method called after clicking <see cref="_promoteButton"/>.</param>
        /// <param name="onShowProfile">Method called after clicking <see cref="_profileButton"/>.</param>
        public void SetUser(IApiUser user, ClanUserState userState, ClanUserState localState, Action<ClanUserEntry> onSelected,
            Action<IApiUser> onKick, Action<IApiUser> onPromote, Action<IApiUser> onShowProfile)
        {
            // Setting basic information based on supplied parameters.
            _usernameText.text = user.Username;
            _rankImage.sprite = GetRankSprite(userState);
            _rankImage.gameObject.SetActive(_rankImage.sprite != null);

            // If local user is the same as supplied user, show their name in green color
            // and disable kick and promote options.
            if (user.Id == NakamaSessionManager.Instance.Account.User.Id)
            {
                _usernameText.color = Color.green;
                _kickButton.gameObject.SetActive(false);
                _promoteButton.gameObject.SetActive(false);
            }
            // If local user cannot manager supplied user (because local user's ClanUserState is lower
            // or local user does not belong to this clan), disable kick and promote options.
            else if (CanManageUser(localState, userState) == false)
            {
                _kickButton.gameObject.SetActive(false);
                _promoteButton.gameObject.SetActive(false);
            }
            // Add listeners to buttons
            else
            {
                _kickButton.onClick.AddListener(() => onKick?.Invoke(user));
                _promoteButton.onClick.AddListener(() => onPromote?.Invoke(user));
            }
            _profileButton.onClick.AddListener(() => onShowProfile?.Invoke(user));
            _panelButton.onClick.AddListener(() => onSelected(this));
        }

        /// <summary>
        /// Opens interaction panel.
        /// </summary>
        public void ShowInteractionPanel()
        {
            if (_isShown == false)
            {
                _isShown = true;
                _animator.SetTrigger("Open");
            }
        }

        /// <summary>
        /// Hides interaction panel.
        /// </summary>
        public void HideInteractionPanel()
        {
            if (_isShown == true)
            {
                _isShown = false;
                _animator.SetTrigger("Close");
            }
        }

        /// <summary>
        /// Returns a sprite associated with supplied rank.
        /// Only Admin and Superadmin have rank sprites.
        /// </summary>
        private Sprite GetRankSprite(ClanUserState userState)
        {
            switch (userState)
            {
                case ClanUserState.Superadmin: return _superadminRankSprite;
                case ClanUserState.Admin: return _adminRankSprite;
                default: return null;
            }
        }

        /// <summary>
        /// Returns true if user <paramref name="localState"/> can kick and promote user with <paramref name="managedUser"/>.
        /// </summary>
        private bool CanManageUser(ClanUserState localState, ClanUserState managedUser)
        {
            switch (localState)
            {
                case ClanUserState.Superadmin:
                    return true;
                case ClanUserState.Admin:
                    return
                        managedUser == ClanUserState.Member ||
                        managedUser == ClanUserState.JoinRequest;
                case ClanUserState.Member:
                    return false;
                case ClanUserState.JoinRequest:
                    return false;
                default:
                    return false;
            }
        }

        #endregion
    }

}