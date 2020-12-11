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

using UnityEditor;
using UnityEngine;

namespace PiratePanic
{
	/// <summary>
	/// Holds all "Unity → Window" MenuItems
	/// </summary>
	public class MenuItems
	{
		//  Fields ----------------------------------------

		//  Other Methods ---------------------------------
		[MenuItem (GameConstants.MenuItem_OpenPiratePanicDocumentation)]
		public static void MenuItem_OpenPiratePanicDocumentation()
		{
			Application.OpenURL(GameConstants.DocumentationUrl_PiratePanic);
		}

		[MenuItem(GameConstants.MenuItem_OpenNakamaDocumentation)]
		public static void MenuItem_OpenNakamaDocumentation()
		{
			Application.OpenURL(GameConstants.DocumentationUrl_Nakama);
		}

		[MenuItem(GameConstants.MenuItem_OpenDeveloperConsole)]
		public static void MenuItem_OpenDeveloperConsole()
		{
			Application.OpenURL(GameConstants.DeveloperConsoleUrl);
		}
		
	}
}
