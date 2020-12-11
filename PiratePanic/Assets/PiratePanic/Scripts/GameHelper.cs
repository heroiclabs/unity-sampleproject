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

using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace PiratePanic
{
	/// <summary>
	/// Stores commonly used methods by various systems.
	/// </summary>
	public static class GameHelper 
	{
		/// <summary>
		/// DontDestroyOnLoad requires the object be on root. This handles that.
		/// 
		/// Workflow: Keep the object parented in the Scene for organization,
		/// at runtime call this to persist it between scenes.
		/// 
		/// </summary>
		/// <param name="gameObject"></param>
		public static void MoveToRootAndDontDestroyOnLoad(GameObject gameObject)
		{
			// Move to root
			Transform parent = gameObject.transform.parent;
			if (parent != null)
			{
				gameObject.transform.SetParent(null);
			}

			// Do operation which requires root
			GameObject.DontDestroyOnLoad(gameObject);

		}

		public static void DoFadeCanvasGroupsIn(List<CanvasGroup> canvasGroups)
		{
			GameHelper.DoFadeCanvasGroups(canvasGroups, 0, 1, 1, 0f, 0.05f);
		}

		public static void DoFadeCanvasGroupsOut(List<CanvasGroup> canvasGroups)
		{
			GameHelper.DoFadeCanvasGroups(canvasGroups, 1, 0, 1, 0f, 0.05f);
		}

		private static void DoFadeCanvasGroups(List<CanvasGroup> canvasGroups,
				 float fromAlpha, float toAlpha, float duration, float delayStart, float delayDelta)
		{
			float delay = delayStart;

			foreach (CanvasGroup canvasGroup in canvasGroups)
			{
				// Fade out immediately
				canvasGroup.DOFade(fromAlpha, 0);

				// Fade in slowly
				canvasGroup.DOFade(toAlpha, duration).SetDelay(delay);

				delay += delayDelta;
			}
		}

	}
}
