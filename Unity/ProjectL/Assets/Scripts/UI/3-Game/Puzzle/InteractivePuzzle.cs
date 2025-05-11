#nullable enable

using ProjectLCore.GameActions;
using ProjectLCore.GameManagers;
using ProjectLCore.GamePieces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(GridLayoutGroup))]
public class InteractivePuzzle : MonoBehaviour, IPuzzleListener
{
    #region Fields

    private static List<InteractivePuzzle> _availablePuzzles = new();

    [SerializeField] private PuzzleCell? puzzleCellPrefab;

    private PuzzleCell[]? _puzzleCells;

    private PuzzleWithGraphics? _logicalPuzzle = null;

    private PuzzleWithGraphics? _temporaryCopy = null;

    private Dictionary<DraggableTetromino, BinaryImage> _placedTetrominos = new();

    public uint? CurrentPuzzleId => _logicalPuzzle?.Id;

    #endregion

    #region Methods

    public void MakeInteractive(bool enabled)
    {
        if (_puzzleCells == null) {
            Debug.LogError("Puzzle cells are not initialized.");
            return;
        }
        foreach (var cell in _puzzleCells) {
            cell.SetColliderEnabled(enabled);
        }
    }

    public void SetNewPuzzle(PuzzleWithGraphics logicalPuzzle)
    {
        if (_puzzleCells == null) {
            Debug.LogError("Puzzle cells are not initialized.");
            return;
        }

        // reset cell colors
        foreach (var cell in _puzzleCells) {
            cell.ResetColor();
        }

        // listen to the new puzzle
        _logicalPuzzle?.RemoveListener(this);
        logicalPuzzle.AddListener(this);

        // change sprite
        if (!logicalPuzzle.TryGetSprite(out Sprite? sprite)) {
            Debug.LogError("Failed to get sprite for the puzzle.", this);
            return;
        }
        GetComponent<Image>().sprite = sprite!;

        // remember this puzzle
        _logicalPuzzle = logicalPuzzle;
        _temporaryCopy = (PuzzleWithGraphics)logicalPuzzle.Clone();
    }

    public static void PlaceTetrominoToPuzzle(DraggableTetromino tetromino)
    {
        if (tetromino != DraggableTetromino.SelectedTetromino) {
            return;
        }

        if (!TryGetPuzzleWhichHasShapeOverIt(tetromino.Shape, out InteractivePuzzle? puzzle, out BinaryImage position)) {
            return;
        }

        if (puzzle!._temporaryCopy!.CanPlaceTetromino(position)) {
            tetromino.PlaceToPuzzle(puzzle!.GetCollisionCenter());
            puzzle._placedTetrominos.Add(tetromino, position);
            puzzle._temporaryCopy!.AddTetromino(tetromino.Shape, position);
            tetromino.OnStartDragging += puzzle.RemoveTetromino;
        }

    }

    private static bool TryGetPuzzleWhichHasShapeOverIt(TetrominoShape shape, out InteractivePuzzle? result, out BinaryImage position)
    {
        foreach (var puzzle in _availablePuzzles) {
            if (puzzle == null || puzzle._logicalPuzzle == null || puzzle.gameObject.activeSelf == false) {
                continue;
            }
            position = puzzle.GetTetrominoPosition();
            if (TetrominoManager.CompareShapeToImage(shape, position)) {
                result = puzzle;
                return true;
            }
        }
        result = null;
        position = default;
        return false;
    }

    public void RemoveTetromino(DraggableTetromino tetromino)
    {
        if (tetromino == null) {
            Debug.LogError("Tetromino is null.");
            return;
        }
        if (this == null) {
            return;
        }
        if (_placedTetrominos.ContainsKey(tetromino)) {
            _temporaryCopy!.RemoveTetromino(tetromino.Shape, _placedTetrominos[tetromino]);
            _placedTetrominos.Remove(tetromino);
        }
        tetromino.OnStartDragging -= RemoveTetromino;
        Debug.Log($"{gameObject.name}: removed tetromino {tetromino.gameObject.name}");
    }

