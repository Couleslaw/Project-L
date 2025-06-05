#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectL.Data;
    using ProjectL.Sound;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class PuzzleCard : PuzzleZoneCardBase
    {
        #region Fields

        [SerializeField] private Sprite? _emptyCardImage;

        private Puzzle? _puzzle = null;

        private bool _isRecycleSelected = false;


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

        public override void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            base.SetMode(mode, turnInfo);

            _button!.interactable = false;

            if (mode == PuzzleZoneMode.TakePuzzle && CanTakePuzzle) {
                _button.interactable = true;
            }

            if (mode == PuzzleZoneMode.Recycle && CanRecycle()) {
                _isRecycleSelected = false;
                _button.interactable = true;
            }
            UpdateUI();

            bool CanRecycle() => _puzzle != null;
        }

        protected override bool GetCanTakePuzzle(TurnInfo turnInfo)
        {
            if (_puzzle == null)
                return false;

            if (!_isBlack)
                return true;

            if (turnInfo.GamePhase == GamePhase.EndOfTheGame && turnInfo.TookBlackPuzzle)
                return false;

            return true;
        }


        public override PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardHighlighter()
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

        public override PuzzleZoneManager.DisposableSpriteReplacer GetDisposableCardDimmer()
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

        private void SetEmptySlot()
        {
            if (_button == null || _emptyCardImage == null) {
                return;
            }
            _button.image.type = Image.Type.Sliced;
            _button.image.sprite = _emptyCardImage;
            _button.spriteState = new SpriteState {
                highlightedSprite = _emptyCardImage,
                pressedSprite = _emptyCardImage,
                selectedSprite = _emptyCardImage,
                disabledSprite = _emptyCardImage
            };
        }

        private void UpdateUI()
        {
            if (_button == null) {
                return;
            }

            // empty card
            if (_puzzle == null) {
                SetEmptySlot();
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
            if (_mode == PuzzleZoneMode.Recycle && !_isRecycleSelected) {
                _button.image.sprite = borderDim;
            }
            else {
                _button.image.sprite = borderBright;
            }

            _button.transition = Selectable.Transition.SpriteSwap;
            SpriteState newSpriteState = _mode switch {
                PuzzleZoneMode.TakePuzzle => new SpriteState {
                    highlightedSprite = highlighted,
                    pressedSprite = borderBright,
                    selectedSprite = borderBright,
                    disabledSprite = borderDim
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

            _button.spriteState = newSpriteState;
        }

        protected override void InitializeDraggablePuzzle(DraggablePuzzle puzzle)
        {
            if (_puzzle == null) {
                Debug.LogError("Cannot initialize draggable puzzle with null puzzle", this);
                return;
            }

            var action = new TakePuzzleAction(TakePuzzleAction.Options.Normal, PuzzleId);
            puzzle.Init(action, _puzzle);
        }

        protected override IDisposable GetTakePuzzleDisposable() => new TakePuzzleDisposable(this);

        private class TakePuzzleDisposable : IDisposable
        {
            private readonly PuzzleCard _puzzleCard;
            public TakePuzzleDisposable(PuzzleCard puzzleCard)
            {
                _puzzleCard = puzzleCard;
                _puzzleCard.SetEmptySlot();

            }
            public void Dispose()
            {
                if (_puzzleCard._button != null) {
                    _puzzleCard.UpdateUI();
                }
            }
        }

        #endregion
    }
}
