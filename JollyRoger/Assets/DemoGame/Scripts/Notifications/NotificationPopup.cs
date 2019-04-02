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
using DemoGame.Scripts.Menus;
using Nakama;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Notifications
{

    /// <summary>
    /// Popup menu showed in the top right corner of the screen.
    /// Shown upon finishing a quest.
    /// </summary>
    /// <remarks>
    /// One of the quests prepared for this demo is "new friend quest". To finish it,
    /// add a dummy account named "Richard" to friends.
    /// </remarks>
    public class NotificationPopup : Menu
    {
        #region Fields

        /// <summary>
        /// Title of the notification.
        /// </summary>
        [SerializeField] private Text _titleText = null;

        /// <summary>
        /// Description of the notification.
        /// </summary>
        [SerializeField] private Text _descriptionText = null;

        /// <summary>
        /// Button to hide the notification.
        /// </summary>
        [SerializeField] private Button _dismissButton = null;

        #endregion

        /// <summary>
        /// Serializable class used to retrieve the reward gained from completing a quest.
        /// </summary>
        [Serializable]
        private class Reward
        {
            public int reward = 0;
        }

        #region Mono

        /// <summary>
        /// Sets up dismiss button handlers and hides the popup.
        /// </summary>
        private void Awake()
        {
            _dismissButton.onClick.AddListener(base.Hide);
            NotificationManager.Instance.OnNotification += NotificationReceived;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles incomming notification messages.
        /// </summary>
        private void NotificationReceived(IApiNotification e)
        {
            if (e.Code == (int)NotificationCode.Quest_NewFriend)
            {
                Debug.Log("New notification: " + e.Code + ", " + e.Content);
                Reward reward = JsonUtility.FromJson<Reward>(e.Content);
                base.Show();
                _titleText.text = e.Subject;
                _descriptionText.text = "Received reward: " + reward.reward;
            }
        }

        /// <summary>
        /// Shows a notification panel
        /// </summary>
        private void NotifyQuestComplete(IApiNotification e)
        {
            Debug.Log("New notification: " + e.Code + ", " + e.Content);
            Reward reward = JsonUtility.FromJson<Reward>(e.Content);
            base.Show();
            _titleText.text = e.Subject;
            _descriptionText.text = "Received reward: " + reward.reward;
        }

        #endregion

    }

}