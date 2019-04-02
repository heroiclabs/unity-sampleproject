/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch
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

using UnityEngine.UI;
using UnityEngine;
using Nakama;
using System;
using DemoGame.Scripts.Session;

namespace DemoGame.Scripts.Friends
{

    /// <summary>
    /// Panel for showing friends data in UI
    /// </summary>
    public class FriendPanel : MonoBehaviour
    {
        #region public events

        /// <summary>
        /// event fired when this friend button is selected
        /// </summary>
        public event Action<FriendPanel> OnSelected = delegate { };

        /// <summary>
        /// event fired after successful request
        /// </summary>
        public event Action OnDataChanged = delegate { };

        /// <summary>
        /// id, username
        /// </summary>
        public event Action<string, string> OnChatStartButtonClicked = delegate { };

        #endregion

        #region private serialized fields

        [Header("UI elements")]

        [Header("Buttons")]

        [SerializeField] private Button _friendButton = null;

        [SerializeField] private Button _removeFriendButton = null;

        [SerializeField] private Button _blockFriendButton = null;

        [SerializeField] private Button _startChatButton = null;

        [SerializeField] private Button _acceptInviteButton = null;


        [Header("Buttons texts and Icons")]

        [SerializeField] private Text _nicknameText = null;

        [SerializeField] private Text _blockFriendButtonText = null;

        [SerializeField] private Text _removeFriendButtonText = null;


        [Header("Animations")]

        [SerializeField] private Animator _animator = null;

        [SerializeField] private RectTransform _bottomPanel = null;

        #endregion

        #region private fields

        //References addded in Init method
        private IApiFriend _friend;

        /// <summary>
        /// True if friend panel is selected and opened
        /// </summary>
        private bool _panelOpened = false;

        /// <summary>
        /// True if player is blocked
        /// </summary>
        private bool _blocked;

        #endregion

        #region public methods

        /// <summary>
        /// Initializes panel
        /// </summary>
        /// <param name="friendsList"></param>
        /// <param name="friend"></param>
        public void Init(IApiFriend friend)
        {
            //Setting variables
            _friend = friend;
            _nicknameText.text = friend.User.Username;

            //connecting methods to buttons clicks
            _friendButton.onClick.AddListener(TogglePanel);
            _friendButton.onClick.AddListener(OnSelected_Handler);
            _removeFriendButton.onClick.AddListener(RemoveThisFriend);
            _blockFriendButton.onClick.AddListener(BlockOrUnblockThisFriend);
            _acceptInviteButton.onClick.AddListener(AcceptInviteFromThisFriend);
            _startChatButton.onClick.AddListener(StartChatWithThisFriend);

            //setting current panel state based on friend state
            ActualizeFriendState();
        }

        /// <summary>
        /// Deselecting panel, closing it
        /// </summary>
        public void Deselect(bool closeImmediately = false)
        {
            if (_panelOpened)
            {
                if (closeImmediately)
                {
                    ClosePanelImmediately();
                }
                ClosePanel();
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Sets actual friend panel state basing on friend state in database
        /// </summary>
        private void ActualizeFriendState()
        {
            switch (_friend.State)
            {
                //  Users are friends with each other.
                case 0: break;
                //  This user has sent an invitation and pending acceptance from other user.
                case 1: SetInvitedState(); break;
                //  This user has received an invitation but has not accepted yet.
                case 2: SetInvitingState(); break;
                //  This user has banned other user.
                case 3: SetBlockedState(); break;
            }
        }

        /// <summary>
        /// Handler useful for connecting button click without params with OnSelected event which requires friend panel parameter
        /// </summary>
        private void OnSelected_Handler()
        {
            if (OnSelected != null)
            {
                OnSelected(this);
            }
        }

        #region button methods

        private async void AcceptInviteFromThisFriend()
        {
            bool success = await FriendsManager.AddFriendAsync(_friend, NakamaSessionManager.Instance.Client, NakamaSessionManager.Instance.Session);
            if (success)
            {
                OnDataChanged();
            }
        }

        private async void RemoveThisFriend()
        {
            bool success = await FriendsManager.RemoveFriendAsync(_friend, NakamaSessionManager.Instance.Client, NakamaSessionManager.Instance.Session);
            if (success)
            {
                OnDataChanged();
            }
        }

        private async void BlockOrUnblockThisFriend()
        {
            //Checking if player status is banned.
            if (!_blocked)
            {
                //if is not banned, then block this friend
                bool success = await FriendsManager.BlockFriendAsync(_friend, NakamaSessionManager.Instance.Client, NakamaSessionManager.Instance.Session);
                if (success)
                {
                    OnDataChanged();
                }
            }
            else
            {
                //if is already banned, then unblock
                bool success = await FriendsManager.RemoveFriendAsync(_friend, NakamaSessionManager.Instance.Client, NakamaSessionManager.Instance.Session);
                if (success)
                {
                    OnDataChanged();
                }
            }
        }

        private void StartChatWithThisFriend()
        {
            OnChatStartButtonClicked(_friend.User.Id, _friend.User.Username);
        }

        #endregion

        #region Set states
        private void SetBlockedState()
        {
            _removeFriendButton.gameObject.SetActive(true);
            _removeFriendButtonText.text = "unblock";
            _blockFriendButtonText.text = "Unblock Friend";
            _blocked = true;
        }

        private void SetInvitedState()
        {
            _removeFriendButton.gameObject.SetActive(true);
            _removeFriendButtonText.text = "remove";
        }

        private void SetInvitingState()
        {
            _removeFriendButton.gameObject.SetActive(true);
            _removeFriendButtonText.text = "reject";
            _acceptInviteButton.gameObject.SetActive(true);
        }
        #endregion

        #region opening/closing panel
        /// <summary>
        /// Toggle if panel is opened or closed
        /// </summary>
        private void TogglePanel()
        {
            if (_panelOpened)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        /// <summary>
        /// Plays animation for opening panel
        /// </summary>
        private void OpenPanel()
        {
            _animator.SetTrigger("Open");
            _panelOpened = true;
        }

        /// <summary>
        /// Plays animation for closing
        /// </summary>
        private void ClosePanel()
        {
            _animator.SetTrigger("Close");
            _panelOpened = false;
        }

        /// <summary>
        /// Close without animation
        /// </summary>
        public void ClosePanelImmediately()
        {
            _bottomPanel.sizeDelta = new Vector2(_bottomPanel.sizeDelta.x, 0);
            _panelOpened = false;
        }
        #endregion

        #endregion
    }

}