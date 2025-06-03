#nullable enable

namespace ProjectL.GameScene.ActionZones
{
    using ProjectL.GameScene.ActionHandling;
    using ProjectLCore.GameLogic;
    using UnityEngine;

    public class PuzzleActionZone : ActionZoneBase
    {
        #region Fields

        [SerializeField] private ActionButton? _recycleButton;

        [SerializeField] private ActionButton? _takePuzzleButton;

        #endregion

        #region Methods

        public override void AddListener(HumanPlayerActionCreationManager acm)
        {
            base.AddListener(acm);
            _finishingTouchesButton!.onClick.AddListener(acm.OnClearBoardRequested);
            _takePuzzleButton!.SelectActionEventHandler += acm.OnTakePuzzleActionRequested;
            _recycleButton!.SelectActionEventHandler += acm.OnRecycleActionRequested;
        }

        public override void RemoveListener(HumanPlayerActionCreationManager acm)
        {
            base.RemoveListener(acm);
            _finishingTouchesButton!.onClick.RemoveListener(acm.OnClearBoardRequested);
            _takePuzzleButton!.SelectActionEventHandler -= acm.OnTakePuzzleActionRequested;
            _recycleButton!.SelectActionEventHandler -= acm.OnRecycleActionRequested;
        }

        public void ManuallyClickRecycleButton() => _recycleButton?.ManuallySelectButton();

        public void ManuallyClickTakePuzzleButton() => _takePuzzleButton?.ManuallySelectButton();

        public override void SetPlayerMode(PlayerMode mode)
        {
            base.SetPlayerMode(mode);
            _recycleButton!.Mode = mode;
            _takePuzzleButton!.Mode = mode;
        }

        public override void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
        {
            bool areThereStillSomePuzzles = gameInfo.AvailableBlackPuzzles.Length > 0 || gameInfo.AvailableWhitePuzzles.Length > 0;
            _recycleButton!.CanActionBeCreated = areThereStillSomePuzzles;
            _takePuzzleButton!.CanActionBeCreated = areThereStillSomePuzzles;
        }

        #endregion
    }
}
