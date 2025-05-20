#nullable enable

namespace ProjectL.GameScene.ActionHandling
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the selection of a reward tetromino from a list of available options.
    /// </summary>
    public class SelectRewardAction : GameAction
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectRewardAction"/> class.
        /// </summary>
        /// <param name="rewardOptions">The list of available tetrominos to choose from as rewards.</param>
        /// <param name="selectedReward">The tetromino selected as the reward.</param>
        public SelectRewardAction(List<TetrominoShape>? rewardOptions, TetrominoShape selectedReward)
        {
            SelectedReward = selectedReward;
            RewardOptions = rewardOptions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The tetromino selected as the reward.
        /// </summary>
        public TetrominoShape SelectedReward { get; }

        /// <summary>
        /// The list of available tetrominos to choose from as rewards.
        /// </summary>
        public List<TetrominoShape>? RewardOptions { get; }

        #endregion
    }
}
