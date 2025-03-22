using Kostra.GameManagers;
using Kostra.GamePieces;
using System.Net.Http.Headers;

namespace Kostra.GameLogic {

    /// <summary>
    /// Builder for the <see cref="GameState"/> class.
    /// </summary>
    /// <param name="numInitialTetrominos">The amount of tetrominos of each shape in the shared reserve at the beginning of the game.</param>
    class GameStateBuilder(int numInitialTetrominos)
    {
        private readonly List<Puzzle> _whitePuzzlesDeck = new();
        private readonly List<Puzzle> _blackPuzzlesDeck = new();

        /// <summary>
        /// Adds the given puzzle to the white deck.
        /// </summary>
        /// <param name="puzzle">The puzzle.</param>
        public GameStateBuilder AddWhitePuzzle(Puzzle puzzle) {
            _whitePuzzlesDeck.Add(puzzle);
            return this;
        }

        /// <summary>
        /// Adds the given puzzle to the black deck.
        /// </summary>
        /// <param name="puzzle">The puzzle.</param>
        public GameStateBuilder AddBlackPuzzle(Puzzle puzzle) {
            _blackPuzzlesDeck.Add(puzzle);
            return this;
        }

        /// <summary>
        /// Builds a new instance of the <see cref="GameState"/> class containing shuffled decks of the added puzzles.
        /// </summary>
        public GameState Build() {
            _whitePuzzlesDeck.Shuffle();
            _blackPuzzlesDeck.Shuffle();
            return new GameState(_whitePuzzlesDeck, _blackPuzzlesDeck, numInitialTetrominos);
        }
    }

    /// <summary>
    /// Represents the current state of the shared resources in the game.
    ///   <list type="bullet">
    ///     <item>The row of available white puzzles. </item>
    ///     <item>The row of available black puzzles. </item>
    ///     <item>The tetrominos left in the shared reserve. </item>
    ///     <item>The decks of white and black puzzles. </item>
    ///   </list>
    /// </summary>
    class GameState
    {
        // puzzles in decks and on the game board
        private const int _numPuzzlesInRow = 4;

        private readonly Puzzle?[] _whitePuzzlesRow = new Puzzle?[_numPuzzlesInRow];
        private readonly Puzzle?[] _blackPuzzlesRow = new Puzzle?[_numPuzzlesInRow];

        private readonly Queue<Puzzle> _whitePuzzlesDeck;
        private readonly Queue<Puzzle> _blackPuzzlesDeck;

        /// <summary> The amount of tetrominos of each shape in the shared reserve at the beginning of the game.  </summary>
        private readonly int _numInitialTetrominos;

        /// <summary> Contains the number of tetrominos left in the shared reserve for each shape.  </summary>
        public int[] NumTetrominosLeft { get; init; } = new int[TetrominoManager.NumShapes];

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="whitePuzzlesDeck">A collection of the white puzzles.</param>
        /// <param name="blackPuzzlesDeck">A collection of the black puzzles.</param>
        /// <param name="numInitalTetrominos">The amount of tetrominos of each shape in the shared reserve at the beginning of the game.</param>
        /// <exception cref="ArgumentException">Not enough puzzles to fill the rows.</exception>
        public GameState(ICollection<Puzzle> whitePuzzlesDeck, ICollection<Puzzle> blackPuzzlesDeck, int numInitalTetrominos)
        {
            // check if there are enough puzzles to fill the rows
            if (whitePuzzlesDeck.Count < _numPuzzlesInRow || blackPuzzlesDeck.Count < _numPuzzlesInRow)
            {
                throw new ArgumentException("Not enough puzzles to fill the rows.");
            };

            // create queues representing the decks
            _whitePuzzlesDeck = new Queue<Puzzle>(whitePuzzlesDeck);
            _blackPuzzlesDeck = new Queue<Puzzle>(blackPuzzlesDeck);

            // reveal the top 4 puzzles
            for (int i = 0; i < _numPuzzlesInRow; i++)
            {
                _whitePuzzlesRow[i] = _whitePuzzlesDeck.Dequeue();
                _blackPuzzlesRow[i] = _blackPuzzlesDeck.Dequeue();
            }

            // initialize tetrominos
            _numInitialTetrominos = numInitalTetrominos;
            for (int i = 0; i < NumTetrominosLeft.Length; i++)
            {
                NumTetrominosLeft[i] = _numInitialTetrominos;
            }
        }


        /*--------------- PUZZLES ---------------*/

