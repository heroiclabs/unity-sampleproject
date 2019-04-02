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

using DemoGame.Scripts.Gameplay.NetworkCommunication;
using DemoGame.Scripts.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Gameplay.UI
{

    /// <summary>
    /// Menu showed at the end of a match.
    /// Shows who win as well as displays the funds gained for the match.
    /// </summary>
    public class SummaryMenu : Menu
    {

        #region Fields

        /// <summary>
        /// Displays the reward gained from the match.
        /// </summary>
        [SerializeField] private Text _rewardText = null;

        /// <summary>
        /// Shows who win the match.
        /// </summary>
        [SerializeField] private Text _header = null;

        /// <summary>
        /// Image visualizing local user's victory/defeat.
        /// </summary>
        [SerializeField] private Image _resultImage = null;


        [Space]
        /// <summary>
        /// Winning message displayed in <see cref="_header"/>.
        /// </summary>
        [SerializeField] private string _winHeader = string.Empty;

        /// <summary>
        /// Winning image displayed in <see cref="_resultImage"/>.
        /// </summary>
        [SerializeField] private Sprite _winSprite = null;

        [Space]
        /// <summary>
        /// Losing message displayed in <see cref="_header"/>.
        /// </summary>
        [SerializeField] private string _loseHeader = string.Empty;

        /// <summary>
        /// Losing image displayed in <see cref="_resultImage"/>.
        /// </summary>
        [SerializeField] private Sprite _loseSprite = null;

        #endregion

        #region Monobehaviour

        /// <summary>
        /// Sets the back button listener.
        /// </summary>
        private void Awake()
        {
            base.SetBackButtonHandler(Hide);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Populates all text and image fields of this summary menu.
        /// </summary>
        public void SetResult(bool win, int reward)
        {
            if (win == true)
            {
                _header.text = _winHeader;
                _resultImage.sprite = _winSprite;
                _rewardText.text = $"+{reward}";
            }
            else
            {
                _header.text = _loseHeader;
                _resultImage.sprite = _loseSprite;
                _rewardText.text = $"+{reward}";
            }
        }

        /// <summary>
        /// Hides this menu.
        /// Unloads this scene and loads main menu.
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            MatchCommunicationManager.Instance.LeaveGame();
        }

        #endregion

    }

}