using NUnit.Framework;
using UnityEngine;
using ProjectLCore.GamePieces;
using ProjectLCore.Players;
using System.Collections.Generic;


#nullable enable

public static class GameEndStats
{
    // player names must be unique
    public static Dictionary<Player, GameEndInfo> PlayerGameEndStats = new();
    public static Dictionary<Player, int> FinalResults = new();
    public static void Clear()
    {
        PlayerGameEndStats.Clear();
        FinalResults.Clear();
    }

    public static void AddFinishedPuzzle(Player player, Puzzle puzzle)
    {
        if (!PlayerGameEndStats.ContainsKey(player)) {
            PlayerGameEndStats[player] = new GameEndInfo();
        }
        PlayerGameEndStats[player].FinishedPuzzles.Add(puzzle);
    }

    public static void AddUnfinishedPuzzle(Player player, Puzzle puzzle)
    {
        if (!PlayerGameEndStats.ContainsKey(player)) {
            PlayerGameEndStats[player] = new GameEndInfo();
        }
        PlayerGameEndStats[player].UnfinishedPuzzles.Add(puzzle);
    }

    public static void AddFinishingTouchTetromino(Player player, TetrominoShape tetromino)
    {
        if (!PlayerGameEndStats.ContainsKey(player)) {
            PlayerGameEndStats[player] = new GameEndInfo();
        }
        PlayerGameEndStats[player].FinishingTouchesTetrominos.Add(tetromino);
    }

    public struct GameEndInfo
    {
        public List<Puzzle> FinishedPuzzles;
        public List<Puzzle> UnfinishedPuzzles;
        public List<TetrominoShape> FinishingTouchesTetrominos;

        public GameEndInfo(List<Puzzle>? finishedPuzzles = null, List<Puzzle>? unfinishedPuzzles = null, List<TetrominoShape>? finishingTouchesTetrominos = null)
        {
            FinishedPuzzles = finishedPuzzles ?? new List<Puzzle>();
            UnfinishedPuzzles = unfinishedPuzzles ?? new List<Puzzle>();
            FinishingTouchesTetrominos = finishingTouchesTetrominos ?? new List<TetrominoShape>();
        }
    }
}
