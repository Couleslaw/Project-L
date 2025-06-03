#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectL.Data;
    using ProjectL.Sound;
    using ProjectL.Utils;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class PuzzleCard : MonoBehaviour, IPuzzleZoneCard
    {
        #region Fields

        [SerializeField] private Sprite? _emptyCardImage;

        [SerializeField] private Button? _button;

        private Puzzle? _puzzle = null;

        private PuzzleZoneMode _mode = PuzzleZoneMode.Disabled;

        private bool _isRecycleSelected = false;

        private bool _isBlack;

        #endregion

        #region Properties

        public uint? PuzzleId => _puzzle?.Id;

        #endregion

        #region Methods

        public void SetPuzzle(Puzzle? puzzle)
        {
            _puzzle = puzzle;
            UpdateUI();
        }

        public void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            _mode = mode;
            _button!.interactable = false;

            if (mode == PuzzleZoneMode.TakePuzzle && CanTakePuzzle()) {
                void onCancel() => PuzzleZoneManager.Instance.ReportTakePuzzleChange(new(null));
                void onSelect()
                {
                    var action = new TakePuzzleAction(TakePuzzleAction.Options.Normal, _puzzle!.Id);
                    PuzzleZoneManager.Instance.ReportTakePuzzleChange(new(action));
                }
                PuzzleZoneManager.AddToRadioButtonGroup(_button, onSelect, onCancel);
                _button!.interactable = true;
            }
            else {
                PuzzleZoneManager.RemoveFromRadioButtonGroup(_button);
            }

            if (mode == PuzzleZoneMode.Recycle && CanRecycle()) {
                _isRecycleSelected = false;
                _button.interactable = true;
            }
            UpdateUI();

            bool CanRecycle() => _puzzle != null;

            bool CanTakePuzzle()
            {
                if (_puzzle == null)
                    return false;

                if (!_isBlack)
                    return true;

                if (turnInfo.GamePhase == GamePhase.EndOfTheGame && turnInfo.TookBlackPuzzle)
                    return false;

                return true;
            }
        }

        public void Init(bool isBlack)
        {
            _isBlack = isBlack;
        }

        public PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardHighlighter()
        {
            if (_button == null) {
                return null!;
            }
            Sprite? selectedSprite = null;
            if (_puzzle != null) {
                ResourcesLoader.TryGetPuzzleSprite(_puzzle, PuzzleSpriteType.BorderBright, out selectedSprite);
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
            Sprite? dimmedSprite = null;
            if (_puzzle != null) {
                ResourcesLoader.TryGetPuzzleSprite(_puzzle, PuzzleSpriteType.BorderDim, out dimmedSprite);
            }
            return new(_button, dimmedSprite);
        }

        public void RemoveFromRecycle()
        {
            if (_puzzle == null || _mode != PuzzleZoneMode.Recycle || !_isRecycleSelected) {
                return;
            }

            _isRecycleSelected = false;
            UpdateUI();
            PuzzleZoneManager.Instance.ReportRecycleChange(new(_puzzle, isSelected: false));
        }

        private void Start()
        {
            if (_button == null || _emptyCardImage == null) {
                Debug.LogError("One or more UI components are missing", this);
                return;
            }

            _button.onClick.AddListener(SoundManager.Instance!.PlaySoftTapSoundEffect);

            _button.onClick.AddListener(OnRecycleButtonClick);
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (_button != null) {
                PuzzleZoneManager.RemoveFromRadioButtonGroup(_button);
            }
        }

        private void OnRecycleButtonClick()
        {
            if (_mode != PuzzleZoneMode.Recycle) {
                return;
            }
            if (_puzzle == null) {
                Debug.LogError("Trying to recycle an empty puzzle", this);
                return;
            }

            _isRecycleSelected = !_isRecycleSelected;
            if (!_isRecycleSelected) {
                EventSystem.current.SetSelectedGameObject(null!);
            }

            UpdateUI();
            PuzzleZoneManager.Instance.ReportRecycleChange(new(_puzzle, _isRecycleSelected));
        }

        private void UpdateUI()
        {
            if (_button == null || _emptyCardImage == null) {
                return;
            }

            // empty card
            if (_puzzle == null) {
                _button.image.type = Image.Type.Sliced;
                _button.spriteState = new SpriteState {
                    highlightedSprite = _emptyCardImage,
                    pressedSprite = _emptyCardImage,
                    selectedSprite = _emptyCardImage,
                    disabledSprite = _emptyCardImage
                };
                return;
            }

            // we have a puzzle --> load the sprite
            ResourcesLoader.TryGetPuzzleSprite(_puzzle, PuzzleSpriteType.BorderDim, out Sprite? borderDim);
            ResourcesLoader.TryGetPuzzleSprite(_puzzle, PuzzleSpriteType.BorderBright, out Sprite? borderBright);
            ResourcesLoader.TryGetPuzzleSprite(_puzzle, PuzzleSpriteType.Highlighted, out Sprite? highlighted);

            if (borderDim == null || borderBright == null || highlighted == null) {
                Debug.LogError($"Failed to load puzzle sprite for puzzle: {_puzzle}", this);
                return;
            }

            // set the sprite and transition
            _button.image.type = Image.Type.Simple;
            if (_mode == PuzzleZoneMode.Recycle && _isRecycleSelected) {
                _button.image.sprite = borderBright;
            }
            else {
                _button.image.sprite = borderDim;
            }

            _button.transition = Selectable.Transition.SpriteSwap;
            SpriteState newSpriteState = _mode switch {
                PuzzleZoneMode.TakePuzzle => new SpriteState {
                    highlightedSprite = highlighted,
                    pressedSprite = borderBright,
                    selectedSprite = borderBright,
                    disabledSprite = borderBright
                },
                PuzzleZoneMode.Recycle => new SpriteState {
                    highlightedSprite = _isRecycleSelected ? borderBright : highlighted,
                    pressedSprite = borderBright,
                    selectedSprite = borderBright,
                    disabledSprite = borderDim
                },
                _ => new SpriteState {
                    highlightedSprite = borderBright,
                    pressedSprite = borderBright,
                    selectedSprite = borderBright,
                    disabledSprite = borderBright
                },
            };

            // if taking new puzzle --> update the radio button group sprites
            if (_mode == PuzzleZoneMode.TakePuzzle && _button.interactable) {
                RadioButtonsGroup.UpdateSpritesForButton(_button, newSpriteState);
            }
            // else update sprite state directly
            else {
                _button.spriteState = newSpriteState;
            }
        }

        #endregion
    }
}
