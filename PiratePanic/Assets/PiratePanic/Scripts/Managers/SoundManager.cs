using System.Collections.Generic;
using UnityEngine;

namespace PiratePanic.Managers
{
	/// <summary>
	/// Stores commonly used values which may be changed at edit-time.
	/// </summary>
	public static class SoundConstants
	{
		public const string ButtonClick01 = "ButtonClick01";
		public const string CardDrop01 = "CardDrop01";
		public const string ShipShoot01 = "ShipShoot01";
	}

	/// <summary>
	/// Singleton managing the game's <see cref="AudioSource"/>s and <see cref="AudioClip"/>s.
	/// </summary>
	public class SoundManager : Singleton<SoundManager>
	{
		//  Fields ----------------------------------------

		[SerializeField]
		private GameConfiguration _gameConfiguration = null;

		[SerializeField]
		private List<AudioClip> _audioClips = new List<AudioClip>();

		[SerializeField]
		private List<AudioSource> _audioSources = new List<AudioSource>();

		//  Unity Methods   -------------------------------
		protected void Start()
		{
			GameHelper.MoveToRootAndDontDestroyOnLoad(gameObject);
		}

		//  Other Methods   -------------------------------
		public void PlayAudioClip(string audioClipName)
		{
			foreach (AudioClip audioClip in _audioClips)
			{
				if (audioClip.name == audioClipName)
				{
					PlayAudioClip(audioClip);
					return;
				}
			}
		}

		public void PlayAudioClip(AudioClip audioClip)
		{
			if (!_gameConfiguration.IsAudioEnabled)
			{
				return;
			}

			foreach (AudioSource audioSource in _audioSources)
			{
				if (!audioSource.isPlaying)
				{
					audioSource.volume = _gameConfiguration.AudioVolume;
					audioSource.clip = audioClip;
					audioSource.Play();

					//Debug.Log($"PlayAudioClip() audioClip.name={audioClip.name} at { audioSource.volume}");

					return;
				}
			}
		}

		public void PlayButtonClick()
		{
			PlayAudioClip(SoundConstants.ButtonClick01);
		}
	}
}