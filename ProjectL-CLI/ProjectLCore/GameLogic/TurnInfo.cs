namespace ProjectLCore.GameLogic
{
    /// <summary>
    /// Represents the information about the current turn.
    /// </summary>
    public struct TurnInfo
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TurnInfo"/> struct with the specified parameters.
        /// </summary>
        /// <param name="numActionsLeft">The number of actions the current player has left in this turn.</param>
        /// <param name="gamePhase">The current phase of the game.</param>
        /// <param name="usedMasterAction"><see langword="true"/> if the current player used the Master action this turn; otherwise <see langword="false"/>.</param>
        /// <param name="tookBlackPuzzle"><see langword="true"/> if the current player took a black puzzle this turn; otherwise <see langword="false"/>.</param>
        /// <param name="lastRound"><see langword="true"/> if this is the last round of the game; otherwise <see langword="false"/>.</param>
        public TurnInfo(int numActionsLeft, GamePhase gamePhase, bool usedMasterAction, bool tookBlackPuzzle, bool lastRound)
        {
            NumActionsLeft = numActionsLeft;
            GamePhase = gamePhase;
            UsedMasterAction = usedMasterAction;
            TookBlackPuzzle = tookBlackPuzzle;
            LastRound = lastRound;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of actions the current player has left in this turn.
        /// </summary>
        public int NumActionsLeft { get; set; }

        /// <summary>
        /// The current phase of the game.
        /// </summary>
        public GamePhase GamePhase { get; set; }

        /// <summary>
        /// <see langword="true"/> if the current player used the Master action this turn; otherwise <see langword="false"/>.
        /// </summary>
        public bool UsedMasterAction { get; set; }

        /// <summary>
        /// <see langword="true"/> if the current player took a black puzzle this turn; otherwise <see langword="false"/>.
        /// </summary>
        public bool TookBlackPuzzle { get; set; }

        /// <summary>
        /// <see langword="true"/> if this is the last round of the game; otherwise <see langword="false"/>.
        /// </summary>
        public bool LastRound { get; set; }

        #endregion
    }
}
