#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectL.UI.Sound;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerRowSlot : MonoBehaviour
    {
        #region Fields

        [SerializeField] private InteractivePuzzle? _puzzleCard;
        [SerializeField] private Image? _emptySlot;
        [SerializeField] private Image? _puzzleFrame;

        #endregion

        #region Properties

        public uint? CurrentPuzzleId => _puzzleCard != null ? _puzzleCard.CurrentPuzzleId : null;

        #endregion

        #region Methods

        public void FinishPuzzle()
        {
            if (_puzzleCard == null || _emptySlot == null) {
                return;
            }
            SoundManager.Instance?.PlaySoftTapSoundEffect();
            _puzzleCard.gameObject.SetActive(false);
            _emptySlot.gameObject.SetActive(true);
        }

        public void PlacePuzzle(PuzzleWithGraphics puzzle)
        {
            if (_puzzleCard == null || _emptySlot == null) {
                return;
            }
            SoundManager.Instance?.PlayTapSoundEffect();
            _puzzleCard.gameObject.SetActive(true);
            _emptySlot.gameObject.SetActive(false);
            _puzzleCard.SetNewPuzzle(puzzle);
        }

        public void SetAsCurrentPlayer(bool current)
        {
            if (_puzzleCard == null || _emptySlot == null) {
                return;
            }
            _puzzleCard.MakeInteractive(current);
        }

        public Vector2 GetPlacementPositionFor(BinaryImage placement)
        {
            if (_puzzleCard != null) {
                return _puzzleCard.GetPlacementCenter(placement);
            }
            return default;
        }

        private void Start()
        {
            if (_puzzleCard == null || _emptySlot == null || _puzzleFrame == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            // make sure that
            _emptySlot.gameObject.SetActive(true);
            _puzzleFrame.gameObject.SetActive(false);
            _puzzleCard.gameObject.SetActive(false);
        }

        #endregion

        public TemporaryPuzzleHighlighter CreateTemporaryPuzzleHighlighter() => new(this);

        public class TemporaryPuzzleHighlighter : IDisposable
        {
            private readonly PlayerRowSlot _slot;
            public TemporaryPuzzleHighlighter(PlayerRowSlot slot)
            {
                SoundManager.Instance?.PlaySoftTapSoundEffect();
                _slot = slot;
                _slot._puzzleFrame!.gameObject.SetActive(true);
            }

            public void Dispose()
            {
                _slot._puzzleFrame!.gameObject.SetActive(false);
            }
        }
    }
}
