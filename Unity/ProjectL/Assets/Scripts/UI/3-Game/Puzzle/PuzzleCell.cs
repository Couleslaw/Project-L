#nullable enable

using ProjectL.UI.GameScene.Zones.PieceZone;
using ProjectLCore.GamePieces;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider2D))]
public class PuzzleCell : MonoBehaviour
{
    #region Fields

    private Image? _image;

    private BoxCollider2D? _collider;

    private bool _isColliding = false;
    private TetrominoShape _lastCollidingShape;
    private CellState _state;
    private Color? _color = null;

    #endregion

    #region Events

    public event Action<TetrominoShape>? OnCollisionStateChangedEventHandler;

    #endregion

    public enum CellState
    {
        Filled,
        Empty,
        Shadow,
        Color
    }

    #region Properties

    public bool IsColliding => _isColliding && _collider != null && _collider.enabled;

    public CellState State => _state;

    public bool Interactive {
        set {
            if (_collider == null) {
                return;
            }

            if (_state == CellState.Filled || _state == CellState.Color) {
                _collider.enabled = false;
                _isColliding = false;
            }
            else {
                _collider.enabled = value;
            }
        }
    }

    #endregion

    #region Methods

    public void SetState(CellState state)
    {
        if (_image == null || _collider == null) {
            return;
        }

        _state = state;

        switch (state) {
            case CellState.Filled: {
                Interactive = false;
                _image.color = Color.clear;
                break;
            }
            case CellState.Empty: {
                Interactive = true;
                _image.color = Color.clear;
                break;
            }
            case CellState.Shadow: {
                Interactive = true;
                _image.color = GetCollidingTetrominoShadeColor();
                break;
            }
            case CellState.Color: {
                Interactive = false;
                if (_color == null) {
                    _color = (ColorImage.Color)_lastCollidingShape;
                }
                _image.color = _color.Value;
                break;
            }
        }
    }

    public void SetFillColor(Color color)
    {
        _color = color;
        SetState(CellState.Color);
    }

    private Color GetCollidingTetrominoShadeColor()
    {
        Color color = (ColorImage.Color)_lastCollidingShape;
        color *= 0.7f;
        color.a = 1f;
        return color;
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out DraggableTetromino? tetromino)) {
            return;
        }

        _isColliding = true;
        _lastCollidingShape = tetromino!.Shape;
        OnCollisionStateChangedEventHandler?.Invoke(tetromino.Shape);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out DraggableTetromino? tetromino)) {
            return;
        }

        _isColliding = false;
        OnCollisionStateChangedEventHandler?.Invoke(tetromino!.Shape);
    }

    #endregion
}
