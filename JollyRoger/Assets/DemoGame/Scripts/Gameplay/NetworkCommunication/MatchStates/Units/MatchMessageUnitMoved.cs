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


namespace DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates
{
    /// <summary>
    /// Contains data about unit move
    /// </summary>
    public class MatchMessageUnitMoved : MatchMessage<MatchMessageUnitMoved>
    {
        public readonly int NodeX;
        public readonly int NodeY;
        public readonly int UnitId;
        public readonly string OwnerId;

        public MatchMessageUnitMoved(int nodeX, int nodeY, int unitId, string ownerId)
        {
            NodeX = nodeX;
            NodeY = nodeY;
            UnitId = unitId;
            OwnerId = ownerId;
        }
    }
}
