using UnityEngine;

#nullable enable

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource? buttonPressSound;
    [SerializeField] private AudioSource? inputLineSound;
    [SerializeField] private AudioSource? sliderSound;
    [SerializeField] private AudioSource? errorSound;

    private const float minSEDurationDefault = 0.1f;

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

    public void PlayButtonClickSound() => PlaySoundEffect(buttonPressSound);
    

    public void PlayInputLineSound() => PlaySoundEffect(inputLineSound);



    public void PlaySliderSound() => PlaySoundEffect(sliderSound);

    public void PlayErrorSound() => PlaySoundEffect(errorSound);
}
