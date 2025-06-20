#nullable enable

namespace ProjectL.GameScene.PlayerZone
{
    using ProjectL.Sound;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class PuzzleSlot : MonoBehaviour
    {
        #region Fields

        [Header("Puzzle management")]
        [SerializeField] private InteractivePuzzle? _puzzleCard;

        [SerializeField] private Image? _emptySlot;
        [SerializeField] private Image? _puzzleFrame;

        [SerializeField] private Sprite? _emptySlotSpriteNormal;
        [SerializeField] private Sprite? _emptySlotSpriteHighlighted;

        #endregion

        #region Properties

        public uint? PuzzleId => _puzzleCard != null ? _puzzleCard.PuzzleId : null;

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

            _puzzleCard.FinishPuzzle();
        }

        public void PlacePuzzle(ColorPuzzle puzzle)
        {
            if (_puzzleCard == null || _emptySlot == null) {
                return;
            }
            SoundManager.Instance?.PlayTapSoundEffect();
            _puzzleCard.gameObject.SetActive(true);
            _emptySlot.gameObject.SetActive(false);

            _puzzleCard.SetNewPuzzle(puzzle);
        }

        public void MakePuzzleInteractive(bool current)
        {
            if (_puzzleCard == null || _emptySlot == null) {
                return;
            }
            _puzzleCard.MakeInteractive(current);
        }

        public DisposablePuzzleHighlighter GetDisposablePuzzleHighlighter() => new(this);
        public DisposableEmptySlotHighlighter GetDisposableEmptySlotHighlighter() => new(this);

        private void Start()
        {
            if (_puzzleCard == null || _emptySlot == null || _puzzleFrame == null || _emptySlotSpriteNormal == null || _emptySlotSpriteHighlighted == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            // make sure that
            _puzzleFrame.gameObject.SetActive(false);
            _puzzleCard.gameObject.SetActive(false);   // needs to be in start so that _puzzleCard.Awake runs
            _emptySlot.gameObject.SetActive(true);
            _emptySlot.sprite = _emptySlotSpriteNormal;
        }

        #endregion

        public class DisposableEmptySlotHighlighter : IDisposable
        {
            #region Fields
            private readonly PuzzleSlot _slot;
            #endregion
            #region Constructors
            public DisposableEmptySlotHighlighter(PuzzleSlot slot)
            {
                _slot = slot;
                _slot._emptySlot!.sprite = _slot._emptySlotSpriteHighlighted!;
            }
            #endregion
            #region Methods
            public void Dispose()
            {
                if (_slot._emptySlot != null && _slot._emptySlotSpriteNormal != null) {
                    _slot._emptySlot.sprite = _slot._emptySlotSpriteNormal;
                }
            }
            #endregion
        }

        public class DisposablePuzzleHighlighter : IDisposable
        {
            #region Fields

            private readonly PuzzleSlot _slot;

            #endregion

            #region Constructors

            public DisposablePuzzleHighlighter(PuzzleSlot slot)
            {
                SoundManager.Instance?.PlaySoftTapSoundEffect();
                _slot = slot;
                _slot._puzzleFrame!.gameObject.SetActive(true);
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                SoundManager.Instance?.PlaySoftTapSoundEffect();
                if (_slot._puzzleFrame != null) {
                    _slot._puzzleFrame.gameObject.SetActive(false);
                }
            }

            #endregion
        }
    }
}
