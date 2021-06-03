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

using Nakama;
using Nakama.TinyJson;
using PiratePanic.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PiratePanic
{
	/// <summary>
	/// Core gameplay manager. Handles card playing, game ending and initialization.
	/// </summary>
	public class Scene02BattleController : Singleton<Scene02BattleController>
	{
		/// <summary>
		/// Reference to camera parent object.
		/// This transform will be rotated by 180 degrees if local user is not host.
		/// </summary>
		[SerializeField] private Transform _cameraHolder = null;

		/// <summary>
		/// Reference to summary menu shown at the end of the match.
		/// </summary>
		[SerializeField] private SummaryMenu _summary = null;

		/// <summary>
		/// In-game time of beginning of the match.
		/// </summary>
		private float _timerStart;

		/// <summary>
		/// Reference to the hand side bar with cards.
		/// </summary>
		[SerializeField] private HandPanel _localHandPanel = null;

		/// <summary>
		/// User hand model.
		/// </summary>
		private ReplicatedList<Card> _hand = new ReplicatedList<Card>();

		/// <summary>
	    /// All cards played within this match for a user.
    	/// </summary>
		private ReplicatedList<PlayedCard> _playedCards = new ReplicatedList<PlayedCard>();

		/// <summary>
	    /// The card collection of a user.
    	/// </summary>
		private Replicated<CardCollection> _cardCollection = new Replicated<CardCollection>();

		/// <summary>
		/// Reference to the bottom panel displaying current gold.
		/// </summary>
		[SerializeField] private GoldPanel _localGoldPanel = null;

		/// <summary>
		/// The gold of a user.
		/// </summary>
		private Replicated<int> _gold = null;

		/// <summary>
		/// 2d array of nodes units can move to during a match.
		/// Initialized by <see cref="NodeMapManager"/>.
		/// </summary>
		public Node[,] Nodes { get; private set; }

		/// <summary>
		/// Map size in nodes.
		/// </summary>
		public Vector2Int MapSize { get; private set; }

		[SerializeField] private GameConnection _connection;

		private GameStateManager _stateManager;
		private UnitsManager _unitsManager;
		private SpellsManager _spellsManager;

		protected override void Awake()
		{
			_playedCards.CanAdd = (playedCard) => {
				int cost = playedCard.CardData.GetCardInfo().Cost;
				_gold.ForClient(card.PlayerId) < cost;
			};

			DrawCards(3);

			Card newCard = userHand.DrawCard();

			bool isHost = _connection.BattleConnection.HostId == _connection.Session.UserId;
			Vector3 position = new Vector3(message.X, message.Y, message.Z);
			Vector2Int nodePosition = ScreenToNodePos(position, isHost, message.Card.CardData.GetCardInfo().DropRegion);
			Node node = Nodes[nodePosition.x, nodePosition.y];

			if (node != null && (node.Unit == null || message.Card.CardData.GetCardInfo().CanBeDroppedOverOtherUnits))
			{
				Card newCard = userHand.DrawCard();
			}

			_stateManager = new GameStateManager(_connection);
			_localHandPanel.OnCardPlayed += HandleUICardPlayed;
			_localHandPanel.Init(_connection, _stateManager);

			_summary.SetBackButtonHandler(() =>
			{
				_summary.Hide();
				_stateManager.LeaveGame();
			});

			_unitsManager = new UnitsManager(_connection, _stateManager);
			_unitsManager.OnAfterUnitInstantiated += HandleAfterUnitInstantiated;

			base.Awake();
		}

		protected async void Start()
		{
			IMatch match = await _connection.Socket.JoinMatchAsync(_connection.BattleConnection.Matched);

			_connection.BattleConnection.MatchId = match.Id;

			if (match.Presences.Count() == 1)
			{
				string opponentId = match.Presences.First().UserId;
				_connection.BattleConnection.OpponentId = opponentId;
				_connection.BattleConnection.HostId = opponentId;
			}
			else
			{
				_connection.BattleConnection.HostId = _connection.Session.UserId;
				_connection.Socket.ReceivedMatchPresence += HandleOtherPlayerJoin;
			}

			_spellsManager = new SpellsManager(_stateManager, _unitsManager, _connection.BattleConnection.HostId == _connection.Session.UserId);
		}

		private void SetInitialPlayerState()
		{
			if (_connection.BattleConnection.HostId == _connection.Session.UserId)
			{
				_unitsManager.BuildStartingStructures(_connection.Session.UserId);
				_unitsManager.BuildStartingStructures(_connection.BattleConnection.OpponentId);

				_timerStart = Time.unscaledTime;
			}
			else
			{
				_cameraHolder.Rotate(Vector3.up, 180);
			}
		}

		private void HandleOtherPlayerJoin(IMatchPresenceEvent obj)
		{
			if (!obj.Joins.Any())
			{
				return;
			}

			string opponentId = obj.Joins.Select(join => join.UserId).First(id => id != _connection.Session.UserId);
			_connection.BattleConnection.OpponentId = opponentId;
			_connection.Socket.ReceivedMatchPresence -= HandleOtherPlayerJoin;
			SetInitialPlayerState();
		}

		/// <summary>
		/// Handles exitting the game.
		/// </summary>
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				_stateManager.LeaveGame();
			}
		}

		/// <summary>
		/// Initializes nodes in the map.
		/// Invoked by <see cref="NodeMapManager"/>.
		/// </summary>
		public void InitMap(Node[,] nodes, Vector2Int mapSize)
		{
			Nodes = nodes;
			MapSize = mapSize;
		}

		/// <summary>
		/// Takes the first card from user's deck and places it in hand.
		/// If there are no cards left in deck, shuffles all already played
		/// cards and puts them in deck.
		/// </summary>
		private Card DrawCard()
		{
			int index = UnityEngine.Random.Range(0, _cardCollection.GetDeckList().Count);
			string randId = _cardCollection.GetDeckList()[index];
			return _cardCollection.GetDeckCard(randId);
		}

		private async List<Card> DrawCards(int numCards)
		{
			var response = await _connection.Client.RpcAsync(_connection.Session, "load_user_cards", "");
			_cardCollection = response.Payload.FromJson<CardCollection>();
		}

		/// <summary>
		/// Handles card play request.
		/// If user playing the card has insufficient gold or used card can not be played on specified node,
		/// a cancel message is sent to that card owner and its effects don't resolve.
		/// </summary>
		private async void HandleUICardPlayed(PlayedCard card)
		{
			try
			{
				await _playedCards.Add(card);
			}
			catch (ReplicationException e)
			{
				// TODO handle cancel
			}
		}

		/// <summary>
		/// Invoked whenever a card is played and it wan't canceled.
		/// Removes that card from its owner's hand and reduces their gold count by <see cref="CardInfo.Cost"/>.
		/// Card owner draws a new card from their deck.
		/// </summary>
		/// <param name="message"></param>
		private void OnCardPlayed(PlayedCard card)
		{
			SoundManager.Instance.PlayAudioClip(SoundConstants.CardDrop01);

			_localHandPanel.ResolveCardPlay(card.HandIndex);
			_localHandPanel.DrawCard(card.Card, card.HandIndex);
			_gold -= card.Card.CardData.GetCardInfo().Cost;
		}

		/// <summary>
		/// Given the <see cref="DropRegion"/>, returns a node closest to given world position.
		/// </summary>
		public Vector2Int ScreenToNodePos(Vector3 position, bool isHost, DropRegion dropRegion)
		{
			switch (dropRegion)
			{
				case DropRegion.WholeMap:
					return GetClosestNode_WholeMap(position);
				case DropRegion.EnemyHalf:
					return GetClosestNode_HalfMap(position, !isHost);
				case DropRegion.EnemySpawn:
					return GetClosestNode_Spawn(position, !isHost);
				case DropRegion.AllyHalf:
					return GetClosestNode_HalfMap(position, isHost);
				case DropRegion.AllySpawn:
					return GetClosestNode_Spawn(position, isHost);
				default:
					break;
			}

			Debug.LogError("Drop region " + dropRegion + " was not handled");
			return new Vector2Int();
		}

		/// <summary>
		/// Returns a node from the first (or last) column of the map (spawn region).
		/// </summary>
		private Vector2Int GetClosestNode_Spawn(Vector3 position, bool leftSide)
		{
			float minDistance = float.MaxValue;
			Node closestNode = null;
			int x = leftSide == true ? 0 : Nodes.GetLength(0) - 1;

			for (int y = 0; y < Nodes.GetLength(1); y += 2)
			{
				Node node = Nodes[x, y];
				if (node == null) continue;
				float distance = Vector3.Distance(position, node.transform.position);
				if (distance < minDistance)
				{
					minDistance = distance;
					closestNode = node;
				}
			}

			return closestNode.Position;
		}

		/// <summary>
		/// Returns a node cosest to the <paramref name="position"/> from a half of the map,
		/// depending on the value of <paramref name="leftSide"/>.
		/// </summary>
		private Vector2Int GetClosestNode_HalfMap(Vector3 position, bool leftSide)
		{
			float minDistance = float.MaxValue;
			Node closestNode = null;

			for (int y = 0; y < Nodes.GetLength(1); y++)
			{
				int width = y % 2 == 0 ? Nodes.GetLength(0) : Nodes.GetLength(0) - 1;
				int half = width / 2;

				int beg = leftSide == true ? 0 : width - half;
				int end = leftSide == true ? half : width;

				for (int x = beg; x < end; x++)
				{
					Node node = Nodes[x, y];
					if (node == null) continue;
					float distance = Vector3.Distance(position, node.transform.position);
					if (distance < minDistance)
					{
						minDistance = distance;
						closestNode = node;
					}
				}
			}

			return closestNode.Position;
		}

		/// <summary>
		/// Returns a node closest to <paramref name="position"/>.
		/// </summary>
		private Vector2Int GetClosestNode_WholeMap(Vector3 position)
		{
			float minDistance = float.MaxValue;
			Node closestNode = null;

			for (int x = 0; x < Nodes.GetLength(0); ++x)
			{
				for (int y = 0; y < Nodes.GetLength(1); ++y)
				{
					Node node = Nodes[x, y];
					if (node == null) continue;
					float distance = Vector3.Distance(position, node.transform.position);
					if (distance < minDistance)
					{
						minDistance = distance;
						closestNode = node;
					}
				}
			}

			return closestNode.Position;
		}

		private void HandleAfterUnitInstantiated(Unit instantiatedUnit)
		{
			if (instantiatedUnit.Card.type == CardType.MainFort)
			{
				instantiatedUnit.OnAfterDestroyed += OnAfterMainFortDestroyed;
			}
		}

		private async void OnAfterMainFortDestroyed(string playerId, int unitId)
		{
			bool allyDestroyed = playerId == _connection.Session.UserId;
			string loser = allyDestroyed ? _connection.Session.UserId : _connection.BattleConnection.OpponentId;
			string winner = allyDestroyed ? _connection.BattleConnection.OpponentId : _connection.Session.UserId;
			PlayerColor winnerColor = allyDestroyed ? PlayerColor.Black : PlayerColor.Red;
			PlayerColor loserColor = allyDestroyed ? PlayerColor.Red : PlayerColor.Black;

			string matchId = _connection.BattleConnection.MatchId;
			float matchDuration = Time.unscaledTime - _timerStart;
			int winnerTowersDestroyed = 1 + _unitsManager.NumDestroyedTowers(winnerColor);
			int loserTowersDestroyed = 0 + _unitsManager.NumDestroyedTowers(loserColor);

			var placement = allyDestroyed ? MatchEndPlacement.Loser : MatchEndPlacement.Winner;

			var matchEndRequest = new MatchEndRequest(matchId,
				placement,
				matchDuration,
				allyDestroyed ? loserTowersDestroyed : winnerTowersDestroyed);

			IApiRpc response;

			try
			{
				response = await _connection.Client.RpcAsync(_connection.Session, "handle_match_end", matchEndRequest.ToJson());
				var matchEndResponse = response.Payload.FromJson<MatchEndResponse>();
				_summary.Init(matchEndResponse.Gems, placement, matchEndResponse.Score);
				_summary.Show();
			}
			catch (ApiResponseException e)
			{
				Debug.LogError("Error handling match end " + e.Message);
				_stateManager.LeaveGame();
			}
		}

		private async void OnApplicationQuit()
		{
			await _connection.Socket.CloseAsync();
		}
	}
}
