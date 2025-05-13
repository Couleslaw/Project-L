#nullable enable

namespace ProjectL.UI.GameScene.Actions
{
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.GameScene.Zones.PieceZone;
    using ProjectL.UI.GameScene.Zones.PuzzleZone;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
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

    public enum ActionType
    {
        TakePuzzle,
        Recycle,
        TakeBasicTetromino,
        ChangeTetromino,
        PlacePiece,
        Master,
        SelectReward
    }

    public interface IGameActionController
    {
        #region Methods

        public void SetPlayerMode(PlayerMode mode);

        public void SetActionMode(ActionMode mode);

        #endregion
    }

    public interface IHumanPlayerActionListener<out T> where T : GameAction
    {
        #region Methods

        void OnActionRequested();

        void OnActionCanceled();

        void OnActionConfirmed();

        #endregion
    }

    public class ActionCreationManager : GraphicsManager<ActionCreationManager>, ICurrentTurnListener
    {
        #region Fields

        private static readonly Dictionary<Type, ActionType> _typeToEnumActionType = new() {
            { typeof(TakePuzzleAction), ActionType.TakePuzzle },
            { typeof(RecycleAction), ActionType.Recycle },
            { typeof(TakeBasicTetrominoAction), ActionType.TakeBasicTetromino },
            { typeof(ChangeTetrominoAction), ActionType.ChangeTetromino },
            { typeof(PlaceTetrominoAction), ActionType.PlacePiece },
            { typeof(MasterAction), ActionType.Master }
        };

        private readonly Dictionary<ActionType, IActionEventSet> _actionEventSets = new();

        private GameAction? _lastValidAction;
        private TetrominoShape? _lastValidReward;

        private HumanPlayer? _currentPlayer;

        private ActionVerifier? _actionVerifier;

        private ActionType? _currentActionType;

        #endregion

        private interface IActionEventSet
        {
            #region Methods

            void Subscribe<T>(IHumanPlayerActionListener<T> listener) where T : GameAction;

            void Unsubscribe<T>(IHumanPlayerActionListener<T> listener) where T : GameAction;

            void RaiseRequested();

            void RaiseCanceled();

            void RaiseConfirmed();

            #endregion
        }

        #region Properties

        private IActionEventSet? CurrentEventSet => _currentActionType == null ? null : _actionEventSets[_currentActionType.Value];

        #endregion

        #region Methods

        public void AddListener<T>(IHumanPlayerActionListener<T> listener) where T : GameAction
        {
            ActionType actionType = _typeToEnumActionType[typeof(T)];
            if (_actionEventSets.TryGetValue(actionType, out var eventSet)) {
                eventSet.Subscribe(listener);
            }
            else {
                Debug.LogError($"No action event set found for action type {typeof(T)}");
            }
        }

        public void RemoveListener<T>(IHumanPlayerActionListener<T> listener) where T : GameAction
        {
            ActionType actionType = _typeToEnumActionType[typeof(T)];
            if (_actionEventSets.TryGetValue(actionType, out var eventSet)) {
                eventSet.Unsubscribe(listener);
            }
            else {
                Debug.LogError($"No action event set found for action type {typeof(T)}");
            }
        }

        public void Register(HumanPlayer player)
        {
            Debug.Log($"Registered humanplayer {player.Name}");
            player.ActionRequested += Player_ActionRequested;
            player.RewardChoiceRequested += Player_RewardChoiceRequested;
        }

