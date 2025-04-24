using UnityEngine;

#nullable enable

/// <summary>
/// Manages the ErrorMessageBox prefab.
/// </summary>
public class GameEndedBox : MonoBehaviour
{
    private SoundManager? _soundManager;
    private SceneTransitions? _sceneTransitions;

    void Start()
    {
        // try to find the sound manager
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();

        // get transitions
        _sceneTransitions = gameObject.transform.GetComponent<SceneTransitions>();
    }

    public void OnCalculateScoreButtonClick()
    {
        // load the main menu
        _soundManager?.PlayButtonClickSound();
        _sceneTransitions?.LoadFinalResults();
    }
}
