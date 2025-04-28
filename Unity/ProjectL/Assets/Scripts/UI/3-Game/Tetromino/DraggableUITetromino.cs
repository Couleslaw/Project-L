using ProjectLCore.GamePieces;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(RectTransform))] // Good practice for UI
public class DraggableUITetromino : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Rigidbody2D rb;
    private RectTransform rectTransform;
    private Canvas canvas; // The parent canvas
    private Camera worldCamera; // Camera viewing the World Space canvas

    private bool isDragging = false;
    private Vector2 pointerOffset; // Offset in local RectTransform space

    private static int _abandonedPieceLayer;
    private static int _selectedPieceLayer;


    [SerializeField] private TetrominoShape _shape;
    public TetrominoShape Shape => _shape;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>(); // Find the parent canvas

        if (canvas.renderMode != RenderMode.WorldSpace) {
            Debug.LogError("DraggableTetromino requires the parent Canvas to be in World Space mode!", canvas);
            return;
        }
        worldCamera = canvas.worldCamera; // Get the camera associated with the World Space Canvas (often Camera.main)
        if (worldCamera == null)
            worldCamera = Camera.main; // Fallback

        _abandonedPieceLayer = LayerMask.NameToLayer("AbandonedPiece");
        _selectedPieceLayer = LayerMask.NameToLayer("SelectedPiece");
    }

    // Called by the EventSystem when a pointer (mouse/touch) presses down on this UI element
    public void OnPointerDown(PointerEventData eventData)
    {
        if (worldCamera == null)
            return;

        isDragging = true;

        // --- Switch to Kinematic ---
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Calculate offset relative to the object's pivot in its local space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position, // Use eventData's screen position
            worldCamera,        // Use the camera rendering the canvas
            out pointerOffset);

        Debug.Log($"Started dragging {gameObject.name}");

        // Optional: Bring to front visually while dragging (if needed)
         transform.SetAsLastSibling();
    }

    // Called by the EventSystem when the pointer is dragged over this UI element (after OnPointerDown)
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || worldCamera == null)
            return;

        // Convert current screen position to the local space of the *parent* RectTransform (the Canvas)
        RectTransform parentRect = canvas.transform as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, worldCamera, out Vector2 localPointerPosition)) {

            // Apply offset in local space, then convert the result to world space
            Vector2 targetLocalPosition = localPointerPosition - pointerOffset * transform.localScale;
            Vector3 targetWorldPosition = parentRect.TransformPoint(targetLocalPosition);

            // Use MovePosition for kinematic interaction
            rb.MovePosition(targetWorldPosition);
        }
    }


    // Called by the EventSystem when a pointer is released over this UI element (that received the OnPointerDown)
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;
        rb.bodyType = RigidbodyType2D.Dynamic; // Revert to dynamic physics

        Debug.Log($"Stopped dragging {gameObject.name}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // change layer to selected piece
        gameObject.layer = _selectedPieceLayer;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // change layer to abandoned piece
        gameObject.layer = _abandonedPieceLayer;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"{gameObject.name} detected collision with {collision.gameObject.name}");
    }
}