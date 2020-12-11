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
using UnityEngine.UI;
using Nakama;
using System;
using Nakama.TinyJson;

namespace PiratePanic
{
    /// <summary>
    /// Manages user interface for chat
    /// </summary>
    public class ChatChannelUI : MonoBehaviour
	{
		[Header("Prefabs")]

		/// <summary>
		/// Object used for instantiating local user messages
		/// </summary>
		[SerializeField] private GameObject _thisUserMessagePrefab = null;

		/// <summary>
		/// Object used for instantiating other users messages
		/// </summary>
		[SerializeField] private GameObject _otherUserMessagePrefab = null;

		/// <summary>
		/// Object used for instantiating server messages
		/// </summary>
		[SerializeField] private GameObject _serverMessagePrefab = null;

		[Space()]
		[Header("UI Elements")]

		/// <summary>
		/// Text component for viewing chat channel name
		/// </summary>
		[SerializeField] protected Text _chatNameText = null;

		/// <summary>
		/// Button for closing chat
		/// </summary>
		[SerializeField] private Button _closeButton = null;

		/// <summary>
		/// Button for sending message to channel
		/// </summary>
		[SerializeField] private Button _sendMessageButton = null;

		/// <summary>
		/// Input field for typing messages
		/// </summary>
		[SerializeField] private InputField _chatInputField = null;

		/// <summary>
		/// Parent of all chat messages
		/// </summary>
		[SerializeField] private RectTransform _content = null;

		/// <summary>
		/// All chat messages
		/// </summary>
		private Dictionary<string, ChatMessageUI> _messages = new Dictionary<string, ChatMessageUI>();

		/// <summary>
		/// Username of user who send last message
		/// </summary>
		private string _lastMessageUsername;

		protected GameConnection _connection;

		protected IChannel _chatChannel;

		public void Init(GameConnection connection)
		{
			_connection = connection;
		}

		/// <summary>
		/// Invoked when object is set to enabled
		/// </summary>
		private void OnEnable()
		{
			//Registering methods to buttons
			_sendMessageButton.onClick.AddListener(SendMessage);
			_closeButton.onClick.AddListener(CloseChannelUI);
			_chatInputField.onEndEdit.AddListener(SendMessageIfReturnButton);
		}

		/// <summary>
		/// Invoked when object is set to disabled
		/// </summary>
		private void OnDisable()
		{
			//Unregistering methods to buttons
			_sendMessageButton.onClick.RemoveListener(SendMessage);
			_closeButton.onClick.RemoveListener(CloseChannelUI);
		}

		/// <summary>
		/// Sets current <see cref="ChatChannel"/> and unplug previous if was
		/// </summary>
		public virtual void SetChatChannel(IChannel channel)
		{
			//Checking if chat channel was set previously
			if (_chatChannel != null)
			{
				//If previous instance is the same as new - return
				if (_chatChannel == channel)
				{
					return;
				}

				_connection.Socket.ReceivedChannelMessage -= AddMessage;
			}

			//Setting new ChatChannel
			_chatChannel = channel;

			//Register methods to new ChatChannel
			_connection.Socket.ReceivedChannelMessage += AddMessage;

			//Setting channel name
			_chatNameText.text = channel.RoomName;

			//Reseting last message username
			_lastMessageUsername = "";
		}

		/// <summary>
		/// Adds new user message to channel ui
		/// </summary>
		private void AddMessage(IApiChannelMessage message)
		{
			//Declaring temp variables
			GameObject messagePrefab;

			//Checking if user who send message is local user
			if (message.SenderId == _connection.Account.User.Id)
			{
				messagePrefab = _thisUserMessagePrefab;
			}
			else
			{
				messagePrefab = _otherUserMessagePrefab;
			}

			// Instantiating message object as a child of _content
			GameObject messageGO = Instantiate(messagePrefab, _content) as GameObject;

			// Getting ChatMessageUI component from already instantiated message
			ChatMessageUI messageUI = messageGO.GetComponent<ChatMessageUI>();

			if (messageUI)
			{
				// If the message is from the same user as newest we should hide his username on this message
				bool hideUsername = (message.Username == _lastMessageUsername) && !message.Persistent;

				// Initializing message with given data
				messageUI.InitMessage(message.MessageId, message.Username, message.Content.FromJson<Dictionary<string, string>>()["content"], message.CreateTime, hideUsername);

				// Adding message to messages dict
				_messages.Add(message.MessageId, messageUI);

				// Setting last
				_lastMessageUsername = message.Username;

				//If message is historical change order in hierarchy, the latest historical message is the oldest
				if (message.Persistent)
				{
					messageUI.transform.SetSiblingIndex(1);
				}
			}
			else
			{
				Debug.LogError("Invalid _thisUserMessagePrefab or _otherUserMessagePrefab! It should contains ChatMessageUI script.");
				Destroy(messageGO);
				return;
			}
		}

		/// <summary>
		/// Adds new server message to chanel ui
		/// </summary>
		private void AddServerMessage(string messageId, string content, bool historical)
		{
			//Instantiating message object as a child of _content
			GameObject messageGO = Instantiate(_serverMessagePrefab, _content) as GameObject;

			//Getting ChatServerMessageUI component from already instantiated message
			ChatServerMessageUI message = messageGO.GetComponent<ChatServerMessageUI>();

			if (message)
			{
				//Initializing message with content
				message.Init(content);

				//Reseting last message user username (server doesn't have username)
				_lastMessageUsername = "";

				//If message is historical change order in hierarchy, the latest historical message is the oldest
				if (historical)
				{
					message.transform.SetSiblingIndex(1);
				}
			}
			else
			{
				Debug.LogError("Invalid _serverMessagePrefab! It should contains ChatServerMessageUI script.");
				Destroy(messageGO);
				return;
			}
		}

		/// <summary>
		/// Clear all previously loaded messages in ui
		/// </summary>
		private void ClearMessages()
		{
			List<ChatMessageUI> messages = new List<ChatMessageUI>(_messages.Values);
			for (int i = 0; i < messages.Count; i++)
			{
				Destroy(messages[i].gameObject);
			}
			_messages.Clear();
		}

		/// <summary>
		/// Sending new message to channel
		/// </summary>
		private void SendMessage()
		{
			if (string.IsNullOrEmpty(_chatInputField.text))
			{
				return;
			}

			try
			{
				var content = new Dictionary<string, string>(){{"content", _chatInputField.text}}.ToJson();
				_connection.Socket.WriteChatMessageAsync(_chatChannel.Id, content);
			}
			catch (Exception e)
			{
				Debug.LogWarning("Error writing chat message: " + e.Message);
			}

			_chatInputField.text = "";
		}

		/// <summary>
		/// Method that helps to identify if enter key was clicked to commit new message when end editing input field text.
		/// </summary>
		private void SendMessageIfReturnButton(string value)
		{
			if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(value))
			{
				SendMessage();
			}
		}

		/// <summary>
		/// Closes chat window
		/// </summary>
		private async void CloseChannelUI()
		{
			try
			{
				await _connection.Socket.LeaveChatAsync(_chatChannel.Id);
			}
			catch (Exception e)
			{
				Debug.LogError("Error leaving chat : " + e.Message);
			}

			ClearMessages();
			gameObject.SetActive(false);
		}
	}
}
