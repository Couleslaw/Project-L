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
    /// Represents an AI player in the game.
    /// </summary>
    /// <seealso cref="Player" />
    abstract class AIPlayerBase : Player
    {
        public override PlayerType Type => PlayerType.AI;

        /// <summary>
        /// Function for initializing the AI player. This function is called once at the beginning of the game.
        /// </summary>
        /// <param name="numPlayers">The number of players in the game.</param>
        /// <param name="numInitialTetrominos">The number of available tetrominos of each shape at the start of the game.</param>
        /// <param name="filePath">The path to a file where the player might be storing some information.</param>
        public abstract void Init(int numPlayers, int numInitialTetrominos, string? filePath);

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

        public override async Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            // extract the state of THIS player and the OTHER players from playerInfos
            PlayerState.PlayerInfo? myState = null;
            List<PlayerState.PlayerInfo> enemyStates = new();
            for (int i = 0; i < playerInfos.Length; i++)
            {
                if (playerInfos[i].PlayerId == Id)
                {
                    myState = playerInfos[i];
                }
                else
                {
                    enemyStates.Add(playerInfos[i]);
                }
            }
            if (myState == null)
            {
                throw new ArgumentException($"PlayerState for player {Id} not found!");
            }

            // call the method that implements the AI algorithm
            return await Task.Run(() => GetAction(gameInfo, myState, enemyStates, turnInfo, verifier));
        }

        /// <summary>
        /// Implementation of an algorithm that decides the shape the player wants as a reward for completing a puzzle.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <returns>The shape the player wants to take.</returns>
        public abstract TetrominoShape GetReward(List<TetrominoShape> rewardOptions, Puzzle puzzle);

        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions, Puzzle puzzle)
        {
            // call the method that implements the AI algorithm
            return await Task.Run(() => GetReward(rewardOptions, puzzle));
        }
    }
}
