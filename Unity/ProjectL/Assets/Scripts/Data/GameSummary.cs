#nullable enable

namespace ProjectL.Data
{
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System.Collections.Generic;
    using UnityEngine;


    /// <summary>
    /// Provides functionality to track information about players needed to calculate the final score.
    /// </summary>
    public static class GameSummary
    {
        #region Fields

        /// <summary>
        /// Stores <see cref="Stats"/> information about each player.
        /// </summary>
        public static Dictionary<Player, Stats> PlayerStats { get; private set; } = new();

        /// <summary>
        /// Stores the final order of the players.
        /// </summary>
        public static Dictionary<Player, int> FinalResults { get; set; } = new();

        #endregion

        #region Methods

        /// <summary>
        /// Clears all stored information.
        /// </summary>
        public static void Clear()
        {
            PlayerStats.Clear();
            FinalResults.Clear();
        }

        /// <summary>
        /// Sets the number of leftover tetrominos for the specified player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="numLeftoverTetrominos">The number of leftover tetrominos.</param>
        public static void SetNumLeftoverTetrominos(Player player, int numLeftoverTetrominos)
        {
            if (!PlayerStats.ContainsKey(player)) {
                PlayerStats.Add(player, new Stats());
            }
            PlayerStats[player].NumLeftoverTetrominos = numLeftoverTetrominos;
        }

        /// <summary>
        /// Adds a finished puzzle to the specified player's info.
        /// </summary>
        /// <param name="player">The player who finished the puzzle.</param>
        /// <param name="puzzle">The puzzle that was finished.</param>
        public static void AddFinishedPuzzle(Player player, Puzzle puzzle)
        {
            if (!PlayerStats.ContainsKey(player)) {
                PlayerStats.Add(player, new Stats());
            }
            PlayerStats[player].FinishedPuzzles.Add(puzzle);
        }

        /// <summary>
        /// Adds an unfinished puzzle to the specified player's info.
        /// </summary>
        /// <param name="player">The player who did not finish the puzzle.</param>
        /// <param name="puzzle">The puzzle that was not finished.</param>
        public static void AddUnfinishedPuzzle(Player player, Puzzle puzzle)
        {
            Debug.Log($"Added unfinished puzzle for player {player.Name}, id={puzzle.Id}");
            if (!PlayerStats.ContainsKey(player)) {
                PlayerStats.Add(player, new Stats());
            }
            PlayerStats[player].UnfinishedPuzzles.Add(puzzle);
        }

        /// <summary>
        /// Adds a finishing touches tetromino to the specified player's info.
        /// </summary>
        /// <param name="player">The player who placed the tetromino.</param>
        /// <param name="tetromino">The tetromino shape.</param>
        public static void AddFinishingTouchesTetromino(Player player, TetrominoShape tetromino)
        {
            Debug.Log($"Added finishing touches tetromino for player {player.Name}, shape={tetromino}");
            if (!PlayerStats.ContainsKey(player)) {
                PlayerStats.Add(player, new Stats());
            }
            PlayerStats[player].FinishingTouchesTetrominos.Add(tetromino);
        }

        #endregion

        /// <summary>
        /// Represents information needed to calculate the final score for a player.
        /// </summary>
        public class Stats
        {
            #region Fields

            /// <summary>
            /// A list of puzzles that the player has finished.
            /// </summary>
            public List<Puzzle> FinishedPuzzles = new();

            /// <summary>
            /// A list of puzzles that the player has started but did not finished.
            /// </summary>
            public List<Puzzle> UnfinishedPuzzles = new();

            /// <summary>
            /// A list of tetromino shapes that the player placed during finishing touches.
            /// </summary>
            public List<TetrominoShape> FinishingTouchesTetrominos = new();

            /// <summary>
            /// The number of leftover tetrominos that the player has at the end of the game.
            /// </summary>
            public int NumLeftoverTetrominos = 0;

            #endregion
        }
    }
}