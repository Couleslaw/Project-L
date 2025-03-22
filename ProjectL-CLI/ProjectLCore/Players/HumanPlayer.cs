namespace ProjectLCore.Players
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a human player in the game.
    /// </summary>
    /// <seealso cref="Player" />
    public class HumanPlayer : Player
    {
        #region Fields

        // completion sources for setting the result of the async methods
        private TaskCompletionSource<VerifiableAction> _getActionCompletionSource = new();

        private TaskCompletionSource<TetrominoShape> _getRewardCompletionSource = new();

        #endregion

        #region Properties

        public override PlayerType Type => PlayerType.Human;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the next action the player wants to take. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="action">The action.</param>
        public void SetAction(VerifiableAction action) => _getActionCompletionSource.SetResult(action);

        /// <summary>
        /// Sets the reward the player wants to get. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="reward">The reward.</param>
        public void SetReward(TetrominoShape reward) => _getRewardCompletionSource.SetResult(reward);

        public override async Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            _getActionCompletionSource = new();
            return await _getActionCompletionSource.Task;
        }

        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions, Puzzle puzzle)
        {
            _getRewardCompletionSource = new();
            return await _getRewardCompletionSource.Task;
        }

        #endregion
    }
}
