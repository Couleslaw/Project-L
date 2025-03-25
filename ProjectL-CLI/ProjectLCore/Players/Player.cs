namespace ProjectLCore.Players
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;


    /// <summary>
    /// Represents a player in the game.
    /// </summary>
    public abstract class Player
    {
        #region Fields

        private static uint _idCounter = 0;

        #endregion

        #region Properties

        /// <summary>
        /// The unique ID of the player. Starts at zero and is incremented by one for each new player created.
        /// This means that the n-th player created has <see cref="Id"/> = n-1.
        /// </summary>
        public uint Id { get; } = _idCounter++;

        /// <summary>
        /// The name of the player.
        /// </summary>
        public abstract string Name { get; init; }

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously gets the action the player wants to take based on the current game context.
        /// </summary>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="playerInfos">Information about the resources of the players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <returns>The action the player wants to take.</returns>
        public abstract Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier);

        /// <summary>
        /// Asynchronously gets the shape the player wants as a reward for completing a puzzle.
        /// Note that the player doesn't get the current game context here. 
        /// This is because this function will be called right after he completes a puzzle and therefore he knows the current game state from the last <see cref="GetActionAsync"/> call.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <returns>The shape the player wants to take.</returns>
        public abstract Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions, Puzzle puzzle);

        #endregion
    }
}
