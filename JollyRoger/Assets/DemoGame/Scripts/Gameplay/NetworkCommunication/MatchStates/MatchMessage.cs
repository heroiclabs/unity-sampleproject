/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch
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

using Nakama.TinyJson;


namespace DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates
{
    /// <summary>
    /// Base class for all match messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MatchMessage<T>
    {
        /// <summary>
        /// Parses json gained from server to MatchMessage class object
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Parse(string json)
        {
            return JsonParser.FromJson<T>(json);
        }

        /// <summary>
        /// Creates string with json to be send as match state message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string ToJson(T message)
        {
            return JsonWriter.ToJson(message);
        }
    }
}
