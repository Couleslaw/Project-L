#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using UnityEngine;

    public class PuzzleZoneManager : GameZoneManager<PuzzleZoneManager>, IGameStatePuzzleListener, ICurrentTurnListener
    {
        [Header("Puzzle columns")]
        [SerializeField] private PuzzleColumn? _whitePuzzleColumn;
        [SerializeField] private PuzzleColumn? _blackPuzzleColumn;

        [Header("Deck cover cards")]
        [SerializeField] private DeckCoverCard? _whiteDeckCoverCard;
        [SerializeField] private DeckCoverCard? _blackDeckCoverCard;

        public override void Init(GameCore game)
        {
            game.AddListener(this);
            game.GameState.AddListener(this);
        }

        protected override void Awake()
        {
            base.Awake();
            if (_whitePuzzleColumn == null || _blackPuzzleColumn == null ||
                _whiteDeckCoverCard == null || _blackDeckCoverCard == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
        }

        public void OnWhitePuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_whitePuzzleColumn != null) {
                _whitePuzzleColumn[index] = puzzle;
            }
        }

        public void OnBlackPuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_blackPuzzleColumn != null) {
                _blackPuzzleColumn[index] = puzzle;
            }
        }

        public void OnBlackPuzzleDeckChanged(int deckSize)
        {
            _blackDeckCoverCard?.SetDeckSize(deckSize);
            if (deckSize == 0) {
                _blackDeckCoverCard?.DisableCard();
            }
        }

        public void OnWhitePuzzleDeckChanged(int deckSize)
        {
            _whiteDeckCoverCard?.SetDeckSize(deckSize);
            if (deckSize == 0) {
                _whiteDeckCoverCard?.DisableCard();
            }
        }

        public void OnCurrentTurnChanged(TurnInfo info)
        {
            if (info.GamePhase == GamePhase.EndOfTheGame && info.TookBlackPuzzle) {
                _blackPuzzleColumn?.DisableColumn();
                _blackDeckCoverCard?.DisableCard();
            }
            else {
                _blackPuzzleColumn?.EnableColumn();
                _blackDeckCoverCard?.EnableCard();
            }
        }
    }
}
