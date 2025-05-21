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

    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class DeckCoverCard : MonoBehaviour, IPuzzleZoneCard
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI? _label;

        private Button? _button;

        private int _deckSize;

        private bool _isBlack;

        private Sprite? _dimmedSprite;

        #endregion

        #region Methods

        public void Init(bool isBlack)
        {
            _isBlack = isBlack;
        }

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

        public void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            _button!.interactable = false;

            if (mode == PuzzleZoneMode.TakePuzzle && CanTakePuzzle()) {
                void onSelect()
                {
                    TakePuzzleAction.Options option = _isBlack ? TakePuzzleAction.Options.TopBlack : TakePuzzleAction.Options.TopWhite;
                    var action = new TakePuzzleAction(option);
                    PuzzleZoneManager.Instance!.ReportTakePuzzleChange(new(action));
                }
                void onCancel() => PuzzleZoneManager.Instance!.ReportTakePuzzleChange(new(null));
                PuzzleZoneManager.AddToRadioButtonGroup(_button, onSelect, onCancel);
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

        public PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardHighlighter()
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

        public PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardDimmer()
        {
            if (_button == null) {
                return null!;
            }
            return new(_button, _dimmedSprite);
        }

        private void Start()
        {
            _button = GetComponent<Button>();
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

        #endregion
    }
}
