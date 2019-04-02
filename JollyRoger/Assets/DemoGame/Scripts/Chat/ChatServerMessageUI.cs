/**
 * Copyright 2019 Heroic Labs and contributors
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

using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Chat
{
    /// <summary>
    /// UI visualisation of server message
    /// </summary>
    public class ChatServerMessageUI : MonoBehaviour
    {
        /// <summary>
        /// Message Text component for viewing content
        /// </summary>
        [SerializeField] private Text _contentText = null;

        /// <summary>
        /// Initializes message with given content
        /// </summary>
        public void Init(string content)
        {
            _contentText.text = content;
        }
    }
}
