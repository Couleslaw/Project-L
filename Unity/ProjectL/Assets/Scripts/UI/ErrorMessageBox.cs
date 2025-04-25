using UnityEngine;
using EasyUI;

#nullable enable

/// <summary>
/// Manages the ErrorMessageBox prefab.
/// </summary>
public class ErrorMessageBox : MonoBehaviour
{
    #region Fields

    private SoundManager? _soundManager;

    private SceneTransitions? _sceneTransitions;

    #endregion

    #region Methods

    /// <summary>
    /// Handles the click event for the "Main Menu" button. Returns to main menu.
    /// </summary>
    public void OnMainMenuButtonClick()
    {
        // load the main menu
        _soundManager?.PlayButtonClickSound();
        _sceneTransitions?.LoadMainMenuAsync();
    }

    /// <summary>
    /// Handles the click event for the "Open log" button. Opens the logger UI.
    /// </summary>
    public void OnOpenLogButtonClick()
    {
        EasyUI.Logger.Instance?.ToggleLogUI();
    }

    private void Start()
    {
        // play error sound
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();
        _soundManager?.PlayErrorSound();

        // get transitions
        _sceneTransitions = gameObject.transform.GetComponent<SceneTransitions>();
    }

    #endregion
}
