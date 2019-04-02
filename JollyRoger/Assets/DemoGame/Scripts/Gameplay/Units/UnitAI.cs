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
using System.Linq;
using DemoGame.Scripts.Gameplay.NetworkCommunication;
using DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates;
using DemoGame.Scripts.Gameplay.Nodes;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Units
{

    /// <summary>
    /// Base class for unit AI
    /// </summary>
    public class UnitAI : MonoBehaviour
    {
        public struct UnitMove
        {
            public readonly Unit Unit;
            public readonly Node Node;

            public UnitMove(Unit unit, Node node)
            {
                Unit = unit;
                Node = node;
            }
        }

        [SerializeField] protected Unit _unit;

        /// <summary>
        /// Current enemy
        /// </summary>
        protected Unit _enemy;

        #region MONO

        /// <summary>
        /// Movement and attack loop
        /// </summary>
        protected virtual void Update()
        {
            if (MatchCommunicationManager.Instance.IsHost && _unit.IsDestroyed == false)
            {
                if (_enemy)
                {
                    if (_unit.CanAttack)
                    {
                        if (_unit.CurrentNode.ConnectedNodes.ContainsKey(_enemy.CurrentNode))
                        {
                            SendAttackRequest(_enemy, _unit.Damage, _unit.AttackType);
                        }
                    }
                    if (_unit.CanMove)
                    {
                        FindAndSetEnemy();
                        if (!_unit.CurrentNode.ConnectedNodes.ContainsKey(_enemy.CurrentNode))
                        {
                            Node nextNode = SelectNextNode();
                            if (nextNode)
                            {
                                SendMoveRequest(nextNode);
                            }
                        }
                    }
                }
                else
                {
                    FindAndSetEnemy();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (_enemy)
            {
                _enemy.OnDestroyed -= FindAndSetEnemy;
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Search for nearest enemy and moves towards it
        /// </summary>
        public virtual void FindAndSetEnemy()
        {
            if (_enemy)
            {
                _enemy.OnDestroyed -= FindAndSetEnemy;
            }

            _enemy = UnitsManager.Instance.GetNearestEnemyUnit(_unit.OwnerColor, _unit);

            if (_enemy)
            {
                _enemy.OnDestroyed += FindAndSetEnemy;
            }
        }

        /// <summary>
        /// Returns true if unit can move to other node and still be in contact with current enemy
        /// </summary>
        /// <param name="unitsMovesStack"></param>
        /// <returns></returns>
        public virtual bool RearrangeIfCan(Stack<UnitMove> unitsMovesStack)
        {
            Node newNode = SelectEnemyNeighbourNode(unitsMovesStack);
            if (newNode)
            {
                SendMoveRequest(newNode);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region PROTECTED METHODS

        /// <summary>
        /// Returns next node towards enemy
        /// </summary>
        /// <returns></returns>
        protected virtual Node SelectNextNode()
        {
            Node neighbourNode = SelectEnemyNeighbourNode();

            if (neighbourNode)
            {
                return neighbourNode;
            }

            return _unit.CurrentNode.GetNeighbourNodeWithNearestAngle(Vector3.SignedAngle(Vector3.forward, _enemy.transform.position - transform.position, Vector3.up));
        }

        /// <summary>
        /// Returns free node connected with enemy and current node if it exists. 
        /// Otherwise try to push friendly units in way that they still are in contact with the same enemy.
        /// If it not ends with success returns null.
        /// </summary>
        /// <param name="previousUnitsMoves"></param>
        /// <returns></returns>
        protected virtual Node SelectEnemyNeighbourNode(Stack<UnitMove> previousUnitsMoves = null)
        {
            if (previousUnitsMoves == null)
            {
                previousUnitsMoves = new Stack<UnitMove>();
            }

            var enemyNeighbourNodes = _unit.CurrentNode.ConnectedNodes.Keys.Where(node => node.ConnectedNodes.Keys.Contains(_enemy.CurrentNode));

            if (enemyNeighbourNodes.Count() > 0)
            {
                var enemyNeighbourFreeNodes = enemyNeighbourNodes.Where(node => !node.Occupied);
                if (enemyNeighbourFreeNodes.Count() > 0)
                {
                    return enemyNeighbourFreeNodes.ElementAt(UnityEngine.Random.Range(0, enemyNeighbourFreeNodes.Count()));
                }
                else
                {
                    foreach (Node node in enemyNeighbourNodes)
                    {
                        if (!node.Unit.CanMove || node.Unit.OwnerColor != _unit.OwnerColor || previousUnitsMoves.Any(move => move.Node == node))
                        {
                            continue;
                        }
                        UnitMove unitMove = new UnitMove(_unit, node);
                        previousUnitsMoves.Push(unitMove);
                        bool canRearrange = node.Unit.UnitAI.RearrangeIfCan(previousUnitsMoves);
                        if (canRearrange)
                        {
                            return node;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Sends move request to other players and self.
        /// Use only on host!
        /// </summary>
        /// <param name="node"></param>
        protected virtual void SendMoveRequest(Node node)
        {
            MatchMessageUnitMoved message = new MatchMessageUnitMoved(node.Position.x, node.Position.y, _unit.Id, _unit.OwnerId);
            MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitMoved, message);
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitMoved, message);
        }

        /// <summary>
        /// Sends attack request to other players and self.
        /// Use only on host!
        /// </summary>
        protected virtual void SendAttackRequest(Unit enemy, int damage, AttackType attackType)
        {
            MatchMessageUnitAttacked message = new MatchMessageUnitAttacked(_unit.Id, _unit.OwnerId, enemy.Id, _unit.Damage, _unit.AttackType);
            MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitAttacked, message);
            MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitAttacked, message);
        }

        #endregion
    }

}