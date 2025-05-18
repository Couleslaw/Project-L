#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerZoneRow : MonoBehaviour, IPlayerStatePuzzleListener
    {
        #region Fields

        private readonly PlayerRowSlot[] _puzzles = new PlayerRowSlot[PlayerState.MaxPuzzles];

        [SerializeField] private TextMeshProUGUI? _playerNameLabel;

        [SerializeField] private PlayerRowSlot? playerRowSlotPrefab;

        private BoxCollider2D? _collider;

        private Image? _backgroundImage;

        private PlayerRowSlot? _takePuzzleActionPlacePosition = null;

        #endregion

        public bool CanConfirmTakePuzzleAction {
            set {
                foreach (var puzzle in _puzzles) {
                    puzzle.EnableEmptySlotButton(value);
                }
            }
        }

        #region Methods

        public void Init(string playerName, PlayerState playerState)
        {
            if (_playerNameLabel != null) {
                _playerNameLabel.text = playerName;
            }
            playerState.AddListener(this);
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

            foreach (var puzzle in _puzzles) {
                puzzle.SetAsCurrentPlayer(current);
            }
        }

        public Vector2 GetPlacementPositionFor(PlaceTetrominoAction action)
        {
            // find puzzle with matching ID
            for (int i = 0; i < _puzzles.Length; i++) {
                if (_puzzles[i].PuzzleId == action.PuzzleId) {
                    return _puzzles[i].GetPlacementPositionFor(action.Position);
                }
            }
            return default;
        }

        public bool TryGetPuzzleWithId(uint puzzleId, out PlayerRowSlot? result)
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

            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;
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
            if (puzzle is not PuzzleWithGraphics) {
                Debug.LogError($"Puzzle {puzzle} is not a {nameof(PuzzleWithGraphics)}.");
                return;
            }

            if (_takePuzzleActionPlacePosition != null) {
                _takePuzzleActionPlacePosition.PlacePuzzle((PuzzleWithGraphics)puzzle);
                _takePuzzleActionPlacePosition = null;
                return;
            }

            foreach (var puzzleSlot in _puzzles) {
                if (puzzleSlot.PuzzleId == null) {
                    puzzleSlot.PlacePuzzle((PuzzleWithGraphics)puzzle);
                    return;
                }
            }
        }
        #endregion
    }
}
