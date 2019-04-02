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
using DemoGame.Scripts.DataStorage;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Clans
{

    /// <summary>
    /// Entry visible in clan search engine.
    /// Displays clan information as well as join button.
    /// </summary>
    public class ClanSearchResult : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Text displaying associated clan's name.
        /// </summary>
        [SerializeField] private Text _clanName = null;

        /// <summary>
        /// Image displaying clan's avatar.
        /// </summary>
        [SerializeField] private Image _clanAvatar = null;

        /// <summary>
        /// Button invoking join method.
        /// </summary>
        [SerializeField] private Button _joinClanButton = null;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes UI for this entry.
        /// </summary>
        public void SetClan(IApiGroup clan, Action<IApiGroup> onJoin)
        {
            _clanName.text = clan.Name;
            _joinClanButton.onClick.AddListener(() => onJoin(clan));
            Sprite avatar = AvatarManager.Instance.LoadEmblem(clan.AvatarUrl);
            _clanAvatar.sprite = avatar;
        }

        #endregion
    }

}