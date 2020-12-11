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


using PiratePanic.Managers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{

	/// <summary>
	/// Manages visibility of the gameobject.
	/// </summary>
	public class Menu : MonoBehaviour, IMenu
	{

		/// <summary>
		/// Reference to <see cref="CanvasGroup"/> used to show or hide the gameobject.
		/// </summary>
		[SerializeField] private CanvasGroup _canvasGroup = null;

		/// <summary>
		/// Button returning from this panel to main menu.
		/// </summary>
		[SerializeField] protected Button _backButton = null;




		/// <summary>
		/// If true, <see cref="Show"/> method was called and
		/// this panel is visible to the viewer.
		/// </summary>
		public bool IsShown { get; protected set; }



		/// <summary>
		/// Makes this menu visible to the viewer.
		/// </summary>
		[ContextMenu("Show")]
		public virtual void Show(bool isMuteButtonClick = false)
		{
			if (!isMuteButtonClick)
			{
				SoundManager.Instance.PlayButtonClick();
			}

			_canvasGroup.alpha = 1;
			_canvasGroup.blocksRaycasts = true;
			IsShown = true;
		}

		/// <summary>
		/// Hides this menu.
		/// </summary>
		[ContextMenu("Hide")]
		public virtual void Hide(bool isMuteButtonClick = false)
		{
			if (!isMuteButtonClick)
			{
				SoundManager.Instance.PlayButtonClick();
			}
			
			_canvasGroup.alpha = 0;
			_canvasGroup.blocksRaycasts = false;
			IsShown = false;
		}

		/// <summary>
		/// Sets the handler for <see cref="_backButton"/>.
		/// </summary>
		public virtual void SetBackButtonHandler(Action onBack)
		{
			_backButton?.onClick.AddListener(() =>
			{
				SoundManager.Instance.PlayButtonClick();
				onBack();
			});
		}
	}
}