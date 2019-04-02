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


using DemoGame.Scripts.Gameplay.Units;

namespace DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates
{

    /// <summary>
    /// Contains data about unit attacking other unit
    /// </summary>
    public class MatchMessageUnitAttacked : MatchMessage<MatchMessageUnitAttacked>
    {
        public readonly int UnitId;
        public readonly string OwnerId;
        public readonly int EnemyId;
        public readonly int Damage;
        public readonly AttackType AttackType;

        public MatchMessageUnitAttacked(int unitId, string ownerId, int enemyId, int damage, AttackType attackType)
        {
            UnitId = unitId;
            OwnerId = ownerId;
            EnemyId = enemyId;
            Damage = damage;
            AttackType = attackType;
        }
    }

}