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
using Nakama;
using System;

namespace PiratePanic
{
	public class UnitsManager
	{
		public event Action<Unit> OnAfterUnitInstantiated;

		/// <summary>
		/// Dictionary containing Dictionary of Units ids to units objects for each player
		/// </summary>
		private Dictionary<PlayerColor, Dictionary<int, Unit>> _units = new Dictionary<PlayerColor, Dictionary<int, Unit>>();

		/// <summary>
		/// Next id to give for new unit
		/// </summary>
		private int _nextId = 0;

		private GameStateManager _stateManager;
		private GameConnection _connection;

		public UnitsManager(GameConnection connection, GameStateManager gameStateManager)
		{
			_connection = connection;
			_stateManager = gameStateManager;

			_units[PlayerColor.Black] = new Dictionary<int, Unit>();
			_units[PlayerColor.Red] = new Dictionary<int, Unit>();

			_stateManager.OnCardPlayed += CardPlayed;
			_stateManager.OnUnitMoved += MoveUnit;
			_stateManager.OnUnitAttacked += MakeUnitAttack;
			_stateManager.OnUnitSpawned += SpawnUnit;
		}

		/// <summary>
		/// Returns unit with given id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Unit GetUnit(int id)
		{
			if (_units[PlayerColor.Black].ContainsKey(id))
			{
				return _units[PlayerColor.Black][id];
			}
			else
			{
				return _units[PlayerColor.Red][id];
			}
		}

		/// <summary>
		/// Removes given unit from dictionary
		/// </summary>
		/// <param name="unit"></param>
		public void HandleAfterUnitDestroyed(string playerId, int unitId)
		{
			PlayerColor color = playerId == _connection.Session.UserId ? PlayerColor.Black : PlayerColor.Red;
			_units[color].Remove(unitId);
		}

		/// <summary>
		/// Returns the nearest enemy unit to given unit
		/// </summary>
		/// <param name="playerColor"></param>
		/// <param name="unit"></param>
		/// <returns></returns>
		public Unit GetNearestEnemyUnit(PlayerColor playerColor, Unit unit)
		{
			Unit unitToReturn = null;

			if (playerColor == PlayerColor.Black)
			{
				if (_units[PlayerColor.Red].Count > 0)
				{
					unitToReturn = _units[PlayerColor.Red].Values.Aggregate((tempU, u) =>
					(tempU == null || Vector3.Distance(u.transform.position, unit.transform.position) < Vector3.Distance(tempU.transform.position, unit.transform.position) ?
					u : tempU));
				}
			}
			else
			{
				if (_units[PlayerColor.Black].Count > 0)
				{
					unitToReturn = _units[PlayerColor.Black].Values.Aggregate((tempU, u) =>
					(tempU == null || Vector3.Distance(u.transform.position, unit.transform.position) < Vector3.Distance(tempU.transform.position, unit.transform.position) ?
					u : tempU));
				}
			}

			return unitToReturn;
		}

		public int NumDestroyedTowers(PlayerColor color)
		{
			return _units[color].Count(keyVal => keyVal.Value.Card.type == CardType.Fort);
		}

