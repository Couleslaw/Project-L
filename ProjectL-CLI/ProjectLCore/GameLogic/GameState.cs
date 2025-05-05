namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

        /// <summary>
        /// The row of available white puzzles. Each element represents a puzzle or is <see langword="null"/> if the slot is empty.
        /// </summary>
        private readonly Puzzle?[] _whitePuzzlesRow = new Puzzle?[NumPuzzlesInRow];

        /// <summary>
        /// The row of available black puzzles. Each element represents a puzzle or is <see langword="null"/> if the slot is empty.
        /// </summary>
        private readonly Puzzle?[] _blackPuzzlesRow = new Puzzle?[NumPuzzlesInRow];

        /// <summary>
        /// The deck of white puzzles represented as a queue. Puzzles are drawn from the front of the queue.
        /// </summary>
        private readonly Queue<Puzzle> _whitePuzzlesDeck;

        /// <summary>
        /// The deck of black puzzles represented as a queue. Puzzles are drawn from the front of the queue.
        /// </summary>
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

        /// <summary>
        /// Called when the white puzzle row changes. The parameters are the position of the change and the new puzzle.
        /// </summary>
        public Action<int, Puzzle?>? OnWhitePuzzleRowChanged { get; set; }

        /// <summary>
        /// Called when the black puzzle row changes. The parameters are the position of the change and the new puzzle.
        /// </summary>
        public Action<int, Puzzle?>? OnBlackPuzzleRowChanged { get; set; }

        /// <summary>
        /// Called when the white puzzle deck changes. The parameter is the number of puzzles left in the deck.
        /// </summary>
        public Action<int>? OnWhitePuzzleDeckChanged { get; set; }

        /// <summary>
        /// Called when the black puzzle deck changes. The parameter is the number of puzzles left in the deck.
        /// </summary>
        public Action<int>? OnBlackPuzzleDeckChanged { get; set; }

        /// <summary>
        /// Called when the number of tetrominos in the shared reserve changes. The parameters are the type of the tetromino and the number of tetrominos of this type in the reserve after the change.
        /// </summary>
        public Action<TetrominoShape, int>? OnTetrominosReserveChanged { get; set; }

        /// <summary> Contains the number of tetrominos left in the shared reserve for each shape.  </summary>
        public int[] NumTetrominosLeft { get; } = new int[TetrominoManager.NumShapes];

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
        /// <param name="numWhitePuzzles">The resulting <see cref="GameState"/> will contain <paramref name="numWhitePuzzles"/> random puzzles from the file. This number can exceed the number of puzzles in the file. Should be at least <see cref="NumPuzzlesInRow"/>.</param>
        /// <param name="numBlackPuzzles">The resulting <see cref="GameState"/> will contain <paramref name="numBlackPuzzles"/> random puzzles from the file. This number can exceed the number of puzzles in the file. Should be at least <see cref="NumPuzzlesInRow"/> + 1.</param>
        /// <returns>Initialized <see cref="GameState"/>.</returns>
        /// <seealso cref="GameStateBuilder">       
        /// <seealso cref="PuzzleParser"/>
        public static GameState CreateFromFile(string puzzlesFilePath, int numInitialTetrominos = 15, int numWhitePuzzles = int.MaxValue, int numBlackPuzzles = int.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(puzzlesFilePath)) {
                throw new ArgumentException("The file path cannot be null or empty.", nameof(puzzlesFilePath));
            }

            try {
                using FileStream fileStream = new FileStream(puzzlesFilePath, FileMode.Open, FileAccess.Read);
                return CreateFromStream(fileStream, numInitialTetrominos, numWhitePuzzles, numBlackPuzzles);
            }
            catch (Exception ex) {
                throw new IOException($"Failed to create a stream from the file at path: {puzzlesFilePath}", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameState"/> class from a stream containing puzzles.
        /// </summary>
        /// <param name="puzzleStream">Stream to read the puzzles from.</param>
        /// <param name="numInitialTetrominos">The number initial tetrominos.</param>
        /// <param name="numWhitePuzzles">The resulting <see cref="GameState"/> will contain <paramref name="numWhitePuzzles"/> random puzzles from the stream. This number can exceed the number of puzzles in the stream. Should be at least <see cref="NumPuzzlesInRow"/>.</param>
        /// <param name="numBlackPuzzles">The resulting <see cref="GameState"/> will contain <paramref name="numBlackPuzzles"/> random puzzles from the stream. This number can exceed the number of puzzles in the stream. Should be at least <see cref="NumPuzzlesInRow"/> + 1.</param>
        /// <returns>Initialized <see cref="GameState"/>.</returns>
        /// <seealso cref="GameStateBuilder"/>"
        /// <seealso cref="PuzzleParser"/>
        public static GameState CreateFromStream(Stream puzzleStream, int numInitialTetrominos = 15, int numWhitePuzzles = int.MaxValue, int numBlackPuzzles = int.MaxValue)
        {
            // parse the puzzles
            PuzzleParser? puzzleParser = null;
            List<Puzzle> whitePuzzles = new();
            List<Puzzle> blackPuzzles = new();

            using (puzzleParser = new PuzzleParser(puzzleStream)) {
                while (true) {
                    Puzzle? puzzle = puzzleParser.GetNextPuzzle();
                    if (puzzle is null) {
                        break;
                    }
                    // add the puzzle 
                    if (puzzle.IsBlack) {
                        blackPuzzles.Add(puzzle);
                    }
                    else {
                        whitePuzzles.Add(puzzle);
                    }
                }
            }

            // create the game state by picking random puzzles
            whitePuzzles.Shuffle();
            blackPuzzles.Shuffle();

            var gameStateBuilder = new GameStateBuilder(numInitialTetrominos);
            for (int i = 0; i < numWhitePuzzles && i < whitePuzzles.Count; i++) {
                gameStateBuilder.AddPuzzle(whitePuzzles[i]);
            }
            for (int i = 0; i < numBlackPuzzles && i < blackPuzzles.Count; i++) {
                gameStateBuilder.AddPuzzle(blackPuzzles[i]);
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
            var result = _whitePuzzlesDeck.Dequeue();
            OnWhitePuzzleDeckChanged?.Invoke(_whitePuzzlesDeck.Count);
            return result;
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
            var result = _blackPuzzlesDeck.Dequeue();
            OnBlackPuzzleDeckChanged?.Invoke(_blackPuzzlesDeck.Count);
            return result;
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
                    OnWhitePuzzleRowChanged?.Invoke(i, null);
                    return;
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is not null && _blackPuzzlesRow[i]!.Id == id) {
                    _blackPuzzlesRow[i] = null;
                    OnBlackPuzzleRowChanged?.Invoke(i, null);
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
                    OnWhitePuzzleRowChanged?.Invoke(i, _whitePuzzlesRow[i]);
                }
            }
            for (int i = 0; i < _blackPuzzlesRow.Length; i++) {
                if (_blackPuzzlesRow[i] is null && _blackPuzzlesDeck.Count > 0) {
                    _blackPuzzlesRow[i] = TakeTopBlackPuzzle();
                    OnBlackPuzzleRowChanged?.Invoke(i, _blackPuzzlesRow[i]);
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
                OnBlackPuzzleDeckChanged?.Invoke(_blackPuzzlesDeck.Count);
            }
            else {
                _whitePuzzlesDeck.Enqueue(puzzle);
                OnWhitePuzzleDeckChanged?.Invoke(_whitePuzzlesDeck.Count);
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
            OnTetrominosReserveChanged?.Invoke(shape, NumTetrominosLeft[(int)shape]);
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
            OnTetrominosReserveChanged?.Invoke(shape, NumTetrominosLeft[(int)shape]);
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
        public class GameInfo
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="GameInfo"/> class.
            /// </summary>
            /// <param name="gameState">The game state that will be wrapped.</param>
            public GameInfo(GameState gameState)
            {
                NumWhitePuzzlesLeft = gameState.NumWhitePuzzlesLeft;
                NumBlackPuzzlesLeft = gameState.NumBlackPuzzlesLeft;
                AvailableWhitePuzzles = gameState.GetAvailableWhitePuzzles().Select(p => p.Clone()).ToArray();
                AvailableBlackPuzzles = gameState.GetAvailableBlackPuzzles().Select(p => p.Clone()).ToArray();
                NumTetrominosLeft = Array.AsReadOnly(gameState.NumTetrominosLeft);
            }

            #endregion

            #region Properties

            /// <summary> The number of puzzles left in the white deck.  </summary>
            public int NumWhitePuzzlesLeft { get; }

            /// <summary> The number of puzzles left in the black deck.  </summary>
            public int NumBlackPuzzlesLeft { get; }

            /// <summary> The puzzles in the white row. </summary>
            public Puzzle[] AvailableWhitePuzzles { get; }

            /// <summary> The puzzles in the black row. </summary>
            public Puzzle[] AvailableBlackPuzzles { get; }

            /// <summary> The number of tetrominos of each shape left in the shared reserve. </summary>
            public IReadOnlyList<int> NumTetrominosLeft { get; }

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
