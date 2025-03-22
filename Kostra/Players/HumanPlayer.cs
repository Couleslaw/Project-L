using Kostra.GameActions;
using Kostra.GameLogic;
using Kostra.GameManagers;
using Kostra.GamePieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kostra.Players
{
    /// <summary>
    /// Represents a human player in the game.
    /// </summary>
    /// <seealso cref="Player" />
    class HumanPlayer : Player
    {
        public override PlayerType Type => PlayerType.Human;

        // completion sources for setting the result of the async methods
        private TaskCompletionSource<VerifiableAction> _getActionCompletionSource = new();
        private TaskCompletionSource<TetrominoShape> _getRewardCompletionSource = new();

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
    }
}
