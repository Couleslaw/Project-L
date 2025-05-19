#nullable enable

namespace ProjectL.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides parameters and settings for starting a game.
    /// </summary>
    public static class GameSettings
    {
        #region Constants

        /// <summary>
        /// The default number of initial Tetrominos.
        /// </summary>
        private const int _defaultNumInitialTetrominos = 15;

        /// <summary>
        /// The default value indicating whether players should be shuffled.
        /// </summary>
        private const bool _defaultShouldShufflePlayers = true;

        #endregion

        #region Fields

        /// <summary>
        /// Sais the number of black puzzles in the game for each number of players.
        /// </summary>
        private static readonly Dictionary<int, int> _numPlayersToNumBlackPuzzles = new() {
        {1, 10}, {2, 12}, {3, 14}, {4, 16}
    };

        #endregion

        #region Properties

        /// <summary>
        /// The number of initial Tetrominos. Defaults to <see cref="_defaultNumInitialTetrominos"/>.
        /// </summary>
        public static int NumInitialTetrominos { get; set; } = _defaultNumInitialTetrominos;

        /// <summary>
        /// Indicates whether players should be shuffled. Defaults to <see cref="_defaultShouldShufflePlayers"/>.
        /// </summary>
        public static bool ShouldShufflePlayers { get; set; } = _defaultShouldShufflePlayers;

        /// <summary>
        /// Dictionary of player names and their associated player type information.
        /// Note that this implies that all player names must be unique.
        /// </summary>
        public static Dictionary<string, PlayerTypeInfo> Players { get; set; } = new();

        public static int NumBlackPuzzles {
            get {
                if (_numPlayersToNumBlackPuzzles.ContainsKey(Players.Count)) 
                    return _numPlayersToNumBlackPuzzles[Players.Count];
                else
                    return 0;
            }
        }

        #endregion

    }
}