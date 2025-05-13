#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectLCore.GameLogic;
    using System;
    using UnityEngine;
    using ProjectL.UI.GameScene.Actions;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine.Android;
    using System.Runtime.CompilerServices;

    public class ActionZonesManager : GraphicsManager<ActionZonesManager>, ICurrentTurnListener, IGameActionController
    {
        private GameCore? _game;

        [SerializeField] private PuzzleActionZone? _puzzleActionZone;
        [SerializeField] private PieceActionZone? _pieceActionZone;

        public override void Init(GameCore game)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                Debug.LogError("One or more action zones are not assigned in the inspector", this);
                return;
            }

            _game = game;
            _game.AddListener(this);
        }

        public bool CanConfirmAction {
            set {
                if (_pieceActionZone != null && _puzzleActionZone != null) {
                    _pieceActionZone!.CanConfirmAction = value;
                    _puzzleActionZone!.CanConfirmAction = value;
                }
            }
        }

        public bool CanSelectReward {
            set {
                if (_pieceActionZone != null && _puzzleActionZone != null) {
                    _pieceActionZone!.CanSelectReward = value;
                    _puzzleActionZone!.CanSelectReward = value;
                }
            }
        }

        private void AddListener(ActionCreationManager acm)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                return;
            }

            ActionButton.CancelActionEventHandler += acm.OnActionCanceled;
            _pieceActionZone.AddListener(acm);
            _puzzleActionZone.AddListener(acm);
        }

        private void RemoveListener(ActionCreationManager acm)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                return;
            }

            ActionButton.CancelActionEventHandler -= acm.OnActionCanceled;
            _pieceActionZone.RemoveListener(acm);
            _puzzleActionZone.RemoveListener(acm);
        }

        public void SetPlayerMode(PlayerMode mode)
        {
            _pieceActionZone?.SetPlayerMode(mode);
            _puzzleActionZone?.SetPlayerMode(mode);

            Debug.Log($"ActionZonesManager: SetPlayerMode {mode}");
            if (mode == PlayerMode.Interactive)
                AddListener(ActionCreationManager.Instance);
            if (mode == PlayerMode.NonInteractive)
                RemoveListener(ActionCreationManager.Instance);
        }

        public void SetActionMode(ActionMode mode)
        {
            _pieceActionZone?.SetActionMode(mode);
            _puzzleActionZone?.SetActionMode(mode);
        }


        void ICurrentTurnListener.OnCurrentTurnChanged(TurnInfo currentTurnInfo)
        {
            var gameInfo = _game!.GameState.GetGameInfo();
            var playerInfo = _game.PlayerStates[_game.CurrentPlayer].GetPlayerInfo();

            _pieceActionZone?.EnabledButtonsBasedOnGameState(gameInfo, playerInfo);
            _puzzleActionZone?.EnabledButtonsBasedOnGameState(gameInfo, playerInfo);
        }

        public class SimulateButtonClickDisposable : IDisposable
        {
            private readonly Button _button;

            public SimulateButtonClickDisposable(Button button) 
            {
                _button = button;
                switch (button) {
                    case Button.TakePuzzle:
                        Instance._puzzleActionZone?.ManuallyClickTakePuzzleButton();
                        break;
                    case Button.Recycle:
                        Instance._puzzleActionZone?.ManuallyClickRecycleButton();
                        break;
                    case Button.TakeBasicTetromino:
                        Instance._pieceActionZone?.ManuallyClickTakeBasicTetrominoButton();
                        break;
                    case Button.ChangeTetromino:
                        Instance._pieceActionZone?.ManuallyClickChangeTetrominoButton();
                        break;
                    case Button.MasterAction:
                        Instance._pieceActionZone?.ManuallyClickMasterActionButton();
                        break;
                    case Button.SelectReward:
                        Instance.SetActionMode(ActionMode.RewardSelection);
                        break;
                    case Button.EndFinishingTouches:
                        Instance._pieceActionZone?.ManuallyClickFinishingTouchesButton();
                        break;
                    default:
                        break;
                }
            }

            public void Dispose()
            {
                switch (_button) {
                    case Button.TakePuzzle:
                    case Button.Recycle:
                    case Button.TakeBasicTetromino:
                    case Button.ChangeTetromino:
                    case Button.MasterAction:
                        ActionButton.DeselectCurrentButton();
                        break;
                    case Button.SelectReward:
                        Instance.SetActionMode(ActionMode.Normal);
                        break;
                    case Button.EndFinishingTouches:
                        break;
                    default:
                        break;
                }
                // TODO: end finishing touches
            }
        }

        public enum Button
        {
            TakePuzzle,
            Recycle,
            TakeBasicTetromino,
            ChangeTetromino,
            MasterAction,
            EndFinishingTouches,
            SelectReward
        }
    }
}
