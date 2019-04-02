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
using UnityEngine.UI;

namespace DemoGame.Scripts.Menus
{

    /// <summary>
    /// Used to store reference to <see cref="Menu"/> prefab and a button 
    /// responsible for its visibility on the scene.
    /// </summary>
    [Serializable]
    public class MenuButtonModel : MenuModel
    {

        #region Fields

        /// <summary>
        /// Button responsible for showing <see cref="_menu"/>.
        /// </summary>
        [SerializeField] private Button _button = null;

        #endregion

        #region Properties

        /// <summary>
        /// Returns button responsible for showing <see cref="Menu"/>.
        /// </summary>
        public Button Button { get { return _button; } }

        #endregion

    }

}