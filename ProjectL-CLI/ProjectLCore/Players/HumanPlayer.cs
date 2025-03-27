namespace ProjectLCore.Players
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a human player in the game. Human players pick their action using the UI.
    /// </summary>
    /// <seealso cref="Player" />
    public sealed class HumanPlayer : Player
    {
        #region Fields

        // completion sources for setting the result of the async methods
        private TaskCompletionSource<IAction> _getActionCompletionSource = new();

        private TaskCompletionSource<TetrominoShape> _getRewardCompletionSource = new();

        #endregion

        #region Methods

        /// <summary>
        /// Sets the next action the player wants to take. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="action">The action.</param>
        public void SetAction(IAction action) => _getActionCompletionSource.SetResult(action);

        /// <summary>
        /// Sets the reward the player wants to get. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="reward">The reward.</param>
        public void SetReward(TetrominoShape reward) => _getRewardCompletionSource.SetResult(reward);

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource"/> for <see cref="IAction"/> and asynchronously waits until it is set by calling the <see cref="SetAction"/> method.
        /// </summary>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="playerInfos">Information about the resources of the players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <returns>
        /// The action the player wants to take.
        /// </returns>
        public override async Task<IAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            _getActionCompletionSource = new();
            return await _getActionCompletionSource.Task;
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource"/> for <see cref="TetrominoShape"/> and asynchronously waits until it is set by calling the <see cref="SetReward"/> method.
        /// Note that the player doesn't get the current game context here.
        /// This is because this function will be called right after he completes a puzzle and therefore he knows the current game state from the last <see cref="GetActionAsync" /> call.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <returns>
        /// The shape the player wants to take.
        /// </returns>
        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions, Puzzle puzzle)
        {
            _getRewardCompletionSource = new();
            return await _getRewardCompletionSource.Task;
        }

        #endregion
    }
}
