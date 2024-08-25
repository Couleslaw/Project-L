namespace Kostra {

    class GameStateBuilder {
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
        public Puzzle? GetTopWhitePuzzle();
        public Puzzle? GetTopBlackPuzzle();
        public Puzzle? GetPuzzleWithId(uint id);
    }

    // TODO: mozna radsi GameStateWithGraphics : GameState
    interface IGraphicsComponent {
        public void Draw();
    }

    class GameState : IPuzzleProvider {
        private const int _numBackPuzzlesInRow = 4;
        private const int _numWhitePuzzlesInRow = 4;

        private Puzzle?[] _whitePuzzlesRow;
        private Puzzle?[] _blackPuzzlesRow;

        private readonly List<Puzzle> _whitePuzzlesDeck;
        private readonly List<Puzzle> _blackPuzzlesDeck;

        public GameState(List<Puzzle> whitePuzzlesDeck, List<Puzzle> blackPuzzlesDeck) {
            if (whitePuzzlesDeck.Count < _numWhitePuzzlesInRow || blackPuzzlesDeck.Count < _numBackPuzzlesInRow) {
                throw new ArgumentException("Not enough puzzles");
            }
            _whitePuzzlesDeck = whitePuzzlesDeck;
            _blackPuzzlesDeck = blackPuzzlesDeck;
            _whitePuzzlesRow = _whitePuzzlesDeck.Take(_numWhitePuzzlesInRow).ToArray();
            _blackPuzzlesRow = _blackPuzzlesDeck.Take(_numBackPuzzlesInRow).ToArray();
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
        public Puzzle? GetTopWhitePuzzle() {
            if (_whitePuzzlesDeck.Count == 0) {
                return null;
            }
            return _whitePuzzlesDeck[_whitePuzzlesDeck.Count - 1];
        }
        public Puzzle? GetTopBlackPuzzle() {
            if (_blackPuzzlesDeck.Count == 0) {
                return null;
            }
            return _blackPuzzlesDeck[_blackPuzzlesDeck.Count - 1];
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
        public void RemoveTopWhitePuzzle() {
            if (_whitePuzzlesDeck.Count == 0) {
                throw new InvalidOperationException("No white puzzles left");
            }
            _whitePuzzlesDeck.RemoveAt(_whitePuzzlesDeck.Count - 1);
        }
        public void RemoveTopBlackPuzzle() {
            if (_blackPuzzlesDeck.Count == 0) {
                throw new InvalidOperationException("No black puzzles left");
            }
            _blackPuzzlesDeck.RemoveAt(_blackPuzzlesDeck.Count - 1);
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
                    _whitePuzzlesRow[i] = GetTopWhitePuzzle();
                    RemoveTopWhitePuzzle();
                }
            }
        }
        public void RefillBlackPuzzles() {
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is null && _blackPuzzlesDeck.Count > 0) {
                    _blackPuzzlesRow[i] = GetTopBlackPuzzle();
                    RemoveTopBlackPuzzle();
                }
            }
        }
        public void RecycleWhitePuzzles() {
            // Remove all white puzzles and put them at the bottom of the deck in random order
            var recycledPuzzles = new List<Puzzle>();
            for (int i = 0; i < _whitePuzzlesRow.Length; i++) {
                if (_whitePuzzlesRow[i] is not null) {
                    recycledPuzzles.Add(_whitePuzzlesRow[i]!);
                    _whitePuzzlesRow[i] = null;
                }
            }
            recycledPuzzles.Shuffle();
            _whitePuzzlesDeck.InsertRange(0, recycledPuzzles);
            // Refill the white puzzles
            RefillWhitePuzzles();
        }
        public void RecycleBlackPuzzles() {
            // Remove all black puzzles and put them at the bottom of the deck in random order
            var recycledPuzzles = new List<Puzzle>();
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is not null) {
                    recycledPuzzles.Add(_blackPuzzlesRow[i]!);
                    _blackPuzzlesRow[i] = null;
                }
            }
            recycledPuzzles.Shuffle();
            _blackPuzzlesDeck.InsertRange(0, recycledPuzzles);
            // Refill the black puzzles
            RefillBlackPuzzles();
        }


        public class Graphics(GameState gameState) : IGraphicsComponent {
            private readonly GameState _gameState = gameState;

            public void Draw() {
                // draw gameState._whitePuzzlesRow and gameState._blackPuzzlesRow
            }
        }
    }
}