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

namespace DemoGame.Scripts.Menus
{

    /// <summary>
    /// Used to store reference to <see cref="Menu"/> prefab and a button 
    /// responsible for its visibility on the scene.
    /// </summary>
    [Serializable]
    public class MenuModel
    {
        #region Fields

        /// <summary>
        /// Reference to the menu prefab.
        /// </summary>
        [SerializeField] private GameObject _menuGameObject = null;

        /// <summary>
        /// If true, all menus beneath this one will be hidden.
        /// </summary>
        [SerializeField] private bool _hideBeneath = false;


        /// <summary>
        /// Reference to monobehaviour implementing <see cref="IMenu"/> interface.
        /// </summary>
        private IMenu _menu;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the reference to supplied menu prefab.
        /// </summary>
        public IMenu Menu
        {
            get
            {
                if (_menu == null && _menuGameObject != null)
                {
                    _menu = _menuGameObject.GetComponent<IMenu>();
                    if (_menu == null)
                    {
                        Debug.LogError("GameObject " + _menuGameObject.name + " does not " +
                            " have a component implementing IMenu interface.");
                    }
                }
                return _menu;
            }
        }

        /// <summary>
        /// Returns button responsible for showing <see cref="Menu"/>.
        /// </summary>
        public bool HideBeneath { get { return _hideBeneath; } }

        #endregion

    }

}