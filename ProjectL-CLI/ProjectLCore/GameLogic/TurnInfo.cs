namespace ProjectLCore.GameLogic
{
    /// <summary>
    /// Represents the information about the current turn.
    /// </summary>
    /// <param name="NumActionsLeft">The number of actions the current player has left in this turn.</param>
    /// <param name="GamePhase">The current phase of the game.</param>
    /// <param name="UsedMasterAction"><see langword="true"/> if the current player used the Master action this turn; otherwise <see langword="false"/>.</param>
    /// <param name="TookBlackPuzzle"><see langword="true"/> if the current player took a black puzzle this turn; otherwise <see langword="false"/>.</param>
    /// <param name="LastRound"><see langword="true"/> if this is the last round of the game; otherwise <see langword="false"/>.</param>
    public record struct TurnInfo(
        int NumActionsLeft,
        GamePhase GamePhase,
        bool UsedMasterAction,
        bool TookBlackPuzzle,
        bool LastRound
        );
}
