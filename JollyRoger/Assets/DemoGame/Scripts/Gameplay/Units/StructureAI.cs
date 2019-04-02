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
using System.Collections.Generic;
using DemoGame.Scripts.Gameplay.NetworkCommunication;
using DemoGame.Scripts.Gameplay.Nodes;

namespace DemoGame.Scripts.Gameplay.Units
{

    /// <summary>
    /// AI of nonmovable objects that could attack, like i.e. forts
    /// </summary>
    public class StructureAI : UnitAI
    {
        protected override void Update()
        {
            if (MatchCommunicationManager.Instance.IsHost && _unit.IsDestroyed == false)
            {
                if (_enemy)
                {
                    if (!_enemy.CurrentNode.ConnectedNodes.ContainsKey(_unit.CurrentNode))
                    {
                        FindAndSetEnemy();
                    }
                    if (_unit.IsWeaponReloaded)
                    {
                        if (_unit.CurrentNode.ConnectedNodes.ContainsKey(_enemy.CurrentNode))
                        {
                            SendAttackRequest(_enemy, _unit.Damage, _unit.AttackType);
                        }
                    }
                }
                else
                {
                    FindAndSetEnemy();
                }
            }
        }

        public override bool RearrangeIfCan(Stack<UnitMove> unitsMovesStack)
        {
            return false;
        }

        protected override Node SelectEnemyNeighbourNode(Stack<UnitMove> previousUnitsMoves = null)
        {
            return null;
        }
    }

}