        public void ReportStateChanged()
        {
            if (_currentActionType == null) {
                return;
            }

            Debug.Log("ReportStateChanged");

            // TODO: handle state changes
            switch (_currentActionType.Value) {
                case ActionType.SelectReward:
                    break;
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

            // get action - put to verifier and set confirm button visibility
        }

        public void OnActionCanceled()
        {
            Debug.Log("Action cancellation requested");
            CurrentEventSet?.RaiseCanceled();
            _lastValidAction = null;
            _currentActionType = null;

            ActionZonesManager.Instance.CanConfirmAction = false;
        }

        public void OnActionConfirmed()
        {
            if (_lastValidAction == null || _currentPlayer == null) {
                Debug.LogError("Internal error", this);  // should never happen
                return;
            }
            CurrentEventSet?.RaiseConfirmed();
            SetPlayerMode(PlayerMode.NonInteractive);
            SubmitAction(_lastValidAction);
        }

        private void SubmitAction(GameAction action)
        {
            if (_currentPlayer == null) {
                Debug.LogError("Internal error", this);  // should never happen
                return;
            }
            _currentPlayer.SetAction(action);
            _lastValidAction = null;
            _currentActionType = null;
            _currentPlayer = null;
        }

        public void OnRewardSelected()
        {
            if (_lastValidReward == null || _currentPlayer == null) {
                Debug.LogError("Internal error", this);  // should never happen
                return;
            }
            
            SetActionMode(ActionMode.Normal);
            _currentPlayer.SetReward(_lastValidReward.Value);
            _lastValidReward = null;
            _currentActionType = null;
            _currentPlayer = null;
        }

        public void OnEndFinishingTouchesActionRequested()
        {
            SubmitAction(new EndFinishingTouchesAction());
        }

        public void OnClearBoardRequested()
        {
            OnActionCanceled();
        }

        public void OnTakePuzzleActionRequested() => SetNewActionType(ActionType.TakePuzzle);

        public void OnRecycleActionRequested() => SetNewActionType(ActionType.Recycle);

        public void OnTakeBasicTetrominoActionRequested() => SetNewActionType(ActionType.TakeBasicTetromino);

        public void OnChangeTetrominoActionRequested() => SetNewActionType(ActionType.ChangeTetromino);

        public void OnPlacePieceActionRequested() => SetNewActionType(ActionType.PlacePiece);

        public void OnMasterActionRequested() => SetNewActionType(ActionType.Master);

        public override void Init(GameCore game)
        {
            game.AddListener(this);
        }

        protected override void Start()
        {
            base.Start();
            _actionEventSets[ActionType.TakePuzzle] = new ActionEventSet<TakePuzzleAction>();
            _actionEventSets[ActionType.Recycle] = new ActionEventSet<RecycleAction>();
            _actionEventSets[ActionType.TakeBasicTetromino] = new ActionEventSet<TakeBasicTetrominoAction>();
            _actionEventSets[ActionType.ChangeTetromino] = new ActionEventSet<ChangeTetrominoAction>();
            _actionEventSets[ActionType.PlacePiece] = new ActionEventSet<PlaceTetrominoAction>();
            _actionEventSets[ActionType.Master] = new ActionEventSet<MasterAction>();

            SetPlayerMode(PlayerMode.NonInteractive);
            SetActionMode(ActionMode.Normal);
        }

        private void SetPlayerMode(PlayerMode mode)
        {
            ActionZonesManager.Instance.SetPlayerMode(mode);
            TetrominoButtonsManager.Instance.SetPlayerMode(mode);
            PuzzleZoneManager.Instance.SetPlayerMode(mode);
        }

        private void SetActionMode(ActionMode mode)
        {
            ActionZonesManager.Instance.SetActionMode(mode);
            TetrominoButtonsManager.Instance.SetActionMode(mode);
            PuzzleZoneManager.Instance.SetActionMode(mode);
        }

        private void Player_ActionRequested(object? sender, HumanPlayer.GetActionEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                throw new ApplicationException("Sender is not a HumanPlayer!");
            }
            Debug.Log($"Action requested by {player.Name}");
            _currentPlayer = player;
            _actionVerifier = e.Verifier;
            SetPlayerMode(PlayerMode.Interactive);
        }

        private void Player_RewardChoiceRequested(object? sender, HumanPlayer.GetRewardEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                throw new ApplicationException("Sender is not a HumanPlayer!");
            }

            Debug.Log($"Reward requested by {player.Name}");
            _currentPlayer = player;
            _currentActionType = ActionType.SelectReward;
            SetActionMode(ActionMode.RewardSelection);
        }

        private void SetNewActionType(ActionType newType)
        {
            if (_currentActionType != null) {
                OnActionCanceled();
            }

            if (newType != ActionType.PlacePiece || newType != ActionType.Master) {
                // TODO: disable spawners
            }

            Debug.Log($"Action type changed to {newType}");

            _currentActionType = newType;
            CurrentEventSet!.RaiseRequested();
        }

        void ICurrentTurnListener.OnCurrentTurnChanged(TurnInfo currentTurnInfo)
        {
            if (currentTurnInfo.GamePhase == GamePhase.FinishingTouches) {
                SetActionMode(ActionMode.FinishingTouches);
            }
        }

        #endregion

        private class ActionEventSet<T> : IActionEventSet where T : GameAction
        {
            #region Events

            public event Action? OnActionRequested;

            public event Action? OnActionCanceled;

            public event Action? OnActionConfirmed;

            #endregion

            #region Methods

            public void Subscribe(IHumanPlayerActionListener<T> listener)
            {
                OnActionRequested += listener.OnActionRequested;
                OnActionCanceled += listener.OnActionCanceled;
                OnActionConfirmed += listener.OnActionConfirmed;
            }

            public void Unsubscribe(IHumanPlayerActionListener<T> listener)
            {
                OnActionRequested -= listener.OnActionRequested;
                OnActionCanceled -= listener.OnActionCanceled;
                OnActionConfirmed -= listener.OnActionConfirmed;
            }

            public void RaiseRequested()
            {
                Debug.Log($"ActionEventSet{typeof(T).Name}: RaiseRequested");
                OnActionRequested?.Invoke();
            }

            public void RaiseCanceled()
            {
                Debug.Log($"ActionEventSet{typeof(T).Name}: RaiseCanceled");
                OnActionCanceled?.Invoke();
            }

            public void RaiseConfirmed()
            {
                Debug.Log($"ActionEventSet{typeof(T).Name}: RaiseConfirmed");
                OnActionConfirmed?.Invoke();
            }

            public void Subscribe<T1>(IHumanPlayerActionListener<T1> listener) where T1 : GameAction
            {
                Subscribe(listener as IHumanPlayerActionListener<T> ??
                    throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T)}")
                    );
            }

            public void Unsubscribe<T1>(IHumanPlayerActionListener<T1> listener) where T1 : GameAction
            {
                Subscribe(listener as IHumanPlayerActionListener<T> ??
                    throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T)}")
                    );
            }

            #endregion
        }
    }
}
