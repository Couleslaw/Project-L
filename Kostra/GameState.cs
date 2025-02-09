using System.Net.Http.Headers;

namespace Kostra {

    class GameStateBuilder {
        // TODO: sem nahazet vsechny puzzle - asi precist ze souboru
        private readonly List<Puzzle> _whitePuzzlesDeck = new();
        private readonly List<Puzzle> _blackPuzzlesDeck = new();

        public GameStateBuilder AddWhitePuzzle(Puzzle puzzle) {
            _whitePuzzlesDeck.Add(puzzle);
            return this;
        }
        public GameStateBuilder AddBlackPuzzle(Puzzle puzzle) {
            _blackPuzzlesDeck.Add(puzzle);
            return this;
        }
        public GameState Build() {
            _whitePuzzlesDeck.Shuffle();
            _blackPuzzlesDeck.Shuffle();
            return new GameState(_whitePuzzlesDeck, _blackPuzzlesDeck);
        }
    }

    interface IPuzzleProvider {
        public Puzzle? TakeTopWhitePuzzle();
        public Puzzle? TakeTopBlackPuzzle();
        public Puzzle? GetPuzzleWithId(uint id);
    }

    // TODO: mozna radsi GameStateWithGraphics : GameState
    interface IGraphicsComponent {
        public void Draw();
    }

    class GameState : IPuzzleProvider {
        private const int _numPuzzlesInRow = 4;

        private Puzzle?[] _whitePuzzlesRow = new Puzzle?[_numPuzzlesInRow];
        private Puzzle?[] _blackPuzzlesRow = new Puzzle?[_numPuzzlesInRow];

        private readonly Queue<Puzzle> _whitePuzzlesDeck;
        private readonly Queue<Puzzle> _blackPuzzlesDeck;

        public GameState(List<Puzzle> whitePuzzlesDeck, List<Puzzle> blackPuzzlesDeck) {
            if (whitePuzzlesDeck.Count < _numPuzzlesInRow|| blackPuzzlesDeck.Count < _numPuzzlesInRow) {
                throw new ArgumentException("Not enough puzzles");
            }

            // shuffle decks
            whitePuzzlesDeck.Shuffle();
            blackPuzzlesDeck.Shuffle();

            // create queues
            _whitePuzzlesDeck = new Queue<Puzzle>(whitePuzzlesDeck);
            _blackPuzzlesDeck = new Queue<Puzzle>(blackPuzzlesDeck);

            // reveal the top 4 puzzles
            for (int i = 0; i < _numPuzzlesInRow; i++)
            {
                _whitePuzzlesRow[i] = _whitePuzzlesDeck.Dequeue();
                _blackPuzzlesRow[i] = _blackPuzzlesDeck.Dequeue();
            }
        }

        public int NumWhitePuzzlesLeft => _whitePuzzlesDeck.Count;
        public int NumBlackPuzzlesLeft => _blackPuzzlesDeck.Count;

        public List<Puzzle> GetAvailableWhitePuzzles() {
            var result = new List<Puzzle>();
            for (int i = 0; i < _whitePuzzlesRow.Length; i++) {
                if (_whitePuzzlesRow[i] is not null) {
                    result.Add(_whitePuzzlesRow[i]!);
                }
            }
            return result;
        }
        public List<Puzzle> GetAvailableBlackPuzzles() {
            var result = new List<Puzzle>();
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is not null) {
                    result.Add(_blackPuzzlesRow[i]!);
                }
            }
            return result;
        }
        public Puzzle? TakeTopWhitePuzzle() {
            if (_whitePuzzlesDeck.Count == 0) {
                return null;
            }
            return _whitePuzzlesDeck.Dequeue();
        }
        public Puzzle? TakeTopBlackPuzzle() {
            if (_blackPuzzlesDeck.Count == 0) {
                return null;
            }
            return _blackPuzzlesDeck.Dequeue();
        }

        public Puzzle? GetPuzzleWithId(uint id) {
            for (int i = 0; i < _whitePuzzlesRow.Length; i++) {
                if (_whitePuzzlesRow[i] is not null && _whitePuzzlesRow[i]!.Id == id) {
                    return _whitePuzzlesRow[i];
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is not null && _blackPuzzlesRow[i]!.Id == id) {
                    return _blackPuzzlesRow[i];
                }
            }
            return null;
        }
        public void RemovePuzzleWithId(uint id) {
            for (int i = 0; i < _whitePuzzlesRow.Length; i++) {
                if (_whitePuzzlesRow[i] is not null && _whitePuzzlesRow[i]!.Id == id) {
                    _whitePuzzlesRow[i] = null;
                    return;
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is not null && _blackPuzzlesRow[i]!.Id == id) {
                    _blackPuzzlesRow[i] = null;
                    return;
                }
            }
        }
        public void RefillWhitePuzzles() {
            for (int i = 0; i < _whitePuzzlesRow.Length; i++) {
                if (_whitePuzzlesRow[i] is null && _whitePuzzlesDeck.Count > 0) {
                    _whitePuzzlesRow[i] = TakeTopWhitePuzzle();
                }
            }
        }
        public void RefillBlackPuzzles() {
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is null && _blackPuzzlesDeck.Count > 0) {
                    _blackPuzzlesRow[i] = TakeTopBlackPuzzle();
                }
            }
        }

        public void EnqueuePuzzle(Puzzle puzzle) {
            if (puzzle.IsBlack)
            {
                _blackPuzzlesDeck.Enqueue(puzzle);
            }
            else { 
                _whitePuzzlesDeck.Enqueue(puzzle);
            }
        }


        public class Graphics(GameState gameState) : IGraphicsComponent {
            private readonly GameState _gameState = gameState;

            public void Draw() {
                // draw gameState._whitePuzzlesRow and gameState._blackPuzzlesRow
            }
        }
    }
}