    public void DiscardChanges()
    {
        if (_temporaryCopy == null) {
            Debug.LogError("Temporary copy is null.");
            return;
        }
        _temporaryCopy = (PuzzleWithGraphics)_logicalPuzzle!.Clone();
        foreach (var tetromino in _placedTetrominos.Keys) {
            tetromino.OnStartDragging -= RemoveTetromino;
            Destroy(tetromino.gameObject);
        }
        _placedTetrominos.Clear();
    }


    internal void Awake()
    {
        if (puzzleCellPrefab == null) {
            Debug.LogError("Puzzle cell prefab is not assigned!", this);
            return;
        }

        // Create puzzle cells
        _puzzleCells = new PuzzleCell[25]; // 5x5 grid
        for (int i = 0; i < _puzzleCells.Length; i++) {
            _puzzleCells[i] = Instantiate(puzzleCellPrefab, transform);
            _puzzleCells[i].OnCollisionStateChanged += TryDrawingTetrominoShadow;
            _puzzleCells[i].gameObject.name = $"PuzzleCell_{1 + i / 5}x{1 + i % 5}";
            _puzzleCells[i].gameObject.SetActive(true);
        }

        // remember this puzzle
        _availablePuzzles.Add(this);

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // remove this puzzle from the list of available puzzles
        _availablePuzzles.Remove(this);

        if (_puzzleCells == null) {
            return;
        }
        // Clean up the cells 
        for (int i = 0; i < _puzzleCells.Length; i++) {
            if (_puzzleCells[i] != null) {
                Destroy(_puzzleCells[i].gameObject);
            }
        }
    }

    private BinaryImage GetTetrominoPosition()
    {
        if (_puzzleCells == null) {
            throw new InvalidOperationException("Puzzle cells are not initialized.");
        }
        Debug.Log($"{gameObject.name}.GetTetrominoPosition");

        bool[] collisions = new bool[_puzzleCells.Length];
        for (int i = 0; i < _puzzleCells.Length; i++) {
            collisions[i] = _puzzleCells[i].IsColliding;
        }
        return new BinaryImage(collisions);
    }

    private void TryDrawingTetrominoShadow()
    {
        if (_puzzleCells == null) {
            throw new InvalidOperationException("Puzzle cells are not initialized.");
        }
        if (DraggableTetromino.SelectedTetromino == null) {
            return;
        }

        BinaryImage position = GetTetrominoPosition();
        TetrominoShape shape = DraggableTetromino.SelectedTetromino.Shape;

        bool goodShape = TetrominoManager.CompareShapeToImage(shape, position);
        bool tetrominoFits = _temporaryCopy!.CanPlaceTetromino(position);
        bool drawShadow = goodShape && tetrominoFits;

        Color color = (ColorImage.Color)shape;
        color *= 0.7f;
        color.a = 1f;

        for (int i = 0; i < _puzzleCells.Length; i++) {
            if (_logicalPuzzle!.Image[i]) {
                continue;
            }
            if (drawShadow && _puzzleCells[i].IsColliding) {
                _puzzleCells[i].ChangeColorTo(color);
            }
            else {
                _puzzleCells[i].ResetColor();
            }
        }
    }

    private Vector3 GetCollisionCenter()
    {
        if (_puzzleCells == null) {
            throw new InvalidOperationException("Puzzle cells are not initialized.");
        }

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

    public Vector2 GetPlacementCenter(BinaryImage placement)
    {
        if (_puzzleCells == null) {
            throw new InvalidOperationException("Puzzle cells are not initialized.");
        }

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

    void IPuzzleListener.OnTetrominoPlaced(TetrominoShape tetromino, BinaryImage position)
    {
        if (_puzzleCells == null) {
            return;
        }

        // set color of these cells
        for (int i = 0; i < _puzzleCells.Length; i++) {
            if (position[i]) {
                _puzzleCells[i].ChangeColorTo((ColorImage.Color)tetromino);
            }
        }
    }

    #endregion
}
