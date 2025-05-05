#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using UnityEngine;

    public class PuzzleZoneManager : MonoBehaviour
    {
        [Header("Puzzle columns")]
        [SerializeField] private PuzzleColumn? _whitePuzzleColumn;
        [SerializeField] private PuzzleColumn? _blackPuzzleColumn;

        [Header("Deck cover cards")]
        [SerializeField] private DeckCoverCard? _whiteDeckCoverCard;
        [SerializeField] private DeckCoverCard? _blackDeckCoverCard;

        public void ListenTo(GameState gameState)
        {
            gameState.OnWhitePuzzleRowChanged += OnWhitePuzzleRowChanged;
            gameState.OnBlackPuzzleRowChanged += OnBlackPuzzleRowChanged;
            gameState.OnWhitePuzzleDeckChanged += OnWhitePuzzleDeckChanged;
            gameState.OnBlackPuzzleDeckChanged += OnBlackPuzzleDeckChanged;
        }

        private void Awake()
        {
            if (_whitePuzzleColumn == null || _blackPuzzleColumn == null ||
                _whiteDeckCoverCard == null || _blackDeckCoverCard == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
        }

        private void OnWhitePuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_whitePuzzleColumn != null) {
                _whitePuzzleColumn[index] = puzzle;
            }
        }

        private void OnBlackPuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_blackPuzzleColumn != null) {
                _blackPuzzleColumn[index] = puzzle;
            }
        }

        private void OnBlackPuzzleDeckChanged(int deckSize)
        {
            _blackDeckCoverCard?.SetDeckSize(deckSize);
        }

        private void OnWhitePuzzleDeckChanged(int deckSize)
        {
            _whiteDeckCoverCard?.SetDeckSize(deckSize);
        }
    }
}
