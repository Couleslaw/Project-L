#nullable enable

namespace ProjectL.GameScene.MessageBoxes
{
    using UnityEngine;
    using ProjectL.Management;
    using ProjectL.Sound;

    /// <summary>
    /// Manages the ErrorMessageBox prefab.
    /// </summary>
    public class GameEndedBox : MonoBehaviour
    {
        #region Methods

        /// <summary>
        /// Handles the click event for the "Calculate score" button.
        /// </summary>
        public void OnCalculateScoreButtonClick()
        {
            // load the main menu
            SoundManager.Instance?.PlayButtonClickSound();
            SceneLoader.Instance?.LoadFinalResultsAsync();
        }

        private void Start()
        {
            SoundManager.Instance?.PlaySoftTapSoundEffect();
        }

        #endregion
    }
}
