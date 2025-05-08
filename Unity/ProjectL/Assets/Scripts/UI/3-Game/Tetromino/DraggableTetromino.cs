#nullable enable

using ProjectL.Management;
using ProjectLCore.GamePieces;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class DraggableTetromino : MonoBehaviour
{
    #region Constants

    private const float _rotationSpeed = 30; // degrees per 1 mouse wheel move

    #endregion

    #region Fields

    [SerializeField] private TetrominoShape _shape;


    private static int _abandonedTetrominosLayer;
    private static int _selectedTetrominoLayer;
    private static int _placedTetrominoLayer;
    private static int _playerPuzzleRowLayer;

    private const int _placedTetrominoSortingOrder = 1;
    private const int _abandonedTetrominosSortingOrder = 2;
    private const int _selectedTetrominoSortingOrder = 3;

    Vector2 _pointerOffset;

    private Rigidbody2D? _rb;
    private RectTransform? _rt;
    private SpriteRenderer? _spriteRenderer;

    private Camera? mainCamera;

    private bool _isDragging = false;

    private bool _isMouseOver = false;

    private bool _isOverPuzzleRow = false;

    private Action? _onDiscard;

    #endregion

    #region Properties

    public static DraggableTetromino? SelectedTetromino { get; private set; } = null;

    public TetrominoShape Shape => _shape;

    public Action<DraggableTetromino>? OnStartDragging { get; set; } = null;

    #endregion

    #region Methods

    // Called by the ButtonShapeSpawner after instantiation
    public void Init(Camera cam, Action onDiscard)
    {
        mainCamera = cam;
        _onDiscard = onDiscard;
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
            _pointerOffset = (Vector2)transform.position - mouseWorldPos;
        }
        else {
            Debug.LogError("Main Camera is not assigned, cannot calculate drag offset.");
            _pointerOffset = Vector2.zero; // Default to no offset if camera is missing
        }
        // --- Calculate Offset --- END


        SelectedTetromino = this;
        OnStartDragging?.Invoke(this);
        _isDragging = true;

        _isMouseOver = true;
        gameObject.layer = _selectedTetrominoLayer; // Set the layer to selected
        _spriteRenderer!.sortingOrder = _selectedTetrominoSortingOrder; // Set the sorting order to selected
        _rb!.bodyType = RigidbodyType2D.Kinematic; // Switch to Kinematic for drag
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // set rotation to the closes mutliple of 90 degrees
        float angle = transform.rotation.eulerAngles.z;
        angle = Mathf.Round(angle / 90f) * 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

    }

    public void StopDragging()
    {
        _isDragging = false;
        _rb!.bodyType = RigidbodyType2D.Dynamic; // Switch back to Dynamic
        _rb.linearVelocity = Vector2.zero;
    }

    public void PlaceToPuzzle(Vector3 center)
    {
        _isDragging = false; // Stop dragging when placing

        if (SelectedTetromino == this) {
            SelectedTetromino = null;
        }
        gameObject.layer = _placedTetrominoLayer;
        _spriteRenderer!.sortingOrder = _placedTetrominoSortingOrder; // Set the sorting order to placed
        _rb!.bodyType = RigidbodyType2D.Static;

        // set rotation to the closes mutliple of 90 degrees
        float angle = transform.rotation.eulerAngles.z;
        angle = Mathf.Round(angle / 90f) * 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector2 offset = Vector2.Scale(_rt!.sizeDelta, (0.5f * Vector2.one - _rt.pivot));
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
        if (_isDragging) {
            StopDragging();
        }
    }

    public void OnMouseEnter()
    {
        _isMouseOver = true;
    }

    public void OnMouseExit()
    {
        _isMouseOver = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == _playerPuzzleRowLayer) {
            _isOverPuzzleRow = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == _playerPuzzleRowLayer) {
            _isOverPuzzleRow = false;
        }
    }


    internal void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rt = GetComponent<RectTransform>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _abandonedTetrominosLayer = LayerMask.NameToLayer("AbandonedTetromino");
        _selectedTetrominoLayer = LayerMask.NameToLayer("SelectedTetromino");
        _placedTetrominoLayer = LayerMask.NameToLayer("PlacedTetromino");
        _playerPuzzleRowLayer = LayerMask.NameToLayer("PlayerPuzzleRow");
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
        if (_isDragging && mainCamera != null) {
            // --- Move the Object ---
            Vector3 mouseScreenPos = Input.mousePosition;
            // Adjust Z before converting so it's within camera view frustum but not ON the near plane
            mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            Vector2 targetWorldPos = mouseWorldPos + _pointerOffset; // Add the offset to the mouse position

            _rb!.MovePosition(targetWorldPos); // Move kinematic body correctly
        }
    }

    private void OnRotateInputAction(InputAction.CallbackContext ctx, bool smooth)
    {
        if (this != SelectedTetromino) {
            return;
        }
        if (smooth)
            transform.Rotate(0f, 0f, ctx.ReadValue<float>() * _rotationSpeed);
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

        if (_isDragging) {
            transform.localScale = TetrominoSizeManager.GetScaleFor(transform);
        }

        // --- Layer Management ---
        if (_isDragging || _isMouseOver) {
            if (SelectedTetromino != null && SelectedTetromino != this) {
                SelectedTetromino.gameObject.layer = _abandonedTetrominosLayer;
                SelectedTetromino._spriteRenderer!.sortingOrder = _abandonedTetrominosSortingOrder; // Set the sorting order to abandoned
            }
            if (SelectedTetromino != this) {
                gameObject.layer = _selectedTetrominoLayer;
                _spriteRenderer!.sortingOrder = _selectedTetrominoSortingOrder; // Set the sorting order to selected
                SelectedTetromino = this;
            }
        }
        else {
            gameObject.layer = _abandonedTetrominosLayer;
            _spriteRenderer!.sortingOrder = _abandonedTetrominosSortingOrder; // Set the sorting order to abandoned
            if (SelectedTetromino == this) {
                SelectedTetromino = null;
            }
        }

        // destroy if abandoned and not over puzzle row
        if (gameObject.layer == _abandonedTetrominosLayer) {
            if (!_isOverPuzzleRow) {
                _onDiscard?.Invoke();
                Destroy(gameObject);
            }
        }
    }


    #endregion
}
