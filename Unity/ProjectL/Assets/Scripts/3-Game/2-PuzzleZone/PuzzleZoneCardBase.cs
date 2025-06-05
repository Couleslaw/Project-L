#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.Sound;
    using ProjectLCore.GameLogic;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public abstract class PuzzleZoneCardBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Fields

        protected Button? _button;

        protected Image? _image;

        protected bool _isBlack;

        protected PuzzleZoneMode _mode;

        [SerializeField] private DraggablePuzzle? _draggablePuzzlePrefab;

        private IDisposable? _takePuzzleDisposable;

        private DraggablePuzzle? _currentDraggingPuzzle;

        #endregion

        #region Properties

        public bool CanTakePuzzle { get; protected set; }

        #endregion

        #region Methods

        public void Init(bool isBlack)
        {
            _isBlack = isBlack;
            if (!didAwake) {
                Awake();
            }
        }

        public virtual void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            _mode = mode;
            CanTakePuzzle = GetCanTakePuzzle(turnInfo);
        }

        public abstract PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardHighlighter();

        public abstract PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardDimmer();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            if (_mode != PuzzleZoneMode.TakePuzzle || !CanTakePuzzle) {
                return;
            }

            if (_takePuzzleDisposable != null) {
                return; // Already dragging this puzzle
            }

            HumanPlayerActionCreationManager.Instance.OnActionCanceled();
            HumanPlayerActionCreationManager.Instance.OnTakePuzzleActionRequested();

            SoundManager.Instance?.PlaySliderSound();

            DraggablePuzzle draggablePuzzle = Instantiate(_draggablePuzzlePrefab, transform.position, Quaternion.identity)!;
            InitializeDraggablePuzzle(draggablePuzzle);

            _takePuzzleDisposable = GetTakePuzzleDisposable();

            draggablePuzzle.RemovedFromSceneEventHandler += () => {
                _takePuzzleDisposable?.Dispose();
                _takePuzzleDisposable = null;
            };

            _currentDraggingPuzzle = draggablePuzzle;
            HumanPlayerActionCreationManager.Instance.OnTakePuzzleActionRequested();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_button == null || _mode != PuzzleZoneMode.TakePuzzle) {
                return;
            }

            if (_currentDraggingPuzzle != null) {
                _currentDraggingPuzzle.StopDragging();
                _currentDraggingPuzzle = null;
            }
        }

        protected abstract bool GetCanTakePuzzle(TurnInfo turnInfo);

        protected abstract void InitializeDraggablePuzzle(DraggablePuzzle puzzle);

        protected abstract IDisposable GetTakePuzzleDisposable();

        protected virtual void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
        }

        #endregion
    }
}
