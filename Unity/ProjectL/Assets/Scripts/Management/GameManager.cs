#nullable enable

namespace ProjectL.Management
{
    using ProjectL.InputActions;
    using ProjectL.Pause;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Core singleton managing game state (pause/resume), player input via <see cref="GameControls"/>,
    /// the in-game logger, and scene-specific configurations.
    /// </summary>
    /// <remarks>
    /// <para>Responsibilities include:</para>
    /// <list type="bullet">
    ///   <item><description>Implementing game pause/resume logic, toggling relevant input action maps.</description></item>
    ///   <item><description>Providing static access to the <see cref="GameControls"/> instance.</description></item>
    ///   <item><description>Managing visibility and clearing of the in-game <see cref="EasyUI.Logger"/>.</description></item>
    ///   <item><description>Adjusting pause capabilities and logger visibility based on the currently loaded scene.</description></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="ProjectL.Singleton&lt;ProjectL.Management.GameManager&gt;" />
    public class GameManager : Singleton<GameManager>
    {
        #region Fields

        [SerializeField] private PauseMenu? _pauseMenu;

        [SerializeField] private EasyUI.Logger? _logger = null;

        private GameControls? _gameControls;

        #endregion

        #region Properties

        /// <summary>
        /// The singleton instance of the <see cref="GameControls"/> class. Can be used to connect to player input actions
        /// </summary>
        public static GameControls? Controls => (Instance != null) ? Instance._gameControls : null;

        /// <summary>
        /// <see langword="true"/> if the game is currently paused; otherwise, <see langword="false"/>.
        /// </summary>
        public static bool IsGamePaused { get; private set; } = false;

        /// <summary>
        /// <see langword="true"/> if the game can be paused; otherwise, <see langword="false"/>.
        /// </summary>
        public static bool CanGameBePaused { get; set; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// Resumes the game if it is currently paused. Hides the pause menu and re-enables gameplay controls.
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused || _pauseMenu == null) {
                return;
            }

            IsGamePaused = false;
            Time.timeScale = 1f;
            _pauseMenu.Hide();

            _gameControls!.UI.Disable();
            _gameControls!.Gameplay.Enable();
        }

        /// <summary>
        /// Opens the in-game logger if it is not already open.
        /// </summary>
        public void OpenLogger()
        {
            if (_logger != null && !_logger.IsOpen) {
                _logger.ToggleLogUI();
            }
        }

        private void CloseLogger()
        {
            if (_logger != null && _logger.IsOpen) {
                _logger.ToggleLogUI();
            }
        }

        protected override void Awake()
        {
            // singleton pattern
            base.Awake();
            if (Instance != this) {
                return;
            }

            if (_pauseMenu == null) {
                Debug.LogError("PauseMenuManager is not assigned in the inspector.");
                return;
            }
            if (_logger == null) {
                Debug.LogError("Logger prefab is not assigned in the inspector.");
                return;
            }

            // pause logic
            _pauseMenu = Instantiate(_pauseMenu, gameObject.transform);
            _pauseMenu.Hide();   // hide the pause menu in main menu

            // logging
            _logger = Instantiate(_logger, gameObject.transform);
            _logger.Hide();    // hide the logger in main menu

            // game controls
            _gameControls = new GameControls();
            _gameControls.UI.Disable();
            _gameControls.Gameplay.Enable();
            _gameControls.UI.ResumeGame.performed += ctx => ResumeGame();
            _gameControls.Gameplay.PauseGame.performed += ctx => PauseGame();

            // scene loading
            SceneManager.sceneLoaded += (scene, _) => OnSceneLoaded(scene);
        }

        private void PauseGame()
        {
            if (!CanGameBePaused || IsGamePaused || _pauseMenu == null) {
                return;
            }

            IsGamePaused = true;
            Time.timeScale = 0f;
            _pauseMenu.Show();

            _gameControls!.Gameplay.Disable();
            _gameControls!.UI.Enable();
        }

        private void OnSceneLoaded(Scene scene)
        {
            ResumeGame();

            // enable pausing iff scene is GAME or FINAL RESULTS
            CanGameBePaused = scene.name == SceneLoader.GameScene || scene.name == SceneLoader.FinalResultsScene;

            CloseLogger();

            // hide and clear logger in main menu
            if (scene.name == SceneLoader.MainMenuScene) {
                _logger!.Hide();
                _logger!.ClearLog();
            }
            else {
                _logger!.Show();
            }
        }

        #endregion
    }
}
