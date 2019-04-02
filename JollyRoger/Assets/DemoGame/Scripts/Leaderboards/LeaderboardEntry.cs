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
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Leaderboards
{

    /// <summary>
    /// Single leaderboard record UI.
    /// </summary>
    public class LeaderboardEntry : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Textbox displaying user rank.
        /// </summary>
        [SerializeField] private Text _rank = null;

        /// <summary>
        /// Textbox containing displayed user's name.
        /// </summary>
        [SerializeField] private Text _username = null;

        /// <summary>
        /// Textbox containing displayed user's score.
        /// </summary>
        [SerializeField] private Text _score = null;

        /// <summary>
        /// On click shows displayed user's profile.
        /// </summary>
        [SerializeField] private Button _profile = null;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the UI of this leaderboard entry.
        /// </summary>
        public void SetPlayer(string username, int rank, string score, Action onProfileClicked)
        {
            _username.text = username;
            _rank.text = rank + ".";
            _score.text = score;
            _profile.onClick.AddListener(() => onProfileClicked());
        }

        #endregion
    }

}