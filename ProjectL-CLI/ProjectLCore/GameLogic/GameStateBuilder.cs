namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Builder for the <see cref="GameState"/> class.
    /// </summary>
    /// <seealso cref="GameState"/>
    /// <seealso cref="PuzzleParser{T}"/>
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
}
