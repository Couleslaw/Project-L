namespace Kostra.GameManagers
{
    using Kostra.GameActions;
    using Kostra.GamePieces;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Utility class for managing rewards and upgrades for players.
    /// </summary>
    internal static class RewardManager
    {
        #region Methods

        /// <summary>Gets the shapes the player can choose from as a reward for completing a puzzles with the given <c>shape</c> as reward.</summary>
        /// <param name="numTetrominosLeft">Contains information about how many tetrominos are left in the shared reserve. <c>numTetrominosLeft[shape]</c> gives information about <c>(<see cref="TetrominoShape" />)shape</c>.</param>
        /// <param name="shape">The shape specified on the puzzle.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item>The <c>shape</c> specified on the puzzle if there is at least one left. </item>
        ///     <item>Shapes of the next available level if the given shape isn't available. </item>
        ///     <item>Shapes of all the lower levels if there aren't any shapes with <c>level &gt;= level(shape)</c> available.</item>
        ///   </list>
        /// </returns>
        /// <exception cref="ArgumentException">Invalid numTetrominosLeft length</exception>
        public static List<TetrominoShape> GetRewardOptions(IReadOnlyList<int> numTetrominosLeft, TetrominoShape shape)
        {
            if (numTetrominosLeft.Count != TetrominoManager.NumShapes) {
                throw new ArgumentException("Invalid numTetrominosLeft length");
            }

            if (numTetrominosLeft[(int)shape] > 0) {
                return new List<TetrominoShape> { shape };
            }

            var result = new List<TetrominoShape>();
            for (int level = TetrominoManager.GetLevelOf(shape); level <= TetrominoManager.MaxLevel; level++) {
                foreach (var s in TetrominoManager.GetShapesWithLevel(level)) {
                    if (numTetrominosLeft[(int)s] > 0) {
                        result.Add(s);
                    }
                }
                if (result.Count > 0) {
                    return result;
                }
            }

            // result.Count == 0
            // there are no higher level tetrominos left --> choose from lover level ones 
            for (int level = 0; level < TetrominoManager.GetLevelOf(shape); level++) {
                foreach (var s in TetrominoManager.GetShapesWithLevel(level)) {
                    if (numTetrominosLeft[(int)s] > 0) {
                        result.Add(s);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the shapes the player can get in exchange for the given <c>shape</c> by using <see cref="ChangeTetrominoAction"/>.
        /// </summary>
        /// <param name="numTetrominosLeft">Contains information about how many tetrominos are left in the shared reserve. <c>numTetrominosLeft[shape]</c> gives information about <c>(<see cref="TetrominoShape" />)shape</c>.</param>
        /// <param name="shape">The shape the player wants to trade.</param>
        /// <returns>
        /// A list of shapes with <c>level(shape) &lt;= level(oldShape)+1</c>. If there are no shapes with <c>level(oldShape)+1</c> available, the player can choose from the next available level.
        /// </returns>
        /// <exception cref="ArgumentException">Invalid numTetrominosLeft length</exception>
        public static List<TetrominoShape> GetUpgradeOptions(IReadOnlyList<int> numTetrominosLeft, TetrominoShape shape)
        {
            if (numTetrominosLeft.Count != TetrominoManager.NumShapes) {
                throw new ArgumentException("Invalid numTetrominosLeft length");
            }

            int oldLevel = TetrominoManager.GetLevelOf(shape);
            var result = new List<TetrominoShape>();
            // first try to find shapes with level(oldShape)+1
            for (int level = oldLevel + 1; level <= TetrominoManager.MaxLevel; level++) {
                foreach (var s in TetrominoManager.GetShapesWithLevel(level)) {
                    if (numTetrominosLeft[(int)s] > 0) {
                        result.Add(s);
                    }
                }
                if (result.Count > 0) {
                    break;
                }
            }
            // now add all shapes of level <= oldLevel
            for (int level = TetrominoManager.MinLevel; level <= oldLevel; level++) {
                foreach (var s in TetrominoManager.GetShapesWithLevel(level)) {
                    if (numTetrominosLeft[(int)s] > 0) {
                        result.Add(s);
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
