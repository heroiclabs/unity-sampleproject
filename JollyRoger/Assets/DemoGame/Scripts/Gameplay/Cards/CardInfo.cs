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

using DemoGame.Scripts.Gameplay.Hands;
using DemoGame.Scripts.Gameplay.Units;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Cards
{
    /// <summary>
    /// Contains information of a single card type.
    /// Stores the prefab instantiated on card play.
    /// </summary>
    [CreateAssetMenu(fileName = "Card", menuName = "Deck/Card")]
    public class CardInfo : ScriptableObject
    {
        #region Fields

        #region Basic Info

        [Header("Basic Info")]
        /// <summary>
        /// The type of this card.
        /// Each distinct <see cref="CardInfo"/> must have a different <see cref="CardType"/>.
        /// </summary>
        [SerializeField] private CardType _cardtype = CardType.None;

        /// <summary>
        /// The name of this card.
        /// </summary>
        [SerializeField] private string _name = string.Empty;

        /// <summary>
        /// Description of the effect.
        /// </summary>
        [Multiline] [SerializeField] private string _description = string.Empty;

        /// <summary>
        /// The sprite visible on this card.
        /// </summary>
        [SerializeField] private Sprite _sprite = null;

        /// <summary>
        /// The coct of playing this card.
        /// </summary>
        [SerializeField] private int _cost = 0;

        #endregion

        #region Stats

        [Header("Stats")]
        /// <summary>
        /// Base unit health.
        /// </summary>
        [SerializeField] private int _health = 0;

        /// <summary>
        /// Health gained per level.
        /// </summary>
        [SerializeField] private int _healthPerLevel = 0;

        /// <summary>
        /// Base unit damage per attack.
        /// </summary>
        [SerializeField] private int _damage = 0;

        /// <summary>
        /// Damage gained per level.
        /// </summary>
        [SerializeField] private int _damagePerLevel = 0;

        /// <summary>
        /// Type of performed attack.
        /// </summary>
        [SerializeField] private AttackType _attackeType = AttackType.Simple;

        /// <summary>
        /// Base unit reload time. The greater reload time the more time
        /// it takes for the unit to attack.
        /// </summary>
        [SerializeField] private float _reloadTime = 0;

        /// <summary>
        /// Reload time gained (decreased) per level.
        /// </summary>
        [SerializeField] private float _reloadTimePerLevel = 0;

        /// <summary>
        /// Base unit move speed. This is the time in seconds this unit needs to move
        /// from one node to another.
        /// </summary>
        [SerializeField] private float _moveSpeed = 0;

        /// <summary>
        /// Move speed gain (decrease) per level.
        /// </summary>
        [SerializeField] private float _moveSpeedPerLevel = 0;

        /// <summary>
        /// Base unit rotation speed. This is the time in seconds this unit needs to rotate around.
        /// </summary>
        [SerializeField] private float _rotationSpeed = 0;

        /// <summary>
        /// rotation speed gain (decrease) per level.
        /// </summary>
        [SerializeField] private float _rotationSpeedPerLevel = 0;

        #endregion

        #region Drag and Drop Info

        [Header("Drag and Drop Info")]
        /// <summary>
        /// Determines where on the battlefield this card can be played.
        /// </summary>
        [SerializeField] private DropRegion _dropRegion = DropRegion.WholeMap;

        /// <summary>
        /// Determines if card can be played on node that contains any units
        /// </summary>
        [SerializeField] private bool _canBeDroppedOverOtherUnits = false;

        /// <summary>
        /// GameObject instantiated whenever this card is hovered over the battlefield.
        /// </summary>
        [SerializeField] private DropVisualizer _visualizerPrefab = null;

        #endregion

        #endregion

        #region Properties

        #region Basic Info

        /// <summary>
        /// Returns the <see cref="_cardtype"/> of this card
        /// </summary>
        public CardType CardType => _cardtype;

        /// <summary>
        /// Returns the <see cref="_name"/> of this card.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Returns the <see cref="_description"/> of this card.
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// Returns the <see cref="_sprite"/> visible on this card.
        /// </summary>
        public Sprite Sprite => _sprite;

        /// <summary>
        /// Returns the <see cref="_cost"/> of playing this card.
        /// </summary>
        public int Cost => _cost;

        #endregion

        #region Stats

        /// <summary>
        /// Base unit health.
        /// </summary>
        public int Health => _health;

        /// <summary>
        /// Health gained per level.
        /// </summary>
        public int HealthPerLevel => _healthPerLevel;

        /// <summary>
        /// Base unit damage per attack.
        /// </summary>
        public int Damage => _damage;

        /// <summary>
        /// Damage gained per level.
        /// </summary>
        public int DamagePerLevel => _damagePerLevel;

        /// <summary>
        /// Type of performed attack.
        /// </summary>
        public AttackType AttackeType => _attackeType;

        /// <summary>
        /// Base unit reload time per second. The greater reload time the more time
        /// it takes for the unit to attack.
        /// </summary>
        public float ReloadTime => _reloadTime;

        /// <summary>
        /// Reload time gained (decreased) per level.
        /// </summary>
        public float ReloadTimePerLevel => _reloadTimePerLevel;

        /// <summary>
        /// Base unit move speed. This is the time in seconds this unit needs to move
        /// from one node to another.
        /// </summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>
        /// Move speed gain (decrease) per level.
        /// </summary>
        public float MoveSpeedPerLevel => _moveSpeedPerLevel;

        /// <summary>
        /// Base unit rotation speed. This is the time in seconds this unit needs to rotate around.
        /// </summary>
        public float RotationSpeed => _rotationSpeed;

        /// <summary>
        /// rotation speed gain (decrease) per level.
        /// </summary>
        public float RotationSpeedPerLevel => _rotationSpeedPerLevel;

        #endregion

        #region Drag and Drop Info

        /// <summary>
        /// Determines where on the battlefield this card can be played.
        /// </summary>
        public DropRegion DropRegion => _dropRegion;

        /// <summary>
        /// Determines if card can be played on node that contains any units
        /// </summary>
        public bool CanBeDroppedOverOtherUnits => _canBeDroppedOverOtherUnits;

        /// <summary>
        /// Returns the prefab spawned whenever this card is hovered over battlefield.
        /// </summary>
        public DropVisualizer VisualizerPrefab => _visualizerPrefab;

        #endregion

        #endregion
    }
}