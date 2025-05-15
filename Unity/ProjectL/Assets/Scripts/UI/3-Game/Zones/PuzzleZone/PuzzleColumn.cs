#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectLCore.GameLogic;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class PuzzleColumn : MonoBehaviour
    {

        [SerializeField] private PuzzleCard? puzzleCardPrefab;
        [SerializeField] private DeckCoverCard? _deckCoverCard;

        private readonly PuzzleCard[] _puzzleCards = new PuzzleCard[GameState.NumPuzzlesInRow];

        public PuzzleCard? this[int index] {
            get {
                if (index < 0 || index >= _puzzleCards.Length) {
                    throw new IndexOutOfRangeException($"Index {index} is out of range.");
                }

                return _puzzleCards[index];
            }
        }

        public DeckCoverCard DeckCard => _deckCoverCard!;

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

        public TemporaryColumnDimmer CreateColumnDimmer(bool shouldDimCoverCard = true) => new(this, shouldDimCoverCard);

        public TemporaryPuzzleHighlighter CreatePuzzleHighlighter(List<uint> puzzleIds) => new(this, puzzleIds);

        public class TemporaryColumnDimmer : IDisposable
        {
            List<PuzzleZoneManager.TemporarySpriteReplacer> _dimmers = new();

            public TemporaryColumnDimmer(PuzzleColumn column, bool shouldDimCoverCard)
            {
                foreach (var puzzle in column._puzzleCards) {
                    _dimmers.Add(puzzle.CreateCardDimmer());
                }
                if (shouldDimCoverCard) {
                    _dimmers.Add(column.DeckCard.CreateCardDimmer());
                }
            }
            public void Dispose()
            {
                foreach (var dimmer in _dimmers) {
                    dimmer.Dispose();
                }
            }
        }

        public class TemporaryPuzzleHighlighter : IDisposable
        {
            List<PuzzleZoneManager.TemporarySpriteReplacer> _highlighters = new();

            public TemporaryPuzzleHighlighter(PuzzleColumn column, List<uint> puzzleIds)
            {

                foreach (var puzzleId in puzzleIds) {
                    if (column.TryGetPuzzleCardWithId(puzzleId, out var puzzleCard)) {
                        _highlighters.Add(puzzleCard!.CreateCardHighlighter());
                    }
                }
            }

            public void Dispose()
            {
                foreach (var highlighter in _highlighters) {
                    highlighter.Dispose();
                }
            }
        }
    }
}
