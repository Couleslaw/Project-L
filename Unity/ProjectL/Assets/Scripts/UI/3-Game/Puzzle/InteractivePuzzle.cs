#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectL.UI.GameScene.Zones.PieceZone;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class InteractivePuzzle : MonoBehaviour, IPuzzleListener,
        IHumanPlayerActionListener<PlaceTetrominoAction>,
        IAIPlayerActionAnimator<PlaceTetrominoAction>
    {
        #region Fields

        private static readonly List<InteractivePuzzle> _availablePuzzles = new();

        private readonly Dictionary<DraggableTetromino, PlaceTetrominoAction> _temporaryPlacements = new();

        private readonly PuzzleCell[] _puzzleCells = new PuzzleCell[25];// 5x5 grid

        [Header("Puzzle cells")]
        [SerializeField] private PuzzleCell? _puzzleCellPrefab;

        private PuzzleWithColor? _logicalPuzzle = null;

        private PuzzleWithColor? _temporaryPuzzleCopy = null;

        #endregion

        #region Events

        public event Action<IActionModification<PlaceTetrominoAction>>? ActionModifiedEventHandler;

        #endregion

        #region Properties

        public uint? PuzzleId => _logicalPuzzle?.Id;

        #endregion

        #region Methods

        public static void TryPlacingToPuzzle(DraggableTetromino tetromino)
        {
            // check if the request is valid
            if (!TryGetPuzzleWithCollisionShape(tetromino.GetConfiguration(), out InteractivePuzzle? puzzle)) {
                return;
            }

            // set position of the tetromino
            tetromino.PlaceToPosition(puzzle!.GetCollisionCenter());

            // place tetromino to the temporary copy
            var action = new PlaceTetrominoAction(puzzle._logicalPuzzle!.Id, tetromino.Shape, puzzle.GetCollisionImage());
            puzzle.AddTemporaryTetromino(tetromino, action);
            tetromino.OnStartDraggingEventHandler += puzzle.RemoveTemporaryTetromino;
        }

        public static bool TryGetPuzzleWithId(uint puzzleId, out InteractivePuzzle? result)
        {
            foreach (var puzzle in _availablePuzzles) {
                if (puzzle.PuzzleId == puzzleId) {
                    result = puzzle;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public void MakeInteractive(bool enabled)
        {
            foreach (var cell in _puzzleCells) {
                cell.Interactive = enabled && _logicalPuzzle != null;
            }
        }

        public void SetNewPuzzle(PuzzleWithColor logicalPuzzle)
        {
            _logicalPuzzle = logicalPuzzle;

            // change sprite
            if (!logicalPuzzle.TryGetSprite(out Sprite? sprite)) {
                Debug.LogError("Failed to get sprite for the puzzle.", this);
                return;
            }
            GetComponent<Image>().sprite = sprite!;

            // reset cells
            for (int i = 0; i < _puzzleCells.Length; i++) {
                PuzzleCell.CellState mode = logicalPuzzle.Image[i] ? PuzzleCell.CellState.Filled : PuzzleCell.CellState.Empty;
                _puzzleCells[i].SetState(mode);
            }
            MakeInteractive(true);

            // listen to the new puzzle
            logicalPuzzle.AddListener(this);

            // listen to action requests
            HumanPlayerActionCreator.Instance.AddListener(this);
        }

        public void FinishPuzzle()
        {
            // stop listening to the puzzle
            _logicalPuzzle?.RemoveListener(this);
            _logicalPuzzle = null;
            MakeInteractive(false);

            // stop listening to action requests
            HumanPlayerActionCreator.Instance.RemoveListener(this);
        }

        public Vector2 GetPlacementCenter(BinaryImage placement)
        {
            Vector3 center = Vector3.zero;
            int count = 0;
            for (int i = 0; i < _puzzleCells.Length; i++) {
                if (placement[i]) {
                    center += _puzzleCells[i].transform.position;
                    count++;
                }
            }
            return center / count;
        }

        private static bool TryGetPuzzleWithCollisionShape(BinaryImage collisionShape, out InteractivePuzzle? result)
        {
            BinaryImage collisionImage;
            foreach (var puzzle in _availablePuzzles) {
                // if puzzle is not active - skip
                if (puzzle == null || puzzle.gameObject.activeSelf == false || puzzle._logicalPuzzle == null) {
                    continue;
                }

                // check if puzzle has the shape over it
                collisionImage = puzzle.GetCollisionImage().MoveImageToTopLeftCorner();
                if (collisionShape.MoveImageToTopLeftCorner() == collisionImage) {
                    result = puzzle;
                    return true;
                }
            }

            // no puzzle found
            result = null;
            return false;
        }

        private void Awake()
        {
            if (_puzzleCellPrefab == null) {
                Debug.LogError("PuzzleCell prefab is not assigned", this);
                return;
            }

            // Create puzzle cells - 5x5 grid
            for (int i = 0; i < _puzzleCells.Length; i++) {
                _puzzleCells[i] = Instantiate(_puzzleCellPrefab, transform);
                _puzzleCells[i].gameObject.SetActive(true);
                _puzzleCells[i].gameObject.name = "PuzzleCell_" + i;
                _puzzleCells[i].OnCollisionStateChangedEventHandler += TryDrawingTetrominoShadow;
            }
            MakeInteractive(false);

            // remember this puzzle
            _availablePuzzles.Add(this);
        }

        private void OnDestroy()
        {
            _availablePuzzles.Remove(this);
        }

        private void AddTemporaryTetromino(DraggableTetromino tetromino, PlaceTetrominoAction action)
        {
            // notify listeners that a tetromino has been placed
            PlaceTetrominoActionModification mod = new(action, PlaceTetrominoActionModification.Options.Placed);
            ActionModifiedEventHandler?.Invoke(mod);

            // place tetromino to the temporary copy
            _temporaryPlacements.Add(tetromino, action);
            _temporaryPuzzleCopy!.AddTetromino(tetromino.Shape, action.Position);
        }

        private void RemoveTemporaryTetromino(DraggableTetromino tetromino)
        {
            if (!_temporaryPlacements.ContainsKey(tetromino)) {
                return;
            }
            // notify listeners that a tetromino has been removed
            PlaceTetrominoAction action = _temporaryPlacements[tetromino];
            PlaceTetrominoActionModification mod = new(action, PlaceTetrominoActionModification.Options.Removed);
            ActionModifiedEventHandler?.Invoke(mod);

            // stop listening to the tetromino
            tetromino.OnStartDraggingEventHandler -= RemoveTemporaryTetromino;

            // remove tetromino from the temporary copy
            BinaryImage position = action.Position;
            _temporaryPlacements.Remove(tetromino);
            _temporaryPuzzleCopy!.RemoveTetromino(tetromino.Shape, position);
        }

        private Vector3 GetCollisionCenter()
        {
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var cell in _puzzleCells) {
                if (cell.IsColliding) {
                    center += cell.transform.position;
                    count++;
                }
            }
            return center / count;
        }

        private BinaryImage GetCollisionImage()
        {
            bool[] collisions = new bool[_puzzleCells.Length];
            for (int i = 0; i < _puzzleCells.Length; i++) {
                collisions[i] = _puzzleCells[i].IsColliding;
            }
            return new BinaryImage(collisions);
        }

        private void TryDrawingTetrominoShadow(DraggableTetromino collidingTetromino)
        {
            // find collisions
            BinaryImage collisionImage = GetCollisionImage().MoveImageToTopLeftCorner();

            // decide whether to draw shadow or not
            bool shouldDrawShadow = collisionImage == collidingTetromino.GetConfiguration().MoveImageToTopLeftCorner();

            for (int i = 0; i < _puzzleCells.Length; i++) {
                // if already filled/colored --> skip
                if (_puzzleCells[i].State == PuzzleCell.CellState.Filled || _puzzleCells[i].State == PuzzleCell.CellState.Color) {
                    continue;
                }

                // either draw shadow or set to empty
                if (shouldDrawShadow && _puzzleCells[i].IsColliding) {
                    _puzzleCells[i].SetState(PuzzleCell.CellState.Shadow);
                }
                else {
                    _puzzleCells[i].SetState(PuzzleCell.CellState.Empty);
                }
            }
        }

        void IPuzzleListener.OnTetrominoPlaced(TetrominoShape tetromino, BinaryImage position)
        {
            // set color of specified cells
            for (int i = 0; i < _puzzleCells.Length; i++) {
                if (position[i]) {
                    _puzzleCells[i].SetFillColor((ColorImage.Color)tetromino);
                }
            }
        }

        void IPuzzleListener.OnTetrominoRemoved(TetrominoShape tetromino, BinaryImage position)
        {
            // make specified cells empty
            for (int i = 0; i < _puzzleCells.Length; i++) {
                if (position[i]) {
                    _puzzleCells[i].SetState(PuzzleCell.CellState.Empty);
                }
            }
        }

        void IHumanPlayerActionListener<PlaceTetrominoAction>.OnActionRequested()
        {
            _temporaryPuzzleCopy = (PuzzleWithColor)_logicalPuzzle!.Clone();
            _temporaryPuzzleCopy.AddListener(this);
        }

        void IHumanPlayerActionListener<PlaceTetrominoAction>.OnActionCanceled()
        {
            var tetrominosToRemove = _temporaryPlacements.Keys.ToList();
            foreach (var tetromino in tetrominosToRemove) {
                RemoveTemporaryTetromino(tetromino);
            }
            _temporaryPuzzleCopy?.RemoveListener(this);
            _temporaryPuzzleCopy = null;
        }

        void IHumanPlayerActionListener<PlaceTetrominoAction>.OnActionConfirmed()
        {
            _temporaryPlacements.Clear();
            _temporaryPuzzleCopy?.RemoveListener(this);
            _temporaryPuzzleCopy = null;
        }

        Task IAIPlayerActionAnimator<PlaceTetrominoAction>.Animate(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            (this as IPuzzleListener).OnTetrominoPlaced(action.Shape, action.Position);
            return Task.CompletedTask;
        }

        #endregion
    }
}
