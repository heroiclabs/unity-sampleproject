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

namespace DemoGame.Scripts.Menus
{

    /// <summary>
    /// Adds all panels to the scene and handles navigation in menu.
    /// </summary>
    public class MenuManager : Singleton<MenuManager>
    {
        /// <summary>
        /// Menu element pushed on <see cref="_menuStack"/>.
        /// </summary>
        private struct MenuStackEntry
        {
            public IMenu menu;
            public bool hideBeneath;
        }

        #region Fields

        /// <summary>
        /// Reference to the main menu panel.
        /// </summary>
        [SerializeField] private List<MenuModel> _openOnAwake = null;

        /// <summary>
        /// List of <see cref="MenuButtonModel"/> used to instantiate panels.
        /// Contains <see cref="Menu"/>s and responsible for their visibility Buttons.
        /// </summary>
        [SerializeField] private List<MenuButtonModel> _menuPairs = null;

        /// <summary>
        /// Currently stacked menus.
        /// To add a menu to the stack, invoke <see cref="ShowMenu(IMenu, bool)"/>.
        /// To remove the top menu from the stack, call <see cref="HideTopMenu"/>.
        /// </summary>
        private Stack<MenuStackEntry> _menuStack;

        #endregion

        #region Mono

        /// <summary>
        /// Sets all panels from <see cref="_menuPairs"/> buttons' on click events.
        /// </summary>
        protected void Start()
        {
            _menuStack = new Stack<MenuStackEntry>();

            foreach (MenuButtonModel menuPair in _menuPairs)
            {
                if (menuPair.Menu == null)
                {
                    menuPair.Button.interactable = false;
                }
                else if (menuPair.Button != null)
                {
                    menuPair.Button.onClick.AddListener(() => ShowMenu(menuPair.Menu, menuPair.HideBeneath));
                }
            }

            foreach (MenuModel entry in _openOnAwake)
            {
                ShowMenu(entry.Menu, entry.HideBeneath);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds given menu to the menu stack.
        /// Hides all menus beneath if <paramref name="hideBeneath"/> is true.
        /// </summary>
        public void ShowMenu(IMenu menu, bool hideBeneath)
        {
            MenuStackEntry newEntry;
            newEntry.menu = menu;
            newEntry.hideBeneath = hideBeneath;

            foreach (MenuStackEntry entry in _menuStack)
            {
                if (hideBeneath == true)
                {
                    if (entry.menu.IsShown == true)
                    {
                        entry.menu.Hide();
                    }
                }
            }

            _menuStack.Push(newEntry);
            menu.Show();
        }

        /// <summary>
        /// Removes the top menu from the stack.
        /// Shows all menus beneath.
        /// </summary>
        public void HideTopMenu()
        {

            MenuStackEntry topEntry = _menuStack.Pop();
            topEntry.menu.Hide();
            if (topEntry.hideBeneath == true)
            {
                foreach (MenuStackEntry entry in _menuStack)
                {
                    entry.menu.Show();
                    if (entry.hideBeneath == true)
                    {
                        break;
                    }
                }
            }
        }

        #endregion

    }

}