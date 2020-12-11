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

namespace PiratePanic
{
	/// <summary>
	/// Stores commonly used values which may be changed at edit-time.
	/// </summary>
	public static class GameConstants
	{
		//  Fields ----------------------------------------

		// CreateAssetMenu
		private const string CreateAssetMenu = "Pirate Panic/";
		public const string CreateAssetMenu_AvatarSprites = CreateAssetMenu + "AvatarSprites";
		public const string CreateAssetMenu_Card = CreateAssetMenu + "Card";
		public const string CreateAssetMenu_CardList = CreateAssetMenu + "Card List";
		public const string CreateAssetMenu_GameConfiguration = CreateAssetMenu + "GameConfiguration";
		public const string CreateAssetMenu_GameConnection = CreateAssetMenu + "GameConnection";

		// MenuItem
		private const string MenuItem = "Window/Pirate Panic/";
		public const string MenuItem_OpenPiratePanicDocumentation = MenuItem + "Open Docs: Pirate Panic";
		public const string MenuItem_OpenNakamaDocumentation = MenuItem + "Open Docs: Nakama";
		public const string MenuItem_OpenDeveloperConsole = MenuItem + "Open Developer Console";

		// PlayerPrefs
		public const string DeviceIdKey = "nakama.deviceId";
		public static string AuthTokenKey = "nakama.authToken";

		// Urls
		public const string DocumentationUrl_PiratePanic = "https://github.com/heroiclabs/unity-sampleproject";
		public const string DocumentationUrl_Nakama = "https://heroiclabs.com/docs/index.html";
		public const string DeveloperConsoleUrl = "http://localhost:7351/";

		
	}
}
