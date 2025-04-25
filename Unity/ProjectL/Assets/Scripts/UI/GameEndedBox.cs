using UnityEngine;

#nullable enable

/// <summary>
/// Manages the ErrorMessageBox prefab.
/// </summary>
public class GameEndedBox : MonoBehaviour
{
    #region Fields

    private SoundManager? _soundManager;

    private SceneTransitions? _sceneTransitions;

    #endregion

    #region Methods

    /// <summary>
    /// Handles the click event for the "Calculate score" button.
    /// </summary>
    public void OnCalculateScoreButtonClick()
    {
        // load the main menu
        _soundManager?.PlayButtonClickSound();
        _sceneTransitions?.LoadFinalResultsAsync();
    }

    private void Start()
    {
        // try to find the sound manager
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();

        // get transitions
        _sceneTransitions = gameObject.transform.GetComponent<SceneTransitions>();
    }

    #endregion
}
