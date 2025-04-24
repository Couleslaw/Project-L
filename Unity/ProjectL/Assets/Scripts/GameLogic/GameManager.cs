using ProjectLCore.GameActions.Verification;
using ProjectLCore.GameActions;
using ProjectLCore.GameLogic;
using ProjectLCore.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ProjectLCore.GameManagers;
using ProjectLCore.GamePieces;
using System.Text;
using Unity.VisualScripting;
using System.IO;
using System.Resources;

#nullable enable
public class GameManager : MonoBehaviour
{
    #region Constants

    private const string _puzzleFilePath = "puzzles";

    #endregion

    #region Fields

    [Header("UI Elements")]
    [SerializeField] private GameObject? loggerPrefab;
    [SerializeField] private GameObject? errorMessageBoxPrefab;
    [SerializeField] private GameObject? gameEndedBoxPrefab;


    private GameCore? _game;
    private bool _gameEndedWithError = false;

    #endregion

    #region Methods

    private void Awake()
    {
        // create a logger instance if it doesn't exist
        if (EasyUI.Logger.Instance == null)
            Instantiate(loggerPrefab);
        else
            EasyUI.Logger.Instance.gameObject.SetActive(true);
    }
    private async void Start()
    {
        // create game core
        _game = CreateGameCore();
        if (_game == null)
            return;

        // initialize players
        if (!_gameEndedWithError) {
            await InitializeAIPlayersAsync(_game.Players, _game.GameState);
        }

        // game loop
        if (!_gameEndedWithError) {
            GameEndStats.Clear();
            await GameLoopAsync();
        }

        // prepare final results
        if (!_gameEndedWithError) {
            PrepareGameEndStats();
            GoToFinalResultsScreen();
        }
    }

    private GameState? LoadGameState()
    {
        string? puzzleFileText = Resources.Load<TextAsset>(_puzzleFilePath)?.text;

        if (string.IsNullOrEmpty(puzzleFileText)) {
            EndGameWithError($"Puzzles file is empty.");
            return null;
        }

        try {
            Stream stream = GenerateStreamFromString(puzzleFileText);
            return GameState.CreateFromStream(stream, GameStartParams.NumInitialTetrominos);
        }
        catch (Exception e) {
            EndGameWithError($"Failed to load game state. {e.Message}");
            return null;
        }

        static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
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


    /// <summary>
    /// Tries to load the puzzles and create players. If successful, it returns a new <see cref="GameCore"/> instance.
    /// </summary>
    /// <returns></returns>
    private GameCore? CreateGameCore()
    {
        GameState? gameState = LoadGameState();
        if (gameState == null) {
            return null;
        }
        Debug.Log("Game state loaded successfully.");

        // create players
        List<Player>? players = LoadPlayers();
        if (players == null) {
            return null;
        }
        Debug.Log("Players created successfully.");

        return new GameCore(gameState, players, GameStartParams.ShufflePlayers);
    }


    private async Task InitializeAIPlayersAsync(Player[] players, GameState gameState)
    {
        List<Task> initializationTasks = new();

        foreach (Player player in players) {
            if (player is AIPlayerBase aiPlayer) {
                string? initPath = GameStartParams.Players[player.Name].InitPath;
                Debug.Log($"Initializing AI player {player.Name}. Init file: {initPath}");

                Task initTask = aiPlayer.InitAsync(players.Length, gameState.GetAllPuzzlesInGame(), initPath)
                    .ContinueWith(t => {
                        if (t.Exception != null) {
                            Debug.LogError($"Initialization of player {player.Name} failed: {t.Exception.InnerException?.Message}");
                            EndGameWithError($"Failed to initialize AI player {player.Name}.");
                        }
                        else {
                            Debug.Log($"AI player {player.Name} initialized successfully.");
                        }
                    });

                initializationTasks.Add(initTask);
            }
        }

        await Task.WhenAll(initializationTasks);
    }

    /// <summary>
    /// Creates a ErrorMessageBox and logs the error message.
    /// </summary>
    /// <param name="error"></param>
    private void EndGameWithError(string error)
    {
        if (_gameEndedWithError)
            return;
        _gameEndedWithError = true;

        Debug.LogError("Fatal error: " + error);
        if (errorMessageBoxPrefab != null) {
            Instantiate(errorMessageBoxPrefab);
        }
        else {
            Debug.LogError("Error message prefab is not set.");
        }
    }

    private void PrepareGameEndStats()
    {
        if (_game == null) {
            Debug.LogError("GameCore is null. Cannot prepare end game stats.");
            return; // safety check
        }
        
        // add final results
        GameEndStats.FinalResults = _game.GetFinalResults();

        // add info about unfinished puzzles
        foreach (Player player in _game.Players) {
            var state = _game.PlayerStates[player];
            foreach (Puzzle puzzle in state.GetUnfinishedPuzzles()) {
                GameEndStats.AddUnfinishedPuzzle(player, puzzle);
            }
        }
    }

    private void GoToFinalResultsScreen()
    {
        if (gameEndedBoxPrefab != null) {
            Instantiate(gameEndedBoxPrefab);
        }
        else {
            Debug.LogError("Game ended box prefab is not set.");
        }
    }


    private async Task GameLoopAsync()
    {
        if (_game == null) {
            Debug.LogError("GameCore is null. Cannot start game loop.");
            return; // safety check
        }

        Debug.Log("Starting game loop.");

        while (true) {
            TurnInfo turnInfo = _game.GetNextTurnInfo();

            // check if game ended
            if (_game.CurrentGamePhase == GamePhase.Finished) {
                Debug.Log("Game ended!");
                _game.GameEnded();
                break;
            }

            // create verifier for the current player
            var gameInfo = _game.GameState.GetGameInfo();
            var playerInfos = _game.GetPlayerInfos();
            var currentPlayerInfo = _game.PlayerStates[_game.CurrentPlayer].GetPlayerInfo();
            var verifier = new ActionVerifier(gameInfo, currentPlayerInfo, turnInfo);

            // get action from player
            IAction? action;
            try {
                action = await _game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier);
                if (action == null) {
                    LogPlayerGetActionReturnedNull();
                }
            }
            catch (Exception e) {
                LogPlayerGetActionThrownException(e.Message);
                action = null;
            }

            // verify if got some action
            if (action != null) {
                var result = verifier.Verify(action);
                if (result is VerificationFailure fail) {
                    LogPlayerProvidedInvalidAction(action, fail);
                    action = null;
                }
            }

            // assign default action if null
            if (action == null) {
                action = GetDefaultAction();
                LogDefaultFunctionAssignment(action);
            }

            // process valid action
            _game.ProcessAction(action);

            // if not Finishing touches --> log finished puzzles
            if (_game.CurrentGamePhase != GamePhase.FinishingTouches) {
                while (_game.TryGetNextPuzzleFinishedBy(_game.CurrentPlayer, out var finishedPuzzleInfo)) {
                    LogPlayerFinishedPuzzle(finishedPuzzleInfo);
                    GameEndStats.AddFinishedPuzzle(_game.CurrentPlayer, finishedPuzzleInfo.Puzzle);
                }
            }

            // if finishing touches --> log used tetrominos
            if (_game.CurrentGamePhase == GamePhase.FinishingTouches && action is PlaceTetrominoAction a) {
                GameEndStats.AddFinishingTouchTetromino(_game.CurrentPlayer, a.Shape);
            }
        }
    }

