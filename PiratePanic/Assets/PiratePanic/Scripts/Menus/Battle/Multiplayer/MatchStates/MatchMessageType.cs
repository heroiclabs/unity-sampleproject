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
	/// Used to easily get op code for sending and reading match state messages
	/// </summary>
	public enum MatchMessageType
	{
		UnitSpawned = 0,
		UnitMoved = 1,
		UnitAttacked = 2,
		SpellActivated = 3,
		StartingHand = 4,
		CardPlayRequest = 5,
		CardPlayed = 6,
		CardCanceled = 7
	}
}
