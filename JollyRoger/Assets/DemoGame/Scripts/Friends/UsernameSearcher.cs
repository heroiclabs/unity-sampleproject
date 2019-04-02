/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch
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
using System.Threading.Tasks;
using DemoGame.Scripts.Session;

namespace DemoGame.Scripts.Friends
{

    public class UsernameSearcher : MonoBehaviour
    {
        /// <summary>
        /// Struct used for capsulating user search results loaded from database
        /// </summary>
        private struct SearchResult
        {
            public string[] usernames;

            public SearchResult(string[] usernames)
            {
                this.usernames = usernames;
            }
        }


        #region public events

        public event Action OnSubmit = delegate { };

        #endregion

        #region public properties

        /// <summary>
        /// Returns current value of searcher input field
        /// </summary>
        public string InputFieldValue
        {
            get
            {
                return _inputField.text;
            }
        }

        #endregion

        #region private serialized fields

        [SerializeField] private GameObject _usernameTipPrefab = null;

        [Header("UI elements")]

        [SerializeField] private InputField _inputField = null;

        [SerializeField] private RectTransform _usernameTipsParent = null;

        #endregion

        #region private fields
        /// <summary>
        /// All search tasks running in this moment
        /// </summary>
        private List<Task> _actualTasks = new List<Task>();

        /// <summary>
        /// Tips showed for matching usernames
        /// </summary>
        private List<UsernameTip> _tips = new List<UsernameTip>();
        #endregion

        #region mono

        private void Start()
        {
            //connecting button clicks to methods
            _inputField.onValueChanged.AddListener(SearchUsers);
            _inputField.onEndEdit.AddListener(SearchEnded);
            _inputField.onEndEdit.AddListener(Submit);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Sets immadiatelly searcher text in input field
        /// </summary>
        /// <param name="username"></param>
        public void SetSearcherText(string username)
        {
            _inputField.onValueChanged.RemoveListener(SearchUsers);
            _inputField.text = username;
            _inputField.onValueChanged.AddListener(SearchUsers);
            DeleteAllTips();
        }

        #endregion

        #region private methods
        /// <summary>
        /// Checks if editing was ended with return key
        /// </summary>
        /// <param name="value"></param>
        public void Submit(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    OnSubmit();
                }
            }
        }

        /// <summary>
        /// Search for users matching name requirements
        /// </summary>
        /// <param name="text"></param>
        private async void SearchUsers(string text)
        {
            //dont search when text is empty
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            //client and session shortcuts
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            //nakama rpc method parameter
            string payload = text;
            //rpc method id from server
            string rpcid = "search_username";

            try
            {
                //creating search task - sending request for running "search_username" method on server with text parameter
                Task<IApiRpc> searchTask = client.RpcAsync(session, rpcid, payload);
                //adding this search task to list
                _actualTasks.Add(searchTask);
                //awaiting for server returning value
                IApiRpc searchResult = await searchTask;
                Debug.Log(searchResult.Payload);
                //unpacking results to SearchResult struct object
                SearchResult result = JsonUtility.FromJson<SearchResult>(searchResult.Payload);

                //checking if its last currently running search task
                if (_actualTasks.Count == 1)
                {
                    DeleteAllTips();
                    //creating tip for first 5 matched usernames
                    for (int i = 0; i < 5 && i < result.usernames.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(result.usernames[i]))
                        {
                            CreateTip(result.usernames[i]);
                        }
                    }
                }
                //removing task from actual tasks list
                _actualTasks.Remove(searchTask);
            }
            catch (Exception e)
            {
                Debug.LogError("Could not search users (" + e.Message + ")");
            }
        }

        #region showing and removing tips

        /// <summary>
        /// Instaties and initializes tip basing on loaded username
        /// </summary>
        /// <param name="username"></param>
        private void CreateTip(string username)
        {
            GameObject go = Instantiate(_usernameTipPrefab, _usernameTipsParent);
            UsernameTip tip = go.GetComponent<UsernameTip>();
            if (tip)
            {
                tip.Init(username, this);
                _tips.Add(tip);
            }
            else
            {
                Debug.LogError("Invalid username tip prefab!");
                Destroy(go);
            }
        }

        /// <summary>
        /// Invoked when user has ended interaction with <see cref="_inputField"/>.
        /// </summary>
        /// <param name="value"></param>
        private void SearchEnded(string value)
        {
            // Calculate tips parent screen rect
            Rect tipParentRect = _usernameTipsParent.rect;
            tipParentRect.position += (Vector2)_usernameTipsParent.position;

            // Check if mouse within tips parent
            if (tipParentRect.Contains(Input.mousePosition) == false)
            {
                DeleteAllTips();
            }
        }

        /// <summary>
        /// Destroys all showed tips
        /// </summary>
        private void DeleteAllTips()
        {
            for (int i = 0; i < _tips.Count; i++)
            {
                Destroy(_tips[i].gameObject, 0.1f);
            }
            _tips.Clear();
        }

        #endregion

        #endregion
    }

}