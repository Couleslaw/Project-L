using Kostra;
using System.Text.Json.Serialization;

namespace Kostra {
    /// <summary>
    /// Represents the type of the player.
    /// </summary>
    enum PlayerType {
        /// <summary>
        /// Human players pick their action using the UI.
        /// </summary>
        Human,

        /// <summary>
        /// AI players pick their action using an algorithm.
        /// </summary>
        AI
    };

    /// <summary>
    /// Represents a player in the game.
    /// </summary>
    abstract class Player {
        private static uint _idCounter = 0;
        /// <summary>
        /// The unique ID of the player.
        /// </summary>
        public uint Id { get; } = _idCounter++;

        /// <summary>
        /// The type of the player.
        /// </summary>
        public abstract PlayerType Type { get; }

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
        /// <returns>The shape the player wants to take.</returns>
        public abstract Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions);
    }

    /// <summary>
    /// Represents a human player in the game.
    /// </summary>
    /// <seealso cref="Kostra.Player" />
    class HumanPlayer : Player {
        public override PlayerType Type => PlayerType.Human;

        // completion sources for setting the result of the async methods
        private TaskCompletionSource<VerifiableAction> _getActionCompletionSource = new();
        private TaskCompletionSource<TetrominoShape> _getRewardCompletionSource = new();

        /// <summary>
        /// Sets the next action the player wants to take. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="action">The action.</param>
        public void SetAction(VerifiableAction action)  => _getActionCompletionSource.SetResult(action);

        /// <summary>
        /// Sets the reward the player wants to get. This method should be called after the player has made a decision through the UI.
        /// </summary>
        /// <param name="reward">The reward.</param>
        public void SetReward(TetrominoShape reward) => _getRewardCompletionSource.SetResult(reward);

        public override async Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier) {
            _getActionCompletionSource = new();
            return await _getActionCompletionSource.Task;
        }

        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions)
        {
            _getRewardCompletionSource = new();
            return await _getRewardCompletionSource.Task;
        }
    }

    /// <summary>
    /// Represents an AI player in the game.
    /// </summary>
    /// <seealso cref="Kostra.Player" />
    abstract class AIPlayerBase : Player {
        public override PlayerType Type => PlayerType.AI;

        /// <summary>
        /// Function for initializing the AI player. This function is called once at the beginning of the game.
        /// </summary>
        /// <param name="filePath">The path to a file where the player might be storing some information.</param>
        public abstract void Init(string? filePath);

        /// <summary>
        /// Implementation of an algorithm that decides the action the player wants to take based on the current game context.
        /// </summary>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="myInfo">Information about the resources of THIS player</param>
        /// <param name="enemyInfos">Information about the resources of the OTHER players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <returns>The action the player wants to take.</returns>
        public abstract VerifiableAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier);

        public override async Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier) {
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
        /// Implementation of an algorithm that decides the shape the player wants as a reward for completing a puzzle.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <returns>The shape the player wants to take.</returns>
        public abstract TetrominoShape GetReward(List<TetrominoShape> rewardOptions);

        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions)
        {
            // call the method that implements the AI algorithm
            return await Task.Run(() => GetReward(rewardOptions));
        }
    }
}

