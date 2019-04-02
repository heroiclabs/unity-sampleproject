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

using DemoGame.Scripts.Menus;
using DemoGame.Scripts.Session;
using UnityEngine;

namespace DemoGame.Scripts.Profile
{

    /// <summary>
    /// Menu showing <see cref="ProfilePanel"/> on enter filed with local user's data.
    /// </summary>
    public class ProfileMenu : Menu
    {
        /// <summary>
        /// Reference to panel containing profile information
        /// </summary>
        [SerializeField] private ProfilePanel _profilePanel = null;

        /// <summary>
        /// Sets the back button listener.
        /// </summary>
        private void Awake()
        {
            base.SetBackButtonHandler(MenuManager.Instance.HideTopMenu);
        }

        /// <summary>
        /// Shows this menu and <see cref="ProfilePanel"/> with local user data.
        /// </summary>
        public async override void Show()
        {
            await _profilePanel.ShowAsync(NakamaSessionManager.Instance.Account.User);
            base.Show();
        }

    }

}