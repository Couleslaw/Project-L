namespace ProjectLCore.Players
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an AI player in the game. AI players pick their action using an algorithm by implementing the <see cref="GetAction"/> and <see cref="GetReward"/> methods.
    /// </summary>
    /// <seealso cref="Player" />
    public abstract class AIPlayerBase : Player
    {
        #region Fields

        private bool _isInitialized = false;

        #endregion

        #region Methods

        /// <summary>
        /// Asynchronously passes the parameters to <see cref="Init"/> and initializes the player. This function should be called once at the beginning of the game.
        /// </summary>
        /// <param name="numPlayers">The number of players in the game.</param>
        /// <param name="allPuzzles">All the puzzles in the game.</param>
        /// <param name="filePath">The path to a file where the player might be storing some information.</param>
        public async void InitAsync(int numPlayers, List<Puzzle> allPuzzles, string? filePath = null)
        {
            await Task.Run(() => Init(numPlayers, allPuzzles, filePath));
            _isInitialized = true;
        }

        /// <summary>
        /// Asynchronously passes the parameters to <see cref="GetAction"/> and returns a <see cref="Task"/> containing the result.
        /// </summary>
        /// <remarks>
        /// This methods waits for initialization of the AI player. It will not return until <see cref="InitAsync(int, List{Puzzle}, string?)"/> is called and finished running.
        /// </remarks>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="playerInfos">Information about the resources of the players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <returns>
        /// The action the player wants to take.
        /// </returns>
        /// <exception cref="System.ArgumentException"><see cref="PlayerState"/> matching this player's <see cref="Player.Id"/> not found in <paramref name="playerInfos"/>.</exception>
        public override sealed async Task<IAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            // check if the player has been initialized
            if (!_isInitialized) {
                await WaitUntil(() => _isInitialized);
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
            return await Task.Run(() => GetAction(gameInfo, myState, enemyStates, turnInfo, verifier));
        }

        /// <summary>
        /// Asynchronously passes the parameters to <see cref="GetReward"/> and returns a <see cref="Task"/> containing the result.
        /// Note that the player doesn't get the current game context here.
        /// This is because this function will be called right after he completes a puzzle and therefore he knows the current game state from the last <see cref="GetActionAsync" /> call.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <returns>
        /// The tetromino the player wants to take.
        /// </returns>
        public override sealed async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions, Puzzle puzzle)
        {
            // call the method that implements the AI algorithm
            return await Task.Run(() => GetReward(rewardOptions, puzzle));
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
        protected abstract IAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier);

        /// <summary>
        /// Implementation of an algorithm that decides the shape the player wants as a reward for completing a puzzle.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <returns>The tetromino the player wants to take.</returns>
        protected abstract TetrominoShape GetReward(List<TetrominoShape> rewardOptions, Puzzle puzzle);

        /// <summary>
        /// Waits until a condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked, in milliseconds.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        private static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () => {
                while (!condition()) {
                    await Task.Delay(frequency);
                }
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout))) {
                throw new TimeoutException();
            }
        }

        #endregion
    }
}
