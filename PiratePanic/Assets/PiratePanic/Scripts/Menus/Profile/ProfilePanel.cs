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

using Nakama;
using Nakama.TinyJson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{

	/// <summary>
	/// Panel displaying user's stats.
	/// </summary>
	public class ProfilePanel : MonoBehaviour
	{
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

		/// <summary>
		/// Available avatar sprites to choose from.
		/// </summary>
		[SerializeField] private AvatarSprites _avatarSprites = null;

		private GameConnection _connection;

		public void Init(GameConnection connection, ProfileUpdatePanel updatePanel)
		{
			_connection = connection;
			_profileUpdateButton.onClick.AddListener(() => updatePanel.Show());
		}

		/// <summary>
		/// Searches for a user with given <paramref name="userId"/>. If a user was found, populates this panel
		/// with retrieved user's stats. Makes this panel visible to the viewer.
		/// </summary>
		/// <param name="userId">Id of searched user.</param>
		public async void ShowAsync(string userId)
		{
			try
			{
				IApiUsers results = await _connection.Client.GetUsersAsync(_connection.Session, new string[] { userId });
				if (results.Users.Count() != 0)
				{
					Show(results.Users.ElementAt(0));
				}
				else
				{
					Debug.LogWarning("Couldn't find user with id: " + userId);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("An error has occured while retrieving user info: " + e);
			}
		}

		/// <summary>
		/// Populates fields of this panel using <see cref="PlayerData"/> gathered from <paramref name="user"/>.
		/// Makes this panel visible to the user.
		/// </summary>
		/// <param name="user">User to be displayed in this panel.</param>
		public async void Show(IApiUser user)
		{
			PopulateDataAsync(user);
			await SetUIAccessAsync(user);
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
			IApiUser localUser = _connection.Account.User;
			bool isFriend = await CanBeFriendAsync(user);
			_avatarImage.sprite = _avatarSprites.GetSpriteByName(user.AvatarUrl);
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
			if (user.Id == _connection.Session.UserId)
			{
				return false;
			}

			var friends = await _connection.Client.ListFriendsAsync(_connection.Session);

			if (friends == null)
			{
				Debug.LogError("Couldn't retrieve friends list");
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
		private void OnAccountUpdated()
		{
			IApiUser user = _connection.Account.User;
			_usernameText.text = user.Username;
			_avatarImage.sprite = _avatarSprites.GetSpriteByName(user.AvatarUrl);
		}

		/// <summary>
		/// Sets fields of this panel to show <see cref="PlayerData"/> gathered from <paramref name="user"/>.
		/// </summary>
		/// <param name="user">User to be displayed in this panel.</param>
		private async void PopulateDataAsync(IApiUser user)
		{
			StorageObjectId personalStorageId = new StorageObjectId();
			personalStorageId.Collection = "personal";
			personalStorageId.UserId = _connection.Session.UserId;
			personalStorageId.Key = "player_data";

			IApiStorageObjects personalStorageObjects = await _connection.Client.ReadStorageObjectsAsync(_connection.Session, personalStorageId);

			PlayerData playerData = new PlayerData();
			IUserGroupListUserGroup clan = null;

			try
			{
				IApiUserGroupList clanList = await _connection.Client.ListUserGroupsAsync(_connection.Session);
				// user should only be in one clan.
				clan = clanList.UserGroups.Any() ? clanList.UserGroups.First() : null;
			}
			catch (ApiResponseException e)
			{
				Debug.LogWarning("Error fetching user clans " + e.Message);
			}

			CardCollection cardCollection = null;

			try
			{
				var response = await _connection.Client.RpcAsync(_connection.Session, "load_user_cards", "");
				cardCollection = response.Payload.FromJson<CardCollection>();
			}
			catch (ApiResponseException e)
			{
				throw e;
			}

			_usernameText.text = user.Username;
			_statsText.text = GenerateStats(playerData).TrimEnd();

			_clanNameText.text = clan == null ?
			"<i><color=#b0b0b0>[Not a clan member yet]</color></i>" :
			clan.Group.Name;

			List<string> deckIds = cardCollection.GetDeckList();
			for (int i = 0; i < deckIds.Count; i++)
			{
				Card card = cardCollection.GetDeckCard(deckIds[i]);
				_cardSlots[i].SetCard(card);
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
	}
}
