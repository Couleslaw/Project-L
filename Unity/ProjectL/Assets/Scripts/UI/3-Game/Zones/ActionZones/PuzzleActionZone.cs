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
        public event Action? OnClearBoardButtonClick;
        public event Action? OnRecycleButtonClick;
        public event Action? OnTakePuzzleButtonClick;

        [SerializeField] private ActionButton? _recycleButton;
        [SerializeField] private ActionButton? _takePuzzleButton;

        public void ManuallyClickRecycleButton() => _recycleButton?.ManuallySelectButton();
        public void ManuallyClickTakePuzzleButton() => _takePuzzleButton?.ManuallySelectButton();

        private new void Awake()
        {
            base.Awake();

            if (_recycleButton == null || _takePuzzleButton == null) {
                Debug.LogError("One or more buttons are not assigned in the inspector", this);
                return;
            }

            _finishingTouchesButton!.onClick.AddListener(
                () => OnClearBoardButtonClick?.Invoke()
            );
            _recycleButton.SelectAction += OnRecycleButtonClick;
            _takePuzzleButton.SelectAction += OnTakePuzzleButtonClick;
        }

        public override void SetPlayerMode(PlayerMode mode)
        {
            _recycleButton!.Mode = mode;
            _takePuzzleButton!.Mode = mode;
        }

        public override void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo)
        {
            bool areThereStillSomePuzzles = gameInfo.AvailableBlackPuzzles.Length > 0 || gameInfo.AvailableWhitePuzzles.Length > 0;
            _recycleButton!.CanActionBeCreated = areThereStillSomePuzzles;
            _takePuzzleButton!.CanActionBeCreated = areThereStillSomePuzzles;
        }
    }
}
