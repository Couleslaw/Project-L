#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectL.UI.Utils;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;

    public class PuzzleColumn : MonoBehaviour
    {
        #region Fields

        [SerializeField] private PuzzleCard? puzzleCardPrefab;

        private PuzzleCard[] _puzzleCards = new PuzzleCard[GameState.NumPuzzlesInRow];

        #endregion

        #region Methods

        public Puzzle? this[int index] {
            set {
                if (index < 0 || index >= _puzzleCards.Length) {
                    throw new IndexOutOfRangeException($"Index {index} is out of range.");
                }

                _puzzleCards[index].SetPuzzle(value);
            }
        }

        public void Initialize(Action<Puzzle> onRecyclePuzzleSelect, Action<Puzzle> onRecyclePuzzleUnselect)
        {
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.OnRecyclePuzzleSelect = onRecyclePuzzleSelect;
                puzzleCard.OnRecyclePuzzleUnselect = onRecyclePuzzleUnselect;
            }
        }

        public void DisableColumn()
        {
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.DisableCard();
            }
        }

        public void EnableColumn()
        {
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.EnableCard();
            }
        }

        public void SetRecycleMode()
        {
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.SetMode(PuzzleCard.Mode.Recycle);
            }
        }

        public void SetNormalMode()
        {
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.SetMode(PuzzleCard.Mode.Normal);
            }
        }

        public void AddToRadioButtonGroup(string groupName, Action? onSelect = null, Action? onCancel = null)
        {
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                if (puzzleCard.Button != null) {
                    RadioButtonsGroup.RegisterButton(puzzleCard.Button, groupName, onSelect, onCancel);
                }
            }
        }

        private void Awake()
        {
            if (puzzleCardPrefab == null) {
                Debug.LogError("PuzzleCard prefab is not assigned!", this);
                return;
            }

            for (int i = 0; i < GameState.NumPuzzlesInRow; i++) {
                _puzzleCards[i] = Instantiate(puzzleCardPrefab, transform);
                _puzzleCards[i].gameObject.SetActive(true);
                _puzzleCards[i].gameObject.name = $"PuzzleCard_{i + 1}";
                _puzzleCards[i].SetMode(PuzzleCard.Mode.Normal);
                _puzzleCards[i].SetPuzzle(null);
            }
        }

        #endregion
    }
}
