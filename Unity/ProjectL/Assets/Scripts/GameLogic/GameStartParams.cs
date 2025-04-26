using System.Collections.Generic;

/// <summary>
/// Provides parameters and settings for starting a game.
/// </summary>
public static class GameStartParams
{
    #region Constants

    /// <summary>
    /// The default number of initial Tetrominos.
    /// </summary>
    public const int NumInitialTetrominosDefault = 15;

    /// <summary>
    /// The default value indicating whether players should be shuffled.
    /// </summary>
    public const bool ShufflePlayersDefault = true;

    #endregion

    #region Fields

    private static readonly Dictionary<int, int> _numPlayersToNumBlackPuzzles = new Dictionary<int, int>() {
        {1, 10}, {2, 12}, {3, 14}, {4, 16}
    };

    #endregion

    #region Properties

    /// <summary>
    /// The number of initial Tetrominos. Defaults to <see cref="NumInitialTetrominosDefault"/>.
    /// </summary>
    public static int NumInitialTetrominos { get; set; } = NumInitialTetrominosDefault;

    /// <summary>
    /// Indicates whether players should be shuffled. Defaults to <see cref="ShufflePlayersDefault"/>.
    /// </summary>
    public static bool ShufflePlayers { get; set; } = ShufflePlayersDefault;

    /// <summary>
    /// Dictionary of player names and their associated player type information.
    /// Note that this implies that all player names must be unique.
    /// </summary>
    public static Dictionary<string, LoadedPlayerTypeInfo> Players { get; set; } = new();

    public static int NumBlackPuzzles {
        get {
            if (_numPlayersToNumBlackPuzzles.ContainsKey(Players.Count))
                return _numPlayersToNumBlackPuzzles[Players.Count];
            else
                return 0;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Resets all game start parameters to their default values and clears the player list.
    /// </summary>
    public static void Reset()
    {
        NumInitialTetrominos = NumInitialTetrominosDefault;
        ShufflePlayers = ShufflePlayersDefault;
        Players.Clear();
    }

    #endregion
}
