#nullable enable

namespace ProjectL.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the SoundControls prefab. Cycles through volume levels upon sound button click and updates its icon accordingly.
    /// </summary>
    public class SoundVolumeController : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// Name of the <see cref="PlayerPrefs"> entry for sound volume.
        /// </summary>
        private const string soundVolumePlayerPrefsName = "soundVolumeIndex";

        #endregion

        #region Fields

        /// <summary>
        /// Icons for the sound volume button, one for each volume level. Lower index = lower volume.
        /// </summary>
        [SerializeField] private Sprite[] soundIcons = new Sprite[4];

        private int _currentIconIndex;

        private Button? _soundControlButton;

        private Image? _soundVolumeImage;

        #endregion

        #region Methods

        /// <summary>
        /// Handles clicks on the sound volume button.
        /// </summary>
        public void OnSoundButtonClick()
        {
            // get index of the new icon
            _currentIconIndex = (_currentIconIndex + 1) % soundIcons.Length;

            SavePreferences();
            UpdateVolume();
            SoundManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Saves the current sound index to <see cref="PlayerPrefs"> .
        /// </summary>
        private void SavePreferences()
        {
            PlayerPrefs.SetInt(soundVolumePlayerPrefsName, _currentIconIndex);
        }

        /// <summary>
        /// Loads the sound index from <see cref="PlayerPrefs"> .
        /// </summary>
        private void LoadPreference()
        {
            _currentIconIndex = PlayerPrefs.GetInt(soundVolumePlayerPrefsName);
        }

        /// <summary>
        /// Updates the volume level and sound button icon.
        /// </summary>
        private void UpdateVolume()
        {
            if (_soundVolumeImage == null) {
                Debug.LogWarning("Sound Volume image is not assigned");
                return;
            }

            // update icon
            _soundVolumeImage.sprite = soundIcons[_currentIconIndex];
            // update volume
            AudioListener.volume = (float)_currentIconIndex / (soundIcons.Length - 1);
        }

        private void Awake()
        {
            // get button and and its image components
            _soundControlButton = GetComponent<Button>();
            _soundVolumeImage = GetComponent<Image>();
            if (_soundControlButton == null || _soundVolumeImage == null) {
                Debug.LogWarning("SoundVolumeController script not attached to a button");
                return;
            }

            // add listener to the button
            _soundControlButton.onClick.AddListener(OnSoundButtonClick);

            // sets the max volume if player preferences aren't set
            if (!PlayerPrefs.HasKey(soundVolumePlayerPrefsName)) {
                PlayerPrefs.SetInt(soundVolumePlayerPrefsName, soundIcons.Length - 1);
            }

            // initialize the sound volume
            LoadPreference();
            UpdateVolume();
        }

        #endregion
    }
}