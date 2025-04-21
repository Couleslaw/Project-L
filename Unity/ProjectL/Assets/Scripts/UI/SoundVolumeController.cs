using UnityEngine;
using UnityEngine.UI;

#nullable enable

public class SoundVolumeController : MonoBehaviour
{
    // this scripts manages the sound control button

    [SerializeField] Sprite[] soundIcons = new Sprite[4];  // icons for the button

    int currentIconIndex;  // 0 = lowest volume, 3 = highest volume
    const string soundVolumePlayerPrefsName = "soundVolumeIndex";

    private Button? _soundControlButton;
    private Image? _soundVolumeImage;
    private SoundManager? _soundManager;

    void Start()
    {
        _soundControlButton = GetComponent<Button>();
        _soundVolumeImage = GetComponent<Image>();
        if (_soundControlButton == null || _soundVolumeImage == null) {
            Debug.LogWarning("SoundVolumeController script not attached to a button");
            return;
        }

        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();

        _soundControlButton.onClick.AddListener(OnButtonPress);
        // sets the max volume if player preferences aren't set
        if (!PlayerPrefs.HasKey(soundVolumePlayerPrefsName)) {
            PlayerPrefs.SetInt(soundVolumePlayerPrefsName, soundIcons.Length - 1);
        }

        LoadPreference();
        UpdateVolume();
    }

    void SavePreferences()
    {
        // saves the index to player preferences
        PlayerPrefs.SetInt(soundVolumePlayerPrefsName, currentIconIndex);
    }

    void LoadPreference()
    {
        // loads the index from player preferences
        currentIconIndex = PlayerPrefs.GetInt(soundVolumePlayerPrefsName);
    }

    void UpdateVolume()
    {
        if (_soundVolumeImage == null) {
            Debug.LogWarning("Sound Volume image is not assigned");
            return;
        }

        _soundVolumeImage.sprite = soundIcons[currentIconIndex];
        // volume(0) = 0, volume(soundIcons.Length - 1) = 1
        AudioListener.volume = currentIconIndex / (float)(soundIcons.Length - 1);
    }

    public void OnButtonPress()
    {
        // button presses cycle through the volume levels
        currentIconIndex = (currentIconIndex + 1) % soundIcons.Length;
        SavePreferences();
        UpdateVolume();
        if (_soundManager != null)
            _soundManager.PlayButtonClickSound();
    }
}
