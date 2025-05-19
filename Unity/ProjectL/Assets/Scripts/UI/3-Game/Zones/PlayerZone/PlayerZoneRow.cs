#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using System.Runtime.CompilerServices;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerZoneRow : MonoBehaviour, IPlayerStatePuzzleListener,
        IHumanPlayerActionListener<TakePuzzleAction>
    {
        #region Fields

        private readonly PuzzleSlot[] _puzzles = new PuzzleSlot[PlayerState.MaxPuzzles];

        [SerializeField] private TextMeshProUGUI? _playerNameLabel;

        [SerializeField] private PuzzleSlot? playerRowSlotPrefab;
        private Camera? _mainCamera;
        private BoxCollider2D? _collider;

        private Image? _backgroundImage;

        private PuzzleSlot? _takePuzzleActionPlacePosition = null;

        #endregion

        public bool CanConfirmTakePuzzleAction {
            set {
                foreach (var puzzle in _puzzles) {
                    puzzle.EnableEmptySlotButton(value);
                }
            }
        }

        public bool IsMouseOverRow => IsMouseOver();

        event Action<IActionModification<TakePuzzleAction>>? IHumanPlayerActionListener<TakePuzzleAction>.ActionModifiedEventHandler {
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

        #region Methods

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
            _playerNameLabel!.color = current ? GameGraphicsSystem.ActiveColor : GameGraphicsSystem.InactiveColor;

            // enable / disable puzzles container
            _collider.enabled = current;
            ToggleBackground(current);

            // activate puzzles
            foreach (var puzzle in _puzzles) {
                puzzle.MakePuzzleInteractive(current);
            }

            // listen to take puzzle action
            if (current) {
                HumanPlayerActionCreator.Instance.AddListener(this);
            }
            else {
                HumanPlayerActionCreator.Instance.RemoveListener(this);
            }
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

                puzzle.OnEmptySlotClickEventHandler += () => {
                    _takePuzzleActionPlacePosition = puzzle;
                    HumanPlayerActionCreator.Instance.OnActionConfirmed();
                };
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
            if (puzzle is not PuzzleWithColor) {
                Debug.LogError($"Puzzle {puzzle} is not a {nameof(PuzzleWithColor)}.");
                return;
            }

            if (_takePuzzleActionPlacePosition != null) {
                _takePuzzleActionPlacePosition.PlacePuzzle((PuzzleWithColor)puzzle);
                _takePuzzleActionPlacePosition = null;
                return;
            }

            foreach (var puzzleSlot in _puzzles) {
                if (puzzleSlot.PuzzleId == null) {
                    puzzleSlot.PlacePuzzle((PuzzleWithColor)puzzle);
                    return;
                }
            }
        }

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionRequested()
        {
            // don't block raycasting for empty slot buttons in the row
            _collider!.enabled = false;
        }

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionCanceled() => _collider!.enabled = true;

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionConfirmed() => _collider!.enabled = true;

        #endregion
    }
}
