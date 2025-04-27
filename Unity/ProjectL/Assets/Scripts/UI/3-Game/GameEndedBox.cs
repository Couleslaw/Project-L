#nullable enable

namespace UI.GameScene
{
    using UnityEngine;
    using ProjectL.UI;

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

        #endregion
    }
}
