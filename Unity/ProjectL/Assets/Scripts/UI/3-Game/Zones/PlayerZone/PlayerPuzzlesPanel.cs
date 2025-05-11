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
    public class PlayerPuzzlesPanel : MonoBehaviour
    {
        [SerializeField] private PlayerRowSlot? playerRowSlotPrefab;

        private PlayerRowSlot[] _puzzles = new PlayerRowSlot[PlayerState.MaxPuzzles];
        private BoxCollider2D? _collider;
        private Image? _image;

        private readonly static Color _currentPlayerBorderColor = new Color(34f / 255, 34f / 255, 34f / 255);
        private readonly static Color _notCurrentPlayerBorderColor = Color.black;

        private void Awake()
        {
            if (playerRowSlotPrefab == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }

            _collider = GetComponent<BoxCollider2D>();
            _collider.isTrigger = true;
            _image = GetComponent<Image>();

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
            if (_image == null || _collider == null) {
                return;
            }
            _image.color = current ? _currentPlayerBorderColor : _notCurrentPlayerBorderColor;
            _collider.enabled = current;

            foreach (var puzzle in _puzzles) {
                puzzle.SetAsCurrentPlayer(current);
            }
        }

        public Vector2 GetPlacementPositionFor(PlaceTetrominoAction action)
        {
            // find puzzle with matching ID
            for (int i = 0; i < _puzzles.Length; i++) {
                if (_puzzles[i].CurrentPuzzleId == action.PuzzleId) {
                    return _puzzles[i].GetPlacementPositionFor(action.Position);
                }
            }
            return default;
        }
    }
}
