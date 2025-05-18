#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.Sound;
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
        TakeBasic,
        ChangeTetromino,
    }

    public class PieceZoneManager : StaticInstance<PieceZoneManager>,
        ITetrominoCollectionListener,
        IGameActionController,
        IAIPlayerActionAnimator<TakeBasicTetrominoAction>,
        IAIPlayerActionAnimator<ChangeTetrominoAction>,
        IAIPlayerActionAnimator<SelectRewardAction>,
        IHumanPlayerActionListener<TakeBasicTetrominoAction>,
        IHumanPlayerActionListener<ChangeTetrominoAction>,
        IHumanPlayerActionListener<SelectRewardAction>
    {

        private IDisposable? _finishedPuzzleHighlighter = null;


        private Dictionary<TetrominoShape, TetrominoButton> _tetrominoButtons = new();

        private PieceCountColumn? _currentPieceColumn;
        private PieceZoneMode _mode;

        private SelectRewardActionCreator? _selectRewardActionCreator;
        private ChangeTetrominoActionCreator? _changeTetrominoActionCreator;

        private event Action<IActionChange<TakeBasicTetrominoAction>>? TakeBasicStateChangedEventHandler;
        event Action<IActionChange<TakeBasicTetrominoAction>>? IHumanPlayerActionListener<TakeBasicTetrominoAction>.StateChangedEventHandler {
            add => TakeBasicStateChangedEventHandler += value;
            remove => TakeBasicStateChangedEventHandler -= value;
        }

        private event Action<IActionChange<ChangeTetrominoAction>>? ChangeTetrominoStateChangedEventHandler;
        event Action<IActionChange<ChangeTetrominoAction>>? IHumanPlayerActionListener<ChangeTetrominoAction>.StateChangedEventHandler {
            add => ChangeTetrominoStateChangedEventHandler += value;
            remove => ChangeTetrominoStateChangedEventHandler -= value;
        }

        private event Action<IActionChange<SelectRewardAction>>? SelectRewardStateChangedEventHandler;
        event Action<IActionChange<SelectRewardAction>>? IHumanPlayerActionListener<SelectRewardAction>.StateChangedEventHandler {
            add => SelectRewardStateChangedEventHandler += value;
            remove => SelectRewardStateChangedEventHandler -= value;
        }

        public IAIPlayerActionAnimator<PlaceTetrominoAction> GetPlaceTetrominoActionAnimator(TetrominoShape shape)
        {
            // find the spawner for the tetromino shape
            if (!_tetrominoButtons.TryGetValue(shape, out TetrominoButton? spawner)) {
                throw new InvalidOperationException($"No spawner found for tetromino shape {shape}");
            }
            return spawner.SpawnTetromino(isInteractable: false);
        }

        public void RegisterListener(ITetrominoSpawnerListener listener)
        {
            foreach (var spawner in _tetrominoButtons.Values) {
                spawner.AddListener(listener);
            }
        }

        public void SetCurrentPieceColumn(PieceCountColumn column)
        {
            _currentPieceColumn?.RemoveListener(this);
            column.AddListener(this);
            _currentPieceColumn = column;
            foreach (var spawner in _tetrominoButtons.Values) {
                int count = column.GetDisplayCount(spawner.Shape);
                spawner.IsGrayedOut = count == 0;
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
            HumanPlayerActionCreator.RegisterController(this);
            HumanPlayerActionCreator.Instance.AddListener(this as IHumanPlayerActionListener<TakeBasicTetrominoAction>);
            HumanPlayerActionCreator.Instance.AddListener(this as IHumanPlayerActionListener<ChangeTetrominoAction>);
            HumanPlayerActionCreator.Instance.AddListener(this as IHumanPlayerActionListener<SelectRewardAction>);
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

        void ITetrominoCollectionListener.OnTetrominoCollectionChanged(TetrominoShape shape, int count)
        {
            _tetrominoButtons[shape].IsGrayedOut = count == 0;
        }

        async Task IAIPlayerActionAnimator<SelectRewardAction>.Animate(SelectRewardAction action, CancellationToken cancellationToken)
        {
            // highlight reward options
            using (new TemporaryButtonHighlighter(action.RewardOptions!)) {

                await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);

                // highlight and select the reward
                var spawner = _tetrominoButtons[action.SelectedReward];
                using (new TemporaryButtonHighlighter(action.SelectedReward, playSound: false)) {
                    using (spawner.CreateTemporaryButtonSelector(SelectionSideEffect.GiveToPlayer)) {
                        await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);
                    }
                }
            }
        }

        async Task IAIPlayerActionAnimator<TakeBasicTetrominoAction>.Animate(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            // select the O1 piece
            List<TetrominoShape> options = new() { TetrominoShape.O1 };
            SelectRewardAction selectAction = new(options, TetrominoShape.O1);
            await (this as IAIPlayerActionAnimator<SelectRewardAction>).Animate(selectAction, cancellationToken);
        }

        async Task IAIPlayerActionAnimator<ChangeTetrominoAction>.Animate(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            var oldSpawner = _tetrominoButtons[action.OldTetromino];

            // select the old piece
            using (oldSpawner.CreateTemporaryButtonSelector(SelectionSideEffect.RemoveFromPlayer, SelectionButtonEffect.MakeSmaller)) {
                // highlighting the old piece, important !!!
                // if player had only 1 piece, the display count will decrement to 0, and so the piece would go gray
                using (new TemporaryButtonHighlighter(action.OldTetromino, playSound: false)) {

                    // select the reward
                    var tetrominosInShardReserve = SharedReserveManager.Instance.GetNumTetrominosLeft();
                    var upgradeOptions = RewardManager.GetUpgradeOptions(tetrominosInShardReserve, action.OldTetromino);
                    upgradeOptions.Add(action.OldTetromino);  // keep the old one highlighgted
                    SelectRewardAction selectAction = new(upgradeOptions, action.NewTetromino);
                    await (this as IAIPlayerActionAnimator<SelectRewardAction>).Animate(selectAction, cancellationToken);
                }
            }
        }

        void IGameActionController.SetPlayerMode(PlayerMode mode)
        {
            PieceZoneMode buttonMode = mode == PlayerMode.NonInteractive ? PieceZoneMode.Disabled : PieceZoneMode.Spawning;
            SetMode(buttonMode);
        }

        void IGameActionController.SetActionMode(ActionMode mode)
        {
            PieceZoneMode buttonMode = mode switch {
                ActionMode.ActionCreation => PieceZoneMode.Spawning,
                ActionMode.RewardSelection => PieceZoneMode.SelectReward,
                ActionMode.FinishingTouches => PieceZoneMode.Spawning,
                _ => throw new ArgumentOutOfRangeException(nameof(mode)),
            };
            SetMode(buttonMode);
        }

        private void SetMode(PieceZoneMode mode)
        {
            _mode = mode;
            foreach (var spawner in _tetrominoButtons.Values) {
                spawner.SetMode(mode);
            }
        }

        void IHumanPlayerActionListener<SelectRewardAction>.OnActionRequested()
        {
            var eventArgs = HumanPlayerActionCreator.Instance.CurrentRewardEventArgs!;

            _currentPieceColumn!.SetColor(GameGraphicsSystem.ActiveColor);

            var puzzleSlot = PlayerZoneManager.Instance.GetPuzzleWithId(eventArgs.Puzzle.Id)!;
            _finishedPuzzleHighlighter = puzzleSlot.CreateTemporaryPuzzleHighlighter();

            EnableRewardSelection(eventArgs.RewardOptions);
        }
        void IHumanPlayerActionListener<SelectRewardAction>.OnActionCanceled() => OnActionCanceled();
        void IHumanPlayerActionListener<SelectRewardAction>.OnActionConfirmed() => OnActionConfirmed();

        void IHumanPlayerActionListener<TakeBasicTetrominoAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.TakeBasic);
            EnableRewardSelection(new List<TetrominoShape> { TetrominoShape.O1 });
        }
        void IHumanPlayerActionListener<TakeBasicTetrominoAction>.OnActionCanceled() => OnActionCanceled();
        void IHumanPlayerActionListener<TakeBasicTetrominoAction>.OnActionConfirmed() => OnActionConfirmed();

        void IHumanPlayerActionListener<ChangeTetrominoAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.ChangeTetromino);
            _changeTetrominoActionCreator = new(SharedReserveManager.Instance.GetNumTetrominosLeft());
        }
        void IHumanPlayerActionListener<ChangeTetrominoAction>.OnActionCanceled() => OnActionCanceled();
        void IHumanPlayerActionListener<ChangeTetrominoAction>.OnActionConfirmed() => OnActionConfirmed();

        private void OnActionCanceled()
        {
            DisposeActionEffects();
            SetMode(PieceZoneMode.Spawning);
        }

        private void OnActionConfirmed()
        {
            DisposeActionEffects();
            SetMode(PieceZoneMode.Disabled);
        }

        private void EnableRewardSelection(List<TetrominoShape> rewardOptions)
        {
            _selectRewardActionCreator = new SelectRewardActionCreator(rewardOptions);
        }

        public void ReportButtonPress(TetrominoButton button)
        {
            Debug.Log($"Button pressed: {button.Shape}, mode={_mode}");

            switch (_mode) {
                case PieceZoneMode.Disabled:
                case PieceZoneMode.Spawning:
                    return;
                case PieceZoneMode.TakeBasic:
                    _selectRewardActionCreator!.ReportButtonPress(button);
                    bool isSelected = _selectRewardActionCreator.SelectedReward == TetrominoShape.O1;
                    TakeBasicStateChangedEventHandler?.Invoke(new TakeBasicTetrominoActionChange(isSelected));
                    break;
                case PieceZoneMode.SelectReward:
                    _selectRewardActionCreator!.ReportButtonPress(button);
                    TetrominoShape? selectedReward = _selectRewardActionCreator.SelectedReward;
                    SelectRewardStateChangedEventHandler?.Invoke(new SelectRewardActionChange(selectedReward));
                    break;
                case PieceZoneMode.ChangeTetromino:
                    _changeTetrominoActionCreator!.ReportButtonPress(button);
                    TetrominoShape? oldTetromino = _changeTetrominoActionCreator.OldTetromino;
                    TetrominoShape? newTetromino = _changeTetrominoActionCreator.NewTetromino;
                    ChangeTetrominoStateChangedEventHandler?.Invoke(new ChangeTetrominoActionChange(oldTetromino, newTetromino));
                    break;
                default:
                    break;
            }
        }

        private void DisposeActionEffects()
        {
            _finishedPuzzleHighlighter?.Dispose();
            _finishedPuzzleHighlighter = null;

            _selectRewardActionCreator?.Dispose();
            _selectRewardActionCreator = null;

            _changeTetrominoActionCreator?.Dispose();
            _changeTetrominoActionCreator = null;
        }

        private class TemporaryButtonHighlighter : IDisposable
        {

            private readonly bool[] _originalSettings = new bool[TetrominoManager.NumShapes];

            public TemporaryButtonHighlighter(ICollection<TetrominoShape> buttonsToHighlight, bool playSound = true)
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

            public TemporaryButtonHighlighter(TetrominoShape button, bool playSound = true)
                : this(new List<TetrominoShape> { button })
            {
            }

            public void Dispose()
            {
                for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                    var spawner = Instance._tetrominoButtons[(TetrominoShape)i];
                    spawner.IsGrayedOut = _originalSettings[i];
                }
            }
        }

        private class SelectRewardActionCreator : IDisposable
        {
            public TetrominoShape? SelectedReward { get; private set; } = null;

            private readonly List<TetrominoShape> _rewardOptions;
            private IDisposable _rewardOptionsHighlighter;
            private IDisposable? _selectedRewardSelector;

            public SelectRewardActionCreator(List<TetrominoShape> rewardOptions)
            {
                _rewardOptions = rewardOptions;
                _rewardOptionsHighlighter = new TemporaryButtonHighlighter(rewardOptions, playSound: false);
            }

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
                _selectedRewardSelector = button.CreateTemporaryButtonSelector(SelectionSideEffect.GiveToPlayer);
            }

            private void ResetHighlight()
            {
                Dispose();
                _rewardOptionsHighlighter = new TemporaryButtonHighlighter(_rewardOptions, playSound: false);
            }

            public void Dispose()
            {
                _selectedRewardSelector?.Dispose();
                _rewardOptionsHighlighter?.Dispose();
            }
        }

        private class ChangeTetrominoActionCreator : IDisposable
        {
            public TetrominoShape? OldTetromino { get; private set; } = null;
            public TetrominoShape? NewTetromino { get; private set; } = null;

            private IDisposable? _oldTetrominoSelector;
            private SelectRewardActionCreator? _newTetrominoSelector;

            private int[] _numTetrominosInSharedReserve;

            public ChangeTetrominoActionCreator(int[] numTetrominosInSharedReserve)
            {
                _numTetrominosInSharedReserve = numTetrominosInSharedReserve;
            }

            public void ReportButtonPress(TetrominoButton button)
            {
                // didn't have old tetromino
                if (OldTetromino == null) {
                    // highlight old tetromino
                    OldTetromino = button.Shape;
                    _oldTetrominoSelector = button.CreateTemporaryButtonSelector(SelectionSideEffect.RemoveFromPlayer, SelectionButtonEffect.MakeSmaller);

                    // pick new one 
                    var changeOptions = RewardManager.GetUpgradeOptions(_numTetrominosInSharedReserve, OldTetromino.Value);
                    changeOptions.Add(OldTetromino.Value);
                    _newTetrominoSelector = new SelectRewardActionCreator(changeOptions);

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
        }
    }
}
