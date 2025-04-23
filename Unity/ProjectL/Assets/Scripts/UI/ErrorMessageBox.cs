using UnityEngine;

#nullable enable

/// <summary>
/// Manages the ErrorMessageBox prefab.
/// </summary>
public class ErrorMessageBox : MonoBehaviour
{
    private SoundManager? _soundManager;
    void Start()
    {
        // play error sound
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();
        _soundManager?.PlayErrorSound();
    }

    public void OnMainMenuButtonClick()
    {
        // load the main menu
        _soundManager?.PlayButtonClickSound();
        GameObject.FindAnyObjectByType<SceneTransitions>()?.LoadMainMenu();
    }
}
