using UnityEngine;

#nullable enable

/// <summary>
/// Manages the ErrorMessageBox prefab.
/// </summary>
public class ErrorMessageBox : MonoBehaviour
{
    private SoundManager? _soundManager;
    private SceneTransitions? _sceneTransitions;
    
    private void Start()
    {
        // play error sound
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();
        _soundManager?.PlayErrorSound();

        // get transitions
        _sceneTransitions = gameObject.transform.GetComponent<SceneTransitions>();
    }

    /// <summary>
    /// Handles the click event for the "Main Menu" button.
    /// </summary>
    public void OnMainMenuButtonClick()
    {
        // load the main menu
        _soundManager?.PlayButtonClickSound();
        _sceneTransitions?.LoadMainMenuAsync();
    }
}
