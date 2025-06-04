#nullable enable

namespace ProjectL.GameScene.ActionHandling
{
    using ProjectL.Animation;
    using ProjectL.GameScene.ActionZones;
    using ProjectL.GameScene.PlayerZone;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class HumanPlayerActionCreationManager : GraphicsManager<HumanPlayerActionCreationManager>,
        ICurrentTurnListener, IPlayerStatePuzzleFinishedAsyncListener
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

        private static readonly List<IActionCreationController> _actionControllers = new();

        private readonly Dictionary<ActionType, IActionEventSet> _actionEventSets = new();

        private readonly Dictionary<ActionType, IActionConstructor> _actionConstructors = new();

        private Queue<GameAction> _finishingTouchesPlacements = new();

        private Queue<PlaceTetrominoAction> _placeActionsQueue = new();

        private HumanPlayer? _currentPlayer;

        private ActionVerifier? _actionVerifier;

        private ActionType? _currentActionType;

        private ActionMode _currentActionMode = ActionMode.ActionCreation;

        private PlayerMode _currentPlayerMode = PlayerMode.NonInteractive;

        private TurnInfo _currentTurnInfo;

        #endregion

        private enum ActionType
        {
            TakePuzzle,
            Recycle,
            TakeBasicTetromino,
            ChangeTetromino,
            PlacePiece,
            MasterAction,
            FinishingTouches,
            SelectReward,
        }

        private interface IActionEventSet
        {
            #region Methods

            void Subscribe<T>(IHumanPlayerActionCreator<T> listener) where T : GameAction;

            void Unsubscribe<T>(IHumanPlayerActionCreator<T> listener) where T : GameAction;

            void RaiseRequested();

            void RaiseCanceled();

            void RaiseConfirmed();

            #endregion
        }

        #region Properties

        public HumanPlayer.GetRewardEventArgs? CurrentRewardEventArgs { get; private set; }

        private IActionEventSet? CurrentEventSet => _currentActionType == null ? null : _actionEventSets[_currentActionType.Value];

        private IActionConstructor? CurrentActionConstructor => _currentActionType == null ? null : _actionConstructors[_currentActionType.Value];

        #endregion

        #region Methods

        public static void RegisterController(IActionCreationController controller)
        {
            _actionControllers.Add(controller);
            if (Instance != null) {
                controller.SetPlayerMode(Instance._currentPlayerMode);
                controller.SetActionMode(Instance._currentActionMode);
            }
        }

        public void RegisterPlayer(HumanPlayer player, PlayerState playerState)
        {
            playerState.AddListener((IPlayerStatePuzzleFinishedAsyncListener)this);
            player.ActionRequested += Player_ActionRequested;
            player.RewardChoiceRequested += Player_RewardChoiceRequested;
        }

        public void AddListener<T>(IHumanPlayerActionCreator<T> listener) where T : GameAction
        {
            ActionType actionType = _typeToEnumActionType[typeof(T)];
            _actionEventSets[actionType].Subscribe(listener);
            listener.ActionModifiedEventHandler += OnActionModified;
        }

        public void RemoveListener<T>(IHumanPlayerActionCreator<T> listener) where T : GameAction
        {
            ActionType actionType = _typeToEnumActionType[typeof(T)];
            _actionEventSets[actionType].Unsubscribe(listener);
            listener.ActionModifiedEventHandler -= OnActionModified;
        }

        public void OnClearBoardRequested() => OnActionCanceled();

        public void OnTakePuzzleActionRequested() => SetNewActionType(ActionType.TakePuzzle);

        public void OnRecycleActionRequested() => SetNewActionType(ActionType.Recycle);

        public void OnTakeBasicTetrominoActionRequested() => SetNewActionType(ActionType.TakeBasicTetromino);

        public void OnChangeTetrominoActionRequested() => SetNewActionType(ActionType.ChangeTetromino);

        public void OnPlacePieceActionRequested()
        {
            if (_currentActionType == ActionType.MasterAction || _currentActionType == ActionType.FinishingTouches) {
                return;
            }
            // force deselect action button
            ActionButton.DeselectCurrentButton();

            SetNewActionType(ActionType.PlacePiece);
        }

        public void OnMasterActionRequested()
        {
            SetNewActionType(ActionType.MasterAction);
            UpdateConfirmButtonsIntractability();
        }

