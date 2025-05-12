#nullable enable

namespace ProjectL.UI.GameScene.Actions
{
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public enum PlayerMode
    {
        Interactive, NonInteractive
    }

    public enum ActionMode
    {
        Normal, FinishingTouches, RewardSelection
    }

    public interface IHumanPlayerActionListener
    {
        #region Methods

        public void OnActionRequested();

        public void OnActionCanceled();

        public void OnActionConfirmed();

        #endregion
    }

    public interface ITakePuzzleActionListener : IHumanPlayerActionListener { }
    public interface IRecycleActionListener : IHumanPlayerActionListener { }
    public interface ITetrominoActionListener : IHumanPlayerActionListener { }
    public interface IPlacePieceActionListener : IHumanPlayerActionListener { }
    public interface IMasterActionListener : IHumanPlayerActionListener { }


    public class ActionEventSet
    {
        #region Events

        public event Action? OnActionRequested;
        public event Action? OnActionCanceled;
        public event Action? OnActionConfirmed;

        #endregion

        #region Methods

        public void Subscribe(IHumanPlayerActionListener listener)
        {
            OnActionRequested += listener.OnActionRequested;
            OnActionCanceled += listener.OnActionCanceled;
            OnActionConfirmed += listener.OnActionConfirmed;
        }

        public void Unsubscribe(IHumanPlayerActionListener listener)
        {
            OnActionRequested -= listener.OnActionRequested;
            OnActionCanceled -= listener.OnActionCanceled;
            OnActionConfirmed -= listener.OnActionConfirmed;
        }

        public void RaiseRequested() => OnActionRequested?.Invoke();
        public void RaiseCanceled() => OnActionCanceled?.Invoke();
        public void RaiseConfirmed() => OnActionConfirmed?.Invoke();

        #endregion
    }

    public class ActionCreationManager : StaticInstance<ActionCreationManager>
    {
        #region Fields

        private GameAction? _lastValidAction;

        private HumanPlayer? _currentPlayer;
        private ActionVerifier? _actionVerifier;

        private ActionType? _currentActionType;

        private ActionEventSet? CurrentEventSet => _currentActionType == null ? null : _actionEventSets[_currentActionType.Value];

        private enum ActionType
        {
            TakePuzzle,
            Recycle,
            TakeBasicTetromino,
            ChangeTetromino,
            PlacePiece,
            Master
        }

        private readonly ActionEventSet _tetrominoActionEvents = new();
        private readonly Dictionary<ActionType, ActionEventSet> _actionEventSets = new();

        #endregion

        #region Methods

        public void AddListener(ITakePuzzleActionListener listener) => _actionEventSets[ActionType.TakePuzzle].Subscribe(listener);
        public void RemoveListener(ITakePuzzleActionListener listener) => _actionEventSets[ActionType.TakePuzzle].Unsubscribe(listener);

        public void AddListener(IRecycleActionListener listener) => _actionEventSets[ActionType.Recycle].Subscribe(listener);
        public void RemoveListener(IRecycleActionListener listener) => _actionEventSets[ActionType.Recycle].Unsubscribe(listener);

        public void AddListener(ITetrominoActionListener listener) => _tetrominoActionEvents.Subscribe(listener);
        public void RemoveListener(ITetrominoActionListener listener) => _tetrominoActionEvents.Unsubscribe(listener);

        public void AddListener(IPlacePieceActionListener listener) => _actionEventSets[ActionType.PlacePiece].Subscribe(listener);
        public void RemoveListener(IPlacePieceActionListener listener) => _actionEventSets[ActionType.PlacePiece].Unsubscribe(listener);

        public void AddListener(IMasterActionListener listener) => _actionEventSets[ActionType.Master].Subscribe(listener);
        public void RemoveListener(IMasterActionListener listener) => _actionEventSets[ActionType.Master].Unsubscribe(listener);

        public void Register(HumanPlayer player)
        {
            player.ActionRequested += Player_ActionRequested;
            player.RewardChoiceRequested += Player_RewardChoiceRequested;
        }

        public void Start()
        {
            _actionEventSets[ActionType.TakePuzzle] = new();
            _actionEventSets[ActionType.Recycle] = new();
            _actionEventSets[ActionType.TakeBasicTetromino] = _tetrominoActionEvents;
            _actionEventSets[ActionType.ChangeTetromino] = _tetrominoActionEvents;
            _actionEventSets[ActionType.PlacePiece] = new();
            _actionEventSets[ActionType.Master] = new();

            // TODO: connect to spawners

            ActionZonesManager.Instance.ConnectToButtons(this);
            ActionZonesManager.Instance.SetPlayerMode(PlayerMode.NonInteractive);
            ActionZonesManager.Instance.SetActionMode(ActionMode.Normal);
        }

        public void ReportStateChanged()
        {
            if (_currentActionType == null) {
                return;
            }

            // TODO: handle state changes
            switch (_currentActionType.Value) {
                case ActionType.TakePuzzle:
                    break;
                case ActionType.Recycle:
                    break;
                case ActionType.TakeBasicTetromino:
                    break;
                case ActionType.ChangeTetromino:
                    break;
                case ActionType.PlacePiece:
                    break;
                case ActionType.Master:
                    break;
                default:
                    break;
            }

            // TODO: verify if the action is valid
            // TODO: enable / disable confirm buttons
        }

        public void OnActionConfirmed()
        {
            if (_lastValidAction == null || _currentPlayer == null) {
                Debug.LogError("Internal error", this);  // should never happen
                return;
            }
            CurrentEventSet?.RaiseConfirmed();
            ActionZonesManager.Instance.SetPlayerMode(PlayerMode.NonInteractive);

            _currentPlayer.SetAction(_lastValidAction);
            _lastValidAction = null;
            _currentPlayer = null;
        }

        public void OnActionCanceled()
        {
            CurrentEventSet?.RaiseCanceled();
            _lastValidAction = null;
            _currentActionType = null;

            ActionZonesManager.Instance.DisableConfirmButtons();
            // TODO: enable spawners
        }

        public void OnTakePuzzleActionRequested() => SetNewActionType(ActionType.TakePuzzle);

        public void OnRecycleActionRequested() => SetNewActionType(ActionType.Recycle);

        public void OnTakeBasicTetrominoActionRequested() =>  SetNewActionType(ActionType.TakeBasicTetromino);

        public void OnChangeTetrominoActionRequested() => SetNewActionType(ActionType.ChangeTetromino);

        public void OnPlacePieceActionRequested() => SetNewActionType(ActionType.PlacePiece);

        public void OnMasterActionRequested() => SetNewActionType(ActionType.Master);

        private void Player_ActionRequested(object? sender, HumanPlayer.GetActionEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                Debug.LogError("Sender is not a HumanPlayer!", this);
                return;
            }
            _currentPlayer = player;
            _actionVerifier = e.Verifier;
            ActionZonesManager.Instance.SetPlayerMode(PlayerMode.Interactive);
            // TODO: enable spawners
        }

        private void Player_RewardChoiceRequested(object? sender, HumanPlayer.GetRewardEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                Debug.LogError("Sender is not a HumanPlayer!", this);
                return;
            }

            // TODO: handle reward choice
            throw new NotImplementedException();
        }

        private void SetNewActionType(ActionType newType)
        {
            if (_currentActionType != null) {
                OnActionCanceled();
            }

            if (newType != ActionType.PlacePiece || newType != ActionType.Master) {
                // TODO: disable spawners
            }

            _currentActionType = newType;
            CurrentEventSet?.RaiseRequested();
        }

        #endregion
    }
}
