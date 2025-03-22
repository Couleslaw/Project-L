namespace Kostra.GameLogic
{
    /// <summary>
    /// Represents the information about the current turn.
    /// </summary>
    /// <param name="NumActionsLeft">The number of actions the current player has left in this turn.</param>
    /// <param name="GamePhase">The current phase of the game.</param>
    /// <param name="UsedMasterAction">True if the current player used the Master action this turn.</param>
    /// <param name="TookBlackPuzzle">True if the current player took a black puzzle this turn.</param>
    /// <param name="LastRound">True if this is the last round of the game.</param>
    public record struct TurnInfo(
        int NumActionsLeft,
        GamePhase GamePhase,
        bool UsedMasterAction,
        bool TookBlackPuzzle,
        bool LastRound
        );
}
