using ProjectLCore.GameActions;
using ProjectLCore.GameActions.Verification;
using ProjectLCore.GameLogic;
using ProjectLCore.GameManagers;
using ProjectLCore.GamePieces;
using ProjectLCore.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

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

    [Header("Text game")]
    [SerializeField] private TextBasedGame? textGame;

    private GameCore? _game;

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves the current scores of all players in the game.
    /// </summary>
    /// <returns>
    /// A dictionary where the key is the player's name and the value is their score; 
    /// or <see langword="null"/> if the game is not initialized.
    /// </returns>
    public Dictionary<string, int>? GetPlayerScores()
    {
        if (_game == null) {
            return null;
        }

        var scores = new Dictionary<string, int>();
        foreach (var player in _game.Players) {
            scores[player.Name] = _game.PlayerStates[player].Score;
        }

        return scores;
    }

    /// <summary>
    /// Gets the name of the current player.
    /// </summary>
    /// <returns>
    /// The name of the current player, or <see langword="null"/> if the game is not initialized.
    /// </returns>
    public string? GetCurrentPlayerName()
    {
        if (_game == null) {
            return null;
        }
        return _game.CurrentPlayer.Name;
    }

    /// <summary>
    /// Retrieves the number of actions left for the current player in their turn.
    /// </summary>
    /// <returns>
    /// The number of actions left, or <see langword="null"/> if the game is not initialized.
    /// </returns>
    public int? GetCurrentPlayerActionsLeft()
    {
        if (_game == null) {
            return null;
        }
        return _game.CurrentTurn.NumActionsLeft;
    }

    /// <summary>
    /// Gets the current phase of the game.
    /// </summary>
    /// <returns>
    /// The current game phase, or <see langword="null"/> if the game is not initialized.
    /// </returns>
    public GamePhase? GetCurrentGamePhase()
    {
        if (_game == null) {
            return null;
        }
        return _game.CurrentGamePhase;
    }

    private void Awake()
    {
        // create a logger instance if it doesn't exist
        if (EasyUI.Logger.Instance == null)
            Instantiate(loggerPrefab);
        else
            EasyUI.Logger.Instance.gameObject.SetActive(true);

        // setup error message box
        if (errorMessageBoxPrefab != null) {
            ErrorGameEnd.Setup(errorMessageBoxPrefab);
        }
        else {
            Debug.LogError("Error message box prefab is not set.");
        }
    }

    private async void Start()
    {
        // create game core
        _game = CreateGameCore();

        // if error occurred --> end
        if (_game == null) {
            return;
        }

        // initialize game
        _game.InitializeGame();

        // initialize players
        InitializeAIPlayersAsync(_game.Players, _game.GameState);

        // game loop
        GameEndStats.Clear();
        await textGame!.GameLoopAsync(_game);


        // final results
        PrepareGameEndStats();
        GoToFinalResultsScreen();
    }

    private void Update()
    {
        if (ErrorGameEnd.ShouldEndGameWithError && !ErrorGameEnd.EndedGameWithError) {
            ErrorGameEnd.EndGameWithError();
        }
    }

    /// <summary>
    /// Tries to load puzzles from the Resources folder and create a <see cref="GameState"/> instance.
    /// </summary>
    /// <returns>A <see cref="GameState"/> instance if successful; otherwise <see langword="null"/>.</returns>
    private GameState? LoadGameState()
    {
        if (ErrorGameEnd.ShouldEndGameWithError) {
            return null;
        }

        // read the puzzles file
        string? puzzleFileText = Resources.Load<TextAsset>(_puzzleFilePath)?.text;

        // check if it contains any text
        if (string.IsNullOrEmpty(puzzleFileText)) {
            ErrorGameEnd.FatalErrorOccurred($"Puzzles file is empty.");
            return null;
        }

        // try to parse the puzzles and create a game state
        try {
            return GameState.CreateFromStream(
                puzzleStream: GenerateStreamFromString(puzzleFileText),
                numInitialTetrominos: GameStartParams.NumInitialTetrominos,
                numBlackPuzzles: GameStartParams.NumBlackPuzzles
                );
        }
        catch (Exception e) {
            ErrorGameEnd.FatalErrorOccurred($"Failed to load game state. {e.Message}");
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

    /// <summary>
    /// Tries to create players based on the player types specified in the game creation scene.
    /// </summary>
    /// <returns>A list of instantiated players if successful; otherwise <see langword="null"/>.</returns>
    private List<Player>? LoadPlayers()
    {
        if (ErrorGameEnd.ShouldEndGameWithError) {
            return null;
        }

        // check if player selection happened
        if (GameStartParams.Players.Count == 0) {
            ErrorGameEnd.FatalErrorOccurred("No players selected.");
            return null;
        }

        // try to create players
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
            ErrorGameEnd.FatalErrorOccurred($"Failed to create players: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Tries to load the puzzles and create players.
    /// </summary>
    /// <returns>A <see cref="GameCore"/> instance if successful; otherwise <see langword="null"/>.</returns>
    private GameCore? CreateGameCore()
    {
        // try to load puzzles
        GameState? gameState = LoadGameState();
        if (gameState == null) {
            return null;
        }
        Debug.Log($"Game state loaded successfully. Number of puzzles: {gameState.GetAllPuzzlesInGame().Count}");

        // try to create players
        List<Player>? players = LoadPlayers();
        if (players == null) {
            return null;
        }
        Debug.Log("Players created successfully.");

        return new GameCore(gameState, players, GameStartParams.ShufflePlayers);
    }

    /// <summary>
    /// Asynchronously initializes all AI players by calling their <see cref="AIPlayerBase.InitAsync(int, List{Puzzle}, string?)"/> method.
    /// </summary>
    /// <param name="players">List of players to initialize.</param>
    /// <param name="gameState">The game state.</param>
    private void InitializeAIPlayersAsync(Player[] players, GameState gameState)
    {
        if (ErrorGameEnd.ShouldEndGameWithError) {
            return;
        }

        foreach (Player player in players) {
            if (player is AIPlayerBase aiPlayer) {
                string? initPath = GameStartParams.Players[player.Name].InitPath;
                Debug.Log($"Initializing AI player {player.Name}. Init file: {initPath}");

                aiPlayer.InitAsync(players.Length, gameState.GetAllPuzzlesInGame(), initPath, destroyCancellationToken)
                    .ContinueWith(t => {
                        if (t.Exception != null) {
                            ErrorGameEnd.FatalErrorOccurred($"Initialization of player {player.Name} failed: {t.Exception.InnerException?.Message}");
                        }
                        else {
                            Debug.Log($"AI player {player.Name} initialized successfully.");
                        }
                    },
                    destroyCancellationToken
                );
            }
        }
    }

    /// <summary>
    /// Prepares the <see cref="GameEndStats"/> by adding final results and unfinished puzzles.
    /// </summary>
    private void PrepareGameEndStats()
    {
        if (ErrorGameEnd.ShouldEndGameWithError) {
            return;
        }

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

    /// <summary>
    /// Creates the GameEndedBox, which has a button to go to the final results screen.
    /// Also disables the pause logic.
    /// </summary>
    private void GoToFinalResultsScreen()
    {
        if (ErrorGameEnd.ShouldEndGameWithError) {
            return;
        }

        PauseLogic.CanBePaused = false;
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

        while (!destroyCancellationToken.IsCancellationRequested && !ErrorGameEnd.ShouldEndGameWithError) {
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
                action = await _game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier, destroyCancellationToken);
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

        IAction GetDefaultAction()
        {
            return (_game?.CurrentGamePhase == GamePhase.FinishingTouches)
                        ? new EndFinishingTouchesAction()
                        : new DoNothingAction();
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
        Debug.LogWarning($"{_game?.CurrentPlayer.Name} provided an invalid {action}\nVerification result:\n{fail.GetType()}: {fail.Message}\n");
    }

    private void LogDefaultFunctionAssignment(IAction action)
    {
        Debug.Log($"{_game?.CurrentPlayer.Name} provided no action. Defaulting to {action}");
    }

    private void LogPlayerFinishedPuzzle(FinishedPuzzleInfo puzzleInfo)
    {
        var logText = new StringBuilder();
        logText.AppendLine($"{_game?.CurrentPlayer.Name} completed puzzle with ID={puzzleInfo.Puzzle.Id}");
        logText.AppendLine($"   Returned pieces: {GetUsedTetrominos()}");
        logText.AppendLine($"   Reward: {puzzleInfo.SelectedReward}");
        logText.AppendLine($"   Points: {puzzleInfo.Puzzle.RewardScore}");
        Debug.Log(logText.ToString());

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

    private static class ErrorGameEnd
    {
        public static bool ShouldEndGameWithError { get; private set; } = false;
        public static bool EndedGameWithError { get; private set; } = false;
        private static string _message { get; set; } = "Game ended with error.";

        private static GameObject? _errorMessageBoxPrefab;

        public static void Setup(GameObject errorMessageBoxPrefab)
        {
            _message = "Game ended with error.";
            ShouldEndGameWithError = false;
            EndedGameWithError = false;
            _errorMessageBoxPrefab = errorMessageBoxPrefab;
        }

        public static void FatalErrorOccurred(string message)
        {
            if (ShouldEndGameWithError) {
                return;
            }
            ShouldEndGameWithError = true;
            _message = message;
        }

        /// <summary>
        /// Instantiates the error message box prefab. This method needs to be called from the main thread.
        /// </summary>
        public static void EndGameWithError()
        {
            if (EndedGameWithError) {
                return;
            }
            EndedGameWithError = true;
            PauseLogic.CanBePaused = false;

            Debug.LogError("Fatal error: " + _message);
            if (_errorMessageBoxPrefab == null)
                Debug.LogError("Error message prefab is not set.");
            else
                Instantiate(_errorMessageBoxPrefab);
        }
    }
}
