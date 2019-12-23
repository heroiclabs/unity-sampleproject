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

using UnityEngine;
using UnityEngine.UI;
using Nakama;
using DemoGame.Scripts.Menus;
using DemoGame.Scripts.Chat;
using DemoGame.Scripts.Session;

namespace DemoGame.Scripts.Friends
{

    public class FriendsListUI : Menu
    {
        [Space]
        /// <summary>
        /// Prefab of friend panel which is instantiated when refreshing list
        /// </summary>
        [SerializeField] private GameObject _friendPanelPrefab = null;

        /// <summary>
        /// Reference of scrollRect for changing its content basing on currently selected tab
        /// </summary>
        [SerializeField] private ScrollRect _scrollRect = null;

        /// <summary>
        /// Button used for manual refreshing list
        /// </summary>
        [SerializeField] private Button _refreshButton = null;

        /// <summary>
        /// Button used for sending friend invitates to users
        /// </summary>
        [SerializeField] private Button _addFriendButton = null;

        /// <summary>
        /// Reference to chatChannelUI object for starting chat with user
        /// </summary>
        [SerializeField] private ChatChannelUI _chatChannelUI = null;

        [SerializeField] private UsernameSearcher _usernameSearcher = null;

        [Header("Tabs buttons")]
        [SerializeField] private Button _friendsTabButton = null;
        [SerializeField] private Button _sentInvitesTabButton = null;
        [SerializeField] private Button _receivedInvitesTabButton = null;
        [SerializeField] private Button _bannedUsersTabButton = null;

        [Header("Content panels")]
        [SerializeField] private RectTransform _friendsContent = null;
        [SerializeField] private RectTransform _sentInvitesContent = null;
        [SerializeField] private RectTransform _receivedInvitesContent = null;
        [SerializeField] private RectTransform _bannedUsersContent = null;

        /// <summary>
        /// Content of currently selected tab
        /// </summary>
        private RectTransform _currentTabContent;

        /// <summary>
        /// Button for currently selected tab
        /// </summary>
        private Button _currentTabButton;

        /// <summary>
        /// Currently selected friend panel
        /// </summary>
        private FriendPanel _selectedFriendPanel;

        #region Mono
        private void Awake()
        {
            //Checking if nakama session manager is connected to server
            if (NakamaSessionManager.Instance.IsConnected)
            {
                //if is - inintializing friends list
                Init();
            }
            else
            {
                //if not - plugging to event OnConnectionSuccess initializing of friends list
                NakamaSessionManager.Instance.OnConnectionSuccess += Init;
            }
            SetBackButtonHandler(MenuManager.Instance.HideTopMenu);

            //selecting friends tab automatically
            FriendsTabButtonClicked();

            //connecting methods to button clicks
            _friendsTabButton.onClick.AddListener(FriendsTabButtonClicked);
            _sentInvitesTabButton.onClick.AddListener(SentInvitesTabButtonClicked);
            _receivedInvitesTabButton.onClick.AddListener(ReceivedInvitesTabButtonClicked);
            _bannedUsersTabButton.onClick.AddListener(BannedUsersTabButtonClicked);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Initializes friends view
        /// </summary>
        /// <param name="client"></param>
        /// <param name="session"></param>
        private async void Init()
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            //loading friends data
            var friends = await FriendsManager.LoadFriendsAsync(client, session);
            if (friends != null)
            {
                //refreshing ui view with downloaded data
                RefreshFriendsListUI(friends);
                NakamaSessionManager.Instance.OnConnectionSuccess -= Init;
            }

            //connecting events that need server connection
            _addFriendButton.onClick.AddListener(AddFriend);
            _usernameSearcher.OnSubmit += AddFriend;
            _refreshButton.onClick.AddListener(ActualizeFriendsList);
        }

        /// <summary>
        /// Populating friend list with elements created basing on loaded friends list from database
        /// </summary>
        private void RefreshFriendsListUI(IApiFriendList friends)
        {
            ClearLists();
            foreach (IApiFriend friend in friends.Friends)
            {
                RectTransform content;

                //selecting to which tab should friend panel be instantiated
                switch (friend.State)
                {
                    //  Users are friends with each other.
                    case 0: content = _friendsContent; break;
                    //  This user has sent an invitation and pending acceptance from other user.
                    case 1: content = _sentInvitesContent; break;
                    //  This user has received an invitation but has not accepted yet.
                    case 2: content = _receivedInvitesContent; break;
                    //  This user has banned other user.
                    case 3: content = _bannedUsersContent; break;
                    //  If state is none of upper log error
                    default: Debug.LogError("Wrong friend state value: \"" + friend.State + "\" in " + friend.User.Username + "!"); return;
                }
                //instantiating friend panel object
                GameObject panelGO = Instantiate(_friendPanelPrefab, content) as GameObject;
                FriendPanel panel = panelGO.GetComponent<FriendPanel>();
                if (panel)
                {
                    //initializing object with FriendList object and friend data
                    panel.Init(friend);
                    panel.OnSelected += SelectedPanelChange;
                    //subscribing to event fired after every successful request to friends list in database
                    panel.OnDataChanged += ActualizeFriendsList;
                    panel.OnChatStartButtonClicked += StartChatWithUser;
                }
                else
                {
                    Debug.LogError("Invalid friend panel prefab!");
                    Destroy(panelGO);
                }
            }
        }

