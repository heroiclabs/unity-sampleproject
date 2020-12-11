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

namespace PiratePanic
{
	/// <summary>
	/// Stores commonly used values which may be changed at edit-time and runtime.
	/// </summary>
	[CreateAssetMenu(
	   menuName = GameConstants.CreateAssetMenu_GameConfiguration)]
	public class GameConfiguration : ScriptableObject
	{
		//  Properties ------------------------------------
		public string SceneNameMainMenu { get { return _sceneNameMainMenu; } }
		public string SceneNameBattle { get { return _sceneNameBattle; } }

		// Gameplay - Local Player
		public int StartingGold { get { return _startingGold; } }
		public int MaxGoldCount { get { return _maxGoldCount; } }
		public float GoldPerSecond { get { return _goldPerSecond; } }

		// Audio
		public bool IsAudioEnabled { get { return _isAudioEnabled; } }
		public float AudioVolume { get { return _audioVolume; } }

		//  Fields ----------------------------------------
		[Header("Scenes")]
		[SerializeField] private string _sceneNameMainMenu = "Scene01MainMenu";
		[SerializeField] private string _sceneNameBattle = "Scene02Battle";

		[Header("Gameplay - Local Player")]
		/// <summary>
		/// Starting gold count.
		/// </summary>
		[Range(1, 3)]
		[SerializeField] private int _startingGold = 3;

		/// <summary>
		/// Maximum gold a user can have at a time.
		/// </summary>
		[Range(3, 10)]
		[SerializeField] private int _maxGoldCount = 10;

		/// <summary>
		/// Gold income per second.
		/// </summary>
		[Range(0.1f, 2f)]
		[SerializeField] private float _goldPerSecond = 0.5f;

		[Header("Audio")]
		[SerializeField] private bool _isAudioEnabled = true;

		[Range(0, 1f)]
		[SerializeField] float _audioVolume = 1;
	}
}