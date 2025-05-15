#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.Sound;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class TetrominoButtonsManager : StaticInstance<TetrominoButtonsManager>, ITetrominoCollectionListener,
        IAIPlayerActionAnimator<TakeBasicTetrominoAction>,
        IAIPlayerActionAnimator<ChangeTetrominoAction>,
        IAIPlayerActionAnimator<SelectRewardAction>
    {
        #region Fields

        private Dictionary<TetrominoShape, TetrominoSpawner> _tetrominoSpawners = new();

        private PieceCountColumn? _currentPieceColumn;

        #endregion

        #region Methods

        public DraggableTetromino SpawnTetromino(TetrominoShape shape)
        {
            // find the spawner for the tetromino shape
            if (!_tetrominoSpawners.TryGetValue(shape, out TetrominoSpawner? spawner)) {
                throw new InvalidOperationException($"No spawner found for tetromino shape {shape}");
            }
            return spawner.SpawnTetromino();
        }

        public void RegisterListener(ITetrominoSpawnerListener listener)
        {
            foreach (var spawner in _tetrominoSpawners.Values) {
                spawner.AddListener(listener);
            }
        }

        public void SetCurrentPieceColumn(PieceCountColumn column)
        {
            _currentPieceColumn?.RemoveListener(this);
            column.AddListener(this);
            _currentPieceColumn = column;
            foreach (var spawner in _tetrominoSpawners.Values) {
                int count = column.GetDisplayCount(spawner.Shape);
                spawner.IsGrayedOut = count == 0;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _tetrominoSpawners = FindSpawners();

            // check that there is a 1-1 mapping between TetrominoShape and TetrominoSpawner
            if (_tetrominoSpawners.Count != TetrominoManager.NumShapes) {
                Debug.LogError($"Number of TetrominoSpawners ({_tetrominoSpawners.Count}) does not match TetrominoShape count ({TetrominoManager.NumShapes})", this);
            }
        }

        private Dictionary<TetrominoShape, TetrominoSpawner> FindSpawners()
        {
            Dictionary<TetrominoShape, TetrominoSpawner> spawners = new();

            // loop through all children of this GameObject
            // in each child, look for a child with a TetrominoSpawner component
            foreach (Transform child in transform) {
                TetrominoSpawner? spawner = child.GetComponentInChildren<TetrominoSpawner>();
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
            _tetrominoSpawners[shape].IsGrayedOut = count == 0;
        }

        async Task IAIPlayerActionAnimator<SelectRewardAction>.Animate(SelectRewardAction action, CancellationToken cancellationToken)
        {
            // highlight reward options
            using (new TemporaryButtonHighlighter(action.RewardOptions!)) {

                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);

                // highlight and select the reward
                var spawner = _tetrominoSpawners[action.SelectedReward];
                using (new TemporaryButtonHighlighter(action.SelectedReward, playSound: false)) {
                    using (spawner.CreateTemporaryButtonSelector(SelectionEffect.GiveToPlayer)) {
                        await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);
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
            var oldSpawner = _tetrominoSpawners[action.OldTetromino];

            // select the old piece
            using (oldSpawner.CreateTemporaryButtonSelector(SelectionEffect.RemoveFromPlayer)) {
                // highlighting the old piece, important !!!
                // if player had only 1 piece, the display count will decrement to 0, and so the piece would go gray
                using (new TemporaryButtonHighlighter(action.OldTetromino, playSound: false)) {

                    await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);

                    // select the reward
                    var tetrominosInShardReserve = SharedReserveManager.Instance.GetNumTetrominosLeft();
                    var upgradeOptions = RewardManager.GetUpgradeOptions(tetrominosInShardReserve, action.OldTetromino);
                    SelectRewardAction selectAction = new(upgradeOptions, action.NewTetromino);
                    await (this as IAIPlayerActionAnimator<SelectRewardAction>).Animate(selectAction, cancellationToken);
                }
            }
        }

        #endregion

        private class TemporaryButtonHighlighter : IDisposable
        {
            #region Fields

            private readonly bool[] _originalSettings = new bool[TetrominoManager.NumShapes];

            #endregion

            #region Constructors

            public TemporaryButtonHighlighter(ICollection<TetrominoShape> buttonsToHighlight, bool playSound = true)
            {
                if (playSound) {
                    SoundManager.Instance?.PlaySoftTapSoundEffect();
                }
                for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                    var spawner = Instance._tetrominoSpawners[(TetrominoShape)i];
                    _originalSettings[i] = spawner.IsGrayedOut;
                    spawner.IsGrayedOut = !buttonsToHighlight.Contains((TetrominoShape)i);
                }
            }

            public TemporaryButtonHighlighter(TetrominoShape button, bool playSound = true)
                : this(new List<TetrominoShape> { button })
            {
            }

            public TemporaryButtonHighlighter(TetrominoShape button1, TetrominoShape button2, bool playSound = true)
                : this(new List<TetrominoShape> { button1, button2 })
            {
            }

            public TemporaryButtonHighlighter(ICollection<TetrominoShape> buttonsCollection, TetrominoShape extraButton, bool playSound = true)
                : this(buttonsCollection.Union(new List<TetrominoShape> { extraButton }).ToList())
            {
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                    var spawner = Instance._tetrominoSpawners[(TetrominoShape)i];
                    spawner.IsGrayedOut = _originalSettings[i];
                }
            }

            #endregion
        }
    }
}
