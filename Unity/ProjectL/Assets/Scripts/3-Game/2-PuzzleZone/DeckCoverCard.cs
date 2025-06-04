#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectL.Sound;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class DeckCoverCard : PuzzleZoneCardBase
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI? _label;

        private int _deckSize;

        private Sprite? _dimmedSprite;

        #endregion

        #region Methods


        public void SetDeckSize(int n)
        {
            if (n < 0) {
                throw new ArgumentOutOfRangeException(nameof(n), "Deck size cannot be negative.");
            }
            _deckSize = n;
            if (_label == null) {
                return;
            }
            _label.text = n.ToString();
            if (n == 0) {
                _button!.image.sprite = _dimmedSprite!;
                _button.transition = Selectable.Transition.None;
            }
        }

        public override void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            base.SetMode(mode, turnInfo);
            _button!.interactable = false;

            if (mode == PuzzleZoneMode.TakePuzzle && CanTakePuzzle()) {
                PuzzleZoneManager.AddToRadioButtonGroup(_button);
                _button!.interactable = true;
            }
            else {
                PuzzleZoneManager.RemoveFromRadioButtonGroup(_button);
            }

            bool CanTakePuzzle()
            {
                if (_deckSize == 0)
                    return false;

                if (!_isBlack)
                    return true;

                if (turnInfo.GamePhase == GamePhase.EndOfTheGame && turnInfo.TookBlackPuzzle)
                    return false;

                return true;
            }
        }

        public override PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardHighlighter()
        {
            if (_button == null) {
                return null!;
            }
            Sprite? selectedSprite = null;
            if (_button.transition == Selectable.Transition.SpriteSwap) {
                selectedSprite = _button.spriteState.selectedSprite;
            }
            if (selectedSprite != null) {
                SoundManager.Instance!.PlaySoftTapSoundEffect();
            }
            return new(_button, selectedSprite);
        }

        public override PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardDimmer()
        {
            if (_button == null) {
                return null!;
            }
            return new(_button, _dimmedSprite);
        }

        private void Start()
        {
            if (_button == null || _label == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            _button.onClick.AddListener(SoundManager.Instance!.PlaySoftTapSoundEffect);
            _dimmedSprite = _button.image.sprite;
        }

        private void OnDestroy()
        {
            if (_button != null) {
                PuzzleZoneManager.RemoveFromRadioButtonGroup(_button);
            }
        }

        protected override void InitializeDraggablePuzzle(DraggablePuzzle puzzle)
        {
            if (_deckSize == 0) {
                Debug.LogWarning("Cannot initialize DraggablePuzzle because the deck size is zero.");
                return;
            }

            var action = new TakePuzzleAction(_isBlack ? TakePuzzleAction.Options.TopBlack : TakePuzzleAction.Options.TopWhite);
            puzzle.Init(action, null!);
        }

        protected override IDisposable GetTakePuzzleDisposable() => new TakePuzzleDisposable(this);

        private class TakePuzzleDisposable : IDisposable
        {
            private readonly DeckCoverCard _deckCoverCard;
            public TakePuzzleDisposable(DeckCoverCard deckCoverCard)
            {
                _deckCoverCard = deckCoverCard;
                _deckCoverCard.SetDeckSize(_deckCoverCard._deckSize - 1);
                _deckCoverCard._button!.interactable = false;
            }
            public void Dispose()
            {
                _deckCoverCard.SetDeckSize(_deckCoverCard._deckSize + 1);
                if (_deckCoverCard._button != null) {
                    _deckCoverCard._button.interactable = true;
                }
            }
        }

        #endregion
    }
}
