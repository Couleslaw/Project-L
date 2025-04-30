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

    private Image? image;

    private int _numCollisions = 0;

    private Color defaultColor = Color.clear; // new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.2f);// semi-transparent blue

    #endregion

    #region Properties

    public bool IsColliding => _numCollisions > 0;

    public Action? OnCollisionStateChanged { get; set; }


    #endregion

    #region Methods

    public void ChangeColorTo(Color color)
    {
        if (image == null) {
            return;
        }
        image.color = color;
    }

    public void ResetColor() => ChangeColorTo(defaultColor);

    private void Awake()
    {
        image = GetComponent<Image>();
        var collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        // default color = transparent
        image.color = defaultColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _numCollisions++;
        OnCollisionStateChanged?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _numCollisions--;
        OnCollisionStateChanged?.Invoke();
    }

    #endregion
}
