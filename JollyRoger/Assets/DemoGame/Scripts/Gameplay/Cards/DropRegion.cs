/**
 * Copyright 2019 Heroic Labs and contributors
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

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Determines where on the battlefield a given card can be played
    /// </summary>
    public enum DropRegion
    {
        /// <summary>
        /// Card can be played on the whole map.
        /// </summary>
        WholeMap = 0,

        /// <summary>
        /// Card can be played on the half of the battlefield controlled by the opponent.
        /// </summary>
        EnemyHalf = 1,

        /// <summary>
        /// Card can be played on the enemy spawn area.
        /// </summary>
        EnemySpawn = 2,

        /// <summary>
        /// Card can be played on the half of the battlefield controlled by the local user.
        /// </summary>
        AllyHalf = 3,

        /// <summary>
        /// Card can be played on the caster's spawn area
        /// </summary>
        AllySpawn = 4
    }
}
