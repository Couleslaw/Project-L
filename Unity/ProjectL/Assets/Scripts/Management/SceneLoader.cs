#nullable enable

namespace ProjectL.Management
{
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.SceneManagement;


    /// <summary>
    /// Manages transitioning between different scenes.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Animator? fadeAnimator;

        private const string _fadeInAnimation = "FadeIn";
        private const string _fadeOutAnimation = "FadeOut";

        public const string MainMenuScene = "1-MainMenu";
        public const string PlayerSelectionScene = "2-PlayerSelection";
        public const string GameScene = "3-Game";
        public const string FinalResultsScene = "4-FinalResults";

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
            await FadeOutAndLoadSceneAsync(MainMenuScene);
        }

        /// <summary>
        /// Loads the player selection scene. Also clears the logger.
        /// </summary>
        public async void LoadPlayerSelectionAsync()
        {
            await FadeOutAndLoadSceneAsync(PlayerSelectionScene);
        }

        /// <summary>
        /// Loads the game scene with.
        /// </summary>
        public async void LoadGameAsync()
        {
            await FadeOutAndLoadSceneAsync(GameScene);
        }

        /// <summary>
        /// Loads the final results scene.
        /// </summary>
        public async void LoadFinalResultsAsync()
        {
            await FadeOutAndLoadSceneAsync(FinalResultsScene);
        }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // safety check
            if (fadeAnimator == null) {
                Debug.LogError("Fade animator is not assigned in the inspector.");
                return;
            }

            // fade in when a scene is loaded
            SceneManager.sceneLoaded += (_, _) => FadeIn();
        }

        private void FadeIn()
        {
            if (fadeAnimator == null) {
                return;
            }
            fadeAnimator.CrossFade(_fadeInAnimation, 0, 0);
        }

        private async Task FadeOutAndLoadSceneAsync(string sceneName)
        {
            // fade out
            GameManager.CanGameBePaused = false;

            if (fadeAnimator != null) {
                fadeAnimator.CrossFade(_fadeOutAnimation, 0, 0);
                float animationLength = fadeAnimator.runtimeAnimatorController.animationClips[0].length;
                await Awaitable.WaitForSecondsAsync(animationLength);
            }
            await SceneManager.LoadSceneAsync(sceneName);
        }

        #endregion
    }
}