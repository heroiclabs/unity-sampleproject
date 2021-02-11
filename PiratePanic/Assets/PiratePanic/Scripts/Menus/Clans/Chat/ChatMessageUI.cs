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
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{
	/// <summary>
	/// Manages chat message viewing in UI
	/// </summary>
	public class ChatMessageUI : MonoBehaviour
	{

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

		[Header("UI Elements")]
		[SerializeField] private Text _usernameText = null;
		[SerializeField] private Text _dateText = null;
		[SerializeField] private Text _contentText = null;

		private string _messageId;

		/// <summary>
		/// Forma used for viewing date of creating message
		/// </summary>
		private const string DATE_FORMAT = "M.d.yy H:mm";

		/// <summary>
		/// Initializes message with given message data
		/// </summary>
		public void InitMessage(string messageId, string username, string content, string date, bool hideUsername)
		{
			_messageId = messageId;

			// Hide username or sets username text if not hidden
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
		}
    }
}
