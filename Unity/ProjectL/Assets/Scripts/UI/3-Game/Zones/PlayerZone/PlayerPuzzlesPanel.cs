#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectLCore.GameLogic;
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine.UI;
    using ProjectLCore.GameActions;
    using System.Threading;
    using System.Threading.Tasks;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerPuzzlesPanel : MonoBehaviour, IEnumerable<PlayerRowSlot>
    {
        [SerializeField] private PlayerRowSlot? playerRowSlotPrefab;

        private readonly PlayerRowSlot[] _puzzles = new PlayerRowSlot[PlayerState.MaxPuzzles];
        private BoxCollider2D? _collider;
        private Image? _backgroundImage;

        private void Awake()
        {
            if (playerRowSlotPrefab == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }

            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;
            _backgroundImage = GetComponent<Image>();

            for (int i = 0; i < PlayerState.MaxPuzzles; i++) {
                var puzzle = Instantiate(playerRowSlotPrefab, transform);
                puzzle.gameObject.SetActive(true);
                _puzzles[i] = puzzle;
            }
        }

        public PlayerRowSlot this[int index] {
            get {
                if (index < 0 || index >= _puzzles.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                return _puzzles[index];
            }
        }

        public void SetAsCurrentPlayer(bool current)
        {
            if (_backgroundImage == null || _collider == null) {
                return;
            }
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

        private void ToggleBackground(bool show)
        {
            _backgroundImage!.color = show ? Color.white : Color.clear;
        }

        public IEnumerator<PlayerRowSlot> GetEnumerator()
        {
            return ((IEnumerable<PlayerRowSlot>)_puzzles).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
