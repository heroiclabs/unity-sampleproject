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

namespace DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates
{

    /// <summary>
    /// Used to store incomming messages while the match hasn't started yet.
    /// </summary>
    public class IncommingMessageState
    {

        #region Fields

        /// <summary>
        /// The code of incomming message.
        /// </summary>
        public long opCode;

        /// <summary>
        /// Data send with incomming message.
        /// </summary>
        public string message;

        #endregion

        public IncommingMessageState(long opCode, string message)
        {
            this.opCode = opCode;
            this.message = message;
        }

    }

}