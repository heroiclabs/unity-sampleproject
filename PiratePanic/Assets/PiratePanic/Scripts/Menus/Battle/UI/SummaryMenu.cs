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


using UnityEngine;
using UnityEngine.UI;

namespace PiratePanic
{

    /// <summary>
    /// Menu showed at the end of a match.
    /// Shows who win as well as displays the funds gained for the match.
    /// </summary>
    public class SummaryMenu : Menu
	{
		/// <summary>
		/// Displays the reward gained from the match.
		/// </summary>
		[SerializeField] private Text _rewardText = null;

		/// <summary>
		/// Shows who win the match.
		/// </summary>
		[SerializeField] private Text _header = null;

		/// <summary>
		/// Image visualizing local user's victory/defeat.
		/// </summary>
		[SerializeField] private Image _resultImage = null;

		[Space]
		/// <summary>
		/// Winning message displayed in <see cref="_header"/>.
		/// </summary>
		[SerializeField] private string _winHeader = string.Empty;

		/// <summary>
		/// Winning image displayed in <see cref="_resultImage"/>.
		/// </summary>
		[SerializeField] private Sprite _winSprite = null;

		[Space]
		/// <summary>
		/// Losing message displayed in <see cref="_header"/>.
		/// </summary>
		[SerializeField] private string _loseHeader = string.Empty;

		/// <summary>
		/// Losing image displayed in <see cref="_resultImage"/>.
		/// </summary>
		[SerializeField] private Sprite _loseSprite = null;

		private int _gems;
		private MatchEndPlacement _placement;
		private int _score;

		public void Init(int gems, MatchEndPlacement placement, int score)
		{
			_gems = gems;
			_placement = placement;
			_score = score;
		}

        public override void Show(bool isMuteButtonClick = false)
        {
			base.Show(isMuteButtonClick);

			if (_placement == MatchEndPlacement.Winner)
			{
				_header.text = _winHeader;
				_resultImage.sprite = _winSprite;
				_rewardText.text = $"+{_gems}";
			}
			else
			{
				_header.text = _loseHeader;
				_resultImage.sprite = _loseSprite;
				_rewardText.text = $"+{_gems}";
			}

        }
	}
}