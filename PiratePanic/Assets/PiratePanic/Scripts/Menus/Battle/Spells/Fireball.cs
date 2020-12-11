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
using UnityEngine;
using System.Linq;
using System;

namespace PiratePanic
{
    /// <summary>
    /// Fireball spell
    /// </summary>
    public class Fireball : MonoBehaviour, ISpell
	{
		public event Action<ISpell> OnHide;

		public SpellType SpellType { get { return SpellType.Fireball; } }

		public int Range;
		public int Damage;

		[SerializeField] private Animator _animator = null;

		private GameStateManager _stateManager;
		private bool _isHost;

		public void Init(GameStateManager stateManager, bool isHost)
		{
			_stateManager = stateManager;
			_isHost = isHost;
		}

		/// <summary>
		/// Activates fireball do damage units in range area then sends it to clients
		/// </summary>
		/// <param name="node"></param>
		/// <param name="playerId"></param>
		public void Activate(Node node, string playerId)
		{
			if (_isHost)
			{
				List<Node> impactedNodes = new List<Node>();
				for (int x = node.Position.x - Range; x <= node.Position.x + Range; x++)
				{
					if (x < 0 || x >= Scene02BattleController.Instance.MapSize.x || (x % 2 == 1 && x >= Scene02BattleController.Instance.MapSize.x - 1))
					{
						continue;
					}
					for (int y = node.Position.y - Range; y <= node.Position.y + Range; y++)
					{
						if (y < 0 || y >= Scene02BattleController.Instance.MapSize.y)
						{
							continue;
						}
						if (Scene02BattleController.Instance.Nodes[x, y])
						{
							impactedNodes.Add(Scene02BattleController.Instance.Nodes[x, y]);
						}
					}
				}
				List<Unit> impactedUnits = new List<Unit>();
				foreach (Node n in impactedNodes)
				{
					if (n.Unit && n.Unit.CheckIfIsInRange(node.transform.position, Range))
					{
						if (n.Unit.OwnerId != playerId)
						{
							impactedUnits.Add(n.Unit);
						}
					}
				}

				SendSpellActivatedRequest(node, impactedUnits, playerId);
			}
		}

		/// <summary>
		/// Makes damage on units
		/// </summary>
		/// <param name="units"></param>
		public void MakeImpactOnUnits(List<Unit> units)
		{
			foreach (Unit unit in units)
			{
				unit.TakeDamage(Damage, AttackType.AoE);
			}
		}

		/// <summary>
		/// Shows explosion animation
		/// </summary>
		/// <param name="node"></param>
		public void Show(Node node)
		{
			gameObject.SetActive(true);
			transform.position = node.transform.position;
			transform.rotation = Camera.main.transform.rotation;
			_animator.SetTrigger("boom");
		}

		/// <summary>
		/// Sends information about fireball activation to other players through Nakama server.
		/// Use only on host!
		/// </summary>
		/// <param name="node"></param>
		/// <param name="impactedUnits"></param>
		/// <param name="playerId"></param>
		private void SendSpellActivatedRequest(Node node, List<Unit> impactedUnits, string playerId)
		{
			List<int> impactedUnitsIds = impactedUnits.Select(u => u.Id).ToList();
			MatchMessageSpellActivated message = new MatchMessageSpellActivated(playerId, SpellType, node.Position.x, node.Position.y, impactedUnitsIds);

			_stateManager.SendMatchStateMessage(MatchMessageType.SpellActivated, message);
			_stateManager.SendMatchStateMessageSelf(MatchMessageType.SpellActivated, message);
		}

		/// <summary>
		/// Is invoked from animator
		/// </summary>
		public void Hide()
		{
			OnHide?.Invoke(this);
			gameObject.SetActive(false);
		}
	}
}