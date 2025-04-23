using UnityEngine;

#nullable enable

/// <summary>
/// Provides methods for playing sound effects in the game.
/// </summary>
public class SoundManager : MonoBehaviour
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

    #endregion

    #region Methods

    public void PlayButtonClickSound() => PlaySoundEffect(buttonPressSound);

    public void PlayInputLineSound() => PlaySoundEffect(inputLineSound);

    public void PlaySliderSound() => PlaySoundEffect(sliderSound);

    public void PlayErrorSound() => PlaySoundEffect(errorSound);


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
