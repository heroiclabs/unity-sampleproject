/**
 * Copyright 2019 Heroic Labs and contributors
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
using DemoGame.Scripts.Session;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Chat
{
    /// <summary>
    /// Manages user interface for chat
    /// </summary>
    public class ChatChannelUI : MonoBehaviour
    {
        #region private serialized variables

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
        /// Button for loading more channel history
        /// </summary>
        [SerializeField] private Button _loadMoreHistoryButton = null;

        /// <summary>
        /// Button for sending message to channel
        /// </summary>
        [SerializeField] private Button _sendMessageButton = null;

        /// <summary>
        /// Button for sending edited changes to channel
        /// </summary>
        [SerializeField] private Button _editMessageButton = null;

        /// <summary>
        /// Button for canceling editing of message
        /// </summary>
        [SerializeField] private Button _cancelEditMessageButton = null;

        /// <summary>
        /// Input field for typing messages
        /// </summary>
        [SerializeField] private InputField _chatInputField = null;

        /// <summary>
        /// Parent of all chat messages
        /// </summary>
        [SerializeField] private RectTransform _content = null;

        #endregion

        #region protected and private vatiables

        /// <summary>
        /// Current <see cref="ChatChannel"/> which data is vieweds
        /// </summary>
        protected ChatChannel _chatChannel;

        /// <summary>
        /// All chat messages
        /// </summary>
        private Dictionary<string, ChatMessageUI> _messages = new Dictionary<string, ChatMessageUI>();

        /// <summary>
        /// Username of user who send last message
        /// </summary>
        private string _lastMessageUsername;

        /// <summary>
        /// Currently edited message
        /// </summary>
        private ChatMessageUI _editedMessage;

        #endregion

        #region Mono

        /// <summary>
        /// Invoked when object is set to enabled
        /// </summary>
        private void OnEnable()
        {
            //Registering methods to buttons
            _sendMessageButton.onClick.AddListener(SendMessage);
            _editMessageButton.onClick.AddListener(SendEditRequestAsync);
            _cancelEditMessageButton.onClick.AddListener(CancelEditing);
            _closeButton.onClick.AddListener(CloseChannelUI);
            _chatInputField.onEndEdit.AddListener(SendMessageIfReturnButton);
            _loadMoreHistoryButton.onClick.AddListener(LoadHistoryAsync);

            //Deactivating hidden elements
            _editMessageButton.gameObject.SetActive(false);
            _cancelEditMessageButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Invoked when object is set to disabled
        /// </summary>
        private void OnDisable()
        {
            //Unregistering methods to buttons
            _sendMessageButton.onClick.RemoveListener(SendMessage);
            _editMessageButton.onClick.RemoveListener(SendEditRequestAsync);
            _cancelEditMessageButton.onClick.RemoveListener(CancelEditing);
            _closeButton.onClick.RemoveListener(CloseChannelUI);
            _loadMoreHistoryButton.onClick.RemoveListener(LoadHistoryAsync);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Sets current <see cref="ChatChannel"/> and unplug previous if was
        /// </summary>
        public virtual void SetChatChannel(ChatChannel chatChannel)
        {
            //Checking if chat channel was set previously
            if (_chatChannel != null)
            {
                //If previous instance is the same as new - return
                if (_chatChannel == chatChannel)
                {
                    return;
                }

                //Unregister methods from previous channel
                _chatChannel.OnChatMessage -= AddMessage;
                _chatChannel.OnChatRemove -= RemoveMessage;
                _chatChannel.OnChatUpdate -= UpdateMessage;
                _chatChannel.OnServerMessage -= AddServerMessage;
            }

            //Setting new ChatChannel
            _chatChannel = chatChannel;

            //Register methods to new ChatChannel
            _chatChannel.OnChatMessage += AddMessage;
            _chatChannel.OnChatRemove += RemoveMessage;
            _chatChannel.OnChatUpdate += UpdateMessage;
            _chatChannel.OnServerMessage += AddServerMessage;

            //Setting channel name
            _chatNameText.text = chatChannel.ChannelName;

            //Reseting last message username
            _lastMessageUsername = "";

            //Loading recent history
            LoadHistoryAsync();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Adds new user message to channel ui
        /// </summary>
        private void AddMessage(string messageId, string userId, string username, string content, string date, bool historical)
        {
            //Declaring temp variables
            GameObject messagePrefab;
            bool couldBeEdited;

            //Checking if user who send message is local user
            if (userId == NakamaSessionManager.Instance.Account.User.Id)
            {
                messagePrefab = _thisUserMessagePrefab;
                couldBeEdited = true;
            }
            else
            {
                messagePrefab = _otherUserMessagePrefab;
                couldBeEdited = false;
            }

            //Instantiating message object as a child of _content
            GameObject messageGO = Instantiate(messagePrefab, _content) as GameObject;

            //Getting ChatMessageUI component from already instantiated message
            ChatMessageUI message = messageGO.GetComponent<ChatMessageUI>();

            if (message)
            {
                //If the message is from the same user as newest we should hide his username on this message
                bool hideUsername = (username == _lastMessageUsername) && !historical;

                //Initializing message with given data
                message.InitMessage(messageId, username, content, date, couldBeEdited, hideUsername);

                //Register edit and remove methods to message eventss
                message.OnEditMessageClicked += OnEditMessageClicked;
                message.OnRemoveMessageClicked += OnRemoveMessageClicked;

                //Adding message to messages dict
                _messages.Add(messageId, message);

                //Setting last
                _lastMessageUsername = username;

                //If message is historical change order in hierarchy, the latest historical message is the oldest
                if (historical)
                {
                    message.transform.SetSiblingIndex(1);
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
        /// Updates edited user message in channel ui
        /// </summary>
        private void UpdateMessage(string messageId, string content)
        {
            _messages[messageId].UpdateMessage(content);
        }

        /// <summary>
        /// Removes message from channel ui
        /// </summary>
        private void RemoveMessage(string messageId)
        {
            _messages[messageId].RemoveMessage();
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
            Debug.Log("Sending message: " + _chatInputField.text);
            ChatManager.Instance.SendNewMessage(_chatChannel.Id, _chatInputField.text);
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
        /// Sending update for message to channel
        /// </summary>
        private async void SendEditRequestAsync()
        {
            if (string.IsNullOrEmpty(_chatInputField.text))
            {
                return;
            }

            bool success = await ChatManager.Instance.EditMessageAsync(_chatChannel.Id, _editedMessage.Id, _chatInputField.text);

            //If operation end with succes close edit view and show normal buttons
            if (success)
            {
                _sendMessageButton.gameObject.SetActive(true);
                _editMessageButton.gameObject.SetActive(false);
                _cancelEditMessageButton.gameObject.SetActive(false);
                _editedMessage.Deselect();
                _chatInputField.text = "";
            }
        }


        /// <summary>
        /// Closes edit view and goes back to normal view
        /// </summary>
        private void CancelEditing()
        {
            _sendMessageButton.gameObject.SetActive(true);
            _editMessageButton.gameObject.SetActive(false);
            _cancelEditMessageButton.gameObject.SetActive(false);
            _editedMessage.Deselect();
            _chatInputField.text = "";
        }

        /// <summary>
        /// Shows edit view and starts editing message
        /// </summary>
        /// <param name="messageId"></param>
        private void OnEditMessageClicked(string messageId)
        {
            //If other message was edited cancel this edit
            if (_editedMessage)
            {
                _editedMessage.Deselect();
            }

            //Set new edited message
            _editedMessage = _messages[messageId];

            //Select message in UI
            _editedMessage.SetSelectedState();

            //Fill input field with actual message content
            _chatInputField.text = _messages[messageId].ContentTextValue;

            //Show edit and cancel buttons, hide send button
            _sendMessageButton.gameObject.SetActive(false);
            _editMessageButton.gameObject.SetActive(true);
            _cancelEditMessageButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Sends request to channel for removind message
        /// </summary>
        private async void OnRemoveMessageClicked(string messageId)
        {
            await ChatManager.Instance.RemoveMessageAsync(_chatChannel.Id, messageId);
        }

        /// <summary>
        /// Loads next part of channel history
        /// </summary>
        private async void LoadHistoryAsync()
        {
            //Deactivate button on time of loading
            _loadMoreHistoryButton.interactable = false;

            //Loading history request
            bool nextCursorIsntNull = await ChatManager.Instance.LoadChannelHistoryAsync(_chatChannel);

            //If more history is available activate button 
            if (nextCursorIsntNull)
            {
                _loadMoreHistoryButton.interactable = true;
            }
        }

        /// <summary>
        /// Closes chat window
        /// </summary>
        private async void CloseChannelUI()
        {
            bool succes = await ChatManager.Instance.LeaveChatChannelAsync(_chatChannel.Id);
            if (!succes)
            {
                Debug.LogError("Error occurred when leaving chat.");
            }
            ClearMessages();
            gameObject.SetActive(false);
        }


        #endregion
    }
}
