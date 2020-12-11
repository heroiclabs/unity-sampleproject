/**
 * Copyright 2021 The Nakama Authors
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

using PiratePanic.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace PiratePanic
{

	/// <summary>
	/// Base class for showing units animations
	/// </summary>
	public abstract class UnitModel : MonoBehaviour
	{
		public event Action OnMoveAnimationEnd;
		public event Action OnRotateAnimationEnd;

		[SerializeField]
		protected Unit _unit;

		[SerializeField]
		protected AnimationCurve _translationAnimationCurve;

		[SerializeField]
		protected AnimationCurve _rotationAnimationCurve;

		[SerializeField]
		protected List<CustomAnimator> _attackAnimators;

		public float MoveAnimationTime
		{
			get
			{
				CardInfo cardInfo = _unit.Card.GetCardInfo();
				return cardInfo.MoveSpeed + _unit.Card.level * cardInfo.MoveSpeedPerLevel;
			}
		}

		public float RotateAnimationTime
		{
			get
			{
				CardInfo cardInfo = _unit.Card.GetCardInfo();
				return cardInfo.RotationSpeed + _unit.Card.level * cardInfo.RotationSpeedPerLevel;
			}
		}

		public virtual void Init()
		{
			_unit.OnMoveAnimationStart += StartMoveAnimation;
			_unit.OnRotateAnimationStart += StartRotateAnimation;
			_unit.OnBeforeDestroyed += StartDestroyAnimation;
			_unit.OnAttackAnimationStart += PlayAttackAnimation;

			_attackAnimators.ForEach(aa =>
			{
				aa.SetAnimatorSpeed(1f / _unit.ReloadTime);
				aa.OnDestroyed += RemoveAnimatorFromList;
			});
		}

		protected abstract IEnumerator MoveAnimationCoroutine(Vector3 startPosition, Vector3 targetPosition);

		protected abstract IEnumerator RotationAnimationCoroutine(Quaternion targetRotation);

		protected abstract IEnumerator DestroyAnimationCoroutine(Unit unit);

		protected void EndMoveAnimation()
		{
			OnMoveAnimationEnd?.Invoke();
		}

		protected void EndRotationAnimation()
		{
			OnRotateAnimationEnd?.Invoke();
		}

		protected virtual void PlayAttackAnimation(Vector3 targetPosition, bool fireAllCanons)
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
				PlayAudioShipShoot();

				CustomAnimator animatorToPlay = _attackAnimators.OrderBy(aa => Vector3.Distance(aa.transform.position, targetPosition)).ToList()[0];
				animatorToPlay.Play();
			}
		}

		protected void PlayAudioShipShoot()
		{
			SoundManager.Instance.PlayAudioClip(SoundConstants.ShipShoot01);
		}

		private void StartMoveAnimation(Vector3 startPosition, Vector3 targetPosition)
		{
			StartCoroutine(MoveAnimationCoroutine(startPosition, targetPosition));
		}

		private void StartRotateAnimation(Quaternion targetRotation)
		{
			StartCoroutine(RotationAnimationCoroutine(targetRotation));
		}

		private void StartDestroyAnimation(Unit unit)
		{
			StartCoroutine(DestroyAnimationCoroutine(unit));
		}

		private void RemoveAnimatorFromList(CustomAnimator animator)
		{
			if (_attackAnimators.Contains(animator))
			{
				_attackAnimators.Remove(animator);
			}
		}
	}
}