#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using ProjectL.Animation;
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.GameScene.PlayerZone;
    using ProjectL.Sound;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public enum PieceZoneMode
    {
        Disabled,
        Spawning,
        SelectReward,
        ChangeTetromino,
    }

    public class PieceZoneManager : StaticInstance<PieceZoneManager>,
        ITetrominoCollectionListener,
        IActionCreationController,
        IAIPlayerActionAnimator<TakeBasicTetrominoAction>,
        IAIPlayerActionAnimator<ChangeTetrominoAction>,
        IAIPlayerActionAnimator<SelectRewardAction>,
        IHumanPlayerActionCreator<TakeBasicTetrominoAction>,
        IHumanPlayerActionCreator<ChangeTetrominoAction>,
        IHumanPlayerActionCreator<SelectRewardAction>
    {
        #region Fields

        private IDisposable? _finishedPuzzleHighlighter = null;

        private Dictionary<TetrominoShape, TetrominoButton> _tetrominoButtons = new();

        private TetrominoCountsColumn? _currentTetrominoColumn;

        private PieceZoneMode _mode;

        private DisposableSelectRewardActionCreator? _selectRewardActionCreator;

        private DisposableChangeTetrominoActionCreator? _changeTetrominoActionCreator;

        private DisposableTakeBasicActionCreator? _takeBasicActionCreator;

        private PlayerMode _playerMode;

        private ActionMode _actionMode;

        #endregion

        #region Events

        private event Action<IActionModification<TakeBasicTetrominoAction>>? TakeBasicModifiedEventHandler;
        event Action<IActionModification<TakeBasicTetrominoAction>>? IHumanPlayerActionCreator<TakeBasicTetrominoAction>.ActionModifiedEventHandler {
            add => TakeBasicModifiedEventHandler += value;
            remove => TakeBasicModifiedEventHandler -= value;
        }

        private event Action<IActionModification<ChangeTetrominoAction>>? ChangeTetrominoModifiedEventHandler;
        event Action<IActionModification<ChangeTetrominoAction>>? IHumanPlayerActionCreator<ChangeTetrominoAction>.ActionModifiedEventHandler {
            add => ChangeTetrominoModifiedEventHandler += value;
            remove => ChangeTetrominoModifiedEventHandler -= value;
        }

        private event Action<IActionModification<SelectRewardAction>>? SelectRewardModifiedEventHandler;
        event Action<IActionModification<SelectRewardAction>>? IHumanPlayerActionCreator<SelectRewardAction>.ActionModifiedEventHandler {
            add => SelectRewardModifiedEventHandler += value;
            remove => SelectRewardModifiedEventHandler -= value;
        }
        
        #endregion

        #region Methods

        public IAIPlayerActionAnimator<PlaceTetrominoAction> GetPlaceTetrominoActionAnimator(TetrominoShape shape)
        {
            // find the spawner for the tetromino shape
            if (!_tetrominoButtons.TryGetValue(shape, out TetrominoButton? spawner)) {
                throw new InvalidOperationException($"No spawner found for tetromino shape {shape}");
            }
            return spawner.SpawnTetromino(isAnimation: true);
        }

        public void RegisterListener(ITetrominoSpawnerListener listener)
        {
            foreach (var spawner in _tetrominoButtons.Values) {
                spawner.AddListener(listener);
            }
        }

        public void SetCurrentTetrominoColumn(TetrominoCountsColumn column)
        {
            _currentTetrominoColumn?.RemoveListener(this);
            column.AddListener(this);
            _currentTetrominoColumn = column;
            foreach (var spawner in _tetrominoButtons.Values) {
                int count = column.GetDisplayCount(spawner.Shape);
                spawner.IsGrayedOut = count == 0;
            }
        }

        public void ReportButtonClick(TetrominoButton button)
        {
            switch (_mode) {
                case PieceZoneMode.Disabled:
                case PieceZoneMode.Spawning:
                    break;
                case PieceZoneMode.SelectReward:
                    _selectRewardActionCreator!.ReportButtonPress(button);
                    TetrominoShape? selectedReward = _selectRewardActionCreator.SelectedReward;
                    SelectRewardModifiedEventHandler?.Invoke(new SelectRewardActionModification(selectedReward));
                    break;
                case PieceZoneMode.ChangeTetromino:
                    _changeTetrominoActionCreator!.ReportButtonPress(button);
                    TetrominoShape? oldTetromino = _changeTetrominoActionCreator.OldTetromino;
                    TetrominoShape? newTetromino = _changeTetrominoActionCreator.NewTetromino;
                    ChangeTetrominoModifiedEventHandler?.Invoke(new ChangeTetrominoActionModification(oldTetromino, newTetromino));
                    break;
                default:
                    break;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _tetrominoButtons = FindSpawners();

            // check that there is a 1-1 mapping between TetrominoShape and TetrominoSpawner
            if (_tetrominoButtons.Count != TetrominoManager.NumShapes) {
                Debug.LogError($"Number of TetrominoSpawners ({_tetrominoButtons.Count}) does not match TetrominoShape count ({TetrominoManager.NumShapes})", this);
            }
        }

        private void Start()
        {
            HumanPlayerActionCreationManager.RegisterController(this);
            HumanPlayerActionCreationManager.Instance.AddListener(this as IHumanPlayerActionCreator<TakeBasicTetrominoAction>);
            HumanPlayerActionCreationManager.Instance.AddListener(this as IHumanPlayerActionCreator<ChangeTetrominoAction>);
            HumanPlayerActionCreationManager.Instance.AddListener(this as IHumanPlayerActionCreator<SelectRewardAction>);
        }

        private Dictionary<TetrominoShape, TetrominoButton> FindSpawners()
        {
            Dictionary<TetrominoShape, TetrominoButton> spawners = new();

            // loop through all children of this GameObject
            // in each child, look for a child with a TetrominoSpawner component
            foreach (Transform child in transform) {
                TetrominoButton? spawner = child.GetComponentInChildren<TetrominoButton>();
                if (spawner != null) {
                    TetrominoShape shape = spawner.Shape;
                    if (!spawners.ContainsKey(shape)) {
                        spawners.Add(shape, spawner);
                    }
                    else {
                        Debug.LogError($"Duplicate TetrominoSpawner found for shape {shape}", this);
                    }
                }
            }

            return spawners;
        }

        private PieceZoneMode GetDefaultMode()
        {
            if (_playerMode == PlayerMode.NonInteractive) {
                return PieceZoneMode.Disabled;
            }

            if (_actionMode == ActionMode.RewardSelection) {
                return PieceZoneMode.SelectReward;
            }

            return PieceZoneMode.Spawning;
        }

        private void SetMode(PieceZoneMode mode)
        {
            _mode = mode;
            foreach (var spawner in _tetrominoButtons.Values) {
                spawner.SetMode(mode);
            }
        }

        private void DisposeAndSetSpawning()
        {
            DisposeActionEffects();
            SetMode(PieceZoneMode.Spawning);
        }

        private void DisposeAndSetDisabled()
        {
            DisposeActionEffects();
            SetMode(PieceZoneMode.Disabled);
        }

        private void EnableRewardSelection(List<TetrominoShape> rewardOptions)
        {
            _selectRewardActionCreator = new DisposableSelectRewardActionCreator(rewardOptions);
        }

        private void DisposeActionEffects()
        {
            _finishedPuzzleHighlighter?.Dispose();
            _finishedPuzzleHighlighter = null;

            _selectRewardActionCreator?.Dispose();
            _selectRewardActionCreator = null;

            _changeTetrominoActionCreator?.Dispose();
            _changeTetrominoActionCreator = null;

            _takeBasicActionCreator?.Dispose();
            _takeBasicActionCreator = null;
        }

        void ITetrominoCollectionListener.OnTetrominoCollectionChanged(TetrominoShape shape, int count)
        {
            _tetrominoButtons[shape].IsGrayedOut = count == 0;
        }

        async Task IAIPlayerActionAnimator<SelectRewardAction>.AnimateAsync(SelectRewardAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // highlight reward options
            using (new DisposableButtonHighlighter(action.RewardOptions!)) {

                await AnimationManager.WaitForScaledDelay(1f, cancellationToken);

                // highlight and select the reward
                var spawner = _tetrominoButtons[action.SelectedReward];
                using (new DisposableButtonHighlighter(action.SelectedReward, playSound: false)) {
                    using (spawner.GetDisposableButtonSelector(SelectionSideEffect.GiveToPlayer)) {
                        await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
                    }
                }
            }
        }

        async Task IAIPlayerActionAnimator<TakeBasicTetrominoAction>.AnimateAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // select the O1 piece
            List<TetrominoShape> options = new() { TetrominoShape.O1 };
            SelectRewardAction selectAction = new(options, TetrominoShape.O1);
            await (this as IAIPlayerActionAnimator<SelectRewardAction>).AnimateAsync(selectAction, cancellationToken);
        }

        async Task IAIPlayerActionAnimator<ChangeTetrominoAction>.AnimateAsync(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var oldSpawner = _tetrominoButtons[action.OldTetromino];

            // select the old piece
            using (oldSpawner.GetDisposableButtonSelector(SelectionSideEffect.RemoveFromPlayer, SelectionButtonEffect.MakeSmaller)) {
                // highlighting the old piece, important !!!
                // if player had only 1 piece, the display count will decrement to 0, and so the piece would go gray
                using (new DisposableButtonHighlighter(action.OldTetromino, playSound: false)) {

                    // select the reward
                    var tetrominosInShardReserve = SharedReserveManager.Instance.GetNumTetrominosLeft();
                    var upgradeOptions = RewardManager.GetUpgradeOptions(tetrominosInShardReserve, action.OldTetromino);
                    upgradeOptions.Add(action.OldTetromino);  // keep the old one highlighgted
                    SelectRewardAction selectAction = new(upgradeOptions, action.NewTetromino);
                    await (this as IAIPlayerActionAnimator<SelectRewardAction>).AnimateAsync(selectAction, cancellationToken);
                }
            }
        }

        void IActionCreationController.SetPlayerMode(PlayerMode mode)
        {
            _playerMode = mode;
            SetMode(GetDefaultMode());
        }

        void IActionCreationController.SetActionMode(ActionMode mode)
        {
            _actionMode = mode;
            SetMode(GetDefaultMode());
        }

        void IHumanPlayerActionCreator<SelectRewardAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.SelectReward);

            _currentTetrominoColumn!.SetColor(Color.white);

            var eventArgs = HumanPlayerActionCreationManager.Instance.CurrentRewardEventArgs!;
            var puzzleSlot = PlayerZoneManager.Instance.GetPuzzleWithId(eventArgs.Puzzle.Id)!;
            _finishedPuzzleHighlighter = puzzleSlot.GetDisposablePuzzleHighlighter();

            EnableRewardSelection(eventArgs.RewardOptions);
        }

        void IHumanPlayerActionCreator<SelectRewardAction>.OnActionCanceled() => DisposeAndSetDisabled();

        void IHumanPlayerActionCreator<SelectRewardAction>.OnActionConfirmed() => DisposeAndSetDisabled();

        void IHumanPlayerActionCreator<TakeBasicTetrominoAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.Disabled);
            _takeBasicActionCreator = new DisposableTakeBasicActionCreator();

            // notify that the action was created
            TakeBasicTetrominoActionModification mod = new(isSelected: true);
            TakeBasicModifiedEventHandler?.Invoke(mod);
        }

        void IHumanPlayerActionCreator<TakeBasicTetrominoAction>.OnActionCanceled() => DisposeAndSetSpawning();

        void IHumanPlayerActionCreator<TakeBasicTetrominoAction>.OnActionConfirmed() => DisposeAndSetDisabled();

        void IHumanPlayerActionCreator<ChangeTetrominoAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.ChangeTetromino);
            _changeTetrominoActionCreator = new(SharedReserveManager.Instance.GetNumTetrominosLeft());
        }

        void IHumanPlayerActionCreator<ChangeTetrominoAction>.OnActionCanceled() => DisposeAndSetSpawning();

        void IHumanPlayerActionCreator<ChangeTetrominoAction>.OnActionConfirmed() => DisposeAndSetDisabled();

        #endregion

        private class DisposableButtonHighlighter : IDisposable
        {
            #region Fields

            private readonly bool[] _originalSettings = new bool[TetrominoManager.NumShapes];

            #endregion

            #region Constructors

            public DisposableButtonHighlighter(ICollection<TetrominoShape> buttonsToHighlight, bool playSound = true)
            {
                if (playSound) {
                    SoundManager.Instance?.PlaySoftTapSoundEffect();
                }
                for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                    var spawner = Instance._tetrominoButtons[(TetrominoShape)i];
                    _originalSettings[i] = spawner.IsGrayedOut;
                    spawner.IsGrayedOut = !buttonsToHighlight.Contains((TetrominoShape)i);
                }
            }

            public DisposableButtonHighlighter(TetrominoShape button, bool playSound = true)
                : this(new List<TetrominoShape> { button }, playSound)
            {
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                    var spawner = Instance._tetrominoButtons[(TetrominoShape)i];
                    spawner.IsGrayedOut = _originalSettings[i];
                }
            }

            #endregion
        }

        private class DisposableTakeBasicActionCreator : IDisposable
        {
            #region Fields

            private IDisposable _highlighter;

            private IDisposable _selector;

            #endregion

            #region Constructors

            public DisposableTakeBasicActionCreator()
            {
                _highlighter = new DisposableButtonHighlighter(TetrominoShape.O1, playSound: false);

                TetrominoButton o1Button = PieceZoneManager.Instance._tetrominoButtons[TetrominoShape.O1];
                _selector = o1Button.GetDisposableButtonSelector(SelectionSideEffect.GiveToPlayer, SelectionButtonEffect.MakeBigger);
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                _highlighter?.Dispose();
                _selector?.Dispose();
            }

            #endregion
        }

        private class DisposableSelectRewardActionCreator : IDisposable
        {
            #region Fields

            private readonly List<TetrominoShape> _rewardOptions;

            private IDisposable _rewardOptionsHighlighter;

            private IDisposable? _selectedRewardSelector;

            #endregion

            #region Constructors

            public DisposableSelectRewardActionCreator(List<TetrominoShape> rewardOptions)
            {
                _rewardOptions = rewardOptions;
                _rewardOptionsHighlighter = new DisposableButtonHighlighter(rewardOptions, playSound: false);
            }

            #endregion

            #region Properties

            public TetrominoShape? SelectedReward { get; private set; } = null;

            #endregion

            #region Methods

            public void ReportButtonPress(TetrominoButton button)
            {
                if (SelectedReward != null) {
                    ResetHighlight();
                }
                if (SelectedReward == button.Shape) {
                    SelectedReward = null;
                    return;
                }
                SelectedReward = button.Shape;
                _selectedRewardSelector = button.GetDisposableButtonSelector(SelectionSideEffect.GiveToPlayer);
            }

            public void Dispose()
            {
                _selectedRewardSelector?.Dispose();
                _rewardOptionsHighlighter?.Dispose();
            }

            private void ResetHighlight()
            {
                Dispose();
                _rewardOptionsHighlighter = new DisposableButtonHighlighter(_rewardOptions, playSound: false);
            }

            #endregion
        }

        private class DisposableChangeTetrominoActionCreator : IDisposable
        {
            #region Fields

            private IDisposable? _oldTetrominoSelector;

            private DisposableSelectRewardActionCreator? _newTetrominoSelector;

            private int[] _numTetrominosInSharedReserve;

            #endregion

            #region Constructors

            public DisposableChangeTetrominoActionCreator(int[] numTetrominosInSharedReserve)
            {
                _numTetrominosInSharedReserve = numTetrominosInSharedReserve;
            }

            #endregion

            #region Properties

            public TetrominoShape? OldTetromino { get; private set; } = null;

            public TetrominoShape? NewTetromino { get; private set; } = null;

            #endregion

            #region Methods

            public void ReportButtonPress(TetrominoButton button)
            {
                // didn't have old tetromino
                if (OldTetromino == null) {
                    // highlight old tetromino
                    OldTetromino = button.Shape;
                    _oldTetrominoSelector = button.GetDisposableButtonSelector(SelectionSideEffect.RemoveFromPlayer, SelectionButtonEffect.MakeSmaller);

                    // pick new one 
                    var changeOptions = RewardManager.GetUpgradeOptions(_numTetrominosInSharedReserve, OldTetromino.Value);
                    changeOptions.Add(OldTetromino.Value);
                    _newTetrominoSelector = new DisposableSelectRewardActionCreator(changeOptions);

                    return;
                }

                // did have old AND clicked the same button
                if (OldTetromino == button.Shape) {
                    OldTetromino = null;
                    NewTetromino = null;

                    _newTetrominoSelector?.Dispose();
                    _newTetrominoSelector = null;

                    _oldTetrominoSelector?.Dispose();
                    _oldTetrominoSelector = null;
                    return;
                }

                // clicked a different button --> select new tetromino
                _newTetrominoSelector!.ReportButtonPress(button);
                NewTetromino = _newTetrominoSelector!.SelectedReward;
            }

            public void Dispose()
            {
                _newTetrominoSelector?.Dispose();
                _oldTetrominoSelector?.Dispose();
            }

            #endregion
        }
    }
}
