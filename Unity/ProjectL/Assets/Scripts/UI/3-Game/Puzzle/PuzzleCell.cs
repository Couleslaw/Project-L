#nullable enable

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
    private int _numCollisions = 0;

    private Color defaultColor = Color.clear; // new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.2f);// semi-transparent blue

    #endregion

    #region Properties

    public bool IsColliding => _numCollisions > 0;

    public event Action? OnCollisionStateChangedEventHandler;


    #endregion

    #region Methods

    public void SetColliderEnabled(bool enabled)
    {
        if (_collider == null) {
            return;
        }
        _collider.enabled = enabled;
    }

    public void ChangeColorTo(Color color)
    {
        if (_image == null) {
            return;
        }
        _image.color = color;
    }

    public void ResetColor() => ChangeColorTo(defaultColor);

    private void Awake()
    {
        _image = GetComponent<Image>();
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;
        // default color = transparent
        _image.color = defaultColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _numCollisions++;
        OnCollisionStateChangedEventHandler?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _numCollisions--;
        OnCollisionStateChangedEventHandler?.Invoke();
    }

    #endregion
}
