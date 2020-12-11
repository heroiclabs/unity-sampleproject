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
    /// Responsible for managing UI of the Clan managing panel.
    /// Holds references to all UI elements and handles user input.
    /// </summary>
    public class ClansMenuUI : Menu
	{
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

		[Space]
		/// <summary>
		/// Button for joining currently shown clan using <see cref="JoinDisplayedClan"/>.
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
		/// Prefab with clan search result entry UI and logic.
		/// Instantiated for every entry found.
		/// </summary>
		[SerializeField] private ClanSearchResult _clanSearchResultPrefab = null;

		/// <summary>
		/// Parent list containing all search entries.
		/// </summary>
		[SerializeField] private RectTransform _clanSearchList = null;

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
		/// Tabs are changed using <see cref="ShowSubMenu(CanvasGroup)"/> and <see cref="HideSubMenu(CanvasGroup)"/>.
		/// </summary>
		[SerializeField] private CanvasGroup _searchTab = null;

		/// <summary>
		/// Button used to show <see cref="_searchTab"/> using <see cref="ShowSubMenu(CanvasGroup)"/>.
		/// </summary>
		[SerializeField] private Button _searchTabButton = null;

		/// <summary>
		/// Parent transform containing clan details UI.
		/// Tabs are changed using <see cref="ShowSubMenu(CanvasGroup)"/> and <see cref="HideSubMenu(CanvasGroup)"/>.
		/// </summary>
		[SerializeField] private CanvasGroup _detailsTab = null;

		/// <summary>
		/// Button used to show <see cref="_detailsTab"/> using <see cref="ShowSubMenu(CanvasGroup)"/>.
		/// </summary>
		[SerializeField] private Button _detailsTabButton = null;

		/// <summary>
		/// Chat panel reference used to send and receive clan messages.
		/// </summary>
		[SerializeField] private ChatChannelClanUI _chatChannelClanUI = null;

		private GameConnection _connection;
		private ProfilePopup _profilePopup;
		private readonly ClanMenuUIState _state = new ClanMenuUIState();

		public void Init(GameConnection connection, ProfilePopup profilePopup)
		{
			_connection = connection;
			_profilePopup = profilePopup;
			_chatChannelClanUI.Init(_connection);
		}

		/// <summary>
		/// Adds handlers to buttons.
		/// Restarts clan display and shows <see cref="_searchTab"/> panel.
		/// Awaits Nakama session initialization - on success calls <see cref="Instance_OnConnectionSuccess(Client, ISession)"/>.
		/// </summary>
		private void Awake()
		{
			_clanCreationPrefab.OnClanCreated += clan => {
				_state.UserClan = clan;
				_state.SubMenu = ClanSubMenu.Details;
				RefreshUI(_state);
			};

			_backButton.onClick.AddListener(() => Hide());
			_createClanButton.onClick.AddListener(() =>
			{
				_clanCreationPrefab.Show();
			});

			_removeClanButton.onClick.AddListener(DeleteClan);
			_clanSearchButton.onClick.AddListener(SearchClan);
			_leaveClanButton.onClick.AddListener(LeaveClan);
			_joinClanButton.onClick.AddListener(JoinDisplayedClan);
			_searchTabButton.onClick.AddListener(() => {
				_state.DisplayedClan = null;
				_state.SubMenu = ClanSubMenu.Search;
				RefreshUI(_state);
			});
			_detailsTabButton.onClick.AddListener(() => {
				_state.SubMenu = ClanSubMenu.Details;
				RefreshUI(_state);
			});
			_chatButton.onClick.AddListener(() => StartChat(_state));
			_refreshClanButton.onClick.AddListener(SearchClan);
			_clanSearchInput.onEndEdit.AddListener(SearchClanOnReturnClicked);
		}

		/// <summary>
		/// Deletes the clan local user currently belongs to.
		/// Does nothing if user is not a member of any clan or has insufficient permissions.
		/// The name of newly created clan is determined by <see cref="_clanCreateName"/> textfield.
		/// </summary>
		private async void DeleteClan()
		{
			try
			{
				await _connection.Client.DeleteGroupAsync(_connection.Session, _state.UserClan.Id);
			}
			catch (ApiResponseException e)
			{
				Debug.LogError("Could not delete clan: " + e.Message);
			}

			_state.DisplayedClan = null;
			_state.UserClan = null;
			_state.UserClanRank = null;

			RefreshUI(_state);
		}

		/// <summary>
		/// Invoked by <see cref="ClanSearchResult._joinClanButton"/>.
		/// Joins selected clan.
		/// If user is already member of this clan, changes tab to <see cref="_detailsTab"/>.
		/// If user is already member of another clan, does nothing.
		/// </summary>
		private async void JoinDisplayedClan()
		{
			try
			{
				await _connection.Client.JoinGroupAsync(_connection.Session, _state.DisplayedClan.Id);
				_state.UserClan = _state.DisplayedClan;
				RefreshUI(_state);
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("An exception has occured when joining clan with code " + e.StatusCode + ": " + e.Message);
			}
            catch (Exception e)
			{
				Debug.LogWarning("An interface exception has occured when joining clan " + e.Message);
			}
		}

		/// <summary>
		/// Leaves current clan.
		/// Invokes <see cref="OnClanLeft"/> on success.
		/// </summary>
		private async void LeaveClan()
		{
			try
			{
				await _connection.Client.LeaveGroupAsync(_connection.Session, _state.DisplayedClan.Id);
				_state.UserClan = null;
				_state.SubMenu = ClanSubMenu.Search;
				RefreshUI(_state);
			}
            catch(ApiResponseException e)
            {
				Debug.LogWarning("An API exception has occured when leaving clan. Code: " + e.StatusCode + ", Message: " + e.Message);
            }
			catch (Exception e)
			{
				Debug.LogWarning("An interface exception has occured when leaving clan: " + e.Message);
			}
		}

		/// <summary>
		/// Searches Nakama database in order to find clans containing a keyword in their names.
		/// The keyword is determined by <see cref="_clanSearchText"/> textfield.
		/// </summary>
		private async void SearchClan()
		{
			string name = _clanSearchInput.text;
			try
			{
				IApiGroupList clanList = await _connection.Client.ListGroupsAsync(_connection.Session, name);
				OnClanListFound(clanList);
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("An exception has occured when searching clans: " + e);
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

        public override async void Show(bool isMuteButtonClick = false)
        {
			base.Show(isMuteButtonClick);

			try
			{
				IApiUserGroupList groupList = await _connection.Client.ListUserGroupsAsync(_connection.Session);

				foreach (var group in groupList.UserGroups)
				{
					_state.UserClan = group.Group;
					_state.UserClanRank = group.State;
					_state.DisplayedClan = _state.UserClan;
					_state.SubMenu = ClanSubMenu.Details;
					// user can only belong to one clan in this game
					break;
				}
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Error fetching user clan: " + e.Message);
			}

			RefreshUI(_state);

			_connection.Socket.ReceivedNotification += NotificationReceived;
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

			if (clans.Groups.Any())
			{
				// Creating entiries for every clan found
				foreach (IApiGroup clan in clans.Groups)
				{
					ClanSearchResult result = Instantiate(_clanSearchResultPrefab, _clanSearchList);
					result.SetClan(clan, clickedClan =>
					{
						_state.DisplayedClan = clickedClan;
						_state.SubMenu = ClanSubMenu.Details;
						RefreshUI(_state);
					});
				}
			}
		}

		/// <summary>
		/// Updates the list of members of a clan.
		/// Instantiates UI containing found user data.
		/// If local user is a part of this clan, updates their <see cref="MyUserState"/>
		/// using <see cref="SetMyState(ClanUserState)"/>.
		/// </summary>
		private void OnClanUserListReceived(IEnumerable<IGroupUserListGroupUser> userList)
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
			foreach (IGroupUserListGroupUser user in userList)
			{
				if (user.User.Id == _connection.Account.User.Id)
				{
					_state.UserClanRank = user.State;
					break;
				}
			}

			// Adding entries for each user found
			foreach (IGroupUserListGroupUser user in userList)
			{
				ClanUserEntry userEntry = Instantiate(_clanUserEntryPrefab, _clanUserList);
				userEntry.Init(_connection.Session.UserId);
				userEntry.SetUser(user.User, user.State, _state.UserClanRank.Value, OnUserSelected, OnUserKick, OnUserPromote, OnUserShowProfile);
				_clanMembers.Add(userEntry);
			}

			RefreshUI(_state);

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
		private async void OnUserKick(IApiUser kickedUser)
		{
			try
			{
				await _connection.Client.KickGroupUsersAsync(_connection.Session, _state.UserClan.Id, new string[] { kickedUser.Id });
				var userEnumeration = await _connection.Client.ListGroupUsersAsync(_connection.Session, _state.UserClan.Id, null, 1, null);
				OnClanUserListReceived(userEnumeration.GroupUsers);
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("An exception has occured when kicking user " + e.Message);
			}
		}

		/// <summary>
		/// Method invoked by clicking on <see cref="ClanUserEntry._promoteButton"/>.
		/// Promotes the user to higher <see cref="ClanUserState"/>.
		/// </summary>
		private async void OnUserPromote(IApiUser user)
		{
			try
			{
				await _connection.Client.PromoteGroupUsersAsync(_connection.Session, _state.UserClan.Id, new string[] { user.Id });
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("An exception has occured when promoting user " + user.Username
						+ " in clan " + _state.UserClan.Name + ": " + e);
			}
		}

		/// <summary>
		/// Method invoked by clicking on <see cref="ClanUserEntry._profileButton"/>.
		/// Shows <see cref="ProfilePanel"/> filled with <paramref name="user"/> data.
		/// </summary>
		private void OnUserShowProfile(IApiUser user)
		{
			_profilePopup.Show(user);
		}

		/// <summary>
		/// Refreshes user list when a member joins or leaves the clan.
		/// Changes tabs to clan search when current clan is disbanded.
		/// </summary>
		private void NotificationReceived(IApiNotification notification)
		{
			if (notification.Code == (int)NotificationCode.Clan_RefreshMembers)
			{
				SearchClan();
			}
			if (notification.Code == (int)NotificationCode.Clan_Delete)
			{
				_state.DisplayedClan = null;
				_state.UserClan = null;
				_state.UserClanRank = null;

				RefreshUI(_state);
			}
		}

		/// <summary>
		/// Hides panel setting its alpha to 0 and doesn't block raycasts.
		/// </summary>
		private void HideSubMenu(CanvasGroup tab)
		{
			tab.alpha = 0;
			tab.blocksRaycasts = false;
		}

		/// <summary>
		/// Shows panel setting its alpha to 1 and blocks raycasts.
		/// </summary>
		private void ShowSubMenu(CanvasGroup tab)
		{
			tab.alpha = 1;
			tab.blocksRaycasts = true;
		}

		/// <summary>
		/// Sets the value of <see cref="MyClan"/>.
		/// If <paramref name="userClan"/> is null, disable option to see <see cref="_detailsTab"/> of my clan.
		/// </summary>
		private void RefreshUI(ClanMenuUIState state)
		{
			_clanDisplayName.text = state.UserClan?.Name ?? state.DisplayedClan?.Name ?? "No Clan Selected";

			if (state.SubMenu == ClanSubMenu.Search)
			{
				ShowSubMenu(_searchTab);
				HideSubMenu(_detailsTab);

				_joinClanButton.gameObject.SetActive(true);
				_leaveClanButton.gameObject.SetActive(false);
				_removeClanButton.gameObject.SetActive(false);
				_chatButton.gameObject.SetActive(false);
			}
			else
			{
				ShowSubMenu(_detailsTab);
				HideSubMenu(_searchTab);

				_joinClanButton.gameObject.SetActive(state.UserClan == null && state.DisplayedClan != null);
				_leaveClanButton.gameObject.SetActive(state.UserClan != null);
				// allow if superadmin
				_removeClanButton.gameObject.SetActive(state.UserClan != null && state.UserClanRank == 0);
				_chatButton.gameObject.SetActive(state.UserClan != null);
			}
		}

		/// <summary>
		///  Shows clan chat panel if possible.
		/// </summary>
		private async void StartChat(ClanMenuUIState state)
		{
			IChannel channel;

			try
			{
				channel = await _connection.Socket.JoinChatAsync(state.UserClan.Id, ChannelType.Group, persistence: true, hidden: true);
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Couldn't join chat " + e.Message);
				return;
			}

			_chatChannelClanUI.SetChatChannel(channel);
			_chatChannelClanUI.gameObject.SetActive(true);
		}
	}
}
