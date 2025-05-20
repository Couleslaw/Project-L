#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectLCore.GameLogic;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class PuzzlesColumn : MonoBehaviour
    {
        #region Fields

        private readonly PuzzleCard[] _puzzleCards = new PuzzleCard[GameState.NumPuzzlesInRow];

        [SerializeField] private PuzzleCard? puzzleCardPrefab;

        [SerializeField] private DeckCoverCard? _deckCoverCard;

        #endregion

        #region Properties

        public DeckCoverCard DeckCard => _deckCoverCard!;

        #endregion

        public PuzzleCard? this[int index] {
            get {
                if (index < 0 || index >= _puzzleCards.Length) {
                    throw new IndexOutOfRangeException($"Index {index} is out of range.");
                }

                return _puzzleCards[index];
            }
        }

        #region Methods

        public bool TryGetPuzzleCardWithId(uint puzzleId, out PuzzleCard? puzzleCard)
        {
            puzzleCard = null;
            foreach (var card in _puzzleCards) {
                if (card.PuzzleId == puzzleId) {
                    puzzleCard = card;
                    return true;
                }
            }
            return false;
        }

        public void Init(bool isBlack)
        {
            _deckCoverCard?.Init(isBlack);
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.Init(isBlack);
            }
        }

        public void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            _deckCoverCard?.SetMode(mode, turnInfo);
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.SetMode(mode, turnInfo);
            }
        }

        public void RemoveFromRecycle()
        {
            foreach (PuzzleCard puzzleCard in _puzzleCards) {
                puzzleCard.RemoveFromRecycle();
            }
        }

        public TemporaryColumnDimmer CreateColumnDimmer(bool shouldDimCoverCard = true) => new(this, shouldDimCoverCard);

        public TemporaryPuzzleHighlighter CreatePuzzleHighlighter(List<uint> puzzleIds) => new(this, puzzleIds);

        private void Awake()
        {
            if (puzzleCardPrefab == null) {
                Debug.LogError("PuzzleCard prefab is not assigned!", this);
                return;
            }
            if (_deckCoverCard == null) {
                Debug.LogError("Deck cover card is missing!", this);
                return;
            }

            for (int i = 0; i < GameState.NumPuzzlesInRow; i++) {
                _puzzleCards[i] = Instantiate(puzzleCardPrefab, transform);
                _puzzleCards[i].gameObject.SetActive(true);
                _puzzleCards[i].gameObject.name = $"PuzzleCard_{i + 1}";
                _puzzleCards[i].SetPuzzle(null);
            }
        }

        #endregion

        public class TemporaryColumnDimmer : IDisposable
        {
            #region Fields

            private List<PuzzleZoneManager.TemporarySpriteReplacer> _dimmers = new();

            #endregion

            #region Constructors

            public TemporaryColumnDimmer(PuzzlesColumn column, bool shouldDimCoverCard)
            {
                foreach (var puzzle in column._puzzleCards) {
                    _dimmers.Add(puzzle.CreateCardDimmer());
                }
                if (shouldDimCoverCard) {
                    _dimmers.Add(column.DeckCard.CreateCardDimmer());
                }
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                foreach (var dimmer in _dimmers) {
                    dimmer.Dispose();
                }
            }

            #endregion
        }

        public class TemporaryPuzzleHighlighter : IDisposable
        {
            #region Fields

            private List<PuzzleZoneManager.TemporarySpriteReplacer> _highlighters = new();

            #endregion

            #region Constructors

            public TemporaryPuzzleHighlighter(PuzzlesColumn column, List<uint> puzzleIds)
            {

                foreach (var puzzleId in puzzleIds) {
                    if (column.TryGetPuzzleCardWithId(puzzleId, out var puzzleCard)) {
                        _highlighters.Add(puzzleCard!.CreateCardHighlighter());
                    }
                }
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                foreach (var highlighter in _highlighters) {
                    highlighter.Dispose();
                }
            }

            #endregion
        }
    }
}
