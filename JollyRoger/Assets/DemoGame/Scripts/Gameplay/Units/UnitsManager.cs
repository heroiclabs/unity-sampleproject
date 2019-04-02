/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch and Paweł Stolarczyk
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
using DemoGame.Scripts.Utils;
using DemoGame.Scripts.Gameplay.NetworkCommunication;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Gameplay.NetworkCommunication.MatchStates;
using DemoGame.Scripts.Session;
using DemoGame.Scripts.Gameplay.Nodes;

namespace DemoGame.Scripts.Gameplay.Units
{

    public class UnitsManager : Singleton<UnitsManager>
    {
        [SerializeField]
        private List<Unit> _blackForts;
        [SerializeField]
        private List<Unit> _redForts;

        /// <summary>
        /// Dictionary containing Dictionary of Units ids to units objects for each player
        /// </summary>
        private Dictionary<PlayerColor, Dictionary<int, Unit>> _units = new Dictionary<PlayerColor, Dictionary<int, Unit>>();

        /// <summary>
        /// Next id to give for new unit
        /// </summary>
        private int _nextId = 0;

        #region MONO

        private void Start()
        {
            _units[PlayerColor.Black] = new Dictionary<int, Unit>();
            _units[PlayerColor.Red] = new Dictionary<int, Unit>();

            MatchCommunicationManager.Instance.OnCardPlayed += CardPlayed;
            MatchCommunicationManager.Instance.OnUnitMoved += MoveUnit;
            MatchCommunicationManager.Instance.OnUnitAttacked += MakeUnitAttack;
            MatchCommunicationManager.Instance.OnUnitSpawned += SpawnUnit;
        }

        protected override void OnDestroy()
        {
            MatchCommunicationManager.Instance.OnCardPlayed -= CardPlayed;
            MatchCommunicationManager.Instance.OnUnitMoved -= MoveUnit;
            MatchCommunicationManager.Instance.OnUnitSpawned -= SpawnUnit;
        }

        #endregion

