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
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Units
{

    /// <summary>
    /// Used for playing effects animations
    /// </summary>
    public class CustomAnimator : MonoBehaviour
    {
        public event Action<CustomAnimator> OnPlayed;
        public event Action<CustomAnimator> OnAnimationEnd;
        public event Action<CustomAnimator> OnDestroyed;

        public bool IsPlaying { private set; get; }

        /// <summary>
        /// Name of trigger activated on play
        /// </summary>
        public string TriggerName;

        [SerializeField] private Animator _animator = null;

        public void Play()
        {
            IsPlaying = true;
            _animator.SetTrigger(TriggerName);
            OnPlayed?.Invoke(this);
        }

        public void AnimationEnd()
        {
            IsPlaying = false;
            OnAnimationEnd?.Invoke(this);
        }

        private void OnDestroy()
        {
            OnPlayed = null;
            OnAnimationEnd = null;
            OnDestroyed?.Invoke(this);
        }

        public void SetAnimatorSpeed(float speed)
        {
            _animator.speed = speed;
        }
    }

}