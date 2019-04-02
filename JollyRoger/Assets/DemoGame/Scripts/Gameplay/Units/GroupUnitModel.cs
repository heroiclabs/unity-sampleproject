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
using System.Collections;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Units
{

    /// <summary>
    /// Handles animations for GroupUnits
    /// </summary>
    public class GroupUnitModel : UnitModel
    {
        /// <summary>
        /// GroupUnit which is using this UnitModel
        /// </summary>
        private GroupUnit _groupUnit;

        /// <summary>
        /// Index of last shooting unit
        /// </summary>
        private int _shootingIndex = -1;

        public override void Init()
        {
            _groupUnit = (GroupUnit)_unit;
            foreach (SubUnit subUnit in _groupUnit.SubUnits)
            {
                subUnit.OnDestroyed += StartDestroySubunitAnimationCurve;
            }
            base.Init();
        }

        protected override IEnumerator MoveAnimationCoroutine(Vector3 startPosition, Vector3 targetPosition)
        {
            float timer = 0;

            while (timer < MoveAnimationTime)
            {
                timer += Time.deltaTime;

                transform.position = Vector3.Lerp(startPosition, targetPosition, _translationAnimationCurve.Evaluate(timer / MoveAnimationTime));

                yield return null;
            }

            EndMoveAnimation();
        }

        protected override IEnumerator RotationAnimationCoroutine(Quaternion targetRotation)
        {
            Quaternion startRotation = _groupUnit.SubUnits[0].transform.rotation;

            float animationTime = (Quaternion.Angle(startRotation, targetRotation) / 360f) / RotateAnimationTime;

            float timer = 0;

            while (timer < animationTime)
            {
                timer += Time.deltaTime;

                foreach (SubUnit subunit in _groupUnit.SubUnits)
                {
                    subunit.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, _rotationAnimationCurve.Evaluate(timer / animationTime));
                }

                yield return null;
            }

            EndRotationAnimation();
        }

        private void StartDestroySubunitAnimationCurve(SubUnit subunit)
        {
            StartCoroutine(DestrouSubunitAnimationCoroutine(subunit));
        }

        private IEnumerator DestrouSubunitAnimationCoroutine(SubUnit subunit)
        {
            subunit.Animator.SetTrigger("Death");
            yield return new WaitForSeconds(1);
            Destroy(subunit.gameObject);
        }

        protected override IEnumerator DestroyAnimationCoroutine(Unit unit)
        {
            yield return new WaitForSeconds(1);
            Destroy(unit.gameObject);
        }

        protected override void PlayAttackAnimation(Vector3 targetPosition, bool fireAllCanons)
        {
            if (_attackAnimators.Count <= 0)
            {
                return;
            }

            if (fireAllCanons)
            {
                _attackAnimators.ForEach(aa => aa.Play());
            }
            else
            {
                if (++_shootingIndex >= _attackAnimators.Count)
                {
                    _shootingIndex = 0;
                }
                CustomAnimator animatorToPlay = _attackAnimators[_shootingIndex];
                animatorToPlay.Play();
            }
        }
    }

}