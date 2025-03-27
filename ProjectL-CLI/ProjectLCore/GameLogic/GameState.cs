namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System.Text;

    /// <summary>
    /// Builder for the <see cref="GameState"/> class.
    /// </summary>
    /// <seealso cref="GameState"/>
    /// <seealso cref="PuzzleParser"/>
    public class GameStateBuilder
    {
        #region Fields

        private readonly List<Puzzle> _whitePuzzlesDeck = new();

        private readonly List<Puzzle> _blackPuzzlesDeck = new();

        private readonly int _numInitialTetrominos;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStateBuilder"/> class.
        /// </summary>
        /// <param name="numInitialTetrominos">The number initial tetrominos.</param>
        /// <exception cref="System.ArgumentException">The number of initial tetrominos must be at least <see cref="GameState.MinNumInitialTetrominos"/>.</exception>
        public GameStateBuilder(int numInitialTetrominos)
        {
            // check if the number of initial tetrominos is valid
            if (numInitialTetrominos < GameState.MinNumInitialTetrominos) {
                throw new ArgumentException($"The number of initial tetrominos must be at least {GameState.MinNumInitialTetrominos}");
            }
            _numInitialTetrominos = numInitialTetrominos;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the given puzzle to the appropriate deck.
        /// </summary>
        /// <param name="puzzle">The puzzle.</param>
        /// <returns>The modified <see cref="GameStateBuilder"/>.</returns>
        public GameStateBuilder AddPuzzle(Puzzle puzzle)
        {
            if (puzzle.IsBlack) {
                _blackPuzzlesDeck.Add(puzzle);
            }
            else {
                _whitePuzzlesDeck.Add(puzzle);
            }
            return this;
        }

        /// <summary>
        /// Builds a new instance of the <see cref="GameState"/> class containing shuffled decks of the added puzzles.
        /// </summary>
        /// <returns>A new instance of the <see cref="GameState"/> class.</returns>
        public GameState Build()
        {
            _whitePuzzlesDeck.Shuffle();
            _blackPuzzlesDeck.Shuffle();
            return new GameState(_whitePuzzlesDeck, _blackPuzzlesDeck, _numInitialTetrominos);
        }

        #endregion
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
    /// <seealso cref="GameStateBuilder"/>
    /// <seealso cref="GameInfo"/>
    public class GameState
    {
        #region Constants

        /// <summary>
        /// The minimum number initial tetrominos of each shape in the shared reserve at the beginning of the game.
        /// </summary>
        public const int MinNumInitialTetrominos = 10;

        /// <summary>
        /// The number puzzles in a row.
        /// </summary>
        public const int NumPuzzlesInRow = 4;

        #endregion

        #region Fields

        private readonly Puzzle?[] _whitePuzzlesRow = new Puzzle?[NumPuzzlesInRow];

        private readonly Puzzle?[] _blackPuzzlesRow = new Puzzle?[NumPuzzlesInRow];

        private readonly Queue<Puzzle> _whitePuzzlesDeck;

        private readonly Queue<Puzzle> _blackPuzzlesDeck;

        private readonly List<Puzzle> _allPuzzlesInGame = new();

        /// <summary> The amount of tetrominos of each shape in the shared reserve at the beginning of the game.  </summary>
        private readonly int _numInitialTetrominos;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class.
        /// </summary>
        /// <param name="whitePuzzlesDeck">A collection of the white puzzles.</param>
        /// <param name="blackPuzzlesDeck">A collection of the black puzzles.</param>
        /// <param name="numInitialTetrominos">The amount of tetrominos of each shape in the shared reserve at the beginning of the game.</param>
        /// <exception cref="ArgumentException">
        /// The number of initial tetrominos must be at least <see cref="MinNumInitialTetrominos"/>.
        /// or
        /// The number of white puzzles must be at least <see cref="NumPuzzlesInRow"/>.
        /// or
        /// The number of black puzzles must be at least <see cref="NumPuzzlesInRow"/> + 1.
        /// </exception>
        /// <seealso cref="CreateFromFile"/>
        public GameState(ICollection<Puzzle> whitePuzzlesDeck, ICollection<Puzzle> blackPuzzlesDeck, int numInitialTetrominos)
        {
            // check if number of initial tetrominos is valid
            if (numInitialTetrominos < MinNumInitialTetrominos) {
                throw new ArgumentException($"The number of initial tetrominos must be at least {MinNumInitialTetrominos}");
            }
            // check if the number of puzzles is valid
            if (whitePuzzlesDeck.Count < NumPuzzlesInRow) {
                throw new ArgumentException($"The number of white puzzles must be at least {NumPuzzlesInRow}");
            }
            // we need at least NumPuzzlesInRow + 1 black puzzles for the game to not end immediately
            if (blackPuzzlesDeck.Count < NumPuzzlesInRow + 1) {
                throw new ArgumentException($"The number of black puzzles must be at least {NumPuzzlesInRow + 1}");
            }

            // save the list of all puzzles in the game
            _allPuzzlesInGame.AddRange(whitePuzzlesDeck);
            _allPuzzlesInGame.AddRange(blackPuzzlesDeck);

            // create queues representing the decks
            _whitePuzzlesDeck = new Queue<Puzzle>(whitePuzzlesDeck);
            _blackPuzzlesDeck = new Queue<Puzzle>(blackPuzzlesDeck);

            // initialize tetrominos
            _numInitialTetrominos = numInitialTetrominos;
            for (int i = 0; i < NumTetrominosLeft.Length; i++) {
                NumTetrominosLeft[i] = _numInitialTetrominos;
            }
        }

        #endregion

        #region Properties

        /// <summary> Contains the number of tetrominos left in the shared reserve for each shape.  </summary>
        public int[] NumTetrominosLeft { get; init; } = new int[TetrominoManager.NumShapes];

        /// <summary> The number of puzzles left in the white deck. </summary>
        public int NumWhitePuzzlesLeft => _whitePuzzlesDeck.Count;

        /// <summary> The number of puzzles left in the black deck. </summary>
        public int NumBlackPuzzlesLeft => _blackPuzzlesDeck.Count;

        #endregion

        #region Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class from a file containing puzzles.
        /// </summary>
        /// <param name="puzzlesFilePath">The puzzles file path.</param>
        /// <param name="numInitialTetrominos">The number initial tetrominos.</param>
        /// <param name="numWhitePuzzles">The number of white puzzles to load. Loads all white puzzles, if the number of white puzzles in the source file doesn't exceed this number. Should be at least <see cref="NumPuzzlesInRow"/>.</param>
        /// <param name="numBlackPuzzles">The number of black puzzles to load. Loads all black puzzles, if the number of black puzzles in the source file doesn't exceed this number. Should be at least <see cref="NumPuzzlesInRow"/> + 1.</param>
        /// <returns>Initialized <see cref="GameState"/>.</returns>
        /// <seealso cref="GameStateBuilder"/>"
        /// <seealso cref="PuzzleParser"/>
        public static GameState CreateFromFile(string puzzlesFilePath, int numInitialTetrominos = 15, int numWhitePuzzles = int.MaxValue, int numBlackPuzzles = int.MaxValue)
        {
            int numWhiteParsed = 0;
            int numBlackParsed = 0;

            // create a builder and parse the puzzles
            var gameStateBuilder = new GameStateBuilder(numInitialTetrominos);
            PuzzleParser? puzzleParser = null;
            try {
                puzzleParser = new PuzzleParser(puzzlesFilePath);
                while (true) {
                    Puzzle? puzzle = puzzleParser.GetNextPuzzle();
                    if (puzzle is null) {
                        break;
                    }
                    // add the puzzle if we haven't reached the limit
                    if (puzzle.IsBlack) {
                        if (numBlackParsed < numBlackPuzzles) {
                            gameStateBuilder.AddPuzzle(puzzle);
                            numBlackParsed++;
                        }
                    }
                    else if (numWhiteParsed < numWhitePuzzles) {
                        gameStateBuilder.AddPuzzle(puzzle);
                        numWhiteParsed++;
                    }
                    // check if we have loaded enough puzzles
                    if (numWhiteParsed == numWhitePuzzles && numBlackParsed == numBlackPuzzles) {
                        break;
                    }
                }
            }
            finally {
                puzzleParser?.Dispose();
            }
            return gameStateBuilder.Build();
        }

        /// <summary>
        /// Creates a copy of all the puzzles in the game. This includes puzzles which have already been finished.
        /// The puzzles are cloned to prevent modification of the original data.
        /// </summary>
        /// <returns>A list of all puzzles in the game.</returns>
        public List<Puzzle> GetAllPuzzlesInGame() => _allPuzzlesInGame.Select(p => p.Clone()).ToList();

        /// <summary> Creates a list containing the puzzles in the white row. </summary>
        /// <returns>A list containing the puzzles in the white row.</returns>
        public List<Puzzle> GetAvailableWhitePuzzles()
        {
            var result = new List<Puzzle>();
            for (int i = 0; i < _whitePuzzlesRow.Length; i++) {
                if (_whitePuzzlesRow[i] is not null) {
                    result.Add(_whitePuzzlesRow[i]!);
                }
            }
            return result;
        }

        /// <summary> Creates a list containing the puzzles in the black row. </summary>
        /// <returns>A list containing the puzzles in the black row.</returns>
        public List<Puzzle> GetAvailableBlackPuzzles()
        {
            var result = new List<Puzzle>();
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is not null) {
                    result.Add(_blackPuzzlesRow[i]!);
                }
            }
            return result;
        }

        /// <summary>
        /// Removes 1 puzzle from the top of the white deck and returns it.
        /// </summary>
        /// <returns>The puzzle if the deck is nonempty; <see langword="null"/> otherwise.</returns>
        public Puzzle? TakeTopWhitePuzzle()
        {
            if (_whitePuzzlesDeck.Count == 0) {
                return null;
            }
            return _whitePuzzlesDeck.Dequeue();
        }

        /// <summary>
        /// Removes 1 puzzle from the top of the black deck and returns it.
        /// </summary>
        /// <returns>The puzzle if the deck is nonempty; <see langword="null"/> otherwise.</returns>
        public Puzzle? TakeTopBlackPuzzle()
        {
            if (_blackPuzzlesDeck.Count == 0) {
                return null;
            }
            return _blackPuzzlesDeck.Dequeue();
        }

        /// <summary>
        /// Finds the puzzle matching the given identifier in one of the rows.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The puzzle if it is present; <see langword="null"/> otherwise.</returns>
        public Puzzle? GetPuzzleWithId(uint id)
        {
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

        /// <summary>
        /// Removes the puzzle matching the given identifier from one of the rows, if it is present.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void RemovePuzzleWithId(uint id)
        {
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

        /// <summary>
        /// Refills the blank spots in the puzzle rows with puzzles from the decks.
        /// </summary>
        public void RefillPuzzles()
        {
            for (int i = 0; i < _whitePuzzlesRow.Length; i++) {
                if (_whitePuzzlesRow[i] is null && _whitePuzzlesDeck.Count > 0) {
                    _whitePuzzlesRow[i] = TakeTopWhitePuzzle();
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is null && _blackPuzzlesDeck.Count > 0) {
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
            if (puzzle.IsBlack) {
                _blackPuzzlesDeck.Enqueue(puzzle);
            }
            else {
                _whitePuzzlesDeck.Enqueue(puzzle);
            }
        }

        /// <summary>
        /// Gets the number of tetrominos of the given shape left in the shared reserve.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns>The number of tetrominos of type <paramref name="shape"/> left in the shared reserve.</returns>
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
            if (NumTetrominosLeft[(int)shape] == 0) {
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
            if (NumTetrominosLeft[(int)shape] >= _numInitialTetrominos) {
                throw new InvalidOperationException($"Too many tetrominos of type {shape}");
            }
            NumTetrominosLeft[(int)shape]++;
        }

        /// <summary>
        /// Creates a copy of information about the game wrapped in a <see cref="GameInfo" /> object. It prevents modification of the original data.
        /// </summary>
        /// <returns>Information about the game.</returns>
        public GameInfo GetGameInfo() => new GameInfo(this);

        #endregion

        /// <summary>
        /// Provides information about about the game while preventing modification of the original data.
        /// </summary>
        /// <param name="gameState">The game state that will be wrapped.</param>
        public class GameInfo(GameState gameState)
        {
            #region Fields

            /// <summary> The number of puzzles left in the white deck.  </summary>
            public int NumWhitePuzzlesLeft = gameState.NumWhitePuzzlesLeft;

            /// <summary> The number of puzzles left in the black deck.  </summary>
            public int NumBlackPuzzlesLeft = gameState.NumBlackPuzzlesLeft;

            /// <summary> The puzzles in the white row. </summary>
            public Puzzle[] AvailableWhitePuzzles = gameState.GetAvailableWhitePuzzles().Select(p => p.Clone()).ToArray();

            /// <summary> The puzzles in the black row. </summary>
            public Puzzle[] AvailableBlackPuzzles = gameState.GetAvailableBlackPuzzles().Select(p => p.Clone()).ToArray();

            /// <summary> The number of tetrominos of each shape left in the shared reserve. </summary>
            public IReadOnlyList<int> NumTetrominosLeft = gameState.NumTetrominosLeft.AsReadOnly();

            #endregion

            #region Methods

            /// <summary>
            /// Converts to string. It shows the puzzles in the white and black rows and the number of tetrominos left in the shared reserve.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                var sb = new StringBuilder();
                // append tetromino info
                sb.Append("Tetrominos:");
                for (int i = 0; i < NumTetrominosLeft.Count; i++) {
                    sb.Append($"  {(TetrominoShape)i}: {NumTetrominosLeft[i]}");
                    if (i < NumTetrominosLeft.Count - 1) {
                        sb.Append(',');
                    }
                }
                sb.AppendLine().AppendLine();

                // append puzzle info
                AppendPuzzleRowInfo("White", AvailableWhitePuzzles, NumWhitePuzzlesLeft);
                sb.AppendLine();
                AppendPuzzleRowInfo("Black", AvailableBlackPuzzles, NumBlackPuzzlesLeft);

                return sb.ToString();

                void AppendPuzzleRowInfo(string rowColor, Puzzle[] puzzles, int numPuzzlesLeft)
                {
                    sb.Append("  ");
                    foreach (Puzzle puzzle in puzzles) {
                        string tetrominoName = puzzle.RewardTetromino.ToString().PadLeft(2);
                        sb.Append($"{puzzle.RewardScore}  {tetrominoName}   ");
                    }
                    sb.AppendLine();
                    StringReader[] puzzleReaders = puzzles.Select(p => p.Image.ToString()).Select(p => new StringReader(p)).ToArray();
                    for (int i = 0; i < 5; i++) {
                        for (int j = 0; j < puzzles.Length; j++) {
                            if (j == 0) {
                                sb.Append("  ");
                            }
                            sb.Append(puzzleReaders[j].ReadLine()).Append("   ");
                        }
                        if (i == 2) {
                            sb.Append($"   {rowColor} puzzles left: {numPuzzlesLeft}");
                        }
                        sb.AppendLine();
                    }
                    sb.Append("  ");
                    foreach (Puzzle puzzle in puzzles) {
                        string color = puzzle.IsBlack ? "B" : "W";
                        string puzzleID = $"{color}-{puzzle.Id}";
                        sb.Append($"{puzzleID.PadRight(5)}   ");
                    }
                    sb.AppendLine();
                }
            }

            #endregion
        }
    }
}
