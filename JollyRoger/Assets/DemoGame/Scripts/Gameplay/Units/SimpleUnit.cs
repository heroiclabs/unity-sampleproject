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
using System.Collections.Generic;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Gameplay.Nodes;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Units
{

    /// <summary>
    /// Unit without any subunits
    /// </summary>
    public class SimpleUnit : Unit, IDamagable
    {
        /// <summary>
        /// health, max health
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        public override int Damage
        {
            get
            {
                CardInfo cardInfo = _card.GetCardInfo();
                return cardInfo.Damage + _card.level * cardInfo.DamagePerLevel;
            }
        }

        public int Health
        {
            get
            {
                CardInfo cardInfo = _card.GetCardInfo();
                return cardInfo.Health + _card.level * cardInfo.HealthPerLevel;
            }
        }

        public Animator Animator;

        protected int _health;

        [SerializeField] private List<CustomAnimator> DamageAnimators = null;

        private List<CustomAnimator> _availableDamageAnimators;

        public override void Init(PlayerColor owner, string ownerId, int id, Node startNode, Card card)
        {
            base.Init(owner, ownerId, id, startNode, card);
            _health = Health;

            _availableDamageAnimators = new List<CustomAnimator>(DamageAnimators);

            DamageAnimators.ForEach(da =>
            {
                da.OnPlayed += RemoveAnimatorFromAvailable;
                da.OnAnimationEnd += AddAnimatorToAvailable;
            });
        }

        public override void TakeDamage(int damage, AttackType attackType)
        {
            if ((_health -= damage) <= 0)
            {
                Destroy();
            }

            PlayDamageAnimation();

            OnHealthChanged?.Invoke(_health, Health);
        }

        public void PlayDamageAnimation()
        {
            if (_availableDamageAnimators.Count <= 0)
            {
                Debug.LogWarning("Couldn't play damage animation. No available damage animators in subunit!");
                return;
            }

            int randomNumber = UnityEngine.Random.Range(0, _availableDamageAnimators.Count);

            _availableDamageAnimators[randomNumber].Play();
        }

        private void AddAnimatorToAvailable(CustomAnimator animator)
        {
            if (!_availableDamageAnimators.Contains(animator))
            {
                _availableDamageAnimators.Add(animator);
            }
        }

        private void RemoveAnimatorFromAvailable(CustomAnimator animator)
        {
            if (_availableDamageAnimators.Contains(animator))
            {
                _availableDamageAnimators.Remove(animator);
            }
        }
    }

}