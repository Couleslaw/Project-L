namespace ProjectLCore.GamePieces
{
    using ProjectLCore.Players;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameActions;

    /// <summary>
    /// Contains information about the reward a player chose for finishing a puzzle.
    /// </summary>
    /// <param name="PlayerId">The ID of the <see cref="Player"/> who completed the puzzle.</param>
    /// <param name="Puzzle">The puzzle the player completed.</param>
    /// <param name="RewardOptions">A list of possible rewards the player got to choose from. Or <see langword="null"/> if the puzzle was completed during <see cref="GamePhase.FinishingTouches"/>.</param>
    /// <param name="SelectedReward">The reward the player selected. Or <see langword="null"/> if <paramref name="RewardOptions"/> is empty.</param>
    /// <seealso cref="GameActionProcessor.FinishedPuzzlesQueue"/>
    /// <seealso cref="GameCore.TryGetNextPuzzleFinishedBy(Players.Player, out FinishedPuzzleInfo)"/>
    public record struct FinishedPuzzleInfo(uint PlayerId, Puzzle Puzzle, List<TetrominoShape>? RewardOptions, TetrominoShape? SelectedReward);
}
