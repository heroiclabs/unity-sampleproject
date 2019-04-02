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

using UnityEngine;

namespace DemoGame.Scripts.Utils
{

    /// <summary>
    /// Inherit from this base class to create a singleton.
    /// e.g. public class MyClassName : Singleton<MyClassName> {}
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region Variables

        /// <summary>
        /// Lock used to not allow simultaneous operations on this singleton by multiple sources.
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// Reference to the singleton instance of type <see cref="T"/>.
        /// </summary>
        private static T _instance;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the reference to the singleton instance of type <see cref="T"/>.
        /// </summary>
        public static T Instance
        {
            get
            {
                // Lock preventing from simultaneous access by multiple sources.
                lock (_lock)
                {
                    // If it's the first time accessing this singleton Instance, _instance will always be null
                    // Searching for an active instance of type T in the scene.
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                    }

                    return _instance;
                }
            }
        }

        #endregion

        #region Monobehaviour

        /// <summary>
        /// Checking if an instance of <see cref="Singleton{T}"/> already exists in the scene.
        /// If it exists, destroy this object.
        /// </summary>
        protected virtual void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Removes the reference to this object on destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                _instance = null;
            }
        }

        #endregion
    }

}