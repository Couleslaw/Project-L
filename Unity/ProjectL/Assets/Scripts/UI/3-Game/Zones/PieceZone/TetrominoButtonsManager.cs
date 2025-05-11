#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;



    public class TetrominoButtonsManager : StaticInstance<TetrominoButtonsManager>, ITetrominoCollectionListener
    {
        private Dictionary<TetrominoShape, TetrominoSpawner> _tetrominoSpawners = new();
        private PieceCountColumn? _currentPieceColumn;

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

        protected override void Awake()
        {
            base.Awake();
            _tetrominoSpawners = FindSpawners();

            // check that there is a 1-1 mapping between TetrominoShape and TetrominoSpawner
            if (_tetrominoSpawners.Count != TetrominoManager.NumShapes) {
                Debug.LogError($"Number of TetrominoSpawners ({_tetrominoSpawners.Count}) does not match TetrominoShape count ({TetrominoManager.NumShapes})", this);
            }
        }

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
                spawner.GrayOut(count == 0);
                spawner.EnableSpawner(count > 0);
            }
        }

        void ITetrominoCollectionListener.OnTetrominoCollectionChanged(TetrominoShape shape, int count)
        {
            _tetrominoSpawners[shape].GrayOut(count == 0);
            _tetrominoSpawners[shape].EnableSpawner(count > 0);
        }
    }
}
