#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectLCore.GameLogic;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class PuzzleActionZone : ActionZoneBase
    {
        [SerializeField] private ActionButton? _recycleButton;
        [SerializeField] private ActionButton? _takePuzzleButton;

        public override void AddListener(HumanPlayerActionCreator acm)
        {
            base.AddListener(acm);
            _finishingTouchesButton!.onClick.AddListener(acm.OnClearBoardRequested);
            _takePuzzleButton!.SelectActionEventHandler += acm.OnTakePuzzleActionRequested;
            _recycleButton!.SelectActionEventHandler += acm.OnRecycleActionRequested;
        }

        public override void RemoveListener(HumanPlayerActionCreator acm)
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

        protected override void Awake()
        {
            base.Awake();
            if (_recycleButton == null || _takePuzzleButton == null) {
                Debug.LogError("Action buttons are not assigned in the inspector!", this);
            }
        }
    }
}
