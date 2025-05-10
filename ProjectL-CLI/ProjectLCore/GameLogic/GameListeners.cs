namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;

    /// <summary>
    /// Interface for classes that want to be notified when the next turn starts.
    /// </summary>
    /// <seealso cref="GameCore.AddListener(ICurrentTurnListener)"/>
    /// <seealso cref="GameCore.RemoveListener(ICurrentTurnListener)"/>
    /// <seealso cref="ICurrentPlayerListener"/>
    public interface ICurrentTurnListener
    {
        #region Methods

        /// <summary>
        /// Called when the current turn changes.
        /// </summary>
        /// <param name="currentTurnInfo">Information about the current turn.</param>
        void OnCurrentTurnChanged(TurnInfo currentTurnInfo);

        #endregion
    }

    /// <summary>
    /// Interface for classes that want to be notified when the current player changes.
    /// </summary>
    /// <seealso cref="GameCore.AddListener(ICurrentPlayerListener)"/>
    /// <seealso cref="GameCore.RemoveListener(ICurrentPlayerListener)"/>
    /// <seealso cref="ICurrentTurnListener"/>"/>
    public interface ICurrentPlayerListener
    {
        #region Methods

        /// <summary>
        /// Called when the current player changes.
        /// </summary>
        /// <param name="currentPlayer">The player who is now the current player.</param>
        void OnCurrentPlayerChanged(Player currentPlayer);

        #endregion
    }

    /// <summary>
    /// Interface for classes that want to be notified about changes in a <see cref="PlayerState"/>.
    /// </summary>
    /// <seealso cref="PlayerState.AddListener(IPlayerStatePuzzleListener)"/>
    /// <seealso cref="PlayerState.RemoveListener(IPlayerStatePuzzleListener)"/>
    /// <seealso cref="ITetrominoCollectionListener"/>
    public interface IPlayerStatePuzzleListener
    {
        #region Methods

        /// <summary>
        /// Called when the set of unfinished puzzles changes.
        /// </summary>
        /// <param name="index"> The index (in the row) of the puzzle that was finished.</param>
        /// <param name="info"> The information about the finished puzzle.</param>
        public void OnPuzzleFinished(int index, FinishedPuzzleInfo info);

        /// <summary>
        /// Called when the player takes a new puzzle;
        /// </summary>
        /// <param name="index">The index (in the row) of the puzzle that was added.</param>
        /// <param name="puzzle">The puzzle that was added.</param>
        public void OnPuzzleAdded(int index, Puzzle puzzle);

        #endregion
    }

    /// <summary>
    /// Interface for classes that want to be notified about puzzle changes in a <see cref="GameState"/>.
    /// </summary>
    /// <seealso cref="GameState.AddListener(IGameStatePuzzleListener)"/>
    /// <seealso cref="GameState.RemoveListener(IGameStatePuzzleListener)"/>
    /// <seealso cref="ITetrominoCollectionListener"/>
    public interface IGameStatePuzzleListener
    {
        #region Methods

        /// <summary>
        /// Called when the white puzzle row changes.
        /// </summary>
        /// <param name="index">The index in the row where the change occurred.</param>
        /// <param name="puzzle">The new puzzle at the specified index, or <see langword="null"/> if the slot is empty.</param>
        public void OnWhitePuzzleRowChanged(int index, Puzzle? puzzle);

        /// <summary>
        /// Called when the black puzzle row changes.
        /// </summary>
        /// <param name="index">The index in the row where the change occurred.</param>
        /// <param name="puzzle">The new puzzle at the specified index, or <see langword="null"/> if the slot is empty.</param>
        public void OnBlackPuzzleRowChanged(int index, Puzzle? puzzle);

        /// <summary>
        /// Called when the number of puzzles in the white puzzle deck changes.
        /// </summary>
        /// <param name="count">The new number of puzzles in the white puzzle deck.</param>
        public void OnWhitePuzzleDeckChanged(int count);

        /// <summary>
        /// Called when the number of puzzles in the black puzzle deck changes.
        /// </summary>
        /// <param name="count">The new number of puzzles in the black puzzle deck.</param>
        public void OnBlackPuzzleDeckChanged(int count);

        #endregion
    }

    /// <summary>
    /// Interface for classes that want to be notified about tetromino changes in a <see cref="GameState"/>.
    /// </summary>
    /// <seealso cref="ITetrominoCollectionNotifier.AddListener(ITetrominoCollectionListener)"/>
    /// <seealso cref="ITetrominoCollectionNotifier.RemoveListener(ITetrominoCollectionListener)"/>
    /// <seealso cref="IGameStatePuzzleListener"/>
    /// <seealso cref="IPlayerStatePuzzleListener"/>
    public interface ITetrominoCollectionListener
    {
        #region Methods

        /// <summary>
        /// Called when the number of tetrominos in the shared reserve changes.
        /// </summary>
        /// <param name="shape">The shape of the tetromino that was used or added.</param>
        /// <param name="count"> The number of tetrominos of this shape in the shared reserve after the change.</param>
        public void OnTetrominoCollectionChanged(TetrominoShape shape, int count);

        #endregion
    }

    /// <summary>
    /// Interface for notifying listeners about changes in the tetromino collection.
    /// </summary>
    /// <seealso cref="GameState"/>
    /// <seealso cref="PlayerState"/>
    public interface ITetrominoCollectionNotifier
    {
        /// <summary>
        /// Adds a listener to be notified when the tetromino collection changes.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddListener(ITetrominoCollectionListener listener);

        /// <summary>
        /// Removes a listener so it will no longer be notified about changes in the tetromino collection.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        public void RemoveListener(ITetrominoCollectionListener listener);
    }
}
