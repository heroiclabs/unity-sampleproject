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
using DemoGame.Scripts.Session;
using DemoGame.Scripts.Utils;
using Nakama;
using UnityEngine;

namespace DemoGame.Scripts.Notifications
{

    /// <summary>
    /// Controlls Nakama notification system.
    /// Pools incoming notifications and enqueues them on main thread.
    /// </summary>
    public class NotificationManager : Singleton<NotificationManager>
    {
        #region Mono

        /// <summary>
        /// Awaits connection with Nakama server.
        /// </summary>
        private void Start()
        {
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

        #region Events

        /// <summary>
        /// Invoked upon reciving Nakama notification
        /// </summary>
        public event Action<IApiNotification> OnNotification;

        #endregion

        #region Methods

        /// <summary>
        /// Subscribes to Nakama socket's <see cref="ISocket.OnNotification"/> event.
        /// </summary>
        private void Init()
        {
            NakamaSessionManager.Instance.Socket.ReceivedNotification += NotificationReceived;
        }

        /// <summary>
        /// Handles incomming notification messages.
        /// </summary>
        private void NotificationReceived(IApiNotification notification)
        {
            Debug.Log("Notification received: " + notification.Code);
            PushLocalNotification(notification);
        }

        /// <summary>
        /// Invokes <see cref="OnNotification"/> on the main thread.
        /// </summary>
        public void PushLocalNotification(IApiNotification notification)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => DispatchNotification(notification));
        }

        /// <summary>
        /// Invokes <see cref="OnNotification"/> event.
        /// </summary>
        private void DispatchNotification(IApiNotification notification)
        {
            OnNotification?.Invoke(notification);
        }

        #endregion

    }

}