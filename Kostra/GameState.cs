using System.Net.Http.Headers;

namespace Kostra {

    class GameStateBuilder {
        private readonly List<Puzzle> _whitePuzzlesDeck = new();
        private readonly List<Puzzle> _blackPuzzlesDeck = new();

        // TODO: sem nahazet vsechny puzzle - asi precist ze souboru
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

    class GameState
    {
        // puzzles in decks and on the game board
        private const int _numPuzzlesInRow = 4;

        private Puzzle?[] _whitePuzzlesRow = new Puzzle?[_numPuzzlesInRow];
        private Puzzle?[] _blackPuzzlesRow = new Puzzle?[_numPuzzlesInRow];

        private readonly Queue<Puzzle> _whitePuzzlesDeck;
        private readonly Queue<Puzzle> _blackPuzzlesDeck;

        // tetrominos in the bank
        private const int _numInitialTetrominos = 15;
        public int[] NumTetrominosLeft { get; init; } = new int[TetrominoManager.NumShapes];

        public GameState(List<Puzzle> whitePuzzlesDeck, List<Puzzle> blackPuzzlesDeck)
        {
            if (whitePuzzlesDeck.Count < _numPuzzlesInRow || blackPuzzlesDeck.Count < _numPuzzlesInRow)
            {
                throw new ArgumentException("Not enough puzzles");
            };
            // create queues
            _whitePuzzlesDeck = new Queue<Puzzle>(whitePuzzlesDeck);
            _blackPuzzlesDeck = new Queue<Puzzle>(blackPuzzlesDeck);

            // reveal the top 4 puzzles
            for (int i = 0; i < _numPuzzlesInRow; i++)
            {
                _whitePuzzlesRow[i] = _whitePuzzlesDeck.Dequeue();
                _blackPuzzlesRow[i] = _blackPuzzlesDeck.Dequeue();
            }

            // initialize tetrominos
            for (int i = 0; i < NumTetrominosLeft.Length; i++)
            {
                NumTetrominosLeft[i] = _numInitialTetrominos;
            }
        }


        // PUZZLES

        public int NumWhitePuzzlesLeft => _whitePuzzlesDeck.Count;
        public int NumBlackPuzzlesLeft => _blackPuzzlesDeck.Count;

        public List<Puzzle> GetAvailableWhitePuzzles()
        {
            var result = new List<Puzzle>();
            for (int i = 0; i < _whitePuzzlesRow.Length; i++)
            {
                if (_whitePuzzlesRow[i] is not null)
                {
                    result.Add(_whitePuzzlesRow[i]!);
                }
            }
            return result;
        }
        public List<Puzzle> GetAvailableBlackPuzzles()
        {
            var result = new List<Puzzle>();
            for (int i = 0; i < _blackPuzzlesRow.Length; i++)
            {
                if (_blackPuzzlesRow[i] is not null)
                {
                    result.Add(_blackPuzzlesRow[i]!);
                }
            }
            return result;
        }
        public Puzzle? TakeTopWhitePuzzle()
        {
            if (_whitePuzzlesDeck.Count == 0)
            {
                return null;
            }
            return _whitePuzzlesDeck.Dequeue();
        }
        public Puzzle? TakeTopBlackPuzzle()
        {
            if (_blackPuzzlesDeck.Count == 0)
            {
                return null;
            }
            return _blackPuzzlesDeck.Dequeue();
        }

        public Puzzle? GetPuzzleWithId(uint id)
        {
            for (int i = 0; i < _whitePuzzlesRow.Length; i++)
            {
                if (_whitePuzzlesRow[i] is not null && _whitePuzzlesRow[i]!.Id == id)
                {
                    return _whitePuzzlesRow[i];
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++)
            {
                if (_blackPuzzlesRow[i] is not null && _blackPuzzlesRow[i]!.Id == id)
                {
                    return _blackPuzzlesRow[i];
                }
            }
            return null;
        }
        public void RemovePuzzleWithId(uint id)
        {
            for (int i = 0; i < _whitePuzzlesRow.Length; i++)
            {
                if (_whitePuzzlesRow[i] is not null && _whitePuzzlesRow[i]!.Id == id)
                {
                    _whitePuzzlesRow[i] = null;
                    return;
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++)
            {
                if (_blackPuzzlesRow[i] is not null && _blackPuzzlesRow[i]!.Id == id)
                {
                    _blackPuzzlesRow[i] = null;
                    return;
                }
            }
        }
        public void RefillPuzzles()
        {
            for (int i = 0; i < _whitePuzzlesRow.Length; i++)
            {
                if (_whitePuzzlesRow[i] is null && _whitePuzzlesDeck.Count > 0)
                {
                    _whitePuzzlesRow[i] = TakeTopWhitePuzzle();
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++)
            {
                if (_blackPuzzlesRow[i] is null && _blackPuzzlesDeck.Count > 0)
                {
                    _blackPuzzlesRow[i] = TakeTopBlackPuzzle();
                }
            }
        }
        public void PutPuzzleToTheBottomOfDeck(Puzzle puzzle)
        {
            if (puzzle.IsBlack)
            {
                _blackPuzzlesDeck.Enqueue(puzzle);
            }
            else
            {
                _whitePuzzlesDeck.Enqueue(puzzle);
            }
        }

        // TETROMINOS

        public int GetNumTetrominosOfType(TetrominoShape shape)
        {
            return NumTetrominosLeft[(int)shape];
        }

        public void RemoveTetromino(TetrominoShape shape)
        {
            if (NumTetrominosLeft[(int)shape] == 0)
            {
                throw new InvalidOperationException($"No tetrominos of type {shape} left");
            }
            NumTetrominosLeft[(int)shape]--;
        }
        public void AddTetromino(TetrominoShape shape)
        {
            if (NumTetrominosLeft[(int)shape] >= _numInitialTetrominos)
            {
                throw new InvalidOperationException($"Too many tetrominos of type {shape}");
            }
            NumTetrominosLeft[(int)shape]++;
        }


        public GameInfo GetGameInfo()
        {
            return new GameInfo(this);
        }

        public class GameInfo(GameState gameState)
        {
            // wrapper around GameState to prevent modification
            public int NumWhitePuzzlesLeft = gameState.NumWhitePuzzlesLeft;
            public int NumBlackPuzzlesLeft = gameState.NumBlackPuzzlesLeft;
            public Puzzle[] AvailableWhitePuzzles = gameState.GetAvailableWhitePuzzles().Select(p => p.Clone()).ToArray();
            public Puzzle[] AvailableBlackPuzzles = gameState.GetAvailableBlackPuzzles().Select(p => p.Clone()).ToArray();
            public IReadOnlyList<int> NumTetrominosLeft = gameState.NumTetrominosLeft.AsReadOnly();
        }
    }
}