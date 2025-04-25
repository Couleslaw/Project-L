using UnityEngine;

#nullable enable

/// <summary>
/// Manages the ErrorMessageBox prefab.
/// </summary>
public class GameEndedBox : MonoBehaviour
{
    private SoundManager? _soundManager;
    private SceneTransitions? _sceneTransitions;

    private void Start()
    {
        // try to find the sound manager
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();

        // get transitions
        _sceneTransitions = gameObject.transform.GetComponent<SceneTransitions>();
    }

    /// <summary>
    /// Handles the click event for the "Calculate score" button.
    /// </summary>
    public void OnCalculateScoreButtonClick()
    {
        // load the main menu
        _soundManager?.PlayButtonClickSound();
        _sceneTransitions?.LoadFinalResultsAsync();
    }
}
