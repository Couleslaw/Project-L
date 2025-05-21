#nullable enable

namespace ProjectL.GameScene.ActionZones
{
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.Management;
    using ProjectLCore.GameLogic;
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class ActionZonesManager : GraphicsManager<ActionZonesManager>, ICurrentTurnListener
    {
        #region Fields

        private GameCore? _game;

        [SerializeField] private PuzzleActionZone? _puzzleActionZone;

        [SerializeField] private PieceActionZone? _pieceActionZone;

        private GamePhase _currentGamePhase;

        #endregion

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

        #region Properties

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

        #endregion

        #region Methods

        public override void Init(GameCore game)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                Debug.LogError("One or more action zones are not assigned in the inspector", this);
                return;
            }

            _game = game;
            _game.AddListener((ICurrentTurnListener)this);
        }

        public void ConnectToActionButtons(HumanPlayerActionCreationManager acm)
        {
            if (GameManager.Controls == null) {
                return;
            }
            ActionButton.CancelActionEventHandler += acm.OnActionCanceled;
            GameManager.Controls.Gameplay.CancelAction.performed += OnCancelActionRequested;
            GameManager.Controls.Gameplay.ConfirmAction.performed += OnConfirmActionRequested;
            _pieceActionZone?.AddListener(acm);
            _puzzleActionZone?.AddListener(acm);
        }

        public void DisconnectFromActionButtons(HumanPlayerActionCreationManager acm)
        {
            if (GameManager.Controls == null) {
                return;
            }
            GameManager.Controls.Gameplay.CancelAction.performed -= OnCancelActionRequested;
            GameManager.Controls.Gameplay.ConfirmAction.performed -= OnConfirmActionRequested;
            ActionButton.CancelActionEventHandler -= acm.OnActionCanceled;
            _pieceActionZone?.RemoveListener(acm);
            _puzzleActionZone?.RemoveListener(acm);
        }

        public void ConnectToSelectRewardButtons(HumanPlayerActionCreationManager acm)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                return;
            }
            _pieceActionZone.AddSelectRewardListener(acm);
            _puzzleActionZone.AddSelectRewardListener(acm);
        }

        public void DisconnectFromSelectRewardButton(HumanPlayerActionCreationManager acm)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                return;
            }
            _pieceActionZone.RemoveSelectRewardListener(acm);
            _puzzleActionZone.RemoveSelectRewardListener(acm);
        }

        public void ManuallyClickTakePuzzleButton() => _puzzleActionZone?.ManuallyClickTakePuzzleButton();

        private void OnCancelActionRequested(InputAction.CallbackContext ctx)
        {
            if (HumanPlayerActionCreationManager.Instance != null) {
                HumanPlayerActionCreationManager.Instance.OnActionCanceled();
                ActionButton.DeselectCurrentButton();
            }
        }

        private void OnConfirmActionRequested(InputAction.CallbackContext ctx)
        {
            GamePhase gamePhase = _currentGamePhase;

            if (gamePhase == GamePhase.Finished) {
                return;
            }

            // click CONFIRM / SELECT REWARD / ENF FINISHING TOUCHES
            _pieceActionZone?.SimulateConfirmActionClick();

            // if finishing touches - PuzzleActionZone has CLEAR BOARD --> dont click it
            if (gamePhase != GamePhase.FinishingTouches) {
                _puzzleActionZone?.SimulateConfirmActionClick();
            }
        }

        void ICurrentTurnListener.OnCurrentTurnChanged(TurnInfo currentTurnInfo)
        {
            _currentGamePhase = currentTurnInfo.GamePhase;

            var gameInfo = _game!.GameState.GetGameInfo();
            var playerInfo = _game.PlayerStates[_game.CurrentPlayer].GetPlayerInfo();

            _pieceActionZone?.EnabledButtonsBasedOnGameState(gameInfo, playerInfo, currentTurnInfo);
            _puzzleActionZone?.EnabledButtonsBasedOnGameState(gameInfo, playerInfo, currentTurnInfo);
        }

        #endregion

        public class DisposableButtonSelector : IDisposable
        {
            #region Fields

            private readonly Button _button;

            #endregion

            #region Constructors

            public DisposableButtonSelector(Button button)
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
                        Instance._pieceActionZone?.SetActionMode(ActionMode.RewardSelection);
                        Instance._puzzleActionZone?.SetActionMode(ActionMode.RewardSelection);
                        break;
                    case Button.EndFinishingTouches:
                        Instance._pieceActionZone?.ManuallyClickFinishingTouchesButton();
                        break;
                    default:
                        break;
                }
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                if (Instance == null) {
                    return;
                }

                switch (_button) {
                    case Button.TakePuzzle:
                    case Button.Recycle:
                    case Button.TakeBasicTetromino:
                    case Button.ChangeTetromino:
                    case Button.MasterAction:
                        ActionButton.DeselectCurrentButton();
                        break;
                    case Button.SelectReward:
                        Instance._pieceActionZone?.SetActionMode(ActionMode.ActionCreation);
                        Instance._puzzleActionZone?.SetActionMode(ActionMode.ActionCreation);
                        break;
                    case Button.EndFinishingTouches:
                        break;
                    default:
                        break;
                }
            }

            #endregion
        }
    }
}
