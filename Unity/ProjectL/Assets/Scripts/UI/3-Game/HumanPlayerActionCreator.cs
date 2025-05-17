#nullable enable

namespace ProjectL.UI.GameScene.Actions
{
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.GameScene.Zones.PieceZone;
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
        ActionCreation, FinishingTouches, RewardSelection
    }

    public enum ActionType
    {
        TakePuzzle,
        Recycle,
        TakeBasicTetromino,
        ChangeTetromino,
        PlacePiece,
        MasterAction,
        EndFinishingTouches,
        SelectReward,
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
        #region Events

        event Action<IActionChange<T>>? StateChangedEventHandler;

        #endregion

        #region Methods

        void OnActionRequested();

        void OnActionCanceled();

        void OnActionConfirmed();

        #endregion
    }

    public class SelectRewardAction : GameAction
    {
        #region Constructors

        public SelectRewardAction(List<TetrominoShape>? rewardOptions, TetrominoShape selectedReward)
        {
            SelectedReward = selectedReward;
            RewardOptions = rewardOptions;
        }

        #endregion

        #region Properties

        public TetrominoShape SelectedReward { get; }

        public List<TetrominoShape>? RewardOptions { get; }

        #endregion
    }

    public class HumanPlayerActionCreator : GraphicsManager<HumanPlayerActionCreator>, ICurrentTurnListener
    {
        #region Fields

        private static readonly Dictionary<Type, ActionType> _typeToEnumActionType = new() {
            { typeof(TakePuzzleAction), ActionType.TakePuzzle },
            { typeof(RecycleAction), ActionType.Recycle },
            { typeof(TakeBasicTetrominoAction), ActionType.TakeBasicTetromino },
            { typeof(ChangeTetrominoAction), ActionType.ChangeTetromino },
            { typeof(PlaceTetrominoAction), ActionType.PlacePiece },
            { typeof(MasterAction), ActionType.MasterAction },
            { typeof(SelectRewardAction), ActionType.SelectReward }
        };

        private static readonly List<IGameActionController> _actionControllers = new();

        private readonly Dictionary<ActionType, IActionEventSet> _actionEventSets = new();

        private readonly Dictionary<ActionType, IActionConstructor> _actionConstructors = new();

        private Queue<GameAction>? _finishingTouchesPlacements = null;

        private Queue<PlaceTetrominoAction> _placeActionsQueue = new();

        private HumanPlayer? _currentPlayer;

        private ActionVerifier? _actionVerifier;

        private ActionType? _currentActionType;

        private ActionMode _currentActionMode = ActionMode.ActionCreation;

        private PlayerMode _currentPlayerMode = PlayerMode.NonInteractive;
        private int _numActionsLeft;

        #endregion

        #region Properties

        private IActionEventSet? CurrentEventSet => _currentActionType == null ? null : _actionEventSets[_currentActionType.Value];

        private IActionConstructor? CurrentActionConstructor => _currentActionType == null ? null : _actionConstructors[_currentActionType.Value];

        #endregion

        #region Methods

        public void RegisterPlayer(HumanPlayer player)
        {
            player.ActionRequested += Player_ActionRequested;
            player.RewardChoiceRequested += Player_RewardChoiceRequested;
        }

        public static void RegisterController(IGameActionController controller)
        {
            _actionControllers.Add(controller);
            if (Instance != null) {
                controller.SetPlayerMode(Instance._currentPlayerMode);
                controller.SetActionMode(Instance._currentActionMode);
            }
        }

        public void AddListener<T>(IHumanPlayerActionListener<T> listener) where T : GameAction
        {
            ActionType actionType = _typeToEnumActionType[typeof(T)];
            _actionEventSets[actionType].Subscribe(listener);
            listener.StateChangedEventHandler += OnActionStateChanged;
        }

        public void RemoveListener<T>(IHumanPlayerActionListener<T> listener) where T : GameAction
        {
            ActionType actionType = _typeToEnumActionType[typeof(T)];
            _actionEventSets[actionType].Unsubscribe(listener);
            listener.StateChangedEventHandler -= OnActionStateChanged;
        }

 

        public void OnClearBoardRequested() => OnActionCanceled();

        public void OnTakePuzzleActionRequested() => SetNewActionType(ActionType.TakePuzzle);

        public void OnRecycleActionRequested() => SetNewActionType(ActionType.Recycle);

        public void OnTakeBasicTetrominoActionRequested() => SetNewActionType(ActionType.TakeBasicTetromino);

        public void OnChangeTetrominoActionRequested() => SetNewActionType(ActionType.ChangeTetromino);

        public void OnPlacePieceActionRequested()
        {
            if (_currentActionType == ActionType.MasterAction || _currentActionType == ActionType.EndFinishingTouches) {
                return;
            }
            SetNewActionType(ActionType.PlacePiece);
        }

        public void OnMasterActionRequested()
        {
            SetNewActionType(ActionType.MasterAction);
            UpdateConfirmButtonsIntractability();
        }

        public void OnEndFinishingTouchesActionRequested()
        {
            if (CurrentActionConstructor is not PlaceTetrominoConstructor placeTetrominoConstructor) {
                Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                return;
            }

            _finishingTouchesPlacements = new(placeTetrominoConstructor.GetPlacementsQueue());
            _finishingTouchesPlacements.Enqueue(new EndFinishingTouchesAction());

            var player = PrepareForSubmission();
            player?.SetAction(_finishingTouchesPlacements.Dequeue());
        }

        public void OnActionCanceled()
        {
            // handle master action separately
            if (_currentActionType == ActionType.MasterAction) {
                SetNewActionType(ActionType.PlacePiece);
                UpdateConfirmButtonsIntractability();
                return;
            }

            ActionZonesManager.Instance.CanConfirmAction = false;
            CurrentEventSet?.RaiseCanceled();
            CurrentActionConstructor?.Reset();
            _currentActionType = null;
        }

        public void OnActionConfirmed()
        {
            if (_currentActionType == null) {
                Debug.LogError("Current action type is null", this);
                return;
            }

            GameAction? action;
            if (_currentActionType == ActionType.PlacePiece) {
                if (CurrentActionConstructor is not PlaceTetrominoConstructor placeConstructor) {
                    Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                    return;
                }
                _placeActionsQueue = placeConstructor.GetPlacementsQueue();
                action = _placeActionsQueue.Dequeue();
            }
            else if (_currentActionType == ActionType.MasterAction) {
                if (CurrentActionConstructor is not PlaceTetrominoConstructor placeConstructor) {
                    Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                    return;
                }
                action = placeConstructor.GetMasterAction();
            }
            else {
                action = CurrentActionConstructor!.GetAction<GameAction>();
            }

            if (action == null) {
                Debug.LogError("Action is null", this);
                return;
            }

            var player = PrepareForSubmission();
            player?.SetAction(action);
        }

        public void OnRewardSelected()
        {
            SelectRewardAction? action = CurrentActionConstructor?.GetAction<SelectRewardAction>();
            if (action == null) {
                Debug.LogError("Selected reward is null", this);
                return;
            }

            SetActionMode(ActionMode.ActionCreation);
            var player = PrepareForSubmission();
            player?.SetReward(action.SelectedReward);
        }

        public override void Init(GameCore game)
        {
            game.AddListener(this);
            SetActionMode(ActionMode.ActionCreation);
            SetPlayerMode(PlayerMode.NonInteractive);
        }

        protected override void Awake()
        {
            base.Awake();
            _actionEventSets[ActionType.TakePuzzle] = new ActionEventSet<TakePuzzleAction>();
            _actionEventSets[ActionType.Recycle] = new ActionEventSet<RecycleAction>();
            _actionEventSets[ActionType.TakeBasicTetromino] = new ActionEventSet<TakeBasicTetrominoAction>();
            _actionEventSets[ActionType.ChangeTetromino] = new ActionEventSet<ChangeTetrominoAction>();
            _actionEventSets[ActionType.SelectReward] = new ActionEventSet<SelectRewardAction>();

            var placeEventSet = new ActionEventSet<PlaceTetrominoAction>();
            _actionEventSets[ActionType.PlacePiece] = placeEventSet;
            _actionEventSets[ActionType.MasterAction] = placeEventSet;
            _actionEventSets[ActionType.EndFinishingTouches] = placeEventSet;

            _actionConstructors[ActionType.TakePuzzle] = new TakePuzzleConstructor();
            _actionConstructors[ActionType.Recycle] = new RecycleConstructor();
            _actionConstructors[ActionType.TakeBasicTetromino] = new TakeBasicConstructor();
            _actionConstructors[ActionType.ChangeTetromino] = new ChangeTetrominoConstructor();
            _actionConstructors[ActionType.SelectReward] = new SelectRewardConstructor();

            var placeConstructor = new PlaceTetrominoConstructor();
            _actionConstructors[ActionType.PlacePiece] = placeConstructor;
            _actionConstructors[ActionType.MasterAction] = placeConstructor;
            _actionConstructors[ActionType.EndFinishingTouches] = placeConstructor;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _actionControllers.Clear();
        }

        private void Player_ActionRequested(object? sender, HumanPlayer.GetActionEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                throw new ApplicationException("Sender is not a HumanPlayer!");
            }

            if (_currentActionMode == ActionMode.FinishingTouches) {
                if (_finishingTouchesPlacements != null) {
                    player.SetAction(_finishingTouchesPlacements.Dequeue());
                    return;
                }
            }
            
            if (_placeActionsQueue.Count > 0) {
                player.SetAction(_placeActionsQueue.Dequeue());
                return;
            }

            _currentPlayer = player;
            _actionVerifier = e.Verifier;
            SetPlayerMode(PlayerMode.Interactive);
        }

        private void Player_RewardChoiceRequested(object? sender, HumanPlayer.GetRewardEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                throw new ApplicationException("Sender is not a HumanPlayer!");
            }

            _currentPlayer = player;
            _currentActionType = ActionType.SelectReward;
            SetActionMode(ActionMode.RewardSelection);
            SetNewActionType(ActionType.SelectReward);
            PieceZoneManager.Instance.EnableRewardSelection(e.RewardOptions);
        }

        private void OnActionStateChanged(IActionChange<GameAction> change)
        {
            if (_currentActionType == null) {
                Debug.LogError("Current action type is null", this);
                return;
            }
            CurrentActionConstructor!.ApplyActionChange(change);

            switch (_currentActionMode) {
                case ActionMode.FinishingTouches: {
                    return;
                }
                case ActionMode.RewardSelection: {
                    var selectRewardAction = CurrentActionConstructor.GetAction<SelectRewardAction>();
                    ActionZonesManager.Instance.CanSelectReward = selectRewardAction != null;
                    return;
                }
                case ActionMode.ActionCreation: {
                    UpdateConfirmButtonsIntractability();
                    return;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(_currentActionMode), _currentActionMode, null);
            }
        }

        private void UpdateConfirmButtonsIntractability()
        {
            if (_actionVerifier == null || CurrentActionConstructor == null) {
                return;
            }

            if (_currentActionType == ActionType.PlacePiece) {
                if (CurrentActionConstructor is not PlaceTetrominoConstructor placeConstructor) {
                    Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                    return;
                }
                Queue<PlaceTetrominoAction> placements = placeConstructor.GetPlacementsQueue();
                bool valid = placements.Count > 0 && placements.Count <= _numActionsLeft;
                foreach (var a in placements) {
                    if (_actionVerifier.Verify(a) is not VerificationSuccess) {
                        valid = false;
                        break;
                    }
                }
                ActionZonesManager.Instance.CanConfirmAction = valid;
                return;
            }

            GameAction? action;
            if (_currentActionType == ActionType.MasterAction) {
                if (CurrentActionConstructor is not PlaceTetrominoConstructor placeConstructor) {
                    Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                    return;
                }
                action = placeConstructor.GetMasterAction();
            }
            else {
                action = CurrentActionConstructor.GetAction<GameAction>();
            }

            Debug.Log($"Action state changed... action: {action}");
            ActionZonesManager.Instance.CanConfirmAction = action != null && _actionVerifier.Verify(action) is VerificationSuccess;
        }

        private HumanPlayer? PrepareForSubmission()
        {
            if (_currentPlayer == null) {
                Debug.LogError("Current player is null", this);
                return null;
            }

            SetPlayerMode(PlayerMode.NonInteractive);

            CurrentEventSet?.RaiseConfirmed();
            CurrentActionConstructor?.Reset();
            _currentActionType = null;

            HumanPlayer? player = _currentPlayer;
            _currentPlayer = null;
            return player;
        }

        private void SetPlayerMode(PlayerMode mode)
        {
            foreach (var controller in _actionControllers) {
                controller.SetPlayerMode(mode);
            }
            if (mode == PlayerMode.Interactive)
                ActionZonesManager.Instance.ConnectToActionButtons(this);
            if (mode == PlayerMode.NonInteractive)
                ActionZonesManager.Instance.DisconnectFromActionButtons(this);
        }

        private void SetActionMode(ActionMode mode)
        {
            _currentActionMode = mode;
            foreach (var controller in _actionControllers) {
                controller.SetActionMode(mode);
            }

            if (mode == ActionMode.RewardSelection) {
                ActionZonesManager.Instance.ConnectToSelectRewardButtons(this);
            }
            else {
                ActionZonesManager.Instance.DisconnectFromSelectRewardButton(this);
            }
        }

        private void SetNewActionType(ActionType newType)
        {
            if (CurrentEventSet == _actionEventSets[newType]) {
                _currentActionType = newType;
                return;
            }

            if (_currentActionType != null) {
                OnActionCanceled();
            }

            _currentActionType = newType;
            CurrentEventSet!.RaiseRequested();
        }

        void ICurrentTurnListener.OnCurrentTurnChanged(TurnInfo currentTurnInfo)
        {
            _numActionsLeft = currentTurnInfo.NumActionsLeft;
            if (currentTurnInfo.GamePhase == GamePhase.FinishingTouches) {
                SetActionMode(ActionMode.FinishingTouches);
                SetNewActionType(ActionType.EndFinishingTouches);
            }
        }

        #endregion
    }
}
