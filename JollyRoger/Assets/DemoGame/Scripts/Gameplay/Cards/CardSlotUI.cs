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

using System;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Visual representation of a card shown in <see cref="DeckBuildingMenu"/>.
    /// </summary>
    public class CardSlotUI : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Image representing the card.
        /// </summary>
        [SerializeField] private Image _image = null;

        /// <summary>
        /// Image representing the background of a card.
        /// </summary>
        [SerializeField] private Image _background = null;

        /// <summary>
        /// The sprite shown in <see cref="_image"/> when <see cref="Card"/> is null.
        /// </summary>
        [SerializeField] private Sprite _emptySprite = null;

        /// <summary>
        /// Button used to select this card to show its stats and options
        /// in the <see cref="CardInfoSidePanel"/>.
        /// </summary>
        [SerializeField] private Button _selectButton = null;

        /// <summary>
        /// Textfield containing the level of this card.
        /// </summary>
        [SerializeField] private Text _levelText = null;

        /// <summary>
        /// Textfield displaying the card cost.
        /// </summary>
        [SerializeField] private Text _cost = null;

        /// <summary>
        /// Color of <see cref="_background"/> when this slot is selected.
        /// </summary>
        [SerializeField] private Color _selectedColor = Color.black;

        #endregion

        #region Properties

        /// <summary>
        /// Reference to the card this object is displaying info of.
        /// This can be set using <see cref="SetCard(Card)"/> method.
        /// </summary>
        public Card Card { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        /// Should be invoked whenever an instance of <see cref="CardSlotUI"/>
        /// is created. Initializes the <see cref="_selectButton"/>'s onClick handler.
        /// </summary>
        public void Init(Action<CardSlotUI> onClicked)
        {
            _selectButton.onClick.AddListener(() => onClicked(this));
        }

        /// <summary>
        /// Sets the reference to <see cref="Card"/> displayed by this object.
        /// </summary>
        public virtual void SetCard(Card card = null)
        {
            if (card != null)
            {
                _image.sprite = card.GetCardInfo().Sprite;
                _cost.text = card.GetCardInfo().Cost.ToString();
                _levelText.text = "lvl " + card.level.ToString();
                _selectButton.interactable = true;
            }
            else
            {
                _image.sprite = _emptySprite;
                _cost.text = string.Empty;
                _levelText.text = string.Empty;
                _selectButton.interactable = false;
            }
            Card = card;
        }

        /// <summary>
        /// Changes the color of <see cref="_background"/> image.
        /// </summary>
        public virtual void Select()
        {
            _background.color = _selectedColor;
        }

        /// <summary>
        /// Changes the color of <see cref="_background"/> image.
        /// </summary>
        public virtual void Unselect()
        {
            _background.color = Color.white;
        }

        #endregion
    }
}