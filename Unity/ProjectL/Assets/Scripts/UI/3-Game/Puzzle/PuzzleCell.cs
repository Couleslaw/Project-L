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
    private Mode _mode;
    private Color? _color = null;

    #endregion

    #region Events

    public event Action<TetrominoShape>? OnCollisionStateChangedEventHandler;

    #endregion

    public enum Mode
    {
        Fill,
        Empty,
        Shadow,
        Color
    }

    #region Properties

    public bool IsColliding => _isColliding && _collider != null && _collider.enabled;

    public bool Interactive {
        set {
            if (_collider == null) {
                return;
            }

            if (_mode == Mode.Fill || _mode == Mode.Color) {
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

    public void SetMode(Mode mode)
    {
        if (_image == null || _collider == null) {
            return;
        }

        _mode = mode;

        switch (mode) {
            case Mode.Fill: {
                Interactive = false;
                _image.color = Color.clear;
                break;
            }
            case Mode.Empty: {
                Interactive = true;
                _image.color = Color.clear;
                break;
            }
            case Mode.Shadow: {
                Interactive = true;
                _image.color = GetCollidingTetrominoShadeColor();
                break;
            }
            case Mode.Color: {
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
        SetMode(Mode.Color);
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
