using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

/// <summary>
/// Manages the pause menu functionality. Displays / hides the given pause menu prefab upon pressing the Escape key.
/// When the game is paused, the time scale is set to 0, and the AudioListener is paused.
/// </summary>
public class PauseLogic : MonoBehaviour
{
    [SerializeField] private PauseMenuManager? _pauseMenuManager;

    void Awake()
    {
        if (_pauseMenuManager == null) {
            Debug.LogError("PauseMenuManager is not assigned in the inspector.");
            return;
        }

        _pauseMenuManager.gameObject.SetActive(true);
        IsPaused = false;
        CanBePaused = true;
    }

    /// <summary>
    /// Indicates whether the game is currently paused.
    /// </summary>
    public static bool IsPaused { get; private set; } = false;

    /// <summary>
    /// Indicates whether the game can be paused.
    /// </summary>
    public static bool CanBePaused { get; set; } = true;

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void Pause()
    {
        if (!CanBePaused)
            return;

        IsPaused = true;
        AudioListener.pause = true;        // pause the music
        _pauseMenuManager?.ShowPauseMenu(); // hide the pause menu
        Time.timeScale = 0f;               // stop the flow of time 
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void Resume()
    {
        IsPaused = false;
        AudioListener.pause = false;        // resume the music
        _pauseMenuManager?.HidePauseMenu();
        Time.timeScale = 1f;                // resume the flow of time
    }



    /// <summary>
    /// Pauses or resumes the game when 'Escape' is pressed.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }
}