        #region PUBLIC METHODS

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
        public void RemoveUnit(Unit unit)
        {
            _units[unit.OwnerColor].Remove(unit.Id);
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

        /// <summary>
        /// Builds forts at the start of the game
        /// </summary>
        /// <param name="userId"></param>
        public void BuildStartingStructures(string userId)
        {
            if (MatchCommunicationManager.Instance.IsHost == false)
            {
                Debug.Log("Only host can spawn starting structures.");
                return;
            }

            Card fortCard = new Card()
            {
                cardType = CardType.Fort,
                level = 1
            };
            Card mainFortCard = new Card()
            {
                cardType = CardType.MainFort,
                level = 1
            };

            if (userId == MatchCommunicationManager.Instance.HostId)
            {
                MatchMessageUnitSpawned fortTop = new MatchMessageUnitSpawned(
                    userId, PlayerColor.Black, _nextId++, fortCard, 4, 10);
                MatchMessageUnitSpawned fortBot = new MatchMessageUnitSpawned(
                    userId, PlayerColor.Black, _nextId++, fortCard, 4, 2);
                MatchMessageUnitSpawned fortMain = new MatchMessageUnitSpawned(
                    userId, PlayerColor.Black, _nextId++, mainFortCard, 2, 6);

                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortTop);
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortBot);
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortMain);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortTop);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortBot);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortMain);

                Unit tower2 = _units[PlayerColor.Black][_nextId - 3];
                Unit tower1 = _units[PlayerColor.Black][_nextId - 2];
                Unit castle = _units[PlayerColor.Black][_nextId - 1];
                GameManager.Instance.SetTowers(PlayerColor.Black, castle, tower1, tower2);
            }
            else
            {
                MatchMessageUnitSpawned fortTop = new MatchMessageUnitSpawned(
                    userId, PlayerColor.Red, _nextId++, fortCard, 10, 10);
                MatchMessageUnitSpawned fortBot = new MatchMessageUnitSpawned(
                    userId, PlayerColor.Red, _nextId++, fortCard, 10, 2);
                MatchMessageUnitSpawned fortMain = new MatchMessageUnitSpawned(
                    userId, PlayerColor.Red, _nextId++, mainFortCard, 12, 6);

                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortTop);
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortBot);
                MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitSpawned, fortMain);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortTop);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortBot);
                MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, fortMain);

                Unit tower2 = _units[PlayerColor.Red][_nextId - 3];
                Unit tower1 = _units[PlayerColor.Red][_nextId - 2];
                Unit castle = _units[PlayerColor.Red][_nextId - 1];
                GameManager.Instance.SetTowers(PlayerColor.Red, castle, tower1, tower2);
            }
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Resolves card playing, then spawns unit of given type 
        /// </summary>
        /// <param name="message"></param>
        private void CardPlayed(MatchMessageCardPlayed message)
        {
            if (MatchCommunicationManager.Instance.IsHost == true)
            {
                MatchMessageUnitSpawned unitSpawned = null;
                switch (message.Card.cardType)
                {
                    case CardType.BigShip:
                    case CardType.AoEShip:
                    case CardType.Boats:
                        int id = _nextId + 1;
                        PlayerColor color = message.PlayerId == MatchCommunicationManager.Instance.HostId ? PlayerColor.Black : PlayerColor.Red;
                        unitSpawned = new MatchMessageUnitSpawned(message.PlayerId, color, id, message.Card, message.NodeX, message.NodeY);
                        break;
                    default:
                        break;
                }

                if (unitSpawned != null)
                {
                    _nextId += 1;
                    MatchCommunicationManager.Instance.SendMatchStateMessage(MatchMessageType.UnitSpawned, unitSpawned);
                    MatchCommunicationManager.Instance.SendMatchStateMessageSelf(MatchMessageType.UnitSpawned, unitSpawned);
                }
            }
        }

        /// <summary>
        /// Spawns new unit and adds it to dictionary
        /// </summary>
        /// <param name="message"></param>
        private void SpawnUnit(MatchMessageUnitSpawned message)
        {
            Debug.Log("Spawned Unit Beg: " + message.UnitId);
            CardType cardType = message.Card.GetCardInfo().CardType;
            Node node = GameManager.Instance.Nodes[message.NodeX, message.NodeY];
            PlayerColor player = message.OwnerId == NakamaSessionManager.Instance.Session.UserId ? PlayerColor.Black : PlayerColor.Red;
            int id = message.UnitId;
            string ownerId = message.OwnerId;

            Quaternion rotation;
            if (MatchCommunicationManager.Instance.IsHost)
            {
                rotation = player == PlayerColor.Black ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
            }
            else
            {
                rotation = player == PlayerColor.Black ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
            }

            GameObject prefab = Resources.Load<GameObject>("Units/" + cardType.ToString() + "_" + player.ToString());

            Unit unit = Instantiate(prefab, node.transform.position, rotation, transform).GetComponent<Unit>();
            if (unit)
            {
                unit.Init(player, ownerId, id, node, message.Card);
                Debug.Log("Spawned Unit: " + id);
                _units[player].Add(id, unit);
                unit.OnDestroy += RemoveUnit;
            }
        }

        /// <summary>
        /// Starts unit movement towards given node
        /// </summary>
        /// <param name="message"></param>
        private void MoveUnit(MatchMessageUnitMoved message)
        {
            PlayerColor color = message.OwnerId == NakamaSessionManager.Instance.Session.UserId ? PlayerColor.Black : PlayerColor.Red;
            //Debug.Log("Moved Unit: " + message.UnitId);
            Unit unit = _units[color][message.UnitId];
            Node node = GameManager.Instance.Nodes[message.NodeX, message.NodeY];

            unit.Move(node);
        }

        /// <summary>
        /// Order unit to show attack given in the match state message
        /// </summary>
        /// <param name="message"></param>
        private void MakeUnitAttack(MatchMessageUnitAttacked message)
        {
            PlayerColor color = message.OwnerId == NakamaSessionManager.Instance.Session.UserId ? PlayerColor.Black : PlayerColor.Red;
            PlayerColor enemyColor = color == PlayerColor.Black ? PlayerColor.Red : PlayerColor.Black;
            Unit unit = _units[color][message.UnitId];
            Unit enemy = _units[enemyColor][message.EnemyId];

            unit.Attack(enemy, message.Damage, message.AttackType);
        }

        #endregion
    }

}