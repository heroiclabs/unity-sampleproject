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

using System;
using System.Collections;
using DemoGame.Scripts.Gameplay.Cards;
using DemoGame.Scripts.Gameplay.NetworkCommunication;
using DemoGame.Scripts.Gameplay.Nodes;
using UnityEngine;

namespace DemoGame.Scripts.Gameplay.Hands
{

    /// <summary>
    /// Object represending dragged card shown upon hovering it over allowed drop region.
    /// </summary>
    public class DropVisualizer : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// The speed at which this object will scale up/down upon creation/destruction.
        /// This will create a smooth easing animation.
        /// </summary>
        [SerializeField] private float _zoomSpeed = 10;

        /// <summary>
        /// Maximum zoom in scale.
        /// </summary>
        private float _maxScale = 1;

        /// <summary>
        /// Mimimum zoom out scale.
        /// </summary>
        private float _minScale = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Invoked on every update when this object is visible.
        /// Sends a raycast to determine current poiner position on the battlefield.
        /// </summary>
        public virtual void UpdatePosition(DropRegion dropRegion, LayerMask mask)
        {
            bool isHost = MatchCommunicationManager.Instance.IsHost;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask))
            {
                Vector2Int nodePosition = GameManager.Instance.ScreenToNodePos(hit.point, isHost, dropRegion);
                Node node = GameManager.Instance.Nodes[nodePosition.x, nodePosition.y];
                transform.position = node.transform.position;
            }
        }

        /// <summary>
        /// Makes this visualizer visible to the user over a time period.
        /// </summary>
        public void ShowVisualizer(CardGrabber grabber, bool isHost)
        {
            if (isHost == true)
            {
                transform.Rotate(Vector3.up, 180);
            }
            StartCoroutine(ScaleUpCoroutine(grabber, _zoomSpeed));
        }

        /// <summary>
        /// Makes this visualizer invisible to the user over a time period.
        /// </summary>
        public void HideVisualizer(CardGrabber grabber)
        {
            StartCoroutine(ScaleDownCoroutine(grabber, _zoomSpeed, () =>
            {
                Destroy(gameObject);
            }));
        }

        /// <summary>
        /// Scales this visualizer up over time.
        /// Scales <see cref="CardGrabber"/> held by the user down over the same time period.
        /// Invokes <paramref name="onEnded"/> when finished.
        /// </summary>
        private IEnumerator ScaleUpCoroutine(CardGrabber grabber, float speed, Action onEnded = null)
        {
            float scale = _minScale;
            while (scale < _maxScale)
            {
                scale = Mathf.Min(scale + speed * Time.deltaTime, _maxScale);
                transform.localScale = Vector3.one * scale;
                grabber.transform.localScale = Vector3.one * (1 - scale);
                yield return null;
            }
            onEnded?.Invoke();
        }

        /// <summary>
        /// Scales this visualizer down over time.
        /// Scales <see cref="CardGrabber"/> held by the user up over the same time period.
        /// </summary>
        private IEnumerator ScaleDownCoroutine(CardGrabber grabber, float speed, Action onEnded = null)
        {
            float scale = _maxScale;
            while (scale > _minScale)
            {
                scale = Mathf.Max(scale - speed * Time.deltaTime, _minScale);
                transform.localScale = Vector3.one * scale;
                if (grabber != null)
                {
                    grabber.transform.localScale = Vector3.one * (1 - scale);
                }
                yield return null;
            }
            onEnded?.Invoke();
        }

        #endregion

    }

}