#nullable enable

using UnityEngine;
using ProjectLCore.GamePieces;
using UnityEditor.PackageManager;
public class DraggableTetromino : MonoBehaviour
{
    [SerializeField] private TetrominoShape _shape;
    public TetrominoShape Shape => _shape;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private bool isDragging = false;
    private bool isMouseOver = false;

    private static int _abandonedTetrominosLayer;
    private static int _selectedTetrominoLayer;
    private static int _placedTetrominoLayer;

    private const float rotationSpeed = 300f; // degrees per second

    public static DraggableTetromino? SelectedTetromino { get; private set; } = null;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        _abandonedTetrominosLayer = LayerMask.NameToLayer("AbandonedTetromino");
        _selectedTetrominoLayer = LayerMask.NameToLayer("SelectedTetromino");
        _placedTetrominoLayer = LayerMask.NameToLayer("PlacedTetromino");
    }

    private void Start()
    {
        if (TetrominoSpawnManager.Instance == null) {
            Debug.LogError("TetrominoSpawnManager is not assigned!", this);
            return;
        }

        transform.localScale = TetrominoSpawnManager.Instance.GetScale(this.transform);
    }

    // Called by the ButtonShapeSpawner after instantiation
    public void Initialize(Camera cam)
    {
        mainCamera = cam;
    }

    public void StartDragging()
    {
        isDragging = true;
        isMouseOver = true;
        SelectedTetromino = this;
        gameObject.layer = _selectedTetrominoLayer; // Set the layer to selected
        rb.bodyType = RigidbodyType2D.Kinematic; // Switch to Kinematic for drag
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0; // Ensure gravity stays off

        // set rotation to the closes mutliple of 90 degrees
        float angle = transform.rotation.eulerAngles.z;
        angle = Mathf.Round(angle / 90f) * 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        spriteRenderer.sortingOrder = 10; // Set a higher sorting order to ensure it's on top
    }

    public void StopDragging()
    {
        isDragging = false;
        rb.bodyType = RigidbodyType2D.Dynamic; // Switch back to Dynamic
        rb.linearVelocity = Vector2.zero;

        spriteRenderer.sortingOrder = 1; // Reset sorting order
    }

    public void PlaceToPuzzle(Vector3 center)
    {
        isDragging = false;
        if (SelectedTetromino == this) {
            SelectedTetromino = null;
        }
        rb.bodyType = RigidbodyType2D.Static;
        gameObject.layer = _placedTetrominoLayer;

        transform.position = center;

        // set rotation to the closes mutliple of 90 degrees
        float angle = transform.rotation.eulerAngles.z;
        angle = Mathf.Round(angle / 90f) * 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void OnMouseDown()
    {
        if (isDragging) {
            return;
        }
        StartDragging();
    }

    public void OnMouseUp()
    {
        if (!isDragging) {
            return;
        }
        StopDragging();
    }

    public void OnMouseEnter()
    {
        isMouseOver = true;
    }

    public void OnMouseExit()
    {
        isMouseOver = false;
    }

    void FixedUpdate() // Use FixedUpdate for Rigidbody manipulation
    {
        if (isDragging && mainCamera != null) {
            // --- Move the Object ---
            Vector3 mouseScreenPos = Input.mousePosition;
            // Adjust Z before converting so it's within camera view frustum but not ON the near plane
            mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
            Vector2 targetWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

            rb.MovePosition(targetWorldPos); // Move kinematic body correctly
        }
    }

    void Update() // Scale can be done in Update
    {
        // scale the piece based on distance to the puzzle zone
        if (isDragging) {
            transform.localScale = TetrominoSpawnManager.Instance!.GetScale(this.transform);
        }

        if (gameObject.layer == _placedTetrominoLayer) {
            return; // don't do anything if the tetromino is placed
        }

        // --- Layer Management ---
        if (isDragging || isMouseOver) {
            if (SelectedTetromino != null && SelectedTetromino != this) {
                SelectedTetromino.gameObject.layer = _abandonedTetrominosLayer;
                Debug.Log($"{SelectedTetromino.gameObject.name}: FORCE deselected tetromino");
            }
            if (SelectedTetromino != this) {
                gameObject.layer = _selectedTetrominoLayer;
                SelectedTetromino = this;
                Debug.Log($"{gameObject.name}: selected tetromino");
            }
        }
        else {
            gameObject.layer = _abandonedTetrominosLayer;
            if (SelectedTetromino == this) {
                SelectedTetromino = null;
                Debug.Log($"{gameObject.name}: deselected tetromino");
            }
        }

        if (SelectedTetromino == this && isMouseOver) {
            // Get the mouse scroll wheel input axis
            // Positive value for scroll up/forward, Negative value for scroll down/backward
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // Check if the wheel was actually scrolled this frame
            if (scrollInput != 0f) {
                transform.Rotate(0f, 0f, -scrollInput * rotationSpeed);
            }

            // right button press --> flip the gameobject using transform
            if (Input.GetMouseButtonDown(1)) {
                // flip the gameobject using transform
                Vector3 scale = transform.localScale;
                scale.x *= -1; // flip horizontally
                transform.localScale = scale;
            }
        }
    }
}
