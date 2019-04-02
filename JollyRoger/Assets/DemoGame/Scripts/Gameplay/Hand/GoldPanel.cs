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
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Gameplay.Hands
{

    /// <summary>
    /// Panel displaying current in-game gold ammout.
    /// Visible during a match at the bottom of the screen.
    /// </summary>
    public class GoldPanel : MonoBehaviour
    {

        #region Fields

        /// <summary>
        /// Displays the ammout of gold local user currently own, rounded down.
        /// </summary>
        [SerializeField] private Text _goldCounter = null;

        /// <summary>
        /// List of coin images.
        /// </summary>
        [SerializeField] private List<Image> _coinsImages = null;

        /// <summary>
        /// Gold manager used to determine current gold owned by the local player.
        /// </summary>
        private Gold _goldManager;

        #endregion

        #region Monobehaviour

        /// <summary>
        /// Updates the displayed gold count.
        /// </summary>
        private void Update()
        {
            if (_goldManager != null)
            {
                SetGold(_goldManager.CurrentGold);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes this panel.
        /// </summary>
        public void Init(Gold goldManager)
        {
            _goldManager = goldManager;
            SetGold(_goldManager.CurrentGold);
        }

        /// <summary>
        /// Fills coin images depending on the current gold count.
        /// Updates <see cref="_goldCounter"/>.
        /// </summary>
        private void SetGold(float count)
        {
            int goldCount = Mathf.FloorToInt(count);
            _goldCounter.text = goldCount.ToString();
            for (int i = 0; i < _coinsImages.Count; i++)
            {
                if (i < goldCount)
                {
                    _coinsImages[i].fillAmount = 1;
                }
                else if (i == goldCount)
                {
                    _coinsImages[i].fillAmount = count - goldCount;
                }
                else
                {
                    _coinsImages[i].fillAmount = 0;
                }
            }
        }

        #endregion

    }

}
