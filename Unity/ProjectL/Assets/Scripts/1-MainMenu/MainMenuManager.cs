#nullable enable

namespace ProjectL.MainMenuScene
{
    using ProjectL.Sound;
    using UnityEngine;
    using ProjectL.Management;


    /// <summary>
    /// Manages the <c>Main Menu</c> scene.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
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
        #endregion
    }
}