		/// <summary>
		/// Builds forts at the start of the game.
		/// </summary>
		/// <param name="userId"></param>
		public void BuildStartingStructures(string userId)
		{
			bool isHost = userId == _connection.BattleConnection.HostId;

			CardData fortCard = new CardData()
			{
				type = CardType.Fort,
				level = 1
			};

			CardData mainFortCard = new CardData()
			{
				type = CardType.MainFort,
				level = 1
			};

			int fortX = isHost ? 4 : 10;
			int mainFortX = isHost ? 2 : 12;

			var playerColor = isHost ? PlayerColor.Black : PlayerColor.Red;

			MatchMessageUnitSpawned fortTop = new MatchMessageUnitSpawned(
				userId, playerColor, _nextId++, fortCard, fortX, 10);
			MatchMessageUnitSpawned fortBot = new MatchMessageUnitSpawned(
				userId, playerColor, _nextId++, fortCard, fortX, 2);
			MatchMessageUnitSpawned fortMain = new MatchMessageUnitSpawned(
				userId, playerColor, _nextId++, mainFortCard, mainFortX, 6);

			_stateManager.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortTop);
			_stateManager.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortBot);
			_stateManager.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortMain);

			_stateManager.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortTop);
			_stateManager.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortBot);
			_stateManager.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortMain);

			Unit tower2 = _units[playerColor][_nextId - 3];
			Unit tower1 = _units[playerColor][_nextId - 2];
			Unit castle = _units[playerColor][_nextId - 1];
		}

		/// <summary>
		/// Resolves card playing, then spawns unit of given type
		/// </summary>
		/// <param name="message"></param>
		private void CardPlayed(MatchMessageCardPlayed message)
		{
			if (_connection.BattleConnection.HostId == _connection.Session.UserId)
			{
				MatchMessageUnitSpawned unitSpawned = null;
				switch (message.Card.CardData.type)
				{
					case CardType.BigShip:
					case CardType.AoEShip:
					case CardType.Boats:
						int id = _nextId + 1;
						PlayerColor color = message.PlayerId == _connection.BattleConnection.HostId ? PlayerColor.Black : PlayerColor.Red;
						unitSpawned = new MatchMessageUnitSpawned(message.PlayerId, color, id, message.Card.CardData, message.NodeX, message.NodeY);
						break;
					default:
						break;
				}

				if (unitSpawned != null)
				{
					_nextId += 1;
					_stateManager.SendMatchStateMessage(MatchMessageType.UnitSpawned, unitSpawned);
					_stateManager.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, unitSpawned);
				}
			}
		}

		/// <summary>
		/// Spawns new unit and adds it to dictionary.
		/// </summary>
		/// <param name="message"></param>
		private void SpawnUnit(MatchMessageUnitSpawned message)
		{
			CardType cardType = message.Card.GetCardInfo().CardType;
			Node node = Scene02BattleController.Instance.Nodes[message.NodeX, message.NodeY];
			PlayerColor player = message.OwnerId == _connection.Session.UserId ? PlayerColor.Black : PlayerColor.Red;
			int id = message.UnitId;

			Quaternion rotation;
			if (_connection.BattleConnection.HostId == _connection.Session.UserId)
			{
				rotation = player == PlayerColor.Black ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
			}
			else
			{
				rotation = player == PlayerColor.Black ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
			}

			GameObject prefab = Resources.Load<GameObject>("Units/" + cardType.ToString() + "_" + player.ToString());

			Unit unit = GameObject.Instantiate(prefab, node.transform.position, rotation).GetComponent<Unit>();

			if (unit)
			{
				string ownerId = message.OwnerId;
				unit.Init(player, ownerId, id, node, message.Card);
				_units[player].Add(id, unit);
				unit.OnAfterDestroyed += HandleAfterUnitDestroyed;
			}

			UnitAI unitAI = unit.GetComponent<UnitAI>();

			if (unitAI != null)
			{
				unitAI.Init(_stateManager, this, _connection.BattleConnection.HostId == _connection.Session.UserId);
			}
			else
			{
				Debug.LogWarning("Could not initialize null UnitAI");
			}

			if (OnAfterUnitInstantiated != null)
			{
				OnAfterUnitInstantiated(unit);
			}
		}

		/// <summary>
		/// Starts unit movement towards given node
		/// </summary>
		/// <param name="message"></param>
		private void MoveUnit(MatchMessageUnitMoved message)
		{
			PlayerColor color = message.OwnerId == _connection.Session.UserId ? PlayerColor.Black : PlayerColor.Red;
			Unit unit = _units[color][message.UnitId];
			Node node = Scene02BattleController.Instance.Nodes[message.NodeX, message.NodeY];
			unit.Move(node);
		}

		/// <summary>
		/// Order unit to show attack given in the match state message
		/// </summary>
		/// <param name="message"></param>
		private void MakeUnitAttack(MatchMessageUnitAttacked message)
		{
			PlayerColor color = message.OwnerId == _connection.Session.UserId ? PlayerColor.Black : PlayerColor.Red;
			PlayerColor enemyColor = color == PlayerColor.Black ? PlayerColor.Red : PlayerColor.Black;
			Unit unit = _units[color][message.UnitId];
			Unit enemy = _units[enemyColor][message.EnemyId];

			unit.Attack(enemy, message.Damage, message.AttackType);
		}
	}
}
