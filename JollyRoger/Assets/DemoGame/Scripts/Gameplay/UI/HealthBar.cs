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
using DemoGame.Scripts.Gameplay.Units;
using UnityEngine;
using UnityEngine.UI;

namespace DemoGame.Scripts.Gameplay.UI
{

    /// <summary>
    /// UI element showing health status of unit
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _healthValueImage = null;

        [SerializeField] private GameObject _damagableObject = null;

        private IDamagable _damagable;

        private void Start()
        {
            _damagable = _damagableObject.GetComponent<IDamagable>();
            if (_damagable != null)
            {
                _damagable.OnHealthChanged += SetValue;
            }
            else
            {
                Debug.LogError("Given object does not contains an IDamagable type");
            }
        }

        private void LateUpdate()
        {
            //Look at camera
            transform.rotation = Camera.main.transform.rotation;
        }

        private void SetValue(int health, int maxHealth)
        {
            _healthValueImage.fillAmount = (float)health / maxHealth;
        }
    }

}