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
    /// Represents an AI player in the game. AI players pick their action using an algorithm by implementing the <see cref="GetAction(GameState.GameInfo, PlayerState.PlayerInfo, List{PlayerState.PlayerInfo}, TurnInfo, ActionVerifier)"/> and <see cref="GetReward"/> methods.
    /// </summary>
    /// <seealso cref="Player" />
    public abstract class AIPlayerBase : Player
    {
        #region Fields

        private bool _isInitialized = false;

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously passes the parameters to <see cref="Init"/> and initializes the player. This method should be called once at the beginning of the game.
        /// </summary>
        /// <param name="numPlayers">The number of players in the game.</param>
        /// <param name="allPuzzles">All the puzzles in the game.</param>
        /// <param name="filePath">The path to a file where the player might be storing some information.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">The task was canceled.</exception>
        public async Task InitAsync(int numPlayers, List<Puzzle> allPuzzles, string? filePath = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() => Init(numPlayers, allPuzzles, filePath), cancellationToken);
            _isInitialized = true;
        }

        /// <summary>
        /// Asynchronously passes the parameters to <see cref="GetAction(GameState.GameInfo, PlayerState.PlayerInfo, List{PlayerState.PlayerInfo}, TurnInfo, ActionVerifier)"/> and returns a <see cref="Task"/> containing the result.
        /// </summary>
        /// <remarks>
        /// This methods asynchronously waits for initialization of the AI player. It will not return until <see cref="InitAsync"/> is called and finished running.
        /// </remarks>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="playerInfos">Information about the resources of the players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task representing the action the player wants to take.
        /// </returns>
        /// <exception cref="System.ArgumentException"><see cref="PlayerState"/> matching this player's <see cref="Player.Id"/> not found in <paramref name="playerInfos"/>.</exception>
        /// <exception cref="OperationCanceledException">The task was canceled.</exception>
        public override sealed async Task<GameAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // check if the player has been initialized
            if (!_isInitialized) {
                await WaitUntil(() => _isInitialized, cancellationToken: cancellationToken);
            }

            return await Task.Run(() => GetAction(gameInfo, playerInfos, turnInfo, verifier), cancellationToken);
        }

        /// <summary>
        /// Passes the parameters to <see cref="GetAction(GameState.GameInfo, PlayerState.PlayerInfo, List{PlayerState.PlayerInfo}, TurnInfo, ActionVerifier)"/> and returns the result.
        /// This method should be called only after <see cref="InitAsync"/> has been called and finished running.
        /// </summary>
        /// <remarks>
        /// This method should be used for training AI players as it doesn't come with the async performance overhead of <see cref="GetActionAsync(GameState.GameInfo, PlayerState.PlayerInfo[], TurnInfo, ActionVerifier, CancellationToken)"/>.
        /// It also shouldn't be used outside of CLI applications as it will block the main thread until <see cref="GetAction(GameState.GameInfo, PlayerState.PlayerInfo, List{PlayerState.PlayerInfo}, TurnInfo, ActionVerifier)"/> returns.
        /// </remarks>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="playerInfos">Information about the resources of the players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <returns>
        /// The action the player wants to take.
        /// </returns>
        /// <exception cref="System.ArgumentException"><see cref="PlayerState"/> matching this player's <see cref="Player.Id"/> not found in <paramref name="playerInfos"/>.</exception>
        /// <exception cref="PlayerNotInitializedException">The player has not been initialized yet.</exception>"
        public GameAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            if (!_isInitialized) {
                throw new PlayerNotInitializedException();
            }

            // extract the state of THIS player and the OTHER players from playerInfos
            PlayerState.PlayerInfo? myState = null;
            List<PlayerState.PlayerInfo> enemyStates = new();
            for (int i = 0; i < playerInfos.Length; i++) {
                if (playerInfos[i].PlayerId == Id) {
                    myState = playerInfos[i];
                }
                else {
                    enemyStates.Add(playerInfos[i]);
                }
            }
            if (myState == null) {
                throw new ArgumentException($"PlayerState for player {Id} not found!");
            }

            // call the method that implements the AI algorithm
            return GetAction(gameInfo, myState, enemyStates, turnInfo, verifier);
        }

        /// <summary>
        /// Asynchronously passes the parameters to <see cref="GetReward"/> and returns a <see cref="Task"/> containing the result.
        /// Note that the player doesn't get the current game context here.
        /// This is because this function will be called right after he completes a puzzle and therefore he knows the current game state from the last <see cref="GetActionAsync" /> call.
        /// </summary>
        /// <param name="rewardOptions">A nonempty list containing rewards to choose from.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <remarks>If there is only one reward option, it will be chosen automatically and <see cref="GetReward"/> will not be called.</remarks>
        /// <returns> A task representing the tetromino the player wants to take. </returns>
        /// <exception cref="OperationCanceledException">The task was canceled.</exception>
        public override sealed async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions, Puzzle puzzle, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // if there is only 1 reward option, return it immediately
            if (rewardOptions.Count == 1) {
                return rewardOptions[0];
            }
            // otherwise call the method that implements the AI algorithm
            return await Task.Run(() => GetReward(rewardOptions, puzzle), cancellationToken);
        }

        /// <summary>
        /// Function for initializing the AI player. This function is called once at the beginning of the game.
        /// </summary>
        /// <param name="numPlayers">The number of players in the game.</param>
        /// <param name="allPuzzles">All the puzzles in the game.</param>
        /// <param name="filePath">The path to a file where the player might be storing some information.</param>
        protected abstract void Init(int numPlayers, List<Puzzle> allPuzzles, string? filePath);

        /// <summary>
        /// Implementation of an algorithm that decides the action the player wants to take based on the current game context.
        /// </summary>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="myInfo">Information about the resources of THIS player</param>
        /// <param name="enemyInfos">Information about the resources of the OTHER players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <returns>The action the player wants to take.</returns>
        protected abstract GameAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier);

        /// <summary>
        /// Implementation of an algorithm that decides the shape the player wants as a reward for completing a puzzle.
        /// </summary>
        /// <param name="rewardOptions">A nonempty list containing rewards to choose from.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <returns>The tetromino the player wants to take.</returns>
        protected abstract TetrominoShape GetReward(List<TetrominoShape> rewardOptions, Puzzle puzzle);

        /// <summary>
        /// Waits until a condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked, in milliseconds.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        private static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var waitTask = Task.Run(async () => {
                while (!condition()) {
                    await Task.Delay(frequency, cancellationToken);
                }
            }, cancellationToken);

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout, cancellationToken))) {
                throw new TimeoutException();
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents an exception that is thrown when the player is not initialized but he is expected to be.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class PlayerNotInitializedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerNotInitializedException"/> class.
        /// </summary>
        public PlayerNotInitializedException() : base("Player not initialized!") { }
    }
}
