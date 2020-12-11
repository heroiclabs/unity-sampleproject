/**
 * Copyright 2021 The Nakama Authors
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
using UnityEngine;

namespace PiratePanic
{
    /// <summary>
    /// Base class for unit AI.
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

		protected GameStateManager _stateManager;
		protected UnitsManager _unitsManager;
		protected bool _isHost;

		public void Init(GameStateManager stateManager, UnitsManager unitsManager, bool isHost)
		{
			_stateManager = stateManager;
			_unitsManager = unitsManager;
			_isHost = isHost;
		}

		/// <summary>
		/// Movement and attack loop
		/// </summary>
		protected virtual void Update()
		{
			if (_isHost && !_unit.IsDestroyed)
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
						FindAndSetEnemy(string.Empty, -1);

						// Nullcheck to help with end of game
						if (_unit != null && _enemy != null &&
						!_unit.CurrentNode.ConnectedNodes.ContainsKey(_enemy.CurrentNode))
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
					FindAndSetEnemy(string.Empty, -1);
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (_enemy)
			{
				_enemy.OnAfterDestroyed -= FindAndSetEnemy;
			}
		}

		/// <summary>
		/// Search for nearest enemy and moves towards it
		/// </summary>
		public virtual void FindAndSetEnemy(string playerId, int unitId)
		{
			if (_enemy)
			{
				_enemy.OnAfterDestroyed -= FindAndSetEnemy;
			}

			_enemy = _unitsManager.GetNearestEnemyUnit(_unit.OwnerColor, _unit);

			if (_enemy)
			{
				_enemy.OnAfterDestroyed += FindAndSetEnemy;
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
			_stateManager.SendMatchStateMessage(MatchMessageType.UnitMoved, message);
			_stateManager.SendMatchStateMessageSelf(MatchMessageType.UnitMoved, message);
		}

		/// <summary>
		/// Sends attack request to other players and self.
		/// Use only on host!
		/// </summary>
		protected virtual void SendAttackRequest(Unit enemy, int damage, AttackType attackType)
		{
			MatchMessageUnitAttacked message = new MatchMessageUnitAttacked(_unit.Id, _unit.OwnerId, enemy.Id, _unit.Damage, _unit.AttackType);
			_stateManager.SendMatchStateMessage(MatchMessageType.UnitAttacked, message);
			_stateManager.SendMatchStateMessageSelf(MatchMessageType.UnitAttacked, message);
		}
	}
}