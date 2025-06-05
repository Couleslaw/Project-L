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

        #endregion

        #region Methods

        protected override void Start()
        {
            base.Start();

            if (_pauseMenuButton == null || _recycleButton == null)
            {
                Debug.LogError("PuzzleActionZone is missing required buttons!", this);
                return;
            }

            _pauseMenuButton.onClick.AddListener(OnPauseMenuButtonClicked);
        }

        public override void AddListener(HumanPlayerActionCreationManager acm)
        {
            base.AddListener(acm);
            _finishingTouchesButton!.onClick.AddListener(acm.OnClearBoardRequested);
            _recycleButton!.SelectActionEventHandler += acm.OnRecycleActionRequested;
        }

        public override void RemoveListener(HumanPlayerActionCreationManager acm)
        {
            base.RemoveListener(acm);
            _finishingTouchesButton!.onClick.RemoveListener(acm.OnClearBoardRequested);
            _recycleButton!.SelectActionEventHandler -= acm.OnRecycleActionRequested;
        }

        public void ManuallyClickRecycleButton() => _recycleButton?.ManuallySelectButton();


        public override void SetPlayerMode(PlayerMode mode)
        {
            base.SetPlayerMode(mode);
            _recycleButton!.Mode = mode;
        }

        public override void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
        {
            bool areThereStillSomePuzzles = gameInfo.AvailableBlackPuzzles.Length > 0 || gameInfo.AvailableWhitePuzzles.Length > 0;
            _recycleButton!.CanActionBeCreated = areThereStillSomePuzzles;
        }

        private void OnPauseMenuButtonClicked()
        {
            SoundManager.Instance!.PlayButtonClickSound();
            EventSystem.current.SetSelectedGameObject(null!);
            GameManager.Instance.PauseGame();
        }

        #endregion
    }
}
