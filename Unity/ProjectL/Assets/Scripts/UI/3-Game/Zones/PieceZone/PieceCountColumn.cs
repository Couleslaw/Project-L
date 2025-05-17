#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectLCore.GameManagers;
    using ProjectLCore.GameLogic;
    using System.Collections.Generic;
    using UnityEngine;
    using ProjectLCore.GamePieces;
    using System.Linq;
    using ProjectLCore.GameActions;
    using System;
    using System.Collections;
    using ProjectL.Data;

    public class PieceCountColumn : MonoBehaviour, ITetrominoCollectionListener, ITetrominoCollectionNotifier
    {
        [SerializeField] private PieceCounter? _pieceCounterPrefab;

        private readonly Dictionary<TetrominoShape, PieceCounter> _pieceCounters = new();
        private readonly int[] _realCounts = new int[TetrominoManager.NumShapes];
        private Color _columnColor = GameGraphicsSystem.ActiveColor;
        private bool _shouldColorGains;

        private event Action<TetrominoShape, int>? DisplayCollectionChangedEventHandler;

        private readonly bool[] _realValueUpdated = new bool[TetrominoManager.NumShapes];

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

        public void Init(int numInitialTetrominos, ITetrominoCollectionNotifier notifier, bool shouldColorGains=false)
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

            counter.SetColor(PieceCounter.IncrementedDisplayColor);
            counter.SetColorAfterSeconds(_columnColor, delay);
        }

        public int GetDisplayCount(TetrominoShape shape) => _pieceCounters[shape].Count;
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
                counter.SetColor(PieceCounter.IncrementedDisplayColor);
            else
                counter.SetColor(PieceCounter.DecrementedDisplayColor);
        }

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

        public class TemporaryPieceCountChanger : IDisposable
        {
            private readonly PieceCountColumn _pieceCountColumn;
            private readonly TetrominoShape _shape;
            private readonly int _originalCount;
            public TemporaryPieceCountChanger(PieceCountColumn pieceCountColumn, TetrominoShape shape, int newCount)
            {
                _pieceCountColumn = pieceCountColumn;
                _shape = shape;
                _originalCount = pieceCountColumn.GetDisplayCount(shape);
                pieceCountColumn.SetDisplayCount(shape, newCount);
            }
            public void Dispose()
            {
                _pieceCountColumn.SetDisplayCount(_shape, _originalCount);
            }
        }
    }
}
