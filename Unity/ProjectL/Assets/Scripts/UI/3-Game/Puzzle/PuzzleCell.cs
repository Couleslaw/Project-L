#nullable enable

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(BoxCollider2D))]
public class PuzzleCell : MonoBehaviour
{
    private Image? image;

    public bool IsColliding => _numCollisions > 0;

    private int _numCollisions = 0;

    private Color defaultColor = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.5f); // semi-transparent blue

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
        if (!IsColliding) {
            ChangeColorTo(Color.red);
        }

        _numCollisions++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _numCollisions--;
        if (!IsColliding) {
            ChangeColorTo(defaultColor);
        }
    }

    public void ChangeColorTo(Color color)
    {
        if (image == null) {
            return;
        }
        image.color = color;
    }
}
