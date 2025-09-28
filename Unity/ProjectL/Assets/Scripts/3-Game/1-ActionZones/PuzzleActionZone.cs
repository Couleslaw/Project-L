#nullable enable

namespace ProjectL.GameScene.ActionZones
{
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.Management;
    using ProjectL.Sound;
    using ProjectLCore.GameLogic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class PuzzleActionZone : ActionZoneBase
    {
        #region Fields

        [SerializeField] private ActionButton? _recycleButton;
        [SerializeField] private Button? _pauseMenuButton;
        [SerializeField] private Button? _endTurnButton;
        [SerializeField] private EndTurnBox? _endTurnBoxPrefab;

        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();

            if (_pauseMenuButton == null || _recycleButton == null || _endTurnButton == null || _endTurnBoxPrefab == null) {
                Debug.LogError("PuzzleActionZone is missing a required component!", this);
                return;
            }

            _pauseMenuButton.onClick.AddListener(OnPauseMenuButtonClicked);
            _endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
            _endTurnBoxPrefab = Instantiate(_endTurnBoxPrefab);
            _endTurnBoxPrefab.gameObject.SetActive(false);
        }

        public override void AddListener(HumanPlayerActionCreationManager acm)
        {
            base.AddListener(acm);
            _finishingTouchesButton!.onClick.AddListener(acm.OnClearBoardRequested);
            _endTurnBoxPrefab!.AddListener(acm.OnEndTurnRequested);
            _recycleButton!.SelectActionEventHandler += acm.OnRecycleActionRequested;
        }

        public override void RemoveListener(HumanPlayerActionCreationManager acm)
        {
            base.RemoveListener(acm);
            _finishingTouchesButton!.onClick.RemoveListener(acm.OnClearBoardRequested);
            _endTurnBoxPrefab!.RemoveListener(acm.OnEndTurnRequested);
            _recycleButton!.SelectActionEventHandler -= acm.OnRecycleActionRequested;
        }

        public void ManuallyClickRecycleButton() => _recycleButton?.ManuallySelectButton();


        public override void SetPlayerMode(PlayerMode mode)
        {
            base.SetPlayerMode(mode);
            _recycleButton!.Mode = mode;
            _endTurnButton!.interactable = mode == PlayerMode.Interactive;
        }

        public override void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
        {
            bool areThereStillSomePuzzles = gameInfo.AvailableBlackPuzzles.Length > 0 || gameInfo.AvailableWhitePuzzles.Length > 0;
            _recycleButton!.CanActionBeCreated = areThereStillSomePuzzles;

            // once end of the game is triggered, change the confirm button of the puzzle zone
            // into a skip action button
            if (turnInfo.GamePhase == GamePhase.EndOfTheGame) {
                _endTurnButton!.gameObject.SetActive(true);
                _confirmButton!.gameObject.SetActive(false);
            }
            if (turnInfo.GamePhase == GamePhase.FinishingTouches) {
                _endTurnButton!.gameObject.SetActive(false);
            }
        }

        private void OnPauseMenuButtonClicked()
        {
            SoundManager.Instance!.PlayButtonClickSound();
            EventSystem.current.SetSelectedGameObject(null!);
            GameManager.Instance.PauseGame();
        }

        private void OnEndTurnButtonClicked()
        {
            SoundManager.Instance!.PlayButtonClickSound();
            _endTurnBoxPrefab!.gameObject.SetActive(true);
        }

        #endregion
    }
}
