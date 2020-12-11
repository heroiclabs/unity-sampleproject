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
	/// Handles in-game gold income/outcome.
	/// Used obly by the host.
	/// Gold is a currency used to play cards during a match.
	/// For the currency used to buy and merge cards outside the match,
	/// see <see cref="DeckBuildingMenu._funds"/>.
	/// </summary>
	public class Gold : MonoBehaviour
	{
		/// <summary>
		/// The ammount of gold this user currently have.
		/// </summary>
		/// <remarks>
		/// This value is not an integer, because gold income is linear, not discrete.
		/// This will help visualizing user's gold count.
		/// </remarks>
		public float CurrentGold { get; private set; }

		/// <summary>
		/// Increases the amount of gold this user have.
		/// </summary>
		private void Update()
		{
			float goldPerSecond = GameConfigurationManager.Instance.GameConfiguration.GoldPerSecond;
			int maxGoldCount = GameConfigurationManager.Instance.GameConfiguration.MaxGoldCount;

			CurrentGold = Mathf.Min(CurrentGold + goldPerSecond * Time.deltaTime, maxGoldCount);
		}

		/// <summary>
		/// Invoked whenever user plays a card.
		/// </summary>
		public void ChangeGoldCount(float delta)
		{
			CurrentGold += delta;
		}

		/// <summary>
		/// Restarts user gold count.
		/// </summary>
		public void Restart()
		{
			CurrentGold = GameConfigurationManager.Instance.GameConfiguration.StartingGold;
		}
	}
}