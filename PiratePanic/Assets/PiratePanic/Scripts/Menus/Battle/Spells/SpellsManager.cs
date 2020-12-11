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

namespace PiratePanic
{

    /// <summary>
    /// Manager resolving spells activation. Contains also object pooling mechanic for spells.
    /// </summary>
    public class SpellsManager
	{
		/// <summary>
		/// List of disabled spells objects
		/// </summary>
		private List<ISpell> _nonactiveSpells = new List<ISpell>();

		/// <summary>
		/// List of active spells objects
		/// </summary>
		private List<ISpell> _activeSpells = new List<ISpell>();

		private GameStateManager _stateManager;
		private UnitsManager _unitsManager;
		private bool _isHost;

		public SpellsManager(GameStateManager stateManager, UnitsManager unitsManager, bool isHost)
		{
			_unitsManager = unitsManager;
			_stateManager = stateManager;
			_stateManager.OnSpellActivated += ResolveSpellActivation;
			_stateManager.OnCardPlayed += ActivateSpell;
			_isHost = isHost;
		}

		/// <summary>
		/// Activates spell object of type given in message on node given in message
		/// </summary>
		/// <param name="message"></param>
		private void ActivateSpell(MatchMessageCardPlayed message)
		{
			if (message.Card.CardData.type == CardType.Fireball)
			{
				GetNonactiveSpellOfType(SpellType.Fireball).Activate(Scene02BattleController.Instance.Nodes[message.NodeX, message.NodeY], message.PlayerId);
			}
		}

		/// <summary>
		/// Resolves spell activation on given node
		/// </summary>
		/// <param name="message"></param>
		private void ResolveSpellActivation(MatchMessageSpellActivated message)
		{
			ISpell spell = GetNonactiveSpellOfType(message.SpellType);

			_nonactiveSpells.Remove(spell);
			_activeSpells.Add(spell);

			spell.OnHide += HideSpell;

			spell.Show(Scene02BattleController.Instance.Nodes[message.NodeX, message.NodeY]);

			List<Unit> impactedUnits = new List<Unit>();

			foreach (int unitId in message.ImpactedUnitsIds)
			{
				impactedUnits.Add(_unitsManager.GetUnit(unitId));
			}

			spell.MakeImpactOnUnits(impactedUnits);
		}

		/// <summary>
		/// Gets disabled spell object or spawn one if could not find any
		/// </summary>
		/// <param name="spellType"></param>
		/// <returns></returns>
		private ISpell GetNonactiveSpellOfType(SpellType spellType)
		{
			ISpell spell = _nonactiveSpells.Find(s => s.SpellType == spellType);

			if (spell == null)
			{
				GameObject prefab = Resources.Load<GameObject>("Spells/" + spellType.ToString());
				spell = GameObject.Instantiate(prefab).GetComponent<ISpell>();
				var asFireball = spell as Fireball;
				if (asFireball)
				{
					asFireball.Init(_stateManager, _isHost);
				}

				_nonactiveSpells.Add(spell);
			}

			return spell;
		}

		/// <summary>
		/// Hides spell object and adds it to nonactive spells list
		/// </summary>
		/// <param name="spell"></param>
		private void HideSpell(ISpell spell)
		{
			_activeSpells.Remove(spell);
			_nonactiveSpells.Add(spell);
		}
	}
}