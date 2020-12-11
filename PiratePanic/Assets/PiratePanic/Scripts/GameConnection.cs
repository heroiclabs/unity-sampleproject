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
using UnityEngine;

namespace PiratePanic
{
	/// <summary>
	/// Manages Nakama server user authentication and interation wrapping the
	/// <see cref="IClient"/>, <see cref="ISession"/>, 
	/// <see cref="IApiAccount"/>, and <see cref="ISocket"/>.
	/// </summary>
	/// <remarks>
	/// 
	/// Whenever a user tries to communicate with game server it ensures that 
	/// their session hasn't expired. If the session is expired the user will 
	/// have to reauthenticate the session and obtain a new session.
	/// 
	/// </remarks>
	[CreateAssetMenu(fileName = "GameConnection", 
	menuName = GameConstants.CreateAssetMenu_GameConnection)]
	public class GameConnection : ScriptableObject
	{
		/// <summary>
		/// Used to establish connection between the client and the server.
		/// Contains a list of usefull methods required to communicate with Nakama server.
		/// 
		/// Do not use this directly, use <see cref="Client"/> instead.
		/// 
		/// </summary>
		private IClient _client;

		/// <summary>
		/// Used to communicate with Nakama server.
		/// 
		/// For the user to send and receive messages from the server, 
		/// <see cref="Session"/> must not be expired.
		/// 
		/// Default expiration time is 60s, but for this demo we set it 
		/// to 3 weeks (1 814 400 seconds).
		/// 
		/// To initialize the session, call <see cref="AuthenticateDeviceIdAsync"/> 
		/// or <see cref="AuthenticateFacebookAsync"/> methods.
		/// 
		/// To reinitialize expired session, call <see cref="Reauthenticate"/> method.
		/// </summary>
		public ISession Session { get; set; }

		/// <summary>
		/// Contains all the identifying data of a <see cref="Client"/>, 
		/// like User Id, linked Device IDs, linked Facebook account, username, etc.
		/// </summary>
		public IApiAccount Account { get; set; }

		/// <summary>
		/// Socket responsible for maintaining connection with Nakama 
		/// server and exchanger realtime messages.
		/// 
		/// Do not use this directly, use <see cref="Socket"/> instead.
		/// 
		/// </summary>
		private ISocket _socket;

		/// <summary>
		/// Used to establish connection between the client and the server.
		/// Contains a list of usefull methods required to communicate with Nakama server.
		/// </summary>
		public IClient Client => _client;

		/// <summary>
		/// Socket responsible for maintaining connection with Nakama server 
		/// and exchange realtime messages.
		/// </summary>
		public ISocket Socket => _socket;

		public BattleConnection BattleConnection { get; set; }

		public void Init(	IClient client, ISocket socket, 
							IApiAccount account, ISession session)
		{
			_client = client;
			_socket = socket;
			Account = account;
			Session = session;
		}
	}
}
