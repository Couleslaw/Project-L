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

    public class PieceCountColumn : MonoBehaviour, ITetrominoCollectionListener, ITetrominoCollectionNotifier
    {
        [SerializeField] private PieceCounter? _pieceCounterPrefab;

        private readonly Dictionary<TetrominoShape, PieceCounter> _pieceCounters = new();
        private readonly int[] _realCounts = new int[TetrominoManager.NumShapes];
        private Color _columnColor = GameGraphicsSystem.ActivePlayerColor;

       

        private event Action<TetrominoShape, int>? DisplayCollectionChanged;

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

        public void Init(int numInitialTetrominos, ITetrominoCollectionNotifier notifier)
        {
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

        public void OnTetrominoCollectionChanged(TetrominoShape shape, int count)
        {
            _realCounts[(int)shape] = count;
            SetDisplayCount(shape, count);
        }

        public int GetDisplayCount(TetrominoShape shape) => _pieceCounters[shape].Count;
        public void SetDisplayCount(TetrominoShape shape, int count)
        {
            var counter = _pieceCounters[shape];
            if (counter.Count == count)
                return; // no change


            counter.Count = count;
            DisplayCollectionChanged?.Invoke(shape, count);

            int realCount = _realCounts[(int)shape];
            if (count == realCount)
                counter.SetColor(_columnColor);
            else if (count > realCount)
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
            DisplayCollectionChanged += listener.OnTetrominoCollectionChanged;
        }

        public void RemoveListener(ITetrominoCollectionListener listener)
        {
            DisplayCollectionChanged -= listener.OnTetrominoCollectionChanged;
        }
    }
}
