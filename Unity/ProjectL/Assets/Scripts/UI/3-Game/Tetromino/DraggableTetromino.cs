#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectL.Data;
    using ProjectL.Management;
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class DraggableTetromino : MonoBehaviour, IPlacePieceActionListener
    {
        #region Constants

        private const float _rotationSpeed = 30;// degrees per 1 mouse wheel move

        private const float _animationSpeed = 5f;// units per second

        private const int _placedTetrominoSortingOrder = 1;

        private const int _abandonedTetrominosSortingOrder = 2;

        private const int _selectedTetrominoSortingOrder = 3;

        #endregion

        #region Fields

        internal Vector2 _pointerOffset;

        private static int _abandonedTetrominosLayer;

        private static int _selectedTetrominoLayer;

        private static int _placedTetrominoLayer;

        private static int _playerPuzzleRowLayer;

        [SerializeField] private TetrominoShape _shape;

        private Rigidbody2D? _rb;

        private RectTransform? _rt;

        private SpriteRenderer? _spriteRenderer;

        private Camera? mainCamera;

        private bool _isDragging = false;

        private bool _isMouseOver = false;

        private bool _isOverPuzzleRow = false;

        private bool _isAnimating = false;

        private Action? _onReturnToCollection;

        #endregion

        #region Properties

        public static DraggableTetromino? SelectedTetromino { get; private set; } = null;

        public TetrominoShape Shape => _shape;

        public Action<DraggableTetromino>? OnStartDragging { get; set; } = null;

        #endregion

        #region Methods

        // Called by the ButtonShapeSpawner after instantiation
        public void Init(Camera cam, Action onReturnToCollection)
        {
            mainCamera = cam;
            _onReturnToCollection = onReturnToCollection;
            ActionCreationManager.Instance.AddListener(this);
        }

        public async Task AnimateAIPlayerPlaceActionAsync(Vector2 goalPosition, PlaceTetrominoAction action, CancellationToken cancellationToken = default)
        {
            _isAnimating = true;
            // rotate and flip the tetromino to match the placement
            var transformation = GetTransformation(TetrominoManager.GetImageOf(Shape), action.Position);
            if (transformation.Item1) {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            transform.rotation = Quaternion.Euler(0f, 0f, transformation.Item2);

            // move to the goal position
            Vector2 currentPos = transform.position;

            while (!cancellationToken.IsCancellationRequested) {
                float delta = Time.fixedDeltaTime * _animationSpeed * AnimationSpeed.Multiplier;
                currentPos = Vector2.MoveTowards(currentPos, goalPosition, delta);
                SetPosition(currentPos);
                if (Vector2.Distance(currentPos, goalPosition) < 0.1f) {
                    SetPosition(goalPosition);
                    break;
                }
                await Awaitable.FixedUpdateAsync();
            }

            // returns [shouldFlip], angle
            static (bool, float) GetTransformation(BinaryImage start, BinaryImage goal)
            {
                goal = goal.MoveImageToTopLeftCorner();
                for (int flip = 0; flip <= 1; flip++) {
                    for (int rotate = 0; rotate <= 3; rotate++) {
                        if (start.MoveImageToTopLeftCorner() == goal) {
                            return (flip == 1, rotate * 90);
                        }
                        start = start.RotateLeft();
                    }
                    start = start.FlipVertically();
                }
                return (false, 0);
            }
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

            // set rotation to the closes multiple of 90 degrees
            float angle = transform.rotation.eulerAngles.z;
            angle = Mathf.Round(angle / 90f) * 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            SetPosition(center);
        }

        public void SetPosition(Vector3 center)
        {
            Vector2 offset = Vector2.Scale(_rt!.sizeDelta, (0.5f * Vector2.one - _rt.pivot));
            offset = Vector2.Scale(offset, (Vector2)transform.localScale);
            Vector2 correctedCenter = center + (Vector3)RotateVector(offset, transform.rotation.eulerAngles.z);
            _rt!.position = correctedCenter;

            static Vector2 RotateVector(Vector2 v, float degrees)
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

        public void ReturnToCollection() => Destroy(gameObject);

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

        internal void Update() // Scale can be done in Update
        {
            // scale the piece based on distance to the puzzle zone

            if (gameObject.layer == _placedTetrominoLayer) {
                return; // don't do anything if the tetromino is placed
            }

            if (_isDragging || _isAnimating) {
                transform.localScale = TetrominoSizeManager.GetScaleFor(transform);
            }

            if (_isAnimating) {
                if (SelectedTetromino != this) {
                    SelectedTetromino = this;
                    gameObject.layer = _selectedTetrominoLayer;
                }
                return;
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
            if (!_isOverPuzzleRow && gameObject.layer == _abandonedTetrominosLayer) {
                ReturnToCollection();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == _playerPuzzleRowLayer) {
                _isOverPuzzleRow = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == _playerPuzzleRowLayer) {
                _isOverPuzzleRow = false;
            }
        }

        private void Start()
        {
            transform.localScale = TetrominoSizeManager.GetScaleFor(transform);

            if (GameManager.Controls == null) {
                Debug.LogError("GameManager controls are not initialized.");
                return;
            }
            GameManager.Controls.Gameplay.RotateSmooth.performed += OnRotateSmoothInputAction;
            GameManager.Controls.Gameplay.Rotate90.performed += OnRotate90InputAction;
            GameManager.Controls.Gameplay.Flip.performed += OnFlipInputAction;
            GameManager.Controls.Gameplay.Place.performed += OnPlaceInputAction;
        }

        private void FixedUpdate() // Use FixedUpdate for Rigidbody manipulation
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

        private void OnDestroy()
        {
            ActionCreationManager.Instance.RemoveListener(this);
            GameManager.Controls!.Gameplay.RotateSmooth.performed -= OnRotateSmoothInputAction;
            GameManager.Controls.Gameplay.Rotate90.performed -= OnRotate90InputAction;
            GameManager.Controls.Gameplay.Flip.performed -= OnFlipInputAction;
            GameManager.Controls.Gameplay.Place.performed -= OnPlaceInputAction;
            if (gameObject.layer != _placedTetrominoLayer) {
                _onReturnToCollection?.Invoke();
            }
        }

        private void OnRotateSmoothInputAction(InputAction.CallbackContext ctx)
        {
            if (this == SelectedTetromino) {
                transform.Rotate(0f, 0f, ctx.ReadValue<float>() * _rotationSpeed);
            }
        }

        private void OnRotate90InputAction(InputAction.CallbackContext ctx)
        {
            if (this == SelectedTetromino) {
                transform.Rotate(0f, 0f, 90 * Mathf.Sign(ctx.ReadValue<float>()));
            }
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
                InteractivePuzzle.PlaceTetrominoToPuzzle(this);
            }
        }

        void IHumanPlayerActionListener.OnActionRequested()
        {
        }

        void IHumanPlayerActionListener.OnActionCanceled() => ReturnToCollection();

        void IHumanPlayerActionListener.OnActionConfirmed() => ReturnToCollection();

        #endregion
    }
}
