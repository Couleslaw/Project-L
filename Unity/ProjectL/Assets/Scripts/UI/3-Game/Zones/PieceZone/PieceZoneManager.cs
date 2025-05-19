#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.Animation;
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
        private TakeBasicActionCreator? _takeBasicActionCreator;
        private PlayerMode _playerMode;
        private ActionMode _actionMode;

        private event Action<IActionModification<TakeBasicTetrominoAction>>? TakeBasicModifiedEventHandler;
        event Action<IActionModification<TakeBasicTetrominoAction>>? IHumanPlayerActionListener<TakeBasicTetrominoAction>.ActionModifiedEventHandler {
            add => TakeBasicModifiedEventHandler += value;
            remove => TakeBasicModifiedEventHandler -= value;
        }

        private event Action<IActionModification<ChangeTetrominoAction>>? ChangeTetrominoModifiedEventHandler;
        event Action<IActionModification<ChangeTetrominoAction>>? IHumanPlayerActionListener<ChangeTetrominoAction>.ActionModifiedEventHandler {
            add => ChangeTetrominoModifiedEventHandler += value;
            remove => ChangeTetrominoModifiedEventHandler -= value;
        }

        private event Action<IActionModification<SelectRewardAction>>? SelectRewardModifiedEventHandler;
        event Action<IActionModification<SelectRewardAction>>? IHumanPlayerActionListener<SelectRewardAction>.ActionModifiedEventHandler {
            add => SelectRewardModifiedEventHandler += value;
            remove => SelectRewardModifiedEventHandler -= value;
        }

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

                await AnimationManager.WaitForScaledDelay(1f, cancellationToken);

                // highlight and select the reward
                var spawner = _tetrominoButtons[action.SelectedReward];
                using (new TemporaryButtonHighlighter(action.SelectedReward, playSound: false)) {
                    using (spawner.CreateTemporaryButtonSelector(SelectionSideEffect.GiveToPlayer)) {
                        await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
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
            _playerMode = mode;
            SetMode(GetDefaultMode());
        }

        void IGameActionController.SetActionMode(ActionMode mode)
        {
            _actionMode = mode;
            SetMode(GetDefaultMode());
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

        void IHumanPlayerActionListener<SelectRewardAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.SelectReward);

            _currentPieceColumn!.SetColor(GameGraphicsSystem.ActiveColor);

            var eventArgs = HumanPlayerActionCreator.Instance.CurrentRewardEventArgs!;
            var puzzleSlot = PlayerZoneManager.Instance.GetPuzzleWithId(eventArgs.Puzzle.Id)!;
            _finishedPuzzleHighlighter = puzzleSlot.CreateTemporaryPuzzleHighlighter();

            EnableRewardSelection(eventArgs.RewardOptions);
        }
        void IHumanPlayerActionListener<SelectRewardAction>.OnActionCanceled() => DisposeAndSetDisabled();
        void IHumanPlayerActionListener<SelectRewardAction>.OnActionConfirmed() => DisposeAndSetDisabled();

        void IHumanPlayerActionListener<TakeBasicTetrominoAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.Disabled);
            _takeBasicActionCreator = new TakeBasicActionCreator();

            // notify that the action was created
            TakeBasicTetrominoActionModification mod = new(isSelected: true);
            TakeBasicModifiedEventHandler?.Invoke(mod);
        }
        void IHumanPlayerActionListener<TakeBasicTetrominoAction>.OnActionCanceled() => DisposeAndSetSpawning();
        void IHumanPlayerActionListener<TakeBasicTetrominoAction>.OnActionConfirmed() => DisposeAndSetDisabled();

        void IHumanPlayerActionListener<ChangeTetrominoAction>.OnActionRequested()
        {
            SetMode(PieceZoneMode.ChangeTetromino);
            _changeTetrominoActionCreator = new(SharedReserveManager.Instance.GetNumTetrominosLeft());
        }
        void IHumanPlayerActionListener<ChangeTetrominoAction>.OnActionCanceled() => DisposeAndSetSpawning();
        void IHumanPlayerActionListener<ChangeTetrominoAction>.OnActionConfirmed() => DisposeAndSetDisabled();

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
            _selectRewardActionCreator = new SelectRewardActionCreator(rewardOptions);
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

        private class TakeBasicActionCreator : IDisposable
        {
            IDisposable _highlighter;
            IDisposable _selector;

            public TakeBasicActionCreator()
            {
                _highlighter = new TemporaryButtonHighlighter(TetrominoShape.O1, playSound: false);

                TetrominoButton o1Button = PieceZoneManager.Instance._tetrominoButtons[TetrominoShape.O1];
                _selector = o1Button.CreateTemporaryButtonSelector(SelectionSideEffect.GiveToPlayer, SelectionButtonEffect.MakeBigger);
            }
            public void Dispose()
            {
                _highlighter?.Dispose();
                _selector?.Dispose();
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
