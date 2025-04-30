#nullable enable

using ProjectL.Management;
using ProjectLCore.GamePieces;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DraggableTetromino : MonoBehaviour
{
    #region Constants

    private const float rotationSpeed = 30; // degrees per 1 mouse wheel move

    #endregion

    #region Fields

    [SerializeField] private TetrominoShape _shape;


    private static int _abandonedTetrominosLayer;
    private static int _selectedTetrominoLayer;
    private static int _placedTetrominoLayer;

    private const int _placedTetrominoSortingOrder = 1;
    private const int _abandonedTetrominosSortingOrder = 2;
    private const int _selectedTetrominoSortingOrder = 3;

    Vector2 pointerOffset;

    private Rigidbody2D rb;
    private RectTransform rt;

    private SpriteRenderer spriteRenderer;

    private Camera? mainCamera;

    private bool isDragging = false;

    private bool isMouseOver = false;

    #endregion

    #region Properties

    public static DraggableTetromino? SelectedTetromino { get; private set; } = null;

    public TetrominoShape Shape => _shape;

    public Action<DraggableTetromino>? OnStartDragging { get; set; } = null;

    #endregion

    #region Methods

    // Called by the ButtonShapeSpawner after instantiation
    public void Initialize(Camera cam)
    {
        mainCamera = cam;
    }

    public void StartDragging()
    {
        // --- Calculate Offset --- START
        if (mainCamera != null) {
            Vector3 mouseScreenPos = Input.mousePosition;
            // Ensure Z is correct for accurate world point conversion
            mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

            // Calculate the difference between the object's center and the mouse click position
            pointerOffset = (Vector2)transform.position - mouseWorldPos;
        }
        else {
            Debug.LogError("Main Camera is not assigned, cannot calculate drag offset.");
            pointerOffset = Vector2.zero; // Default to no offset if camera is missing
        }
        // --- Calculate Offset --- END


        SelectedTetromino = this;
        OnStartDragging?.Invoke(this);
        isDragging = true;

        isMouseOver = true;
        gameObject.layer = _selectedTetrominoLayer; // Set the layer to selected
        spriteRenderer.sortingOrder = _selectedTetrominoSortingOrder; // Set the sorting order to selected
        rb.bodyType = RigidbodyType2D.Kinematic; // Switch to Kinematic for drag
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // set rotation to the closes mutliple of 90 degrees
        float angle = transform.rotation.eulerAngles.z;
        angle = Mathf.Round(angle / 90f) * 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

    }

    public void StopDragging()
    {
        isDragging = false;
        rb.bodyType = RigidbodyType2D.Dynamic; // Switch back to Dynamic
        rb.linearVelocity = Vector2.zero;
    }

    public void PlaceToPuzzle(Vector3 center)
    {
        isDragging = false; // Stop dragging when placing

        if (SelectedTetromino == this) {
            SelectedTetromino = null;
        }
        gameObject.layer = _placedTetrominoLayer;
        spriteRenderer.sortingOrder = _placedTetrominoSortingOrder; // Set the sorting order to placed
        rb.bodyType = RigidbodyType2D.Static;

        // set rotation to the closes mutliple of 90 degrees
        float angle = transform.rotation.eulerAngles.z;
        angle = Mathf.Round(angle / 90f) * 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector2 offset = Vector2.Scale(rt.sizeDelta, (0.5f * Vector2.one - rt.pivot));
        offset = Vector2.Scale(offset, (Vector2)transform.localScale);
        transform.position = center + (Vector3)Rotate(offset, angle);


        static Vector2 Rotate(Vector2 v, float degrees)
        {
            float delta = Mathf.Deg2Rad * degrees;
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }
    }

    public void OnMouseDown()
    {
        StartDragging();
    }

    public void OnMouseUp()
    {
        if (isDragging) {
            StopDragging();
        }
    }

    public void OnMouseEnter()
    {
        isMouseOver = true;
    }

    public void OnMouseExit()
    {
        isMouseOver = false;
    }

    internal void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rt = GetComponent<RectTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        _abandonedTetrominosLayer = LayerMask.NameToLayer("AbandonedTetromino");
        _selectedTetrominoLayer = LayerMask.NameToLayer("SelectedTetromino");
        _placedTetrominoLayer = LayerMask.NameToLayer("PlacedTetromino");
    }

    private void Start()
    {
        transform.localScale = TetrominoSizeManager.GetScaleFor(transform);

        if (GameManager.Controls == null) {
            Debug.LogError("GameManager controls are not initialized.");
            return;
        }
        GameManager.Controls.Gameplay.RotateSmooth.performed += ctx => OnRotateInputAction(ctx, true);
        GameManager.Controls.Gameplay.Rotate90.performed += ctx => OnRotateInputAction(ctx, false);
        GameManager.Controls.Gameplay.Flip.performed += OnFlipInputAction;
        GameManager.Controls.Gameplay.Place.performed += OnPlaceInputAction;
    }

    internal void FixedUpdate() // Use FixedUpdate for Rigidbody manipulation
    {
        if (isDragging && mainCamera != null) {
            // --- Move the Object ---
            Vector3 mouseScreenPos = Input.mousePosition;
            // Adjust Z before converting so it's within camera view frustum but not ON the near plane
            mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            Vector2 targetWorldPos = mouseWorldPos + pointerOffset; // Add the offset to the mouse position

            rb.MovePosition(targetWorldPos); // Move kinematic body correctly
        }
    }

    private void OnRotateInputAction(InputAction.CallbackContext ctx, bool smooth)
    {
        if (this != SelectedTetromino) {
            return;
        }
        if (smooth)
            transform.Rotate(0f, 0f, ctx.ReadValue<float>() * rotationSpeed);
        else
            transform.Rotate(0f, 0f, 90 * Mathf.Sign(ctx.ReadValue<float>()));
    }

    private void OnFlipInputAction(InputAction.CallbackContext ctx)
    {
        if (this != SelectedTetromino) {
            return;
        }
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        Quaternion rotation = transform.rotation;
        rotation.z *= -1;
        transform.rotation = rotation;
    }

    private void OnPlaceInputAction(InputAction.CallbackContext ctx)
    {
        if (this == SelectedTetromino) {
            Debug.Log($"{gameObject.name}: left button pressed, placing tetromino");
            InteractivePuzzle.PlaceTetrominoToPuzzle(this);
        }
    }



    internal void Update() // Scale can be done in Update
    {
        // scale the piece based on distance to the puzzle zone

        if (gameObject.layer == _placedTetrominoLayer) {
            return; // don't do anything if the tetromino is placed
        }

        if (isDragging) {
            transform.localScale = TetrominoSizeManager.GetScaleFor(transform);
        }

        // --- Layer Management ---
        if (isDragging || isMouseOver) {
            if (SelectedTetromino != null && SelectedTetromino != this) {
                SelectedTetromino.gameObject.layer = _abandonedTetrominosLayer;
                SelectedTetromino.spriteRenderer.sortingOrder = _abandonedTetrominosSortingOrder; // Set the sorting order to abandoned
            }
            if (SelectedTetromino != this) {
                gameObject.layer = _selectedTetrominoLayer;
                spriteRenderer.sortingOrder = _selectedTetrominoSortingOrder; // Set the sorting order to selected
                SelectedTetromino = this;
            }
        }
        else {
            gameObject.layer = _abandonedTetrominosLayer;
            spriteRenderer.sortingOrder = _abandonedTetrominosSortingOrder; // Set the sorting order to abandoned
            if (SelectedTetromino == this) {
                SelectedTetromino = null;
            }
        }
    }


    #endregion
}
