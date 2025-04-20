using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectLCore.Players;
public static class GameStartParams
{
    public const int NumInitialTetrominosDefault = 15;
    public const bool ShufflePlayersDefault = true;
    public static int NumInitialTetrominos { get; set; } = NumInitialTetrominosDefault;
    public static bool ShufflePlayers { get; set; } = ShufflePlayersDefault;
    public static List<(string, LoadedPlayerTypeInfo)> Players { get; set; } = new();

    public static void Reset()
    {
        NumInitialTetrominos = NumInitialTetrominosDefault;
        ShufflePlayers = ShufflePlayersDefault;
        Players.Clear();
    }
}
