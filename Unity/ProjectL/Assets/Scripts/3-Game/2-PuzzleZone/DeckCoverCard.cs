#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectL.Sound;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class DeckCoverCard : PuzzleZoneCardBase
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI? _label;

        [SerializeField] private Sprite? _dimmedSprite;

        private int _deckSize;

        private Sprite? _nonEmptyDeckSprite;

        private SpriteState _nonEmptyDeckSpriteState;

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
                SetEmptyDeckSprites();
            }
            else {
                SetNonEmptyDeckSprites();
            }
        }

        public override void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            if (_button == null) {
                Debug.LogError("Button component is not assigned!", this);
                return;
            }

            base.SetMode(mode, turnInfo);

            _button.interactable = mode == PuzzleZoneMode.TakePuzzle && CanTakePuzzle;
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
            return new(_button, _dimmedSprite!);
        }

        protected override bool GetCanTakePuzzle(TurnInfo turnInfo)
        {
            if (_deckSize == 0)
                return false;

            if (!_isBlack)
                return true;

            if (turnInfo.GamePhase == GamePhase.EndOfTheGame && turnInfo.TookBlackPuzzle)
                return false;

            return true;
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

        private void Start()
        {
            if (_button == null || _label == null || _dimmedSprite == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }

            if (_button.transition != Selectable.Transition.SpriteSwap) {
                Debug.LogError("Button transition must be set to SpriteSwap!", this);
                return;
            }

            _nonEmptyDeckSprite = _button.image.sprite;
            _nonEmptyDeckSpriteState = _button.spriteState;
        }

        private void SetEmptyDeckSprites()
        {
            if (_button == null || _dimmedSprite == null) {
                Debug.LogError("Button or dimmed sprite is not assigned!", this);
                return;
            }
            _button.image.sprite = _dimmedSprite;
            _button.spriteState = new SpriteState {
                highlightedSprite = _dimmedSprite,
                pressedSprite = _dimmedSprite,
                selectedSprite = _dimmedSprite,
                disabledSprite = _dimmedSprite
            };
        }

        private void SetNonEmptyDeckSprites()
        {
            if (_button == null || _nonEmptyDeckSprite == null) {
                Debug.LogError("Button or non-empty deck sprite is not assigned!", this);
                return;
            }
            _button.image.sprite = _nonEmptyDeckSprite;
            _button.spriteState = _nonEmptyDeckSpriteState;
        }

        private void OnDestroy()
        {
            if (_button != null) {
                PuzzleZoneManager.RemoveFromRadioButtonGroup(_button);
            }
        }

        #endregion

        private class TakePuzzleDisposable : IDisposable
        {
            #region Fields

            private readonly DeckCoverCard _deckCoverCard;

            #endregion

            #region Constructors

            public TakePuzzleDisposable(DeckCoverCard deckCoverCard)
            {
                _deckCoverCard = deckCoverCard;
                _deckCoverCard.SetDeckSize(_deckCoverCard._deckSize - 1);
                _deckCoverCard.SetEmptyDeckSprites();
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                _deckCoverCard.SetDeckSize(_deckCoverCard._deckSize + 1);
                _deckCoverCard.SetNonEmptyDeckSprites();
            }

            #endregion
        }
    }
}
