#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectL.Data;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class PuzzleCard : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Sprite? _emptyCardImage;

        [SerializeField] private Button? _button;

        private Puzzle? _puzzle = null;

        private Mode _mode = Mode.Normal;

        private bool _isRecycleSelected = false;

        #endregion

        public enum Mode
        {
            Normal,
            Recycle
        }

        #region Properties

        public Button? Button => _button;

        public Action<Puzzle>? OnRecyclePuzzleSelect { get; set; }

        public Action<Puzzle>? OnRecyclePuzzleUnselect { get; set; }

        public bool IsEmpty => _puzzle == null;

        #endregion

        #region Methods

        public void SetPuzzle(Puzzle? puzzle)
        {
            if (puzzle == null) {
                DisableCard();
            }
            _puzzle = puzzle;
            UpdateUI();
        }

        public void SetMode(Mode mode)
        {
            _mode = mode;
            if (mode == Mode.Recycle) {
                _isRecycleSelected = false;
            }
            UpdateUI();
        }

        public void DisableCard()
        {
            if (_button != null) {
                _button.interactable = false;
            }
        }

        public void EnableCard()
        {
            if (_button != null) {
                _button.interactable = true;
            }
        }

        private void Awake()
        {
            if (_button == null) {
                Debug.LogError("Button component is missing", this);
                return;
            }

            _button.onClick.AddListener(OnRecycleButtonClick);
            DisableCard();
            UpdateUI();
        }

        private void OnRecycleButtonClick()
        {
            if (_mode != Mode.Recycle) {
                return;
            }
            if (_puzzle == null) {
                Debug.LogError("Trying to recycle an empty puzzle", this);
                return;
            }

            _isRecycleSelected = !_isRecycleSelected;
            UpdateUI();
            if (_isRecycleSelected) {
                OnRecyclePuzzleSelect?.Invoke(_puzzle);
            }
            else {
                OnRecyclePuzzleUnselect?.Invoke(_puzzle);
            }
        }

        private void UpdateUI()
        {
            if (_button == null || _emptyCardImage == null) {
                return;
            }

            // empty card
            if (_puzzle == null) {
                _button.image.type = Image.Type.Sliced;
                _button.image.sprite = _emptyCardImage;
                _button.transition = Selectable.Transition.None;
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
            if (_mode == Mode.Recycle && _isRecycleSelected) {
                _button.image.sprite = borderBright;
            }
            else {
                _button.image.sprite = borderDim;
            }

            _button.transition = Selectable.Transition.SpriteSwap;
            _button.spriteState = _mode switch {
                Mode.Normal => new SpriteState {
                    highlightedSprite = highlighted,
                    pressedSprite = borderBright,
                    selectedSprite = borderBright,
                    disabledSprite = borderBright
                },
                Mode.Recycle => new SpriteState {
                    highlightedSprite = _isRecycleSelected ? borderBright : highlighted,
                    pressedSprite = borderBright,
                    selectedSprite = borderBright,
                    disabledSprite = borderDim
                },
                _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, null)
            };
        }

        #endregion
    }
}
