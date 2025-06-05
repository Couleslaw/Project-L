#nullable enable

namespace ProjectL.GameScene.PlayerZone
{
    using ProjectL.Animation;
    using ProjectL.GameScene.ActionHandling;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerPuzzlesRow : MonoBehaviour, IPlayerStatePuzzleListener,
        IHumanPlayerActionCreator<TakePuzzleAction>
    {

        private readonly PuzzleSlot[] _puzzles = new PuzzleSlot[PlayerState.MaxPuzzles];

        [SerializeField] private TextMeshProUGUI? _playerNameLabel;

        [SerializeField] private PuzzleSlot? playerRowSlotPrefab;
        private Camera? _mainCamera;
        private BoxCollider2D? _collider;

        private Image? _backgroundImage;

        private PuzzleSlot? _takePuzzleActionSlot = null;

        private IDisposable? _emptySlotHighlighterDisposable = null;

        public bool IsMouseOverRow => IsMouseOver();

        event Action<IActionModification<TakePuzzleAction>>? IHumanPlayerActionCreator<TakePuzzleAction>.ActionModifiedEventHandler {
            add { }
            remove { }
        }

        private bool IsMouseOver()
        {
            if (_collider == null || _mainCamera == null) {
                return false;
            }

            Vector2 mousePos = Input.mousePosition;
            Vector2 worldPos = Camera.main!.ScreenToWorldPoint(mousePos);
            return _collider.OverlapPoint(worldPos);
        }

        public void Init(string playerName, PlayerState playerState)
        {
            if (_playerNameLabel != null) {
                _playerNameLabel.text = playerName;
            }
            playerState.AddListener((IPlayerStatePuzzleListener)this);
            SetAsCurrentPlayer(false);
        }

        public void SetAsCurrentPlayer(bool current)
        {
            if (_backgroundImage == null || _collider == null || _playerNameLabel == null) {
                return;
            }

            // make name white / gray
            _playerNameLabel!.color = current ? Color.white : ColorManager.gray;

            // enable / disable puzzles container
            _collider.enabled = current;
            ToggleBackground(current);

            // activate puzzles
            foreach (var puzzle in _puzzles) {
                puzzle.MakePuzzleInteractive(current);
            }

            // listen to take puzzle action
            if (current) {
                HumanPlayerActionCreationManager.Instance.AddListener(this);
            }
            else {
                HumanPlayerActionCreationManager.Instance.RemoveListener(this);
            }
        }

        public void SetTakePuzzleActionSlot(PuzzleSlot slot)
        {
            _takePuzzleActionSlot = slot;
        }

        public bool TryGetPuzzleWithId(uint puzzleId, out PuzzleSlot? result)
        {
            result = null;
            foreach (var puzzle in _puzzles) {
                if (puzzle.PuzzleId == puzzleId) {
                    result = puzzle;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetClosestEmptySlot(Vector2 position, out PuzzleSlot? result)
        {
            float minDist = float.MaxValue;
            result = null;

            foreach (PuzzleSlot slot in _puzzles) {
                if (slot.PuzzleId == null) {
                    float dist = Vector2.Distance(slot.transform.position, position);
                    if (dist < minDist) {
                        minDist = dist;
                        result = slot;
                    }
                }
            }
            return result != null;
        }

        public void HighlightClosestEmptySlot(Vector2 position)
        {
            _emptySlotHighlighterDisposable?.Dispose();
            if (TryGetClosestEmptySlot(position, out var closestSlot)) {
                _emptySlotHighlighterDisposable = closestSlot!.GetDisposableEmptySlotHighlighter();
            }
        }

        public void ClearEmptySlotHighlight()
        {
            _emptySlotHighlighterDisposable?.Dispose();
            _emptySlotHighlighterDisposable = null;
        }

        private void Awake()
        {
            if (_playerNameLabel == null || playerRowSlotPrefab == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }

            _mainCamera = Camera.main;
            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;
            _collider.enabled = false;
            _backgroundImage = GetComponent<Image>();

            for (int i = 0; i < PlayerState.MaxPuzzles; i++) {
                var puzzle = Instantiate(playerRowSlotPrefab, transform);
                puzzle.gameObject.SetActive(true);
                _puzzles[i] = puzzle;
            }
        }

        private void ToggleBackground(bool show)
        {
            _backgroundImage!.color = show ? Color.white : Color.clear;
        }

        void IPlayerStatePuzzleListener.OnPuzzleFinished(FinishedPuzzleInfo info)
        {
            if (TryGetPuzzleWithId(info.Puzzle.Id, out var puzzleSlot)) {
                puzzleSlot!.FinishPuzzle();
            }
            else {
                Debug.LogError($"Puzzle with ID {info.Puzzle.Id} not found in player row.");
            }
        }

        void IPlayerStatePuzzleListener.OnPuzzleAdded(Puzzle puzzle)
        {
            if (puzzle is not ColorPuzzle) {
                Debug.LogError($"Puzzle {puzzle} is not a {nameof(ColorPuzzle)}.");
                return;
            }

            if (_takePuzzleActionSlot != null) {
                _takePuzzleActionSlot.PlacePuzzle((ColorPuzzle)puzzle);
                _takePuzzleActionSlot = null;
                return;
            }

            foreach (var puzzleSlot in _puzzles) {
                if (puzzleSlot.PuzzleId == null) {
                    puzzleSlot.PlacePuzzle((ColorPuzzle)puzzle);
                    return;
                }
            }
        }

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionRequested()
        {
            _takePuzzleActionSlot = null;
        }

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionCanceled()
        {
            ClearEmptySlotHighlight();
            _takePuzzleActionSlot = null;
        }

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionConfirmed()
        {
            ClearEmptySlotHighlight();
        }

    }
}
