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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DemoGame.Scripts.Chat
{
    /// <summary>
    /// Manages chat message viewing in UI
    /// </summary>
    public class ChatMessageUI : MonoBehaviour, IPointerClickHandler
    {
        #region public events

        /// <summary>
        /// Fired when edit message button was clicked. Param = messageId.
        /// </summary>
        public event Action<string> OnEditMessageClicked = delegate { };

        /// <summary>
        /// Fired when remove message button was clicked. Param = messageId.
        /// </summary>
        public event Action<string> OnRemoveMessageClicked = delegate { };

        #endregion

        #region public properties

        public string ContentTextValue
        {
            get
            {
                return _contentText.text;
            }
        }

        public string Id
        {
            get
            {
                return _messageId;
            }
        }

        #endregion

        #region private serialized variables

        [Header("UI Elements")]

        [SerializeField] private Text _usernameText = null;

        [SerializeField] private Text _dateText = null;

        [SerializeField] private Text _contentText = null;

        [SerializeField] private Button _editButton = null;

        [SerializeField] private Button _removeButton = null;

        [SerializeField] private Image _panelImage = null;

        #endregion

        #region private variables

        private string _messageId;

        /// <summary>
        /// Forma used for viewing date of creating message
        /// </summary>
        private const string DATE_FORMAT = "M.d.yy H:mm";

        /// <summary>
        /// Indicates if message could be edited
        /// </summary>
        private bool _couldBeEdited = false;

        /// <summary>
        /// Indicates if message was removed
        /// </summary>
        private bool _removed = false;

        #endregion

        #region public methods

        /// <summary>
        /// Initializes message with given message data
        /// </summary>
        public void InitMessage(string messageId, string username, string content, string date, bool couldBeEdited, bool hideUsername)
        {
            _messageId = messageId;
            _couldBeEdited = couldBeEdited;

            //Hidding username or sets username text if not hidden
            if (hideUsername)
            {
                _usernameText.enabled = false;
            }
            else
            {
                _usernameText.text = username;
            }

            //Setting message date
            DateTime dateTime;

            if (DateTime.TryParse(date, out dateTime))
            {
                _dateText.text = dateTime.ToString(DATE_FORMAT);
            }
            else
            {
                _dateText.text = "?.?.?? ?:??";
            }

            //Setting content
            _contentText.text = content;

            //Registering methods to remove and edit buttons
            _removeButton.onClick.AddListener(OnRemoveButtonClicked);
            _editButton.onClick.AddListener(OnEditButtonClicked);
        }

        /// <summary>
        /// Updates message Text with new contents
        /// </summary>
        public void UpdateMessage(string content)
        {
            _contentText.text = content;

            //Stop hide remove and edit buttons after x seconds coroutine
            StopAllCoroutines();

            //Hiding remove and edit buttons
            _removeButton.gameObject.SetActive(false);
            _editButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Removes message viewing "message removed" as new content
        /// </summary>
        public void RemoveMessage()
        {
            _contentText.text = "message removed";
            _removed = true;

            //Stop hide remove and edit buttons after x seconds coroutine
            StopAllCoroutines();

            //Hiding remove and edit buttons
            _removeButton.gameObject.SetActive(false);
            _editButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Opens edit and remove buttons if message could be edited
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_removed && _couldBeEdited)
            {
                _removeButton.gameObject.SetActive(true);
                _editButton.gameObject.SetActive(true);
                StartCoroutine(HideAfterSomeSeconds(5f, new List<GameObject>() { _removeButton.gameObject, _editButton.gameObject }));
            }
        }

        /// <summary>
        /// Sets selected state (darker color) for editing
        /// </summary>
        public void SetSelectedState()
        {
            _panelImage.color = new Color(0.6f, 0.6f, 0.6f, 1);
        }

        /// <summary>
        /// Deselect message
        /// </summary>
        public void Deselect()
        {
            _panelImage.color = Color.white;
            _removeButton.gameObject.SetActive(false);
            _editButton.gameObject.SetActive(false);
            //Stop hide remove and edit buttons after x seconds coroutine
            StopAllCoroutines();
        }

        #endregion

        #region private methods

        //Firing message when edit button clicked
        private void OnEditButtonClicked()
        {
            OnEditMessageClicked(_messageId);
        }

        //Firing message when removed button clicked
        private void OnRemoveButtonClicked()
        {
            OnRemoveMessageClicked(_messageId);
        }

        //Coroutine for hidding edit and remove buttons after x secondss
        private IEnumerator HideAfterSomeSeconds(float seconds, List<GameObject> gameObjects)
        {
            yield return new WaitForSeconds(seconds);
            foreach (GameObject go in gameObjects)
            {
                go.SetActive(false);
            }
        }

        #endregion
    }
}
