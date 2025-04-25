using UnityEngine;

#nullable enable

/// <summary>
/// Manages the pause menu functionality. Displays or hides the given pause menu prefab upon pressing the <c>ESC</c> key.
/// When the game is paused, the time scale is set to 0, and the AudioListener is paused.
/// </summary>
public class PauseLogic : MonoBehaviour
{
    #region Fields

    [SerializeField] private PauseMenuManager? _pauseMenuManager;

    #endregion

    #region Properties

    /// <summary>
    /// Indicates whether the game is currently paused.
    /// </summary>
    public static bool IsPaused { get; private set; } = false;

    /// <summary>
    /// Indicates whether the game can be paused.
    /// </summary>
    public static bool CanBePaused { get; set; } = true;

    #endregion

    #region Methods

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void Pause()
    {
        if (!CanBePaused)
            return;

        IsPaused = true;
        AudioListener.pause = true;
        _pauseMenuManager?.ShowPauseMenu();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void Resume()
    {
        IsPaused = false;
        AudioListener.pause = false;
        _pauseMenuManager?.HidePauseMenu();
        Time.timeScale = 1f;
    }

    internal void Awake()
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
    /// Pauses or resumes the game when <c>ESC</c> is pressed.
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

    #endregion
}
