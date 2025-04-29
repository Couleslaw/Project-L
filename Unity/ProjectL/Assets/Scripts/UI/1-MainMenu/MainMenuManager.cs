#nullable enable

namespace ProjectL.UI.MainMenu
{
    using ProjectL.UI;
    using UnityEngine;

    /// <summary>
    /// Manages the "Main Menu" scene.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        #region Fields

        [SerializeField] private EasyUI.Logger? loggerPrefab = null;

        #endregion

        #region Methods

        /// <summary>
        /// Handles the click event for the "Play" button. Loads the player selection scene.
        /// </summary>
        public void OnNewGameButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            SceneLoader.Instance?.LoadPlayerSelectionAsync();
        }

        /// <summary>
        /// Handles the click event for the "User Guide" button. Opens the <see href="https://couleslaw.github.io/Project-L/UserDocs/">User Guide</see> in the default web browser.
        /// </summary>
        public void OnUserGuideButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            Application.OpenURL("https://couleslaw.github.io/Project-L/UserDocs/");
        }

        /// <summary>
        /// Plays a button click sound using the <see cref="SoundManager"/>.
        /// </summary>
        public void PlayButtonClickSound()
        {
            SoundManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Handles the click event for the "Quit" button. Quits the application.
        /// </summary>
        public void OnQuitButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            Application.Quit();
        }

        private void Awake()
        {
            if (loggerPrefab == null) {
                Debug.LogError("Logger prefab is not assigned in the inspector.");
                return;
            }

            // create a logger instance if it doesn't exist
            if (EasyUI.Logger.Instance == null) {
                Instantiate(loggerPrefab);
            }

            EasyUI.Logger.ClearLog();
            EasyUI.Logger.DisableLogger();
        }

        #endregion
    }
}
