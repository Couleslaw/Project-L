using UnityEngine;

#nullable enable

public class ErrorMessageBox : MonoBehaviour
{
    private SoundManager? _soundManager;
    void Start()
    {
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();
        _soundManager?.PlayErrorSound();
    }

    public void OnMainMenuButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        GameObject.FindAnyObjectByType<SceneTransitions>()?.LoadMainMenu();
    }
}