        public void OnEndFinishingTouchesActionRequested()
        {
            if (CurrentActionConstructor is not PlaceTetrominoActionConstructor placeTetrominoConstructor) {
                Debug.LogError($"Current action constructor is not PlaceTetrominoConstructor but {CurrentActionConstructor?.GetType().Name}", this);
                return;
            }

            _finishingTouchesPlacements = new(placeTetrominoConstructor.GetPlacementsQueue());
            _finishingTouchesPlacements.Enqueue(new EndFinishingTouchesAction());

            var player = PrepareForSubmission();
            player?.SetAction(_finishingTouchesPlacements.Dequeue());
        }

        public void OnActionCanceled()
        {
            // finishing touches --> only remove tetrominos from scene
            if (_currentActionMode == ActionMode.FinishingTouches) {
                CurrentEventSet?.RaiseCanceled();
                CurrentEventSet?.RaiseRequested();
                return;
            }

            // if reward selection --> cancel and reset
            if (_currentActionMode == ActionMode.RewardSelection) {
                ActionZonesManager.Instance.CanSelectReward = false;
                CurrentEventSet?.RaiseCanceled();
                CurrentEventSet?.RaiseRequested();
                return;
            }

            // if master action --> DON'T remove pieces from scene, just turn off master logic
            if (_currentActionType == ActionType.MasterAction) {
                SetNewActionType(ActionType.PlacePiece);
                UpdateConfirmButtonsIntractability();
                return;
            }

            // normal action --> disable action buttons and reset action

            ActionZonesManager.Instance.CanConfirmAction = false;
            PlayerZoneManager.Instance.CanConfirmTakePuzzleAction = false;

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
                if (CurrentActionConstructor is not PlaceTetrominoActionConstructor placeConstructor) {
                    Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                    return;
                }
                _placeActionsQueue = placeConstructor.GetPlacementsQueue();
                action = _placeActionsQueue.Dequeue();
            }
            else if (_currentActionType == ActionType.MasterAction) {
                if (CurrentActionConstructor is not PlaceTetrominoActionConstructor placeConstructor) {
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

            CurrentRewardEventArgs = null;
            var player = PrepareForSubmission();
            SetActionMode(ActionMode.ActionCreation);

            player?.SetReward(action.SelectedReward);
        }

        public override void Init(GameCore game)
        {
            game.AddListener((ICurrentTurnListener)this);
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
            _actionEventSets[ActionType.FinishingTouches] = placeEventSet;

            _actionConstructors[ActionType.TakePuzzle] = new TakePuzzleActionConstructor();
            _actionConstructors[ActionType.Recycle] = new RecycleActionConstructor();
            _actionConstructors[ActionType.TakeBasicTetromino] = new TakeBasicActionConstructor();
            _actionConstructors[ActionType.ChangeTetromino] = new ChangeTetrominoActionConstructor();
            _actionConstructors[ActionType.SelectReward] = new SelectRewardActionConstructor();

            var placeConstructor = new PlaceTetrominoActionConstructor();
            _actionConstructors[ActionType.PlacePiece] = placeConstructor;
            _actionConstructors[ActionType.MasterAction] = placeConstructor;
            _actionConstructors[ActionType.FinishingTouches] = placeConstructor;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _actionControllers.Clear();
            _actionConstructors.Clear();
            _actionEventSets.Clear();
            ActionZonesManager.Instance.DisconnectFromActionButtons(this);
            ActionZonesManager.Instance.DisconnectFromSelectRewardButton(this);
        }

        private void Player_ActionRequested(object? sender, HumanPlayer.GetActionEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                throw new ApplicationException("Sender is not a HumanPlayer!");
            }

            if (_currentActionMode == ActionMode.FinishingTouches) {
                if (_finishingTouchesPlacements.Count > 0) {
                    player.SetAction(_finishingTouchesPlacements.Dequeue());
                    return;
                }
            }

            else if (_placeActionsQueue.Count > 0) {
                player.SetAction(_placeActionsQueue.Dequeue());
                return;
            }

            _currentPlayer = player;
            _actionVerifier = e.Verifier;

            SetPlayerMode(PlayerMode.Interactive);

            if (_currentTurnInfo.GamePhase == GamePhase.FinishingTouches) {
                SetActionMode(ActionMode.FinishingTouches);
                SetNewActionType(ActionType.FinishingTouches);
            }
            else {
                SetActionMode(ActionMode.ActionCreation);
            }
        }

        private void Player_RewardChoiceRequested(object? sender, HumanPlayer.GetRewardEventArgs e)
        {
            if (sender is not HumanPlayer player) {
                throw new ApplicationException("Sender is not a HumanPlayer!");
            }

            _currentPlayer = player;
            CurrentRewardEventArgs = e;

            SetPlayerMode(PlayerMode.Interactive);
            SetActionMode(ActionMode.RewardSelection);
            SetNewActionType(ActionType.SelectReward);
        }

