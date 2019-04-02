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


using DemoGame.Scripts.Gameplay.Cards;

namespace DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates
{
    /// <summary>
    /// Contains data about spawned unit
    /// </summary>
    public class MatchMessageUnitSpawned : MatchMessage<MatchMessageUnitSpawned>
    {
        public readonly string OwnerId;
        public readonly PlayerColor Player;
        public readonly int UnitId;
        public readonly Card Card;
        public readonly int NodeX;
        public readonly int NodeY;

        public MatchMessageUnitSpawned(string ownerId, PlayerColor player, int unitId, Card card, int nodeX, int nodeY)
        {
            OwnerId = ownerId;
            Player = player;
            UnitId = unitId;
            Card = card;
            NodeX = nodeX;
            NodeY = nodeY;
        }
    }
}
