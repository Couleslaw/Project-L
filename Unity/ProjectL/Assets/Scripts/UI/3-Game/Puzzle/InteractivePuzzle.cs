#nullable enable

using ProjectLCore.GameManagers;
using ProjectLCore.GamePieces;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GridLayoutGroup))]
public class InteractivePuzzle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private PuzzleCell? puzzleCellPrefab;

    private PuzzleCell[]? puzzleCells;

    private List<DraggableTetromino> _placedTetrominos = new();

    void Awake()
    {
        if (puzzleCellPrefab == null) {
            Debug.LogError("Puzzle Trigger Zone Prefab is not assigned!", this);
            return;
        }

        // Create puzzle cells
        puzzleCells = new PuzzleCell[25]; // 5x5 grid
        for (int i = 0; i < puzzleCells.Length; i++) {
            puzzleCells[i] = Instantiate(puzzleCellPrefab, transform);
            puzzleCells[i].gameObject.name = $"PuzzleCell_{1 + i / 5}x{1 + i % 5}";
        }
    }

    private void OnDestroy()
    {
        if (puzzleCells == null) {
            return;
        }
        // Clean up the cells 
        for (int i = 0; i < puzzleCells.Length; i++) {
            if (puzzleCells[i] != null) {
                Destroy(puzzleCells[i].gameObject);
            }
        }
        puzzleCells = null;
    }

    private BinaryImage GetTetrominoPosition()
    {
        if (puzzleCells == null) {
            throw new InvalidOperationException("Puzzle cells are not initialized.");
        }

        bool[] collisions = new bool[puzzleCells.Length];
        for (int i = 0; i < puzzleCells.Length; i++) {
            collisions[i] = puzzleCells[i].IsColliding;
        }
        return new BinaryImage(collisions);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        // make sure that the start and end mouse positions are the same - we only want click and not drag
        //if (eventData.pointerPress.transform != eventData.pointerDrag.transform) {
        //    Debug.Log("Pointer press and drag are not the same, ignoring click.");
        //    return;
        //}


        Debug.Log($"{gameObject.name}. pointer click handler");

        if (DraggableTetromino.SelectedTetromino == null) {
            return;
        }

        DraggableTetromino selectedPiece = DraggableTetromino.SelectedTetromino;
        var position = GetTetrominoPosition();
        if (!TetrominoManager.CompareShapeToImage(selectedPiece.Shape, position)) {
            Debug.Log("Tetromino shape does not match the puzzle image.");
            return;
        }
        Debug.Log("Tetromino shape matches the puzzle image.");

        // check my Puzzle if the tetromino can be placed

        selectedPiece.PlaceToPuzzle(GetTetrominoCenter());
    }
}
