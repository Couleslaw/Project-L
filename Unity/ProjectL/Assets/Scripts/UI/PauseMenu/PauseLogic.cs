#nullable enable

namespace ProjectL.UI.PauseMenu
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Manages the pause menu functionality. Displays or hides the given pause menu prefab upon pressing the <c>ESC</c> key.
    /// When the game is paused, the time scale is set to 0, and the AudioListener is paused.
    /// </summary>
    public class PauseLogic : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Indicates whether the game is currently paused.
        /// </summary>
        public static bool IsPaused { get; private set; } = false;

        /// <summary>
        /// Indicates whether the game can be paused.
        /// </summary>
        public static bool CanBePaused { get; set; } = true;

        /// <summary>
        /// Singleton instance of the <see cref="PauseLogic"/> class.
        /// </summary>
        public static PauseLogic? Instance { get; private set; } = null;

        /// <summary>
        /// Event triggered when the game is paused.
        /// </summary>
        public Action? OnPause { get; set; }

        /// <summary>
        /// Event triggered when the game is resumed.
        /// </summary>
        public Action? OnResume { get; set; }

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
            OnPause?.Invoke();
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Resumes the game.
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
            OnResume?.Invoke();
            Time.timeScale = 1f;
        }

        internal void Awake()
        {
            // try to find pause menu and make it active
            var pauseMenu = FindAnyObjectByType<PauseMenuManager>(FindObjectsInactive.Include);
            if (pauseMenu == null) {
                Debug.LogError("PauseLogic: Pause menu not found. Scenes with PauseLogic should include a pause menu");
                return;
            }
            pauseMenu.gameObject.SetActive(true);

            IsPaused = false;
            CanBePaused = true;
            Instance = this;
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
}
