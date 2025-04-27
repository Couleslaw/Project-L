#nullable enable

namespace ProjectL.Utils
{
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using ProjectL.UI.PauseMenu;


    /// <summary>
    /// Manages transitioning between different scenes.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Animator? fadeAnimator;

        private const string _fadeInAnimation = "FadeIn";
        private const string _fadeOutAnimation = "FadeOut";

        private const string _mainMenuScene = "1-MainMenu";
        private const string _playerSelectionScene = "2-PlayerSelection";
        private const string _gameScene = "3-Game";
        private const string _finalResultsScene = "4-FinalResults";

        #endregion

        #region Properties

        /// <summary>
        /// Singleton instance of the <see cref="SceneLoader"/> class.
        /// </summary>
        public static SceneLoader? Instance { get; private set; } = null;

        #endregion

        #region Methods

        /// <summary>
        /// Loads the main menu scene. Also disables the logger if it exists.
        /// </summary>
        public async void LoadMainMenuAsync()
        {
            DisableLogger();
            await FadeOutAndLoadSceneAsync(_mainMenuScene);
        }

        /// <summary>
        /// Loads the player selection scene. Also clears the logger.
        /// </summary>
        public async void LoadPlayerSelectionAsync()
        {
            EasyUI.Logger.ClearLog();
            await FadeOutAndLoadSceneAsync(_playerSelectionScene);
        }

        /// <summary>
        /// Loads the game scene with.
        /// </summary>
        public async void LoadGameAsync()
        {
            await FadeOutAndLoadSceneAsync(_gameScene);
        }

        /// <summary>
        /// Loads the final results scene.
        /// </summary>
        public async void LoadFinalResultsAsync()
        {
            await FadeOutAndLoadSceneAsync(_finalResultsScene);
        }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // safety check
            if (fadeAnimator == null) {
                Debug.LogError("Fade animator is not assigned in the inspector.");
                return;
            }

            // fade in when a scene is loaded
            SceneManager.sceneLoaded += FadeIn;
        }

        private void FadeIn(Scene scene, LoadSceneMode mode)
        {
            if (fadeAnimator == null) {
                return;
            }

            fadeAnimator.CrossFade(_fadeInAnimation, 0, 0);
        }

        private async Task FadeOutAndLoadSceneAsync(string sceneName)
        {
            if (fadeAnimator == null) {
                return;
            }

            // fade out
            PauseLogic.CanBePaused = false;
            fadeAnimator.CrossFade(_fadeOutAnimation, 0, 0);
            float animationLength = fadeAnimator.runtimeAnimatorController.animationClips[0].length;
            await Awaitable.WaitForSecondsAsync(animationLength);
            await SceneManager.LoadSceneAsync(sceneName);
        }

        private void DisableLogger()
        {
            if (EasyUI.Logger.Instance != null) {
                EasyUI.Logger.Instance.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}