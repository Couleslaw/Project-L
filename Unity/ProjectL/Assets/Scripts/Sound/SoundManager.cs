#nullable enable

namespace ProjectL.Sound
{
    using UnityEngine;

    /// <summary>
    /// Provides methods for playing sound effects in the game.
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        #region Constants

        /// <summary>
        /// The default minimum duration for sound effects to be played, before being overridden.
        /// </summary>
        private const float minSEDurationDefault = 0.1f;

        #endregion

        #region Fields

        [Header("Sound Effects")]
        [SerializeField] private AudioSource? buttonPressSound;
        [SerializeField] private AudioSource? inputLineSound;
        [SerializeField] private AudioSource? sliderSound;
        [SerializeField] private AudioSource? errorSound;
        [SerializeField] private AudioSource? tapSoundEffect;
        [SerializeField] private AudioSource? softTapSoundEffect;

        #endregion

        #region Methods

        /// <summary> Plays the "button click" sound effect.  </summary>
        public void PlayButtonClickSound() => PlaySoundEffect(buttonPressSound);

        /// <summary> Plays the "input line" sound effect.  </summary>
        public void PlayInputLineSound() => PlaySoundEffect(inputLineSound);

        /// <summary> Plays the "slider" sound effect.  </summary>
        public void PlaySliderSound() => PlaySoundEffect(sliderSound);

        /// <summary> Plays the "error" sound effect.  </summary>
        public void PlayErrorSound() => PlaySoundEffect(errorSound);

        /// <summary> Plays the "tap" sound effect.  </summary>
        public void PlayTapSoundEffect() => PlaySoundEffect(tapSoundEffect);

        /// <summary> Plays the "soft tap" sound effect.  </summary>
        public void PlaySoftTapSoundEffect() => PlaySoundEffect(softTapSoundEffect);

        /// <summary>
        /// Plays the specified sound effect, ensuring it adheres to the minimum duration rule. Which is that if the given <paramref name="soundEffect"/> is already playing, it will be restarted only if it has played for longer than the specified <paramref name="minSEDuration"/>.
        /// </summary>
        /// <param name="soundEffect">The <see cref="AudioSource"/> to play. If null, a warning is logged.</param>
        /// <param name="minSEDuration">The minimum duration (in seconds) the sound effect must play before it can be stopped and restarted. Defaults to <see cref="minSEDurationDefault"/>.</param>
        private void PlaySoundEffect(AudioSource? soundEffect, float minSEDuration = minSEDurationDefault)
        {
            if (soundEffect != null) {
                if (soundEffect.isPlaying && soundEffect.time > minSEDuration) {
                    soundEffect.Stop();
                }
                if (!soundEffect.isPlaying) {
                    soundEffect.Play();
                }
            }
            else {
                Debug.LogWarning("Sound effect not assigned in the inspector.");
            }
        }

        #endregion
    }
}