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

using System;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace PiratePanic
{
	/// <summary>
	/// Manages Nakama client-server communication including sending/recieving MatchState.
	/// </summary>
	public class GameStateManager
	{
		//This region contains events for all type of match messages that could be send in the game.
		//Events are fired after getting message sent by other players from Nakama server

		//UNITS
		public event Action<MatchMessageUnitSpawned> OnUnitSpawned;
		public event Action<MatchMessageUnitMoved> OnUnitMoved;
		public event Action<MatchMessageUnitAttacked> OnUnitAttacked;

		//SPELLS
		public event Action<MatchMessageSpellActivated> OnSpellActivated;

		//CARDS
		public event Action<MatchMessageCardPlayRequest> OnCardRequested;
		public event Action<MatchMessageCardPlayed> OnCardPlayed;
		public event Action<MatchMessageCardCanceled> OnCardCancelled;
		public event Action<MatchMessageStartingHand> OnStartingHandReceived;

		/// <summary>
		/// Indicates if player is already leaving match
		/// </summary>
		private bool _isLeaving;

		private GameConnection _connection;

		public GameStateManager(GameConnection connection)
		{
			_connection = connection;

			// Listen to incomming match messages and user connection changes
			_connection.Socket.ReceivedMatchPresence += OnMatchPresence;
			_connection.Socket.ReceivedMatchState += ReceiveMatchStateMessage;
		}

		/// <summary>
		/// Starts procedure of leaving match by local player
		/// </summary>
		public async void LeaveGame()
		{
			if (_isLeaving)
			{
				return;
			}

			_isLeaving = true;

			_connection.Socket.ReceivedMatchPresence -= OnMatchPresence;
			_connection.Socket.ReceivedMatchState -= ReceiveMatchStateMessage;

			try
			{
				//Sending request to Nakama server for leaving match
				await _connection.Socket.LeaveMatchAsync(_connection.BattleConnection.MatchId);
			}
			catch (Exception e)
			{
				Debug.LogWarning("Error leaving match: " + e.Message);
			}

			_connection.BattleConnection = null;

			SceneManager.LoadSceneAsync(GameConfigurationManager.Instance.GameConfiguration.SceneNameMainMenu);
			SceneManager.UnloadSceneAsync(GameConfigurationManager.Instance.GameConfiguration.SceneNameBattle);
		}

		/// <summary>
		/// This method sends match state message to other players through Nakama server.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="opCode"></param>
		/// <param name="message"></param>
		public void SendMatchStateMessage<T>(MatchMessageType opCode, T message)
			where T : MatchMessage<T>
		{
			try
			{
				//Packing MatchMessage object to json
				string json = MatchMessage<T>.ToJson(message);

				//Sending match state json along with opCode needed for unpacking message to server.
				//Then server sends it to other players
				_connection.Socket.SendMatchStateAsync(_connection.BattleConnection.MatchId, (long)opCode, json);
			}
			catch (Exception e)
			{
				Debug.LogError("Error while sending match state: " + e.Message);
			}
		}


		/// <summary>
		/// This method is used by host to invoke locally event connected with match message which is sent to other players.
		/// Should be always runned on host client after sending any message, otherwise some of the game logic would not be runned on host game instance.
		/// Don't use this method when client is not a host!
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="opCode"></param>
		/// <param name="message"></param>
		public void SendMatchStateMessageSelf<T>(MatchMessageType opCode, T message)
			where T : MatchMessage<T>
		{
			//Choosing which event should be invoked basing on opCode and firing event
			switch (opCode)
			{
				//UNITS
				case MatchMessageType.UnitSpawned:
					OnUnitSpawned?.Invoke(message as MatchMessageUnitSpawned);
					break;

				case MatchMessageType.UnitMoved:
					OnUnitMoved?.Invoke(message as MatchMessageUnitMoved);
					break;

				case MatchMessageType.UnitAttacked:
					OnUnitAttacked?.Invoke(message as MatchMessageUnitAttacked);
					break;

				//SPELLS
				case MatchMessageType.SpellActivated:
					OnSpellActivated?.Invoke(message as MatchMessageSpellActivated);
					break;

				//CARDS
				case MatchMessageType.CardPlayRequest:
					OnCardRequested?.Invoke(message as MatchMessageCardPlayRequest);
					break;

				case MatchMessageType.CardPlayed:
					OnCardPlayed?.Invoke(message as MatchMessageCardPlayed);
					break;

				case MatchMessageType.CardCanceled:
					OnCardCancelled?.Invoke(message as MatchMessageCardCanceled);
					break;

				case MatchMessageType.StartingHand:
					OnStartingHandReceived?.Invoke(message as MatchMessageStartingHand);
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Reads match messages sent by other players, and fires locally events basing on opCode.
		/// </summary>
		/// <param name="opCode"></param>
		/// <param name="messageJson"></param>
		public void ReceiveMatchStateHandle(long opCode, string messageJson)
		{
			//Choosing which event should be invoked basing on opCode, then parsing json to MatchMessage class and firing event
			switch ((MatchMessageType)opCode)
			{
				//UNITS
				case MatchMessageType.UnitSpawned:
					MatchMessageUnitSpawned matchMessageUnitSpawned = MatchMessageUnitSpawned.Parse(messageJson);
					OnUnitSpawned?.Invoke(matchMessageUnitSpawned);
					break;

				case MatchMessageType.UnitMoved:
					MatchMessageUnitMoved matchMessageUnitMoved = MatchMessageUnitMoved.Parse(messageJson);
					OnUnitMoved?.Invoke(matchMessageUnitMoved);
					break;

				case MatchMessageType.UnitAttacked:
					MatchMessageUnitAttacked matchMessageUnitAttacked = MatchMessageUnitAttacked.Parse(messageJson);
					OnUnitAttacked?.Invoke(matchMessageUnitAttacked);
					break;

				//SPELLS
				case MatchMessageType.SpellActivated:
					MatchMessageSpellActivated matchMessageSpellActivated = MatchMessageSpellActivated.Parse(messageJson);
					OnSpellActivated?.Invoke(matchMessageSpellActivated);
					break;

				//CARDS
				case MatchMessageType.CardPlayRequest:
					if (_connection.BattleConnection.HostId == _connection.Account.User.Id)
					{
						MatchMessageCardPlayRequest matchMessageCardPlayRequest = MatchMessageCardPlayRequest.Parse(messageJson);
						OnCardRequested?.Invoke(matchMessageCardPlayRequest);
					}
					break;

				case MatchMessageType.CardPlayed:
					MatchMessageCardPlayed matchMessageCardPlayed = MatchMessageCardPlayed.Parse(messageJson);
					OnCardPlayed?.Invoke(matchMessageCardPlayed);
					break;


				case MatchMessageType.CardCanceled:
					MatchMessageCardCanceled matchMessageCardCancelled = MatchMessageCardCanceled.Parse(messageJson);
					OnCardCancelled?.Invoke(matchMessageCardCancelled);
					break;

				case MatchMessageType.StartingHand:
					MatchMessageStartingHand matchMessageStartingHand = MatchMessageStartingHand.Parse(messageJson);
					OnStartingHandReceived?.Invoke(matchMessageStartingHand);
					break;
			}
		}

		/// <summary>
		/// Method fired when any user leaves or joins the match
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMatchPresence(IMatchPresenceEvent e)
		{
			if (e.Leaves.Count() > 0)
			{
				Debug.LogWarning($"OnMatchPresence() User(s) left the game");
				LeaveGame();
			}
		}

		/// <summary>
		/// Decodes match state message json from byte form of matchState.State and then sends it to ReceiveMatchStateHandle
		/// for further reading and handling
		/// </summary>
		/// <param name="matchState"></param>
		private void ReceiveMatchStateMessage(IMatchState matchState)
		{
			string messageJson = System.Text.Encoding.UTF8.GetString(matchState.State);

			if (string.IsNullOrEmpty(messageJson))
			{
				return;
			}

			ReceiveMatchStateHandle(matchState.OpCode, messageJson);
		}
	}
}
