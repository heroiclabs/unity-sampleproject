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

namespace DemoGame.Scripts.Chat
{
    /// <summary>
    /// Basic information about chat user
    /// </summary>
    public class ChatUser
    {
        public readonly string UserId;

        public readonly string Username;

        public readonly string Avatar;

        public ChatUser(string userId, string username, string avatar = null)
        {
            UserId = userId;
            Username = username;
            Avatar = avatar;
        }
    }
}
