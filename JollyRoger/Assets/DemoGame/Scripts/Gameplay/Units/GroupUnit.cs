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
using System.Linq;
using System.Collections.Generic;
using DemoGame.Scripts.Gameplay.Nodes;
using DemoGame.Scripts.Gameplay.Cards;

namespace DemoGame.Scripts.Gameplay.Units
{

    /// <summary>
    /// Unit containing some subunits with separate attack values and health
    /// </summary>
    public class GroupUnit : Unit
    {
        /// <summary>
        /// Sum of damage of al subunits
        /// </summary>
        public override int Damage { get { return SubUnits.Sum(su => su.Damage); } }

        /// <summary>
        /// List of all subunits in unit
        /// </summary>
        public List<SubUnit> SubUnits;

        public override void Init(PlayerColor owner, string ownerId, int id, Node startNode, Card card)
        {
            if (SubUnits.Count <= 0)
            {
                Destroy();
            }
            base.Init(owner, ownerId, id, startNode, card);
            foreach (SubUnit subUnit in SubUnits)
            {
                subUnit.Init(_card);
                subUnit.OnDestroyed += RemoveSubUnit;
            }
        }

        public override void TakeDamage(int damage, AttackType attackType)
        {
            if (attackType == AttackType.Simple)
            {
                SubUnits[0].TakeDamage(damage);
            }
            else if (attackType == AttackType.AoE)
            {
                List<SubUnit> temp = new List<SubUnit>(SubUnits);
                foreach (SubUnit subUnit in temp)
                {
                    subUnit.TakeDamage(damage);
                }
            }
        }

        private void RemoveSubUnit(SubUnit subUnit)
        {
            SubUnits.Remove(subUnit);
            if (SubUnits.Count <= 0)
            {
                Destroy();
            }
        }
    }

}