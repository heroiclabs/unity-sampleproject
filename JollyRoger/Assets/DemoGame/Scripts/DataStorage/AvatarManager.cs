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

using System.Collections.Generic;
using DemoGame.Scripts.Utils;
using UnityEngine;

namespace DemoGame.Scripts.DataStorage
{

    /// <summary>
    /// Stores references to all user avatars and clan emblems.
    /// </summary>
    public class AvatarManager : Singleton<AvatarManager>
    {
        #region IconLists

        /// <summary>
        /// List of user avatars.
        /// </summary>
        [SerializeField] private List<Icon> _avatars = null;

        /// <summary>
        /// List of clan emblems.
        /// </summary>
        [SerializeField] private List<Icon> _emblems = null;

        #endregion

        #region IteratingMethods

        /// <summary>
        /// Returns the name of next avatar based on supplied avatar name.
        /// If <paramref name="current"/> name was not found in <see cref="_avatars"/> list,
        /// returns the last icon available.
        /// </summary>
        public string NextAvatar(string current)
        {
            return NextIcon(current, _avatars);
        }

        /// <summary>
        /// Returns the name of next emblem based on supplied emblem name.
        /// If <paramref name="current"/> name was not found in <see cref="_emblems"/> list,
        /// returns the last icon available.
        /// </summary>
        public string NextEmblem(string current)
        {
            return NextIcon(current, _emblems);
        }

        /// <summary>
        /// Returns the name of next icon based on supplied name.
        /// If <paramref name="current"/> name was not found in <paramref name="iconList"/>,
        /// returns the last icon available.
        /// </summary>
        public string NextIcon(string current, List<Icon> iconList)
        {

            if (iconList.Count == 0)
            {
                Debug.LogError("Couldn't get next icon: No icons found");
                return null;
            }

            if (string.IsNullOrEmpty(current) == true)
            {
                return iconList[0].Name;
            }

            for (int i = 0; i < iconList.Count; i++)
            {
                if (iconList[i].Name == current)
                {
                    return iconList[(i + 1) % iconList.Count].Name;
                }
            }

            Debug.Log("Current icon [" + current + "] not found. Returning first icon.");
            return iconList[0].Name;
        }

        #endregion

        #region RetrievingMethods

        /// <summary>
        /// Searches <see cref="_avatars"/> for the avatar with given 
        /// <paramref name="name"/> and returns its sprite.
        /// </summary>
        public Sprite LoadAvatar(string name)
        {
            return LoadIcon(name, _avatars);
        }

        /// <summary>
        /// Searches <see cref="_emblems"/> for the emblem with given 
        /// <paramref name="name"/> and returns its sprite.
        /// </summary>
        public Sprite LoadEmblem(string name)
        {
            return LoadIcon(name, _emblems);
        }

        /// <summary>
        /// Searches supplied <paramref name="iconList"/> for the icon with given 
        /// <paramref name="name"/> and returns its sprite.
        /// </summary>
        private Sprite LoadIcon(string name, List<Icon> iconList)
        {
            if (iconList.Count == 0)
            {
                Debug.LogError("Couldn't load icon: No icons found.");
                return null;
            }
            if (string.IsNullOrEmpty(name) == true)
            {
                return iconList[iconList.Count - 1].Sprite;
            }
            else
            {
                foreach (Icon avatar in iconList)
                {
                    if (avatar.Name == name)
                    {
                        return avatar.Sprite;
                    }
                }
                Debug.LogError("Icon with name " + name + " not found");
                return iconList[iconList.Count - 1].Sprite;
            }
        }

        #endregion

    }

}