        private void OnActionModified(IActionModification<GameAction> change)
        {
            if (_currentActionType == null) {
                Debug.LogError("Current action type is null", this);
                return;
            }
            CurrentActionConstructor!.ApplyActionModification(change);

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
                if (CurrentActionConstructor is not PlaceTetrominoActionConstructor placeConstructor) {
                    Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                    return;
                }
                // get all placements
                Queue<PlaceTetrominoAction> placements = placeConstructor.GetPlacementsQueue();

                // check that there is a reasonable amount of placements
                bool valid = placements.Count > 0 && placements.Count <= _currentTurnInfo.NumActionsLeft;

                // ensure that all placements are valid
                foreach (var a in placements) {
                    if (_actionVerifier.Verify(a) is not VerificationSuccess) {
                        valid = false;
                        break;
                    }
                }

                // ensure that all placements are to the same puzzle
                if (valid && placements.Count > 0) {
                    uint puzzleId = placements.Peek().PuzzleId;
                    foreach (var a in placements) {
                        if (a.PuzzleId != puzzleId) {
                            valid = false;
                            break;
                        }
                    }
                }

                ActionZonesManager.Instance.CanConfirmAction = valid;
                return;
            }

            GameAction? action;
            if (_currentActionType == ActionType.MasterAction) {
                if (CurrentActionConstructor is not PlaceTetrominoActionConstructor placeConstructor) {
                    Debug.LogError("Current action constructor is not PlaceTetrominoConstructor", this);
                    return;
                }
                action = placeConstructor.GetMasterAction();
            }
            else {
                action = CurrentActionConstructor.GetAction<GameAction>();
            }

            bool canConfirm = action != null && _actionVerifier.Verify(action) is VerificationSuccess;
            ActionZonesManager.Instance.CanConfirmAction = canConfirm;

            if (_currentActionType == ActionType.TakePuzzle) {
                PlayerZoneManager.Instance.CanConfirmTakePuzzleAction = canConfirm;
            }
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
            _currentPlayerMode = mode;
            foreach (var controller in _actionControllers) {
                controller.SetPlayerMode(mode);
            }
            if (mode == PlayerMode.Interactive) {
                ActionZonesManager.Instance.ConnectToActionButtons(this);
            }
            if (mode == PlayerMode.NonInteractive) {
                ActionZonesManager.Instance.DisconnectFromActionButtons(this);
                PlayerZoneManager.Instance.CanConfirmTakePuzzleAction = false;
            }
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

        void ICurrentTurnListener.OnCurrentTurnChanged(TurnInfo turnInfo)
        {
            _currentTurnInfo = turnInfo;

            // update action zone if finishing touches
            if (turnInfo.GamePhase == GamePhase.FinishingTouches) {
                SetActionMode(ActionMode.FinishingTouches);
            }
        }

        async Task IPlayerStatePuzzleFinishedAsyncListener.OnPuzzleFinishedAsync(FinishedPuzzleInfo info, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (info.SelectedReward != null) {
                return;
            }

            // highlight completed puzzle
            await AnimationManager.WaitForScaledDelay(0.7f, cancellationToken);
            var puzzleSlot = PlayerZoneManager.Instance.GetPuzzleWithId(info.Puzzle.Id)!;
            using (puzzleSlot.GetDisposablePuzzleHighlighter()) {
                await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
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

            public void Subscribe(IHumanPlayerActionCreator<T> listener)
            {
                OnActionRequested += listener.OnActionRequested;
                OnActionCanceled += listener.OnActionCanceled;
                OnActionConfirmed += listener.OnActionConfirmed;
            }

            public void Unsubscribe(IHumanPlayerActionCreator<T> listener)
            {
                OnActionRequested -= listener.OnActionRequested;
                OnActionCanceled -= listener.OnActionCanceled;
                OnActionConfirmed -= listener.OnActionConfirmed;
            }

            public void RaiseRequested() => OnActionRequested?.Invoke();

            public void RaiseCanceled() => OnActionCanceled?.Invoke();

            public void RaiseConfirmed() => OnActionConfirmed?.Invoke();

            public void Subscribe<T1>(IHumanPlayerActionCreator<T1> listener) where T1 : GameAction
            {
                Subscribe(listener as IHumanPlayerActionCreator<T> ??
                    throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T)}")
                    );
            }

            public void Unsubscribe<T1>(IHumanPlayerActionCreator<T1> listener) where T1 : GameAction
            {
                Unsubscribe(listener as IHumanPlayerActionCreator<T> ??
                    throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T)}")
                    );
            }

            #endregion
        }
    }
}
