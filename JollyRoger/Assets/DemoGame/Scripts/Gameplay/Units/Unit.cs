/**
 * Copyright 2019 The Knights Of Unity, created by Piotr Stoch
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
using System.Collections;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Gameplay.Nodes;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Units
{

    /// <summary>
    /// Base class for every unit in game used for viewing units actions, not sending any actions itself
    /// </summary>
    public abstract class Unit : MonoBehaviour
    {
        #region public events

        public event Action OnMove = delegate { };
        public event Action OnMoveEnd = delegate { };

        public event Action<Unit> OnDestroy = delegate { };
        public event Action OnDestroyed = delegate { };

        //ANIMATIONS
        /// <summary>
        /// startPosition, endPosition
        /// </summary>
        public event Action<Vector3, Vector3> OnMoveAnimationStart;

        /// <summary>
        /// startRotation, endRotation
        /// </summary>
        public event Action<Quaternion> OnRotateAnimationStart;

        /// <summary>
        /// target position, fireAllCanons
        /// </summary>
        public event Action<Vector3, bool> OnAttackAnimationStart;

        #endregion

        public int Id;

        public Node CurrentNode;

        public bool CanMove { get; protected set; }
        public bool IsWeaponReloaded { get; protected set; }
        public bool IsDestroyed { get; protected set; }
        public virtual bool CanAttack { get { return CanMove && IsWeaponReloaded; } }

        public string OwnerId { get; private set; }

        public PlayerColor OwnerColor { get; private set; }

        public UnitAI UnitAI { get { return _unitAI; } }

        public abstract int Damage { get; }

        public Card Card { get { return _card; } }

        public AttackType AttackType
        {
            get
            {
                CardInfo cardInfo = _card.GetCardInfo();
                return cardInfo.AttackeType;
            }
        }

        public float ReloadTime
        {
            get
            {
                CardInfo cardInfo = _card.GetCardInfo();
                return cardInfo.ReloadTime + _card.level * cardInfo.ReloadTimePerLevel;
            }
        }

        [SerializeField]
        protected UnitAI _unitAI;

        [SerializeField]
        protected UnitModel _unitModel;

        protected Card _card;

        public abstract void TakeDamage(int damage, AttackType attackType);

        public virtual void Init(PlayerColor owner, string ownerId, int id, Node startNode, Card card)
        {
            _card = card;
            OwnerColor = owner;
            OwnerId = ownerId;
            Id = id;

            CurrentNode = startNode;
            CurrentNode.Unit = this;
            CurrentNode.Occupied = true;

            IsWeaponReloaded = true;
            UnblockMovement();

            Debug.Log("Unit " + id + " initialized");

            if (_unitModel)
            {
                _unitModel.Init();
                _unitModel.OnMoveAnimationEnd += UnblockMovement;
            }
        }

        public void Move(Node node)
        {
            BlockMovement();

            if (CurrentNode)
            {
                Node oldNode = CurrentNode;
                oldNode.Occupied = false;
                oldNode.Unit = null;
            }

            Vector3 oldPosition = transform.position;

            CurrentNode = node;
            transform.position = node.transform.position;
            CurrentNode.Unit = this;

            CurrentNode.Occupied = true;

            //ANIMATION
            OnMoveAnimationStart(oldPosition, node.transform.position);
            OnRotateAnimationStart(Quaternion.Euler(0, Vector3.SignedAngle(Vector3.forward, oldPosition - node.transform.position, Vector3.up), 0));
        }

        public void Attack(Unit enemy, int attackValue, AttackType attackType)
        {
            OnAttackAnimationStart?.Invoke(enemy.transform.position, attackType == AttackType.AoE);
            enemy.TakeDamage(attackValue, attackType);
            IsWeaponReloaded = false;
            StartCoroutine(ReloadCoroutine());
        }

        public virtual bool CheckIfIsInRange(Vector3 position, int range)
        {
            return Vector3.Distance(position, _unitModel.transform.position) <= range;
        }

        protected IEnumerator ReloadCoroutine()
        {
            float timer = 0;
            while (timer < ReloadTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            IsWeaponReloaded = true;
        }

        protected void Destroy()
        {
            IsDestroyed = true;
            OnDestroy(this);
            OnDestroyed();
            CurrentNode.Occupied = false;
            CurrentNode.Unit = null;
        }

        private void UnblockMovement()
        {
            CanMove = true;
        }

        private void BlockMovement()
        {
            CanMove = false;
        }
    }

}