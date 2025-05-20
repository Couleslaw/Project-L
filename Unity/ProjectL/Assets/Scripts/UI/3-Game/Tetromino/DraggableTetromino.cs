#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectL.Data;
    using ProjectL.Management;
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.Animation;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class DraggableTetromino : MonoBehaviour,
        IHumanPlayerActionListener<PlaceTetrominoAction>,
        IAIPlayerActionAnimator<PlaceTetrominoAction>,
        IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Constants

        private const float _rotationSpeed = 30;
        private const float _animationMovementSpeed = 5f;

        private const int _placedTetrominoSortingOrder = 1;
        private const int _abandonedTetrominosSortingOrder = 2;
        private const int _selectedTetrominoSortingOrder = 3;

        #endregion

        #region Fields

        private Vector2 _draggingPointerOffset;

        private static bool _initializedClass = false;

        [SerializeField] private TetrominoShape _shape;

        private Mode _mode;

        private Rigidbody2D? _rb;

        private RectTransform? _rt;

        private SpriteRenderer? _spriteRenderer;

        private Camera? _camera;

        private bool _isDragging = false;

        private bool _isMouseOver = false;
        private bool _isOverPlayerRow;

        #endregion

        #region Events

        public event Action? RemovedFromSceneEventHandler;

        public event Action<DraggableTetromino>? OnStartDraggingEventHandler;

        event Action<IActionModification<PlaceTetrominoAction>>? IHumanPlayerActionListener<PlaceTetrominoAction>.ActionModifiedEventHandler {
            add { }
            remove { }
        }
        #endregion

        private enum Mode
        {
            Animation,
            Selected,
            Abandoned,
            Placed
        }

        #region Properties

        private static DraggableTetromino? SelectedTetromino { get; set; } = null;

        private bool IsSelectedWithMouse => _isMouseOver || _isDragging;

        public TetrominoShape Shape => _shape;

        private static int AbandonedTetrominoLayer { get; set; }

        private static int SelectedTetrominoLayer { get; set; }

        private static int PlacedTetrominoLayer { get; set; }

        private static int PlayerPuzzleRowLayer { get; set; }

        #endregion

        #region Methods

        public void Init(TetrominoButton spawner, bool isAnimation)
        {
            // create a new tetromino sizer
            TetrominoSizer sizer = gameObject.AddComponent<TetrominoSizer>();
            sizer.Init(spawner);

            // handle animation separately
            if (isAnimation) {
                SetMode(Mode.Animation);
                return;
            }

            // listen to place action events
            HumanPlayerActionCreator.Instance?.AddListener<PlaceTetrominoAction>(this);

            // select the puzzle and start dragging
            SetMode(Mode.Selected);
            StartDragging();
            _isMouseOver = true;
        }

        public void StopDragging()
        {
            if (_mode == Mode.Animation) {
                return;
            }

            _isDragging = false;
            if (!_isOverPlayerRow) {
                SetMode(Mode.Abandoned);
            }
        }

        public BinaryImage GetConfiguration()
        {
            BinaryImage baseConf = TetrominoManager.GetImageOf(_shape);

            // check if flipped 
            if (transform.localScale.x < 0) {
                baseConf = baseConf.FlipHorizontally();
            }

            // check if rotated - first 
            int angle = (int)transform.rotation.eulerAngles.z;
            // modulo angle to 0 - 360 degrees
            angle = (angle % 360 + 360) % 360;
            // get nearest right angle
            angle = Mathf.RoundToInt(angle / 90f) * 90;

            // rotate the image
            for (int i = 0; i < angle / 90f; i++) {
                baseConf = baseConf.RotateLeft();
            }

            return baseConf;
        }

        public void PlaceToPosition(Vector3 center)
        {
            if (_mode == Mode.Animation) {
                return;
            }

            // set the tetromino to placed mode
            SetMode(Mode.Placed);
            SetPosition(center);
        }


        #region Unity Events

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _isMouseOver = true;

            // if right mouse button --> the tetromino should flip
            // this is handled with GameManager controls
            if (eventData.button == PointerEventData.InputButton.Right) {
                return;
            }

            // if middle button --> remove from scene
            if (eventData.button == PointerEventData.InputButton.Middle) {
                // first let listeners know that the tetromino was clicked
                OnStartDraggingEventHandler?.Invoke(this);
                RemoveFromScene();
                return;
            }

            // else start dragging 
            StartDragging();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            StopDragging();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_mode == Mode.Animation || _mode == Mode.Placed) {
                return;
            }
            _isMouseOver = true;

            // if no selected tetromino --> set this as selected
            if (SelectedTetromino == null) {
                SetMode(Mode.Selected);
            }

            // if not dragging selected tetromino --> set this as selected
            else if (!SelectedTetromino._isDragging && SelectedTetromino != this) {
                SetMode(Mode.Selected);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == PlayerPuzzleRowLayer) {
                _isOverPlayerRow = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == PlayerPuzzleRowLayer) {
                _isOverPlayerRow = false;
            }
        }

        #endregion


        #region controls

        private static void InitializeClass()
        {
            if (GameManager.Controls == null) {
                return;
            }

            _initializedClass = true;

            GameManager.Controls.Gameplay.RotateSmooth.performed += OnRotateSmoothInputAction;
            GameManager.Controls.Gameplay.Rotate90.performed += OnRotate90InputAction;
            GameManager.Controls.Gameplay.Flip.performed += OnFlipInputAction;
            GameManager.Controls.Gameplay.ClickPlace.performed += OnClickPlaceInputAction;
            GameManager.Controls.Gameplay.KeyboardPlace.performed += OnKeyboardPlaceInputAction;

            AbandonedTetrominoLayer = LayerMask.NameToLayer("AbandonedTetromino");
            SelectedTetrominoLayer = LayerMask.NameToLayer("SelectedTetromino");
            PlacedTetrominoLayer = LayerMask.NameToLayer("PlacedTetromino");
            PlayerPuzzleRowLayer = LayerMask.NameToLayer("PlayerPuzzleRow");
        }

        private static void OnClickPlaceInputAction(InputAction.CallbackContext ctx)
        {
            if (SelectedTetromino == null || !SelectedTetromino.IsSelectedWithMouse) {
                return;
            }

            InteractivePuzzle.TryPlacingToPuzzle(SelectedTetromino);
        }

        private static void OnKeyboardPlaceInputAction(InputAction.CallbackContext ctx)
        {
            if (SelectedTetromino == null || !PlayerZoneManager.Instance.IsMouseOverCurrentPlayersRow) {
                return;
            }

            InteractivePuzzle.TryPlacingToPuzzle(SelectedTetromino);
        }

        private static void OnRotateSmoothInputAction(InputAction.CallbackContext ctx)
        {
            if (SelectedTetromino == null || !PlayerZoneManager.Instance.IsMouseOverCurrentPlayersRow) {
                return;
            }

            SelectedTetromino.transform.Rotate(0f, 0f, ctx.ReadValue<float>() * _rotationSpeed);
        }

        private static void OnRotate90InputAction(InputAction.CallbackContext ctx)
        {
            if (SelectedTetromino == null || !PlayerZoneManager.Instance.IsMouseOverCurrentPlayersRow) {
                return;
            }

            SelectedTetromino.SnapToNearestRightAngle();
            SelectedTetromino.transform.Rotate(0f, 0f, 90 * Mathf.Sign(ctx.ReadValue<float>()));
        }

        private static void OnFlipInputAction(InputAction.CallbackContext ctx)
        {
            if (SelectedTetromino == null || !PlayerZoneManager.Instance.IsMouseOverCurrentPlayersRow) {
                return;
            }

            Transform tr = SelectedTetromino.transform;

            // change x scale sign
            Vector3 scale = tr.localScale;
            scale.x *= -1;
            SelectedTetromino.transform.localScale = scale;

            // change rotation to visually flip along y axis
            Quaternion rotation = tr.rotation;
            rotation.z *= -1;
            SelectedTetromino.transform.rotation = rotation;
        }

        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rt = GetComponent<RectTransform>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _camera = Camera.main; // Cache the camera

            if (!_initializedClass) {
                InitializeClass();
            }
        }

        private void FixedUpdate()
        {
            // if abandoned tetromino is not over player row --> remove from scene
            if (_mode == Mode.Abandoned && !_isOverPlayerRow) {
                RemoveFromScene();
                return;
            }

            // if dragging --> update tetromino position based on mouse position 
            if (_isDragging) {
                Vector3 mouseScreenPos = Input.mousePosition;
                mouseScreenPos.z = _camera!.WorldToScreenPoint(transform.position).z;

                Vector2 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);
                _rb!.MovePosition(mouseWorldPos + _draggingPointerOffset);
            }
        }

        private void SetMode(Mode mode)
        {
            if (_rb == null || _spriteRenderer == null || _rt == null) {
                return;
            }

            _mode = mode;

            // update selected tetromino
            if (mode == Mode.Selected) {
                // if a different tetromino is selected --> abandon it
                if (SelectedTetromino != null && SelectedTetromino != this) {
                    SelectedTetromino.SetMode(Mode.Abandoned);
                }
                SelectedTetromino = this;
            }
            // if this tetromino is Selected, but the mode is changed to something else --> unselect it
            else if (SelectedTetromino == this) {
                SelectedTetromino = null;
            }


            switch (mode) {
                case Mode.Animation: {
                    // update layers - selected so that puzzles can detect it
                    gameObject.layer = SelectedTetrominoLayer;
                    _spriteRenderer.sortingOrder = _selectedTetrominoSortingOrder;

                    // update rigidbody
                    _rb.bodyType = RigidbodyType2D.Kinematic;
                    _isDragging = false;
                    break;
                }

                case Mode.Selected: {
                    // update layers - selected so that puzzles can detect it
                    gameObject.layer = SelectedTetrominoLayer;
                    _spriteRenderer.sortingOrder = _selectedTetrominoSortingOrder;

                    // update rigidbody
                    _rb.bodyType = RigidbodyType2D.Kinematic;
                    _rb.linearVelocity = Vector2.zero;
                    _rb.angularVelocity = 0f;

                    break;
                }

                case Mode.Abandoned: {
                    // update layers - abandoned: puzzles don't collide with it, but tetrominos do
                    gameObject.layer = AbandonedTetrominoLayer;
                    _spriteRenderer!.sortingOrder = _abandonedTetrominosSortingOrder;

                    // update rigidbody
                    _rb!.bodyType = RigidbodyType2D.Dynamic;
                    break;
                }

                case Mode.Placed: {
                    // update layers - placed: puzzles nor tetrominos collide with it
                    gameObject.layer = PlacedTetrominoLayer;
                    _spriteRenderer!.sortingOrder = _placedTetrominoSortingOrder;

                    // update rigidbody
                    _rb!.bodyType = RigidbodyType2D.Static;
                    _isDragging = false;
                    _isMouseOver = false;

                    SnapToNearestRightAngle();
                    break;
                }
                default:
                    break;
            }
        }

        private void StartDragging()
        {
            if (_mode == Mode.Animation || _isDragging) {
                return;
            }

            _isDragging = true;
            SnapToNearestRightAngle();

            // select the tetromino
            SetMode(Mode.Selected);

            // notify listeners that dragging started
            OnStartDraggingEventHandler?.Invoke(this);

            // calculate pointer offset from object center
            Vector2 mouseWorldPos = _camera!.ScreenToWorldPoint(Input.mousePosition);
            _draggingPointerOffset = (Vector2)transform.position - mouseWorldPos;
        }

        private void SetPosition(Vector3 center)
        {
            // calculate pivot offset
            Vector2 offset = Vector2.Scale(_rt!.sizeDelta, (0.5f * Vector2.one - _rt.pivot));
            offset = Vector2.Scale(offset, (Vector2)transform.localScale);
            offset = RotateVector(offset, transform.rotation.eulerAngles.z);

            // correct the position with offset
            _rt!.position = (Vector2)center + offset;

            static Vector2 RotateVector(Vector2 v, float degrees)
            {
                float delta = Mathf.Deg2Rad * degrees;
                return new Vector2(
                    v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                    v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
                );
            }
        }

        private void SnapToNearestRightAngle()
        {
            float angle = transform.rotation.eulerAngles.z;
            angle = Mathf.Round(angle / 90f) * 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void RemoveFromScene()
        {
            if (this == null || gameObject == null)
                return;

            HumanPlayerActionCreator.Instance?.RemoveListener(this);

            // unselect tetromino
            if (SelectedTetromino == this) {
                SelectedTetromino = null;
            }

            RemovedFromSceneEventHandler?.Invoke();

            // if not placed --> destroy immediately
            if (_mode != Mode.Placed) {
                Destroy(gameObject);
            }
            // if placed --> destroy after a small delay to prevent animation clipping 
            else {
                StartCoroutine(DestroyAfterMilliseconds(50f));
            }

            IEnumerator DestroyAfterMilliseconds(float milliseconds)
            {
                yield return new WaitForSeconds(milliseconds / 1000f);
                Destroy(gameObject);
            }
        }


        void IHumanPlayerActionListener<PlaceTetrominoAction>.OnActionRequested() { }

        void IHumanPlayerActionListener<PlaceTetrominoAction>.OnActionCanceled() => RemoveFromScene();

        void IHumanPlayerActionListener<PlaceTetrominoAction>.OnActionConfirmed() => RemoveFromScene();

        async Task IAIPlayerActionAnimator<PlaceTetrominoAction>.Animate(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // try get puzzle to place the tetromino to
            if (!InteractivePuzzle.TryGetPuzzleWithId(action.PuzzleId, out InteractivePuzzle? puzzle)) {
                return;
            }

            // where on the screen should the tetromino be placed
            Vector2 goalPosition = puzzle!.GetPlacementCenter(action.Position);

            // rotate and flip the tetromino to match the placement
            var transformation = GetTransformation(TetrominoManager.GetImageOf(Shape), action.Position);

            // calculate goal scale
            Vector3 goalScale = transform.localScale;
            if (transformation.Item1) {
                goalScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            // goal rotation
            float goalRotation = transformation.Item2;

            // animation plan:
            // 0/6: start moving, 1/6 : flip, 2/6: start rotation, 5/6: end rotation, 6/6: arrive at goal position

            float tFlip = 1f / 6f;
            float tStartRotate = 2f / 6f;
            float tEndRotate = 5f / 6f;

            // remember original params
            float originalRotation = transform.rotation.eulerAngles.z;
            float originalDistance = Vector2.Distance(transform.position, goalPosition);
            bool isFlipped = false;

            Vector2 currentPos = transform.position;

            while (!cancellationToken.IsCancellationRequested) {
                float delta = Time.fixedDeltaTime * _animationMovementSpeed * AnimationSpeed.Multiplier;
                currentPos = Vector2.MoveTowards(currentPos, goalPosition, delta);
                SetPosition(currentPos);

                float distance = Vector2.Distance(currentPos, goalPosition);

                // calculate how far in the animation are we
                float t = Mathf.InverseLerp(originalDistance, 0f, distance);

                // flip
                if (t >= tFlip && !isFlipped) {
                    isFlipped = true;
                    transform.localScale = goalScale;
                }

                // rotation
                float angle = Mathf.LerpAngle(originalRotation, goalRotation, Mathf.InverseLerp(tStartRotate, tEndRotate, t));
                transform.rotation = Quaternion.Euler(0f, 0f, angle);

                // if tetromino is close enough to the goal position --> snap to it
                if (distance < 0.1f) {
                    SetPosition(goalPosition);
                    break;
                }
                await Awaitable.FixedUpdateAsync();
            }

            cancellationToken.ThrowIfCancellationRequested();

            // set the tetromino to placed mode - prevent collisions
            // switch back to animation mode - prevent user modifications
            SetMode(Mode.Placed);
            _mode = Mode.Animation;

            // color cells of puzzle to match the tetromino and destroy the tetromino
            (puzzle as IAIPlayerActionAnimator<PlaceTetrominoAction>)?.Animate(action, cancellationToken);
            Destroy(gameObject);

            await AnimationManager.WaitForScaledDelay(0.5f, cancellationToken);

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
                    start = start.FlipHorizontally();
                }
                return (false, 0);
            }
        }

        #endregion
    }
}
