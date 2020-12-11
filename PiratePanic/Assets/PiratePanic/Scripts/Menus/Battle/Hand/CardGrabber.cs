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

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PiratePanic
{
    /// <summary>
    /// UI element responsible for displaying card in hand.
    /// Can be drag around and dropped back in hand or on the battlefield to send message to host.
    /// </summary>
    public class CardGrabber : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		/// <summary>
		/// Image displaying card's sprite.
		/// </summary>
		[SerializeField] private Image _cardImage = null;

		/// <summary>
		/// Textfield displaying the level of a card.
		/// </summary>
		[SerializeField] private Text _cardLevel = null;

		/// <summary>
		/// Textfield displaying the card cost.
		/// </summary>
		[SerializeField] private Text _cost = null;

		/// <summary>
		/// Card drag speed.
		/// Every frame this object will reduce the distance to
		/// <see cref="_targetPosition"/> by this value.
		/// </summary>
		[SerializeField] private float _dragSpeed = 0.4f;

		/// <summary>
		/// Layer used to cast ray from camera.
		/// This will help determine where user want to drop their card.
		/// </summary>
		[SerializeField] private LayerMask _rayPlaneMask = new LayerMask();

		/// <summary>
		/// If true, this object is currently being dragged.
		/// </summary>
		private bool _isDragged;

		/// <summary>
		/// If true, this card grabber is currently hovered over <see cref="_dropRegion"/>.
		/// </summary>
		private bool _isVisualized;

		/// <summary>
		/// Object responsible for showing where dragged card will drop.
		/// </summary>
		private DropVisualizer _visualizer;

		/// <summary>
		/// If true, this object has already been played.
		/// Awaiting play acceptance or rejection.
		/// </summary>
		private bool _isPlayed;

		/// <summary>
		/// Set on drag begin.
		/// Determines how far is the mousse cursor from transform's position.
		/// </summary>
		private Vector2 _dragOffset;

		/// <summary>
		/// Position where this object is trying to move to.
		/// </summary>
		private Vector2 _targetPosition;

		/// <summary>
		/// Slot in users hand. Cards that are not currently grabbed will try
		/// to move to slot's position.
		/// </summary>
		private Transform _slot;

		/// <summary>
		/// Used to adjust real mouse position on a scaled canvas.
		/// </summary>
		private CanvasScaler _canvasScaler;

		/// <summary>
		/// Reference to user's hand region. Dropping card in this region
		/// returns the card to it's assigned hand slot.
		/// </summary>
		private RectTransform _handRegion;

		/// <summary>
		/// Reference to the region of the battlefield where this card can be played.
		/// </summary>
		private RectTransform _dropRegion;

		/// <summary>
		/// Returns the underlying card.
		/// </summary>
		public Card Card { get; private set; }

		/// <summary>
		/// Slot in users hand. Cards that are not currently grabbed will try
		/// to move to slot's position.
		/// </summary>
		public Transform Slot { get; private set; }

		/// <summary>
		/// Invoked whenever user clicks on this object.
		/// </summary>
		public event Action<CardGrabber> OnDragStarted;

		/// <summary>
		/// Invoked whenever user releases this object with pointer over their hand region
		/// or over any region other than <see cref="_dropRegion"/>.
		/// </summary>
		public event Action<CardGrabber> OnCardReturned;

		/// <summary>
		/// Invoked whenever user releases this object with pointer over <see cref="_dropRegion"/>.
		/// </summary>
		public event Action<CardGrabber, Vector3> OnCardPlayed;

		private GameStateManager _stateManager;
		private bool _isHost;

		public void Init(GameStateManager stateManager, bool isHost)
		{
			_stateManager = stateManager;
			_isHost = isHost;
		}

		/// <summary>
		/// Adjusts this object's position.
		/// </summary>
		private void Update()
		{
			if (_isDragged)
			{
				_targetPosition = (Vector2)Input.mousePosition + _dragOffset;
				_targetPosition.x = Mathf.Clamp(_targetPosition.x, 0, Screen.width);
				_targetPosition.y = Mathf.Clamp(_targetPosition.y, 0, Screen.height);
			}
			transform.position = Vector2.Lerp(transform.position, _targetPosition, _dragSpeed * Time.deltaTime);
			HandleVisualization();
		}

		/// <summary>
		/// Starts card drag.
		/// </summary>
		public void OnPointerDown(PointerEventData eventData)
		{
			if (_isPlayed)
			{
				// Card was already played
				return;
			}
			_isDragged = true;
			_dragOffset = transform.position - Input.mousePosition;
			OnDragStarted?.Invoke(this);
		}

		/// <summary>
		/// Ends card drag.
		/// </summary>
		public void OnPointerUp(PointerEventData eventData)
		{
			if (_isDragged)
			{
				_isDragged = false;
				Vector2 dropPosition = eventData.position;

				// Clamp the position to screen rect
				dropPosition.x = Mathf.Clamp(dropPosition.x, 0, Screen.width);
				dropPosition.y = Mathf.Clamp(dropPosition.y, 0, Screen.height);

				OnDropped(dropPosition);
			}
		}

		/// <summary>
		/// Creates visualizer to help showing where grabbed card will be dropped.
		/// </summary>
		private void HandleVisualization()
		{
			if (_isDragged == true && OverAssignedRegion(transform.position))
			{
				if (!_isVisualized)
				{
					_visualizer = Instantiate(Card.CardData.GetCardInfo().VisualizerPrefab);
					_visualizer.Init(_stateManager, _isHost);
					_visualizer.ShowVisualizer(this, _isHost);
				}
				_isVisualized = true;
				_visualizer.UpdatePosition(Card.CardData.GetCardInfo().DropRegion, _rayPlaneMask);
			}
			else if (!_isPlayed)
			{
				if (_isVisualized)
				{
					_visualizer.HideVisualizer(this);
					_visualizer = null;
				}
				_isVisualized = false;
			}
		}

		/// <summary>
		/// Returns true if this card grabber hovers over <see cref="_dropRegion"/>.
		/// </summary>
		public bool OverAssignedRegion(Vector2 position)
		{
			if (IsOverRegion(_handRegion, position))
			{
				// Card hovered over hand; can't be played here
				return false;
			}
			else if (_dropRegion != null)
			{
				// Check if card is over _dropRegion

				if (IsOverRegion(_dropRegion, position))
				{
					// Card over drop region
					return true;
				}
				else
				{
					// Card outside drop region
					return false;
				}
			}
			else
			{
				// _dropRegion is null, card can be played anywhere
				return true;
			}

		}

		/// <summary>
		/// Determines whether card was dropped in <see cref="_dropRegion"/> and should be played
		/// or returned to hand.
		/// </summary>
		private void OnDropped(Vector2 position)
		{
			bool isOverDropRegion = OverAssignedRegion(position);
			if (isOverDropRegion)
			{
				_isPlayed = true;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _rayPlaneMask))
				{
					OnCardPlayed?.Invoke(this, hit.point);
				}
				else
				{
					Debug.LogError("Raycast didn't hit anything; is ground plane active?");
					ReturnToHand();
					OnCardReturned?.Invoke(this);
				}
			}
			else
			{
				ReturnToHand();
				OnCardReturned?.Invoke(this);
			}
		}

		/// <summary>
		/// Returns true if <paramref name="point"/> is inside <paramref name="rect"/>.
		/// </summary>
		public bool IsOverRegion(RectTransform rect, Vector2 point)
		{
			// Getting rect of drop region
			Rect dropRect = rect.rect;
			dropRect.position += (Vector2)rect.position;
			dropRect.size *= _canvasScaler.transform.localScale.y;

			if (dropRect.Contains(point))
			{
				// Card over drop region
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Cancels the play.
		/// Card is returned to hand.
		/// </summary>
		public void CancelPlay()
		{
			ReturnToHand();
			_isPlayed = false;
		}

		/// <summary>
		/// Sets assigned slot as the target of this object to move to.
		/// </summary>
		public void ReturnToHand()
		{
			_targetPosition = _slot.position;
		}

		/// <summary>
		/// Initializes card grabber.
		/// Sets the UI and position.
		/// </summary>
		public void Initialize(Card card, Transform slot, RectTransform handRegion, RectTransform dropRegion, CanvasScaler canvasScaler)
		{
			this.Card = card;
			this.Slot = slot;
			this._handRegion = handRegion;
			this._dropRegion = dropRegion;
			_cardImage.sprite = card.CardData.GetCardInfo().Sprite;
			_cost.text = card.CardData.GetCardInfo().Cost.ToString();
			_cardLevel.text = "lvl " + card.CardData.level.ToString();
			_slot = slot;
			_canvasScaler = canvasScaler;
			ReturnToHand();
		}

		/// <summary>
		/// Invoked when card play was allowed.
		/// </summary>
		public void Resolve(MatchMessageCardPlayed message)
		{
			if (_isVisualized)
			{
				_visualizer.HideVisualizer(this);
				_isVisualized = false;
			}
		}

			}
}