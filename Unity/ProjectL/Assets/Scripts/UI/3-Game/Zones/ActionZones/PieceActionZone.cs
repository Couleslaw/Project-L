#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectL.Management;
    using ProjectL.UI.GameScene.Actions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PieceActionZone : ActionZoneBase
    {
        #region Fields

        [SerializeField] private ActionButton? _takeBasicTetrominoButton;

        [SerializeField] private ActionButton? _changeTetrominoButton;

        [SerializeField] private ActionButton? _masterActionButton;

        #endregion


        #region Methods

        public void ManuallyClickTakeBasicTetrominoButton() => _takeBasicTetrominoButton?.ManuallySelectButton();

        public void ManuallyClickChangeTetrominoButton() => _changeTetrominoButton?.ManuallySelectButton();

        public void ManuallyClickMasterActionButton() => _masterActionButton?.ManuallySelectButton();

        public override void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
        {
            _takeBasicTetrominoButton!.CanActionBeCreated = CanTakeBasicTetromino(gameInfo);
            _changeTetrominoButton!.CanActionBeCreated = CanChangeTetromino(gameInfo, playerInfo);
            _masterActionButton!.CanActionBeCreated = CanMasterAction(playerInfo, turnInfo);
        }

        public bool CanMasterAction(PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
        {
            if (turnInfo.UsedMasterAction) {
                return false;
            }
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                if (playerInfo.NumTetrominosOwned[i] > 0) {
                    return true;
                }
            }
            return false;
        }

        public override void SetPlayerMode(PlayerMode mode)
        {
            base.SetPlayerMode(mode);
            _takeBasicTetrominoButton!.Mode = mode;
            _changeTetrominoButton!.Mode = mode;
            _masterActionButton!.Mode = mode;
        }

        public override void AddListener(HumanPlayerActionCreator acm)
        {
            base.AddListener(acm);
            _finishingTouchesButton!.onClick.AddListener(acm.OnEndFinishingTouchesActionRequested);
            _takeBasicTetrominoButton!.SelectActionEventHandler += acm.OnTakeBasicTetrominoActionRequested;
            _changeTetrominoButton!.SelectActionEventHandler += acm.OnChangeTetrominoActionRequested;
            _masterActionButton!.SelectActionEventHandler += acm.OnMasterActionRequested;
        }

        public override void RemoveListener(HumanPlayerActionCreator acm)
        {
            base.RemoveListener(acm);
            _finishingTouchesButton!.onClick.RemoveListener(acm.OnEndFinishingTouchesActionRequested);
            _takeBasicTetrominoButton!.SelectActionEventHandler -= acm.OnTakeBasicTetrominoActionRequested;
            _changeTetrominoButton!.SelectActionEventHandler -= acm.OnChangeTetrominoActionRequested;
            _masterActionButton!.SelectActionEventHandler -= acm.OnMasterActionRequested;
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

        protected override void Awake()
        {
            base.Awake();
            if (_takeBasicTetrominoButton == null || _changeTetrominoButton == null || _masterActionButton == null) {
                Debug.LogError("Action buttons are not assigned in the inspector!", this);
            }
        }


        #endregion
    }
}
