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

using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{

	/// <summary>
	/// Tip showing for each username that fullfil requirements written in username searcher input field
	/// </summary>
	public class UsernameTip : MonoBehaviour
	{
		[SerializeField] private Button _tipButton = null;

		[SerializeField] private Text _usernameText = null;

		private UsernameSearcher _searcher;

		/// <summary>
		/// Initializes tip with downloaded username
		/// </summary>
		/// <param name="username"></param>
		/// <param name="searcher"></param>
		public void Init(string username, UsernameSearcher searcher)
		{
			_tipButton.onClick.AddListener(Select);
			_searcher = searcher;
			_usernameText.text = username;
		}

		/// <summary>
		/// Selects username
		/// </summary>
		private void Select()
		{
			_searcher.SetSearcherText(_usernameText.text);
		}
	}
}