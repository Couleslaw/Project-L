namespace ProjectLCore.Players
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a human player in the game. Human players pick their action using the UI.
    /// </summary>
    /// <seealso cref="Player" />
    public sealed class HumanPlayer : Player
    {
        #region Fields

        // completion sources for setting the result of the async methods
        private TaskCompletionSource<GameAction> _getActionCompletionSource = new();

        private TaskCompletionSource<TetrominoShape> _getRewardCompletionSource = new();

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the player is requested to provide an action during their turn.
        /// </summary>
        public event EventHandler<GetActionEventArgs>? ActionRequested;

        /// <summary>
        /// Occurs when the player is requested to select a reward after completing a puzzle.
        /// </summary>
        public event EventHandler<GetRewardEventArgs>? RewardChoiceRequested;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the next action the player wants to take. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="InvalidOperationException">The action has already been set.</exception>"
        public void SetAction(GameAction action)
        {
            // check if an action has already been set
            if (_getActionCompletionSource.Task.IsCompleted) {
                throw new InvalidOperationException("The action has already been set.");
            }
            // set the action
            _getActionCompletionSource.SetResult(action);
        }

        /// <summary>
        /// Sets the reward the player wants to get. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="reward">The reward.</param>
        /// <exception cref="InvalidOperationException">The reward has already been set.</exception>"
        public void SetReward(TetrominoShape reward)
        {
            // check if a reward has already been set
            if (_getRewardCompletionSource.Task.IsCompleted) {
                throw new InvalidOperationException("The reward has already been set.");
            }
            // set the reward
            _getRewardCompletionSource.SetResult(reward);
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource{IAction}"/> for <see cref="GameAction"/> and asynchronously waits until it is set by calling the <see cref="SetAction"/> method.
        /// </summary>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="playerInfos">Information about the resources of the players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// The action the player wants to take.
        /// </returns>
        public override async Task<GameAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier, CancellationToken cancellationToken = default)
        {
            _getActionCompletionSource = new(cancellationToken);
            if (!TryFindMyInfo(playerInfos, out var playerInfo)) {
                throw new ArgumentException($"PlayerState matching this player's {nameof(Player.Id)} not found in {nameof(playerInfos)}.");
            }

            // request the action
            var args = new GetActionEventArgs(gameInfo, playerInfo!, turnInfo, verifier);
            ActionRequested?.Invoke(this, args);

            // wait until the action has been set from the outside
            return await _getActionCompletionSource.Task;
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource{IAction}"/> for <see cref="TetrominoShape"/> and asynchronously waits until it is set by calling the <see cref="SetReward"/> method.
        /// Note that the player doesn't get the current game context here.
        /// This is because this function will be called right after he completes a puzzle and therefore he knows the current game state from the last <see cref="GetActionAsync" /> call.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// The shape the player wants to take.
        /// </returns>
        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions, Puzzle puzzle, CancellationToken cancellationToken = default)
        {
            _getRewardCompletionSource = new(cancellationToken);

            // request the reward
            var args = new GetRewardEventArgs(rewardOptions, puzzle);
            RewardChoiceRequested?.Invoke(this, args);

            // wait until the reward has been set from the outside
            return await _getRewardCompletionSource.Task;
        }

        private bool TryFindMyInfo(PlayerState.PlayerInfo[] playerInfos, out PlayerState.PlayerInfo? result)
        {
            foreach (var playerInfo in playerInfos) {
                if (playerInfo.PlayerId == Id) {
                    result = playerInfo;
                    return true;
                }
            }
            result = null;
            return false;
        }

        #endregion

        /// <summary>
        /// Provides data for the <see cref="HumanPlayer.ActionRequested"/> event.
        /// </summary>
        public class GetActionEventArgs : EventArgs
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="GetActionEventArgs"/> class with the specified parameters.
            /// </summary>
            /// <param name="gameInfo">Information about the shared resources in the game.</param>
            /// <param name="playerInfo">Information about the resources of the player requesting the action.</param>
            /// <param name="turnInfo">Information about the current turn.</param>
            /// <param name="verifier">The verifier for validating the validity of actions in the current game context.</param>
            public GetActionEventArgs(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo, ActionVerifier verifier)
            {
                GameInfo = gameInfo;
                PlayerInfo = playerInfo;
                TurnInfo = turnInfo;
                Verifier = verifier;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Information about the shared resources in the game.
            /// </summary>
            public GameState.GameInfo GameInfo { get; }

            /// <summary>
            /// Information about the resources of the player requesting the action.
            /// </summary>
            public PlayerState.PlayerInfo PlayerInfo { get; }

            /// <summary>
            /// Information about the current turn.
            /// </summary>
            public TurnInfo TurnInfo { get; }

            /// <summary>
            /// The verifier for validating the validity of actions in the current game context.
            /// </summary>
            public ActionVerifier Verifier { get; }

            #endregion
        }

        /// <summary>
        /// Provides data for the <see cref="HumanPlayer.RewardChoiceRequested"/> event.
        /// </summary>
        public class GetRewardEventArgs : EventArgs
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="GetRewardEventArgs"/> class with the specified reward options and puzzle.
            /// </summary>
            /// <param name="rewardOptions">The list of reward options available to the player.</param>
            /// <param name="puzzle">The puzzle that was completed by the player.</param>
            public GetRewardEventArgs(List<TetrominoShape> rewardOptions, Puzzle puzzle)
            {
                RewardOptions = rewardOptions;
                Puzzle = puzzle;
            }

            #endregion

            #region Properties

            /// <summary>
            /// The list of reward options available to the player.
            /// </summary>
            public List<TetrominoShape> RewardOptions { get; }

            /// <summary>
            /// The puzzle that was completed by the player.
            /// </summary>
            public Puzzle Puzzle { get; }

            #endregion
        }
    }
}