        /// <summary>
        /// Loading friends from database and refreshing list
        /// </summary>
        private async void ActualizeFriendsList()
        {
            //loading data from server
            var friends = await FriendsManager.LoadFriendsAsync(NakamaSessionManager.Instance.Client, NakamaSessionManager.Instance.Session);
            if (friends != null)
            {
                //refreshing viev
                RefreshFriendsListUI(friends);
            }
        }

        /// <summary>
        /// Removing all instantiated panels on all lists
        /// </summary>
        private void ClearLists()
        {
            ClearList(_friendsContent);
            ClearList(_sentInvitesContent);
            ClearList(_receivedInvitesContent);
            ClearList(_bannedUsersContent);
        }

        /// <summary>
        /// Removing instantiated friend panels from list 
        /// </summary>
        private void ClearList(RectTransform content)
        {
            FriendPanel[] friendPanels = content.GetComponentsInChildren<FriendPanel>();
            for (int i = 0; i < friendPanels.Length; i++)
            {
                Destroy(friendPanels[i].gameObject);
            }
        }

        /// <summary>
        /// Sends request for adding friend by using FriendsList.AddFriendByUsernameAsync
        /// </summary>
        public async void AddFriend()
        {
            bool success = await FriendsManager.AddFriendByUsernameAsync(_usernameSearcher.InputFieldValue, NakamaSessionManager.Instance.Client, NakamaSessionManager.Instance.Session);
            if (success)
            {
                Debug.Log("friend added");
                ActualizeFriendsList();
            }
            else
            {
                Debug.LogWarning("friend adding error");
            }
        }

        /// <summary>
        /// Used when new friend panel is selected and we need to close old
        /// </summary>
        /// <param name="friendPanel"></param>
        private void SelectedPanelChange(FriendPanel friendPanel)
        {
            if (_selectedFriendPanel == friendPanel)
            {
                return;
            }
            DeselectCurrentPanel();
            _selectedFriendPanel = friendPanel;
        }

        private void DeselectCurrentPanel(bool closeOldPanelImmediately = false)
        {
            if (_selectedFriendPanel)
            {
                _selectedFriendPanel.Deselect(true);
            }
            _selectedFriendPanel = null;
        }

        private async void StartChatWithUser(string userId, string username)
        {
            ChatChannel chatChannel = await ChatManager.Instance.JoinChatWithUserAsync(userId);

            if (chatChannel != null)
            {
                chatChannel.ChannelName = username;
                _chatChannelUI.SetChatChannel(chatChannel);
                _chatChannelUI.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Couldn't start chat with user");
            }
        }

        #region Tab buttons click handlers

        private void SelectTab(RectTransform content, Button button)
        {
            //return if tab already selected

            if (_currentTabContent == content)
            {
                return;
            }

            DeselectCurrentPanel(true);

            //deselecting current tab

            if (_currentTabContent)
            {
                _currentTabContent.gameObject.SetActive(false);
            }
            if (_currentTabButton)
            {
                _currentTabButton.interactable = true;
            }

            //selecting new tab

            content.gameObject.SetActive(true);
            _currentTabContent = content;

            button.interactable = false;
            _currentTabButton = button;

            _scrollRect.content = _currentTabContent;
        }

        /// <summary>
        /// Selects friend tab
        /// </summary>
        private void FriendsTabButtonClicked()
        {
            SelectTab(_friendsContent, _friendsTabButton);
        }

        /// <summary>
        /// Selects sent invites tab
        /// </summary>
        private void SentInvitesTabButtonClicked()
        {
            SelectTab(_sentInvitesContent, _sentInvitesTabButton);
        }

        /// <summary>
        /// Selects received invites tab
        /// </summary>
        private void ReceivedInvitesTabButtonClicked()
        {
            SelectTab(_receivedInvitesContent, _receivedInvitesTabButton);
        }

        /// <summary>
        /// Selects banned users tab
        /// </summary>
        private void BannedUsersTabButtonClicked()
        {
            SelectTab(_bannedUsersContent, _bannedUsersTabButton);
        }
        #endregion

        #endregion
    }

}