    private void LogPlayerGetActionThrownException(string message)
    {
        Debug.LogWarning($"{_game?.CurrentPlayer.Name} failed to provide an action with error: {message}.");
    }

    private void LogPlayerGetActionReturnedNull()
    {
        Debug.LogWarning($"{_game?.CurrentPlayer.Name} provided no action.");
    }

    private void LogPlayerProvidedInvalidAction(IAction action, VerificationFailure fail)
    {
        Console.WriteLine($"{_game?.CurrentPlayer.Name} provided an invalid {action}\nVerification result:\n{fail.GetType()}: {fail.Message}\n");
    }

    private IAction GetDefaultAction()
    {
        return (_game?.CurrentGamePhase == GamePhase.FinishingTouches)
                    ? new EndFinishingTouchesAction()
                    : new DoNothingAction();
    }

    private void LogDefaultFunctionAssignment(IAction action)
    {
        Console.WriteLine($"{_game?.CurrentPlayer.Name} provided no action. Defaulting to {action}");
    }

    private void LogPlayerFinishedPuzzle(FinishedPuzzleInfo puzzleInfo)
    {
        Console.WriteLine($"{_game?.CurrentPlayer.Name} completed puzzle with ID={puzzleInfo.Puzzle.Id}");
        Console.WriteLine($"   Returned pieces: {GetUsedTetrominos()}");
        Console.WriteLine($"   Reward: {puzzleInfo.SelectedReward}");
        Console.WriteLine($"   Points: {puzzleInfo.Puzzle.RewardScore}");

        string GetUsedTetrominos()
        {
            StringBuilder sb = new StringBuilder();
            int[] tetrominos = new int[TetrominoManager.NumShapes];
            foreach (var tetromino in puzzleInfo.Puzzle.GetUsedTetrominos()) {
                tetrominos[(int)tetromino]++;
            }

            bool first = true;
            for (int i = 0; i < tetrominos.Length; i++) {
                if (tetrominos[i] == 0) {
                    continue;
                }
                if (first) {
                    first = false;
                }
                else {
                    sb.Append(", ");
                }
                sb.Append($"{(TetrominoShape)i}: {tetrominos[i]}");
            }
            return sb.ToString();
        }
    }


    #endregion
}
