/**
 * Copyright 2019 The Knights Of Unity, created by Pawel Stolarczyk
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

namespace DemoGame.Scripts.DataStorage
{

    /// <summary>
    /// Contains the name and reference to icon's <see cref="Sprite"/>.
    /// </summary>
    [Serializable]
    public class Icon
    {
        #region Fields

        /// <summary>
        /// Name under which this icon can be found.
        /// </summary>
        [SerializeField] private string _name = string.Empty;

        /// <summary>
        /// Sprite representation of this icon.
        /// </summary>
        [SerializeField] private Sprite _sprite = null;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the name of this icon.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Returns the sprite of this icon.
        /// </summary>
        public Sprite Sprite { get { return _sprite; } }

        #endregion
    }

}