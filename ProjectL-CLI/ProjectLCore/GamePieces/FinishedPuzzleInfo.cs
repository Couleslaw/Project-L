namespace ProjectLCore.GamePieces
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using System.Collections.Generic;

    /// <summary>
    /// Contains information about the reward a player chose for finishing a puzzle.
    /// </summary>
    /// <seealso cref="GameActionProcessor.FinishedPuzzlesQueue"/>
    /// <seealso cref="GameCore.TryGetNextPuzzleFinishedBy(Players.Player, out FinishedPuzzleInfo)"/>
    public readonly struct FinishedPuzzleInfo
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FinishedPuzzleInfo"/> struct with the specified parameters.
        /// </summary>
        /// <param name="playerId">The ID of the <see cref="Player"/> who completed the puzzle.</param>
        /// <param name="puzzle">The puzzle the player completed.</param>
        /// <param name="rewardOptions">A list of possible rewards the player got to choose from. Or <see langword="null"/> if the puzzle was completed during <see cref="GamePhase.FinishingTouches"/>.</param>
        /// <param name="selectedReward">The reward the player selected. Or <see langword="null"/> if <paramref name="rewardOptions"/> is empty.</param>
        public FinishedPuzzleInfo(uint playerId, Puzzle puzzle, List<TetrominoShape>? rewardOptions, TetrominoShape? selectedReward)
        {
            PlayerId = playerId;
            Puzzle = puzzle;
            RewardOptions = rewardOptions;
            SelectedReward = selectedReward;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the <see cref="Player"/> who completed the puzzle.
        /// </summary>
        public uint PlayerId { get; }

        /// <summary>
        /// The puzzle the player completed.
        /// </summary>
        public Puzzle Puzzle { get; }

        /// <summary>
        /// A list of possible rewards the player got to choose from. Or <see langword="null"/> if the puzzle was completed during <see cref="GamePhase.FinishingTouches"/>.
        /// </summary>
        public List<TetrominoShape>? RewardOptions { get; }

        /// <summary>
        /// The reward the player selected. Or <see langword="null"/> if <see cref="RewardOptions"/> is empty.
        /// </summary>
        public TetrominoShape? SelectedReward { get; }

        #endregion
    }
}
