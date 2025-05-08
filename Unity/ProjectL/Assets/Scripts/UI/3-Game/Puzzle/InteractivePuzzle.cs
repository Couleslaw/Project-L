#nullable enable

using ProjectLCore.GameManagers;
using ProjectLCore.GamePieces;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(GridLayoutGroup))]
public class InteractivePuzzle : MonoBehaviour
{
    #region Fields

    private static List<InteractivePuzzle> _availablePuzzles = new();

    [SerializeField] private PuzzleCell? puzzleCellPrefab;

    private PuzzleCell[]? puzzleCells;

    private PuzzleWithGraphics? _logicalPuzzle = null;

    private PuzzleWithGraphics? _temporaryCopy = null;

    private Dictionary<DraggableTetromino, BinaryImage> _placedTetrominos = new();

    #endregion

    #region Methods

    public void MakeInteractive(bool enabled)
    {
        if (puzzleCells == null) {
            Debug.LogError("Puzzle cells are not initialized.");
            return;
        }
        foreach (var cell in puzzleCells) {
            cell.SetColliderEnabled(enabled);
        }
    }

    public void SetNewPuzzle(PuzzleWithGraphics logicalPuzzle)
    {
        _logicalPuzzle = logicalPuzzle;
        _temporaryCopy = (PuzzleWithGraphics)logicalPuzzle.Clone();

        if (!_temporaryCopy.TryGetSprite(out Sprite? sprite)) {
            Debug.LogError("Failed to get sprite for the puzzle.", this);
            return;
        }
        GetComponent<Image>().sprite = sprite!;
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
            tetromino.PlaceToPuzzle(puzzle!.GetTetrominoCenter());
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
        puzzleCells = new PuzzleCell[25]; // 5x5 grid
        for (int i = 0; i < puzzleCells.Length; i++) {
            puzzleCells[i] = Instantiate(puzzleCellPrefab, transform);
            puzzleCells[i].OnCollisionStateChanged += TryDrawingTetrominoShadow;
            puzzleCells[i].gameObject.name = $"PuzzleCell_{1 + i / 5}x{1 + i % 5}";
        }

        // remember this puzzle
        _availablePuzzles.Add(this);
    }

    private void OnDestroy()
    {
        // remove this puzzle from the list of available puzzles
        _availablePuzzles.Remove(this);

        if (puzzleCells == null) {
            return;
        }
        // Clean up the cells 
        for (int i = 0; i < puzzleCells.Length; i++) {
            if (puzzleCells[i] != null) {
                Destroy(puzzleCells[i].gameObject);
            }
        }
    }

    private BinaryImage GetTetrominoPosition()
    {
        if (puzzleCells == null) {
            throw new InvalidOperationException("Puzzle cells are not initialized.");
        }
        Debug.Log($"{gameObject.name}.GetTetrominoPosition");

        bool[] collisions = new bool[puzzleCells.Length];
        for (int i = 0; i < puzzleCells.Length; i++) {
            collisions[i] = puzzleCells[i].IsColliding;
        }
        return new BinaryImage(collisions);
    }

    private void TryDrawingTetrominoShadow()
    {
        if (puzzleCells == null) {
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

        for (int i = 0; i < puzzleCells.Length; i++) {
            if (drawShadow && puzzleCells[i].IsColliding) {
                puzzleCells[i].ChangeColorTo(color);
            }
            else {
                puzzleCells[i].ResetColor();
            }
        }
    }

    private Vector3 GetTetrominoCenter()
    {
        if (puzzleCells == null) {
            throw new InvalidOperationException("Puzzle cells are not initialized.");
        }

        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (var cell in puzzleCells) {
            if (cell.IsColliding) {
                center += cell.transform.position;
                count++;
            }
        }
        return center / count;
    }

    #endregion
}
