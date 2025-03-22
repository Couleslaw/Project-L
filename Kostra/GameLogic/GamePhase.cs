namespace Kostra.GameLogic
{
    using Kostra.GameActions;

    /// <summary>
    /// Represents the current phase of the game
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// Standard phase of the game in which players take actions.
        /// </summary>
        Normal,
        /// <summary> 
        /// The EndOfTheGame phase is triggered when there are no more black puzzles in the black deck.
        /// </summary>
        EndOfTheGame,
        /// <summary>
        /// The FinishingTouches phase is triggered after the last round of the game.
        /// </summary>
        FinishingTouches,
        /// <summary>
        /// The game is finishing after all players use the <see cref="EndFinishingTouchesAction"/>.
        /// </summary>
        Finished
    }
}
