#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using ProjectL.Data;
    using ProjectL.Animation;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class PieceCountColumn : MonoBehaviour, ITetrominoCollectionListener, ITetrominoCollectionNotifier
    {
        #region Fields

        private static readonly List<TetrominoShape> _shapeOrder = new() {
            TetrominoShape.O1,
            TetrominoShape.I2,
            TetrominoShape.L2,
            TetrominoShape.O2,
            TetrominoShape.I3,
            TetrominoShape.Z,
            TetrominoShape.T,
            TetrominoShape.L3,
            TetrominoShape.I4,
        };

        private readonly Dictionary<TetrominoShape, PieceCounter> _pieceCounters = new();

        private readonly int[] _realCounts = new int[TetrominoManager.NumShapes];

        private readonly bool[] _realValueUpdated = new bool[TetrominoManager.NumShapes];

        [SerializeField] private PieceCounter? _pieceCounterPrefab;

        private Color _columnColor = Color.white;

        private bool _shouldColorGains;

        #endregion

        #region Events

        private event Action<TetrominoShape, int>? DisplayCollectionChangedEventHandler;

        #endregion

        #region Methods

        public void Init(int numInitialTetrominos, ITetrominoCollectionNotifier notifier, bool shouldColorGains = false)
        {
            _shouldColorGains = shouldColorGains;
            for (int i = 0; i < _realCounts.Length; i++) {
                _realCounts[i] = numInitialTetrominos;
            }
            ResetColumn();
            notifier.AddListener(this);
        }

        public void ResetColumn()
        {
            SetColor(_columnColor);
            foreach (TetrominoShape shape in Enum.GetValues(typeof(TetrominoShape))) {
                SetDisplayCount(shape, _realCounts[(int)shape]);
            }
        }

        public int GetDisplayCount(TetrominoShape shape) => _pieceCounters[shape].Count;

        public void IncrementDisplayCount(TetrominoShape shape) => SetDisplayCount(shape, GetDisplayCount(shape) + 1);

        public void DecrementDisplayCount(TetrominoShape shape) => SetDisplayCount(shape, GetDisplayCount(shape) - 1);

        public void SetColor(Color color)
        {
            _columnColor = color;
            foreach (var pieceCounter in _pieceCounters.Values) {
                pieceCounter.SetColor(color);
            }
        }

        public void AddListener(ITetrominoCollectionListener listener)
        {
            DisplayCollectionChangedEventHandler += listener.OnTetrominoCollectionChanged;
        }

        public void RemoveListener(ITetrominoCollectionListener listener)
        {
            DisplayCollectionChangedEventHandler -= listener.OnTetrominoCollectionChanged;
        }

        public TemporaryPieceCountChanger CreateTemporaryCountIncreaser(TetrominoShape shape)
            => new(this, shape, GetDisplayCount(shape) + 1);

        public TemporaryPieceCountChanger CreateTemporaryCountDecreaser(TetrominoShape shape)
            => new(this, shape, GetDisplayCount(shape) - 1);

        private void Awake()
        {
            if (_pieceCounterPrefab == null) {
                Debug.LogError("PieceCounter prefab is not assigned in the inspector.");
                return;
            }

            if (_shapeOrder.Count != TetrominoManager.NumShapes) {
                Debug.LogError($"Number of TetrominoShapes ({TetrominoManager.NumShapes}) does not match number of shapes in _shapeOrder ({_shapeOrder.Count})", this);
            }

            foreach (TetrominoShape shape in _shapeOrder) {
                PieceCounter pieceCounter = Instantiate(_pieceCounterPrefab, transform);
                pieceCounter.gameObject.SetActive(true);
                _pieceCounters.Add(shape, pieceCounter);
            }
        }

        private void SetDisplayCount(TetrominoShape shape, int newCount)
        {
            var counter = _pieceCounters[shape];
            if (counter.Count != newCount) {
                counter.Count = newCount;
                DisplayCollectionChangedEventHandler?.Invoke(shape, newCount);
            }

            int realCount = _realCounts[(int)shape];
            if (newCount == realCount)
                counter.SetColor(_columnColor);
            else if (newCount > realCount)
                counter.SetColor(ColorManager.green);
            else
                counter.SetColor(ColorManager.red);
        }

        void ITetrominoCollectionListener.OnTetrominoCollectionChanged(TetrominoShape shape, int newCount)
        {
            int oldCount = _realCounts[(int)shape];
            _realValueUpdated[(int)shape] = true; // mark that the real value has been updated

            _realCounts[(int)shape] = newCount;
            SetDisplayCount(shape, newCount);

            if (!_shouldColorGains || newCount <= oldCount) {
                return;
            }

            var counter = _pieceCounters[shape];
            float delay = 0.8f * AnimationSpeed.DelayMultiplier;

            counter.SetColor(ColorManager.green);
            counter.SetColorAfterSeconds(_columnColor, delay);
        }

        #endregion

        public class TemporaryPieceCountChanger : IDisposable
        {
            #region Fields

            private readonly PieceCountColumn _pieceCountColumn;

            private readonly TetrominoShape _shape;

            private readonly int _originalCount;

            #endregion

            #region Constructors

            public TemporaryPieceCountChanger(PieceCountColumn pieceCountColumn, TetrominoShape shape, int newCount)
            {
                _pieceCountColumn = pieceCountColumn;
                _shape = shape;
                _originalCount = pieceCountColumn.GetDisplayCount(shape);
                pieceCountColumn.SetDisplayCount(shape, newCount);
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                _pieceCountColumn.SetDisplayCount(_shape, _originalCount);
            }

            #endregion
        }
    }
}
