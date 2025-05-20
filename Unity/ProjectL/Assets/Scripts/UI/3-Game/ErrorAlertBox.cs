#nullable enable

namespace ProjectL.UI.GameScene
{
    using UnityEngine;
    using ProjectL.Management;
    using ProjectL.UI.Sound;

    /// <summary>
    /// Manages the ErrorMessageBox prefab.
    /// </summary>
    public class ErrorAlertBox : MonoBehaviour
    {
        #region Methods

        /// <summary>
        /// Handles the click event for the "Main Menu" button. Returns to main menu.
        /// </summary>
        public void OnMainMenuButtonClick()
        {
            // load the main menu
            SoundManager.Instance?.PlayButtonClickSound();
            SceneLoader.Instance?.LoadMainMenuAsync();
        }

        /// <summary>
        /// Handles the click event for the "Open log" button. Opens the logger UI.
        /// </summary>
        public void OnOpenLogButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            GameManager.Instance?.OpenLogger();
        }

        private void Start()
        {
            SoundManager.Instance?.PlayErrorSound();
        }

        #endregion
    }
}