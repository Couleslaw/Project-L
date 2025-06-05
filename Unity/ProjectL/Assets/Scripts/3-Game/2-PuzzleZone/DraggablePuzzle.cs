#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectL.Data;
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.GameScene.Management;
    using ProjectL.GameScene.PlayerZone;
    using ProjectL.Sound;
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class DraggablePuzzle : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler,
        IHumanPlayerActionCreator<TakePuzzleAction>
    {
        private TakePuzzleAction? _action;
        private RectTransform? _rt;
        private SpriteRenderer? _spriteRenderer;
        private Camera? _camera;
        private bool _isDragging;
        private Vector2 _draggingPointerOffset;

        public event Action? RemovedFromSceneEventHandler;

        event Action<IActionModification<TakePuzzleAction>>? IHumanPlayerActionCreator<TakePuzzleAction>.ActionModifiedEventHandler {
            add { }
            remove { }
        }

        public void Init(TakePuzzleAction action, Puzzle puzzle)
        {
            _rt!.localScale = ScaleManager.Instance.PuzzleZoneScale * Vector3.one;
            _action = action;

            // get sprite based on action
            Sprite? sprite = null;

            if (action.Option == TakePuzzleAction.Options.Normal) {
                ResourcesLoader.TryGetPuzzleSprite(puzzle, PuzzleSpriteType.BorderBright, out sprite);
            }
            if (action.Option == TakePuzzleAction.Options.TopWhite) {
                ResourcesLoader.TryGetDeckCardSprite(isBlack: false, out sprite);
            }
            if (action.Option == TakePuzzleAction.Options.TopBlack) {
                ResourcesLoader.TryGetDeckCardSprite(isBlack: true, out sprite);
            }

            if (sprite == null) {
                Debug.LogError($"Failed to load sprite for puzzle {puzzle.Id} with action {action.Option}");
                return;
            }

            _spriteRenderer!.sprite = sprite;

            StartDragging();
        }

        private void StartDragging()
        {
            if (_isDragging) {
                return;
            }

            _isDragging = true;
            PuzzleZoneManager.Instance.ReportTakePuzzleChange(new(null));

            // calculate pointer offset from object center
            Vector2 mouseWorldPos = _camera!.ScreenToWorldPoint(Input.mousePosition);
            _draggingPointerOffset = (Vector2)transform.position - mouseWorldPos;
        }
        public void StopDragging()
        {
            _isDragging = false;

            if (PlayerZoneManager.Instance.IsMouseOverCurrentPlayersRow) {
                var currentRow = PlayerZoneManager.Instance.CurrentPlayerRow!;
                if (currentRow.TryGetClosestEmptySlot(_rt!.position, out var slot)) {
                    currentRow.SetTakePuzzleActionSlot(slot!);

                    SoundManager.Instance.PlayTapSoundEffect();

                    _rt!.position = slot!.transform.position;

                    PuzzleZoneManager.Instance.ReportTakePuzzleChange(new(_action));
                    return;
                }
            }

            PlayerZoneManager.Instance.CurrentPlayerRow!.ClearEmptySlotHighlight();
            RemovedFromSceneEventHandler?.Invoke();
            SoundManager.Instance.PlaySoftTapSoundEffect();
            RemoveFromScene();
            PuzzleZoneManager.Instance.ReportTakePuzzleChange(new(null));
        }

        private void RemoveFromScene()
        {
            if (this == null || gameObject == null)
                return;

            HumanPlayerActionCreationManager.Instance?.RemoveListener(this);

            RemovedFromSceneEventHandler?.Invoke();
            Destroy(gameObject);
        }

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _camera = Camera.main; // Cache the camera

            HumanPlayerActionCreationManager.Instance.AddListener(this);
        }

        private void FixedUpdate()
        {
            // if dragging --> update tetromino position based on mouse position 
            if (_isDragging) {
                Vector3 mouseScreenPos = Input.mousePosition;
                mouseScreenPos.z = _camera!.WorldToScreenPoint(transform.position).z;

                Vector2 mouseWorldPos = _camera.ScreenToWorldPoint(mouseScreenPos);
                _rt!.position = mouseWorldPos + _draggingPointerOffset;

                if (PlayerZoneManager.Instance.IsMouseOverCurrentPlayersRow) {
                    PlayerZoneManager.Instance.CurrentPlayerRow!.HighlightClosestEmptySlot(mouseWorldPos);
                }
                else {
                    PlayerZoneManager.Instance.CurrentPlayerRow!.ClearEmptySlotHighlight();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                StartDragging();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                StopDragging();
            }
        }

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionRequested() { }

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionCanceled() => RemoveFromScene();

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionConfirmed() => RemoveFromScene();
    }
}
