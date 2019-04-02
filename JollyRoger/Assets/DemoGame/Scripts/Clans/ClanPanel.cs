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
using DemoGame.Scripts.Chat;
using DemoGame.Scripts.Menus;
using DemoGame.Scripts.Notifications;
using DemoGame.Scripts.Profile;
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Clans
{

    /// <summary>
    /// Responsible for managing UI of the Clan managing panel.
    /// Holds references to all UI elements and handles user input.
    /// </summary>
    public class ClanPanel : Menu
    {
        #region Fields

        #region ClanManagement

        [Space]
        /// <summary>
        /// Button for creating new clan using <see cref="CreateClan"/>.
        /// </summary>
        [SerializeField] private Button _createClanButton = null;

        /// <summary>
        /// Parent panel containing clan creation UI.
        /// </summary>
        [SerializeField] private ClanCreationPanel _clanCreationPrefab = null;

        /// <summary>
        /// Button for deleting clan using <see cref="DeleteClan"/>.
        /// </summary>
        [SerializeField] private Button _removeClanButton = null;

        /// <summary>
        /// Button used to resend clan member list and search list entries.
        /// </summary>
        [SerializeField] private Button _refreshClanButton = null;

        #endregion

        #region ClanAssociation

        [Space]
        /// <summary>
        /// Button for joining currently shown clan using <see cref="JoinClan"/>.
        /// </summary>
        [SerializeField] private Button _joinClanButton = null;

        /// <summary>
        /// Button for leaving currently shown clan using <see cref="LeaveClan"/>.
        /// </summary>
        [SerializeField] private Button _leaveClanButton = null;

        /// <summary>
        /// Button responsible for joining clan chat.
        /// </summary>
        [SerializeField] private Button _chatButton = null;


        /// <summary>
        /// List of all members of currently show clan.
        /// </summary>
        private List<ClanUserEntry> _clanMembers = new List<ClanUserEntry>();

        /// <summary>
        /// Currently selected member of a clan.
        /// </summary>
        private ClanUserEntry _selectedMember = null;

        /// <summary>
        /// Name of currently displayed clan.
        /// </summary>
        private string _clanName = string.Empty;

        /// <summary>
        /// Id of currently displayed clan.
        /// </summary>
        private string _clanId = string.Empty;

        #endregion

        #region Search

        [Space]
        /// <summary>
        /// Textbox containing keyword used to search for specific clans.
        /// </summary>
        [SerializeField] private InputField _clanSearchInput = null;

        /// <summary>
        /// Button for starting clan search.
        /// </summary>
        [SerializeField] private Button _clanSearchButton = null;

        /// <summary>
        /// Max number of clans displayed per page.
        /// </summary>
        [SerializeField] private int _clansPerPage = 5;

        /// <summary>
        /// Prefab with clan search result entry UI and logic.
        /// Instantiated for every entry found.
        /// </summary>
        [SerializeField] private ClanSearchResult _clanSearchResultPrefab = null;

        /// <summary>
        /// Parent list containing all search entries.
        /// </summary>
        [SerializeField] private RectTransform _clanSearchList = null;

        #endregion

        #region ClanDetails

        [Space]
        /// <summary>
        /// Prefab with user information and UI.
        /// Instantiated for every user in currently displayed clan.
        /// </summary>
        [SerializeField] private ClanUserEntry _clanUserEntryPrefab = null;

        /// <summary>
        /// Parent list containing all users who belong to currently displayed clan.
        /// </summary>
        [SerializeField] private RectTransform _clanUserList = null;


        [Space]
        /// <summary>
        /// Textbox containing the name of the clan we currently display information of.
        /// </summary>
        [SerializeField] private Text _clanDisplayName = null;

        /// <summary>
        /// Parent transform containing clan search UI.
        /// Tabs are changed using <see cref="ShowTab(CanvasGroup)"/> and <see cref="HideTab(CanvasGroup)"/>.
        /// </summary>
        [SerializeField] private CanvasGroup _searchTab = null;

        /// <summary>
        /// Button used to show <see cref="_searchTab"/> using <see cref="ShowTab(CanvasGroup)"/>.
        /// </summary>
        [SerializeField] private Button _searchTabButton = null;

        /// <summary>
        /// Parent transform containing clan details UI.
        /// Tabs are changed using <see cref="ShowTab(CanvasGroup)"/> and <see cref="HideTab(CanvasGroup)"/>.
        /// </summary>
        [SerializeField] private CanvasGroup _detailsTab = null;

        /// <summary>
        /// Button used to show <see cref="_detailsTab"/> using <see cref="ShowTab(CanvasGroup)"/>.
        /// </summary>
        [SerializeField] private Button _detailsTabButton = null;

        #endregion

        #region Chat

        /// <summary>
        /// Chat panel reference used to send and receive clan messages.
        /// </summary>
        [SerializeField] private ChatChannelClanUI _chatChannelClanUI = null;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Currently displayed clan in <see cref="_detailsTab"/> panel.
        /// </summary>
        private IApiGroup DisplayedClan { get; set; }

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
        /// Adds handlers to buttons.
        /// Restarts clan display and shows <see cref="_searchTab"/> panel.
        /// Awaits Nakama session initialization - on success calls <see cref="Instance_OnConnectionSuccess(Client, ISession)"/>.
        /// </summary>
        private void Awake()
        {
            _createClanButton.onClick.AddListener(() => _clanCreationPrefab.ShowCreationPanel(OnClanChanged));
            _removeClanButton.onClick.AddListener(DeleteClan);
            _clanSearchButton.onClick.AddListener(SearchClan);
            _leaveClanButton.onClick.AddListener(LeaveClan);
            _joinClanButton.onClick.AddListener(JoinClan);
            _searchTabButton.onClick.AddListener(ShowClanSearch);
            _detailsTabButton.onClick.AddListener(ShowMyClanDetails);
            _chatButton.onClick.AddListener(StartChat);
            _refreshClanButton.onClick.AddListener(RefreshClanMenu);
            _clanSearchInput.onEndEdit.AddListener(SearchClanOnReturnClicked);

            SetMyClan(null);
            ShowClanSearch();
            NotificationManager.Instance.OnNotification += NotificationReceived;
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

        #region ClanManagement

        /// <summary>
        /// Deletes the clan local user currently belongs to.
        /// Does nothing if user is not a member of any clan or has insufficient permissions.
        /// The name of newly created clan is determined by <see cref="_clanCreateName"/> textfield.
        /// </summary>
        private async void DeleteClan()
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            bool good = await ClanManager.DeleteClanAsync(Client, Session, clan.Group);
            if (good == true)
            {
                OnClanLeft();
            }
        }

        /// <summary>
        /// Invoked by <see cref="ClanSearchResult._joinClanButton"/>.
        /// Joins selected clan.
        /// If user is already member of this clan, changes tab to <see cref="_detailsTab"/>.
        /// If user is already member of another clan, does nothing.
        /// </summary>
        private async void JoinClan()
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan != null)
            {
                if (clan.Group.Id == DisplayedClan.Id)
                {
                    Debug.Log("This user has already joined clan with name \"" + DisplayedClan.Name + "\"");
                    ShowClanDetails(DisplayedClan);
                }
                else
                {
                    Debug.LogWarning("Cannot join more then one clan. Leave current clan first.");
                }
            }
            else
            {
                IApiGroup newClan = await ClanManager.JoinClanAsync(Client, Session, DisplayedClan);
                if (newClan != null)
                {
                    OnClanChanged(newClan);
                }
            }
        }

        /// <summary>
        /// Leaves current clan.
        /// Invokes <see cref="OnClanLeft"/> on success.
        /// </summary>
        private async void LeaveClan()
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan == null)
            {
                Debug.Log("User is not a member of any clan");
                return;
            }

            bool good = await ClanManager.LeaveClanAsync(Client, Session, clan.Group);
            if (good == true)
            {
                OnClanLeft();
            }
        }

        /// <summary>
        /// Searches Nakama database in order to find clans containing a keyword in their names.
        /// The keyword is determined by <see cref="_clanSearchText"/> textfield.
        /// </summary>
        private async void SearchClan()
        {
            string name = _clanSearchInput.text;
            IApiGroupList clanList = await ClanManager.ListClansAsync(Client, Session, "%" + name + "%", _clansPerPage, null);
            if (clanList != null)
            {
                OnClanListFound(clanList);
            }
        }

        /// <summary>
        /// Invoked whenever user ends search keyword input with Return key.
        /// Searches Nakama database in order to find clans containing a keyword in their names.
        /// The keyword is determined by <see cref="_clanSearchText"/> textfield.
        /// </summary>
        private void SearchClanOnReturnClicked(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SearchClan();
            }
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Called when user connects to Nakama server.
        /// Tries to retrieve the clan local user belongs to.
        /// On success calls <see cref="OnClanChanged(IApiGroup)"/>.
        /// </summary>
        private async void Init()
        {
            NakamaSessionManager.Instance.OnConnectionSuccess -= Init;
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan != null)
            {
                OnClanChanged(clan.Group);
            }
        }

        /// <summary>
        /// Invoked when user changes their clan.
        /// Sets the <see cref="MyClan"/> of local user.
        /// If <paramref name="clan"/> is null, shows <see cref="_detailsTab"/>, else
        /// shows <see cref="_searchTab"/>.
        /// </summary>
        /// <param name="clan"></param>
        private void OnClanChanged(IApiGroup clan)
        {
            Debug.Log("Clan changed");
            SetMyClan(clan);
            if (clan == null)
            {
                ShowClanSearch();
            }
            else
            {
                ShowClanDetails(clan);
            }
        }

        /// <summary>
        /// Invoked when leaving a clan.
        /// </summary>
        public void OnClanLeft()
        {
            Debug.Log("Clan left");
            OnClanChanged(null);
            SetMyState(ClanUserState.None);
        }

        /// <summary>
        /// Invoked when server returns a list of clans we searched for.
        /// Parameter <paramref name="cursor"/> is used to iterate through results.
        /// </summary>
        private void OnClanListFound(IApiGroupList clans)
        {
            // Removing previous results
            foreach (Transform transform in _clanSearchList)
            {
                Destroy(transform.gameObject);
            }
            if (clans.Groups.Count() > 0)
            {
                // Creating entiries for every clan found
                foreach (IApiGroup clan in clans.Groups)
                {
                    Debug.Log("Found clan: " + clan.Name);
                    ClanSearchResult result = Instantiate(_clanSearchResultPrefab, _clanSearchList);
                    result.SetClan(clan, ShowClanDetails);
                }
            }
            else
            {
                Debug.Log("No clans found");
            }
        }

        /// <summary>
        /// Updates user list based on currently <see cref="DisplayedClan"/> 
        /// using <see cref="OnClanUserListReceived(List{IGroupUserListGroupUser})"/>.
        /// Invokes <paramref name="onEnded"/> on succes or failure.
        /// </summary>
        private async Task<bool> UpdateUserListAsync()
        {
            List<IGroupUserListGroupUser> userList = await ClanManager.GetClanUsersAsync(Client, Session, DisplayedClan);
            if (userList != null)
            {
                OnClanUserListReceived(userList);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Updates the list of members of a clan.
        /// Instantiates UI containing found user data.
        /// If local user is a part of this clan, updates their <see cref="MyUserState"/>
        /// using <see cref="SetMyState(ClanUserState)"/>.
        /// </summary>
        private void OnClanUserListReceived(List<IGroupUserListGroupUser> userList)
        {
            _clanMembers.Clear();
            // Removing previous results
            foreach (Transform child in _clanUserList)
            {
                Destroy(child.gameObject);
            }

            // Searching through results in order to find local user.
            // If local user belongs to this clan, set their MyUserState
            // Knowing the role of local user in searched clan allows for better UI handling
            ClanUserState myState = ClanUserState.None;
            foreach (IGroupUserListGroupUser user in userList)
            {
                if (user.User.Id == NakamaSessionManager.Instance.Account.User.Id)
                {
                    myState = (ClanUserState)user.State;
                    SetMyState(myState);
                    break;
                }
            }

            // Adding entries for each user found
            foreach (IGroupUserListGroupUser user in userList)
            {
                Debug.Log("Found user in clan: " + user.User.Username);
                ClanUserEntry userEntry = Instantiate(_clanUserEntryPrefab, _clanUserList);
                userEntry.SetUser(user.User, (ClanUserState)user.State, myState, OnUserSelected, OnUserKick, OnUserPromote, OnUserShowProfile);
                _clanMembers.Add(userEntry);
            }
        }

        /// <summary>
        /// Invoked upon selecting user from clan member list.
        /// Shows user interaction panel.
        /// </summary>
        private void OnUserSelected(ClanUserEntry sender)
        {
            if (_selectedMember == sender)
            {
                _selectedMember.HideInteractionPanel();
                _selectedMember = null;
            }
            else
            {
                if (_selectedMember != null)
                {
                    _selectedMember.HideInteractionPanel();
                }
                _selectedMember = sender;
                _selectedMember.ShowInteractionPanel();
            }

        }

        /// <summary>
        /// Method invoked by clicking on <see cref="ClanUserEntry._kickButton"/>.
        /// Removes user from current clan and updates user list.
        /// </summary>
        private async void OnUserKick(IApiUser user)
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan == null)
            {
                Debug.Log("Not a member of any clan");
            }
            Debug.Log("Kicked user " + user.Username);
            bool good = await ClanManager.KickUserAsync(Client, Session, user, clan.Group);
            if (good == true)
            {
                await UpdateUserListAsync();
            }
        }

        /// <summary>
        /// Method invoked by clicking on <see cref="ClanUserEntry._promoteButton"/>.
        /// Promotes the user to higher <see cref="ClanUserState"/>.
        /// </summary>
        private async void OnUserPromote(IApiUser user)
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan == null)
            {
                Debug.Log("Not a member of any clan");
            }
            Debug.Log("Promoted user " + user.Username);
            bool good = await ClanManager.PromoteUserAsync(Client, Session, user, clan.Group);
            if (good == true)
            {
                await UpdateUserListAsync();
            }
        }

        /// <summary>
        /// Method invoked by clicking on <see cref="ClanUserEntry._profileButton"/>.
        /// Shows <see cref="ProfilePanel"/> filled with <paramref name="user"/> data.
        /// </summary>
        private void OnUserShowProfile(IApiUser user)
        {
            Debug.Log("Showed user " + user.Username + "'s profile");
            ProfilePopup.Instance.Show(user);
        }

        /// <summary>
        /// Refreshes user list when a member joins or leaves the clan.
        /// Changes tabs to clan search when current clan is disbanded.
        /// </summary>
        private void NotificationReceived(IApiNotification notification)
        {
            if (notification.Code == (int)NotificationCode.Clan_RefreshMembers)
            {
                RefreshClanMenu();
            }
            if (notification.Code == (int)NotificationCode.Clan_Delete)
            {
                OnClanLeft();
            }
        }

        #endregion

        #region UIManagement

        /// <summary>
        /// Update clan menu on clan panel enter.
        /// </summary>
        public override void Show()
        {
            Init();
            base.Show();
        }

        /// <summary>
        /// Requests clan search list and clan member list refresh.
        /// </summary>
        public async void RefreshClanMenu()
        {
            SearchClan();
            bool good = await UpdateUserListAsync();
            if (good == false)
            {
                IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
                if (clan == null)
                {
                    OnClanChanged(null);
                }
            }
        }

        /// <summary>
        /// Shows <see cref="_searchTab"/> panel and hides <see cref="_detailsTab"/> panel.
        /// </summary>
        private void ShowClanSearch()
        {
            HideTab(_detailsTab);
            ShowTab(_searchTab);
            _searchTabButton.interactable = false;
            _detailsTabButton.interactable = true;
        }

        /// <summary>
        /// Shows <see cref="_detailsTab"/> panel and hides <see cref="_searchTab"/> panel.
        /// Updates user list.
        /// </summary>
        private async void ShowClanDetails(IApiGroup clan)
        {
            DisplayedClan = clan;
            bool good = await UpdateUserListAsync();
            if (good == true)
            {
                await SetClanManagementButtonsAsync();
                HideTab(_searchTab);
                ShowTab(_detailsTab);
                _clanDisplayName.text = DisplayedClan.Name;
            }
            _searchTabButton.interactable = true;
            _detailsTabButton.interactable = false;
        }

        /// <summary>
        /// Shows <see cref="_detailsTab"/> panel and hides <see cref="_searchTab"/> panel.
        /// Displays local user clan info and updates user list.
        /// </summary>
        private async void ShowMyClanDetails()
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan != null)
            {
                ShowClanDetails(clan.Group);
            }
        }

        /// <summary>
        /// Hides panel setting its alpha to 0 and doesn't block raycasts.
        /// </summary>
        private void HideTab(CanvasGroup tab)
        {
            tab.alpha = 0;
            tab.blocksRaycasts = false;
        }

        /// <summary>
        /// Shows panel setting its alpha to 1 and blocks raycasts.
        /// </summary>
        private void ShowTab(CanvasGroup tab)
        {
            tab.alpha = 1;
            tab.blocksRaycasts = true;
        }

        /// <summary>
        /// Activates or deactivates buttons responsible for leaving, joining and removing clan
        /// depending on whether we belong to currently displayed clan.
        /// </summary>
        private async Task SetClanManagementButtonsAsync()
        {
            IUserGroupListUserGroup clan = await ClanManager.GetUserClanAsync(Client, Session);
            if (clan != null && DisplayedClan.Id == clan.Group.Id)
            {
                _joinClanButton.gameObject.SetActive(false);
                _leaveClanButton.gameObject.SetActive(true);
                _removeClanButton.gameObject.SetActive(clan.State == (int)ClanUserState.Superadmin);
                _chatButton.gameObject.SetActive(true);
            }
            else
            {
                _joinClanButton.gameObject.SetActive(true);
                _leaveClanButton.gameObject.SetActive(false);
                _removeClanButton.gameObject.SetActive(false);
                _chatButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the value of <see cref="MyClan"/>.
        /// If <paramref name="clan"/> is null, disable option to see <see cref="_detailsTab"/> of my clan.
        /// </summary>
        private void SetMyClan(IApiGroup clan)
        {
            if (clan != null)
            {
                _clanId = clan.Id;
                _clanName = clan.Name;
                _clanDisplayName.text = clan.Name;
                _detailsTabButton.interactable = true;
                _createClanButton.interactable = false;
            }
            else
            {
                _clanDisplayName.text = "Clan Panel";
                _detailsTabButton.interactable = false;
                _createClanButton.interactable = true;
            }
        }

        /// <summary>
        /// Changes the <see cref="MyUserState"/>.
        /// If user has <see cref="ClanUserState.Superadmin"/> privileges, this enables <see cref="_removeClanButton"/>.
        /// </summary>
        private void SetMyState(ClanUserState state)
        {
            if (state == ClanUserState.Superadmin)
            {
                _removeClanButton.gameObject.SetActive(true);
            }
            else
            {
                _removeClanButton.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Chat

        /// <summary>
        ///  Shows clan chat panel if possible.
        /// </summary>
        private async void StartChat()
        {
            ChatChannel chatChannel = await ChatManager.Instance.JoinChatWithGroupAsync(_clanId);

            if (chatChannel != null)
            {
                chatChannel.ChannelName = _clanName;
                _chatChannelClanUI.SetChatChannel(chatChannel);
                _chatChannelClanUI.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("Couldn't start chat with clan");
            }
        }

        #endregion
    }

}