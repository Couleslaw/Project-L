using ProjectLCore.GameLogic;
using ProjectLCore.Players;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
public class GameManager : MonoBehaviour
{
    #region Constants

    private const string _puzzleFilePath = "puzzles.txt";

    #endregion

    #region Fields

    private readonly GameCore? _game;

    #endregion

    #region Methods

    internal void Start()
    {
        PrepareGame();
    }

    private void PrepareGame()
    {
        GameState? gameState = LoadGameState();
        if (gameState == null) {
            return;
        }
        Debug.Log("Game state loaded successfully.");

        // create players
        List<Player>? players = LoadPlayers();
        if (players == null) {
            return;
        }
        Debug.Log("Players created successfully.");

        // initialize players
        InitializeAIPlayers(players, gameState);
    }

    private GameState? LoadGameState()
    {
        string path = PlayerTypeLoader.GetAbsolutePath(_puzzleFilePath);
        try {
            return GameState.CreateFromFile(path, GameStartParams.NumInitialTetrominos);
        }
        catch (Exception e) {
            EndGameWithError($"Failed to load game state. {e.Message}");
            return null;
        }
    }

    private List<Player>? LoadPlayers()
    {
        if (GameStartParams.Players.Count == 0) {
            EndGameWithError("No players selected.");
            return null;
        }
        try {
            List<Player> players = new();

            foreach (var playerInfo in GameStartParams.Players) {
                Player player = (Activator.CreateInstance(playerInfo.Value.PlayerType) as Player)!;
                player.Name = playerInfo.Key;
                players.Add(player);
            }
            return players;
        }
        catch (Exception e) {
            EndGameWithError($"Failed to create players: {e.Message}");
            return null;
        }
    }

    private void InitializeAIPlayers(List<Player> players, GameState gameState)
    {
        foreach (Player player in players) {
            if (player is AIPlayerBase aiPlayer) {
                string? initPath = GameStartParams.Players[player.Name].InitPath;
                Task initTask = aiPlayer.InitAsync(players.Count, gameState.GetAllPuzzlesInGame(), initPath);
                Debug.Log($"Initializing AI player {player.Name}. Init file: {initPath}");

                // handle possible exception
                initTask.ContinueWith(t => {
                    if (t.Exception != null) {
                        Debug.LogError($"Initialization of player {player.Name} failed: {t.Exception.InnerException?.Message}");
                        EndGameWithError("Failed to initialize AI player.");
                    }
                    else {
                        Debug.Log($"AI player {player.Name} initialized successfully.");
                    }
                });
            }
        }
    }

    private void EndGameWithError(string error)
    {
        Debug.LogError("ENDING: " + error);
    }

    #endregion
}
