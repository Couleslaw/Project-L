#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;

    public class PieceActionZone : ActionZoneBase
    {
        [SerializeField] private ActionButton? _takeBasicTetrominoButton;
        [SerializeField] private ActionButton? _changeTetrominoButton;
        [SerializeField] private ActionButton? _masterActionButton;

        public event Action? OnEndFinishingTouchesButtonClick;
        public event Action? OnTakeBasicTetrominoButtonClick;
        public event Action? OnChangeTetrominoButtonClick;
        public event Action? OnMasterActionButtonClick;

        public void ManuallyPressTakeBasicTetrominoButton() => _takeBasicTetrominoButton?.ManuallySelectButton();
        public void ManuallyPressChangeTetrominoButton() => _changeTetrominoButton?.ManuallySelectButton();
        public void ManuallyPressMasterActionButton() => _masterActionButton?.ManuallySelectButton();

        public override void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo)
        {
            _takeBasicTetrominoButton!.CanActionBeCreated = CanTakeBasicTetromino(gameInfo);
            _changeTetrominoButton!.CanActionBeCreated = CanChangeTetromino(gameInfo, playerInfo);
            _masterActionButton!.CanActionBeCreated = CanMasterAction(playerInfo);
        }

        private bool CanTakeBasicTetromino(GameState.GameInfo gameInfo)
        {
            return gameInfo.NumTetrominosLeft[(int)TetrominoShape.O1] > 0;
        }

        private bool CanChangeTetromino(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo)
        {
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                if (playerInfo.NumTetrominosOwned[i] > 0) {
                    TetrominoShape shape = (TetrominoShape)i;
                    if (RewardManager.GetUpgradeOptions(gameInfo.NumTetrominosLeft, shape).Count > 0) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanMasterAction(PlayerState.PlayerInfo playerInfo)
        {
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                if (playerInfo.NumTetrominosOwned[i] > 0) {
                    return true;
                }
            }
            return false;
        }

        public override void SetPlayerMode(PlayerMode mode)
        {
            _takeBasicTetrominoButton!.Mode = mode;
            _changeTetrominoButton!.Mode = mode;
            _masterActionButton!.Mode = mode;
        }

        private new void Awake()
        {
            base.Awake();

            if (_takeBasicTetrominoButton == null || _changeTetrominoButton == null || _masterActionButton == null) {
                Debug.LogError("One or more buttons are not assigned in the inspector", this);
                return;
            }

            _finishingTouchesButton!.onClick.AddListener(
                () => OnEndFinishingTouchesButtonClick?.Invoke()
            );

            _takeBasicTetrominoButton.SelectAction += OnTakeBasicTetrominoButtonClick;
            _changeTetrominoButton.SelectAction += OnChangeTetrominoButtonClick;
            _masterActionButton.SelectAction += OnMasterActionButtonClick;
        }
    }
}