        /// <summary> The number of puzzles left in the white deck. </summary>
        public int NumWhitePuzzlesLeft => _whitePuzzlesDeck.Count;

        /// <summary> The number of puzzles left in the black deck. </summary>
        public int NumBlackPuzzlesLeft => _blackPuzzlesDeck.Count;

        /// <summary> Returns a list containing the puzzles in the white row. </summary>
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

        /// <summary> Returns a list containing the puzzles in the black row. </summary>
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

        /// <summary>
        /// Removes 1 puzzle from the top of the white deck and returns it.
        /// </summary>
        /// <returns>The puzzle if the deck is nonempty; <c>null</c> otherwise.</returns>
        public Puzzle? TakeTopWhitePuzzle()
        {
            if (_whitePuzzlesDeck.Count == 0)
            {
                return null;
            }
            return _whitePuzzlesDeck.Dequeue();
        }

        /// <summary>
        /// Removes 1 puzzle from the top of the black deck and returns it.
        /// </summary>
        /// <returns>The puzzle if the deck is nonempty; <c>null</c> otherwise.</returns>
        public Puzzle? TakeTopBlackPuzzle()
        {
            if (_blackPuzzlesDeck.Count == 0)
            {
                return null;
            }
            return _blackPuzzlesDeck.Dequeue();
        }

        /// <summary>
        /// Finds the puzzle matching the given identifier in one of the rows.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The puzzle if it is present; <c>null</c> otherwise.</returns>
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

        /// <summary>
        /// Removes the puzzle matching the given identifier from one of the rows, if it is present.
        /// </summary>
        /// <param name="id">The identifier.</param>
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

        /// <summary>
        /// Refills the blank spots in the puzzle rows with puzzles from the decks.
        /// </summary>
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

        /// <summary>
        /// Puts the puzzle to the bottom of the deck it belongs to.
        /// </summary>
        /// <param name="puzzle">The puzzle.</param>
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

        /*-------------- TETROMINOS --------------*/

        /// <summary>
        /// Returns the number of tetrominos of the given shape left in the shared reserve.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public int GetNumTetrominosOfType(TetrominoShape shape)
        {
            return NumTetrominosLeft[(int)shape];
        }

        /// <summary>
        /// Removes the tetromino of the given shape from the shared reserve.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <exception cref="InvalidOperationException">No tetrominos of type {shape} left</exception>
        public void RemoveTetromino(TetrominoShape shape)
        {
            if (NumTetrominosLeft[(int)shape] == 0)
            {
                throw new InvalidOperationException($"No tetrominos of type {shape} left");
            }
            NumTetrominosLeft[(int)shape]--;
        }

        /// <summary>
        /// Adds the tetromino of the given shape to the shared reserve.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <exception cref="InvalidOperationException">Too many tetrominos of type {shape}</exception>
        public void AddTetromino(TetrominoShape shape)
        {
            if (NumTetrominosLeft[(int)shape] >= _numInitialTetrominos)
            {
                throw new InvalidOperationException($"Too many tetrominos of type {shape}");
            }
            NumTetrominosLeft[(int)shape]++;
        }

        /*-------------- GAME INFO WRAPPER --------------*/

        /// <summary>
        /// Returns a copy of information about the game wrapped in a <see cref="GameInfo" /> object.
        /// </summary>
        public GameInfo GetGameInfo() => new GameInfo(this);


        /// <summary>
        /// Provides information about about the game while preventing modification of the original data.
        /// </summary>
        public class GameInfo(GameState gameState)
        {
            /// <summary>  The number of puzzles left in the white deck.  </summary>
            public int NumWhitePuzzlesLeft = gameState.NumWhitePuzzlesLeft;

            /// <summary>  The number of puzzles left in the black deck.  </summary>
            public int NumBlackPuzzlesLeft = gameState.NumBlackPuzzlesLeft;

            /// <summary> The puzzles in the white row. </summary>
            public Puzzle[] AvailableWhitePuzzles = gameState.GetAvailableWhitePuzzles().Select(p => p.Clone()).ToArray();

            /// <summary> The puzzles in the black row. </summary>
            public Puzzle[] AvailableBlackPuzzles = gameState.GetAvailableBlackPuzzles().Select(p => p.Clone()).ToArray();

            /// <summary>  The number of tetrominos of each shape left in the shared reserve. </summary>
            public IReadOnlyList<int> NumTetrominosLeft = gameState.NumTetrominosLeft.AsReadOnly();
        }
    }
}