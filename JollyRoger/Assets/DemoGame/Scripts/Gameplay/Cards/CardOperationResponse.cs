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

using System;

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Stores response received after performing an operation on deck (e.g. card swap, card merge)
    /// </summary>
    [Serializable] public class CardOperationResponse
    {
        /// <summary>
        /// If true, operation was performed successfully.
        /// </summary>
        public bool response;

        /// <summary>
        /// If <see cref="response"/> is false, this will hold the error message
        /// </summary>
        public string message;
    }
}
