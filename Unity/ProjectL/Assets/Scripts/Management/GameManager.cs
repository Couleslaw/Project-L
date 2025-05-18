#nullable enable

namespace ProjectL.Management
{
    using ProjectL.InputActions;
    using ProjectL.UI.Pause;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class GameManager : MonoBehaviour
    {
        #region Fields

        [SerializeField] private PauseMenu? _pauseMenu;
        [SerializeField] private EasyUI.Logger? _logger = null;


        private GameControls? _gameControls;

        #endregion

        #region Properties

        public static GameControls? Controls => (Instance != null) ? Instance._gameControls : null;

        public static GameManager? Instance { get; private set; } = null;

        public static bool IsGamePaused { get; private set; } = false;

        public static bool CanGameBePaused { get; set; } = false;

        #endregion

        #region Methods

        public void PauseGame()
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

        public void OpenLogger()
        {
            if (_logger != null && !_logger.IsOpen) {
                _logger.ToggleLogUI();
            }
        }

        private void Awake()
        {
            // singleton pattern
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
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

            Instance = this;
            DontDestroyOnLoad(gameObject);

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
            SceneManager.sceneLoaded += (scene, _)  => OnSceneLoaded(scene);
        }

        private void OnSceneLoaded(Scene scene)
        {
            ResumeGame();

            // enable pausing iff scene is GAME or FINAL RESULTS
            CanGameBePaused = scene.name == SceneLoader.GameScene || scene.name == SceneLoader.FinalResultsScene;

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
