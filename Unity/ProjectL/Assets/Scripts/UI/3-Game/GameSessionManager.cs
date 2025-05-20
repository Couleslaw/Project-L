#nullable enable

namespace ProjectL.UI.GameScene.Management
{
    using ProjectL;
    using ProjectL.Data;
    using ProjectL.Management;
    using ProjectL.UI.Animation;
    using ProjectL.UI.GameScene;
    using ProjectL.UI.GameScene.Actions;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class GameSessionManager : StaticInstance<GameSessionManager>
    {
        #region Fields

        [Header("Game End Boxes")]
        [SerializeField] private ErrorAlertBox? _errorAlertBoxPrefab;
        [SerializeField] private GameEndedBox? _gameEndedBoxPrefab;

        private AIPlayerActionAnimator _aiPlayerAnimator = new();

        #endregion

        #region Methods

        protected override void Awake()
        {
            base.Awake();
            // check that all components are assigned
            if (_errorAlertBoxPrefab == null || _gameEndedBoxPrefab == null) {
                Debug.LogError("GameManager: One or more required UI elements are not assigned.");
                return;
            }

            GameErrorHandler.Setup(_errorAlertBoxPrefab);
        }

        private async void Start()
        {
            // create game core
            GameCore? game = CreateGameCore();

            // if error occurred --> end
            if (game == null) {
                return;
            }

            RuntimeGameInfo.RegisterGame(game);

            // start the game
            try {
                await SimulateGameAsync(game, destroyCancellationToken);
            }
            catch (OperationCanceledException) {
                Debug.Log("Game session cancelled.");
            }
            catch (Exception e) {
                GameErrorHandler.FatalErrorOccurred($"An error occurred during the game: {e.Message}");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RuntimeGameInfo.UnregisterGame();
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

            // try to create players
            List<Player>? players = LoadPlayers();
            if (players == null) {
                return null;
            }

            return new GameCore(gameState, players, GameSettings.ShouldShufflePlayers);
        }

        /// <summary>
        /// Tries to load puzzles from the Resources folder and create a <see cref="GameState"/> instance.
        /// </summary>
        /// <returns>A <see cref="GameState"/> instance if successful; otherwise <see langword="null"/>.</returns>
        private GameState? LoadGameState()
        {
            // check if the game didn't end already
            if (GameErrorHandler.GameEndedWithError) {
                return null;
            }

            // read the puzzles file
            if (!ResourcesLoader.TryReadPuzzleFile(out string puzzleFileText)) {
                GameErrorHandler.FatalErrorOccurred("Failed to load puzzles file.");
                return null;
            }

            // try to parse the puzzles and create a game state
            try {
                return GameState.CreateFromStream<PuzzleWithColor>(
                    puzzleStream: GenerateStreamFromString(puzzleFileText),
                    numInitialTetrominos: GameSettings.NumInitialTetrominos,
                    numBlackPuzzles: GameSettings.NumBlackPuzzles
                    );
            }
            catch (Exception e) {
                GameErrorHandler.FatalErrorOccurred($"Failed to load game state. {e.Message}");
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
            // check if the game didn't end already
            if (GameErrorHandler.GameEndedWithError) {
                return null;
            }

            // check if player selection happened
            if (GameSettings.Players.Count == 0) {
                GameErrorHandler.FatalErrorOccurred("No players selected.");
                return null;
            }

            // try to create players
            try {
                List<Player> players = new();

                foreach (var playerInfo in GameSettings.Players) {
                    Player player = (Activator.CreateInstance(playerInfo.Value.PlayerType) as Player)!;
                    player.Name = playerInfo.Key;
                    players.Add(player);
                }
                return players;
            }
            catch (Exception e) {
                GameErrorHandler.FatalErrorOccurred($"Failed to create players: {e.Message}");
                return null;
            }
        }

        private async Task SimulateGameAsync(GameCore game, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // initialize game components
            _aiPlayerAnimator.Init(game);
            await InitializePlayersAsync(game, cancellationToken);
            await InitializeGameAsync(game, cancellationToken);

            // start game loop
            GameSummary.Clear();
            await GameLoopAsync(game, cancellationToken);

            // wait a bit before showing the game ended box
            await AnimationManager.WaitForScaledDelay(1f);

            // finalize game
            cancellationToken.ThrowIfCancellationRequested();
            game.FinalizeGame();
            PrepareGameEndStats(game);

            // go to final results screen
            cancellationToken.ThrowIfCancellationRequested();
            GoToFinalResultsScreen();
        }


        /// <summary>
        /// Asynchronously initializes all AI players by calling their <see cref="AIPlayerBase.InitAsync(int, List{Puzzle}, string?)"/> method.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task InitializePlayersAsync(GameCore game, CancellationToken cancellationToken)
        {
            foreach (Player player in game!.Players) {

                // check if the game didn't end already
                cancellationToken.ThrowIfCancellationRequested();
                if (GameErrorHandler.GameEndedWithError) {
                    return;
                }

                // initialize human player
                if (player is HumanPlayer humanPlayer) {
                    HumanPlayerActionCreator.Instance.RegisterPlayer(humanPlayer, game.PlayerStates[humanPlayer]);
                }

                // initialize AI player
                else if (player is AIPlayerBase aiPlayer) {
                    string? initPath = GameSettings.Players[player.Name].InitPath;
                    string fileStr = initPath != null ? $", (ini file: {initPath})" : string.Empty;

                    Debug.Log($"Initializing {aiPlayer.GetType().Name} {aiPlayer.Name}{initPath}...");

                    try {
                        await aiPlayer.InitAsync(game.Players.Length, game.GameState.GetAllPuzzlesInGame(), initPath, cancellationToken);
                        Debug.Log($"AI player {player.Name} initialized successfully.");
                    }
                    catch (OperationCanceledException) {
                        // explicitly rethrow the exception to cancel the game
                        throw;
                    }
                    catch (AggregateException e) {
                        GameErrorHandler.FatalErrorOccurred($"Initialization of player {player.Name} failed: {e.InnerException?.Message}");
                        break;
                    }
                    catch (Exception e) {
                        GameErrorHandler.FatalErrorOccurred($"Initialization of player {player.Name} failed: {e.Message}");
                        break;
                    }
                }

                // should never happen, but just in case
                else {
                    Debug.LogError($"Player {player.Name} has an unknown type {player.GetType().Name}.");
                }
            }
        }

        private async Task InitializeGameAsync(GameCore game, CancellationToken cancellationToken)
        {
            // check if the game didn't end already
            cancellationToken.ThrowIfCancellationRequested();
            if (GameErrorHandler.GameEndedWithError) {
                return;
            }

            // wait until the graphics system is ready
            while (!GameGraphicsSystem.Instance.IsReadyForInitialization) {
                await Awaitable.WaitForSecondsAsync(0.01f, cancellationToken);
            }

            GameGraphicsSystem.Instance.Init(game);
            await game.InitializeGameAsync(cancellationToken);
        }

        private async Task GameLoopAsync(GameCore game, CancellationToken cancellationToken)
        {
            while (!GameErrorHandler.GameEndedWithError) {
                // check if the game didn't end already
                cancellationToken.ThrowIfCancellationRequested();

                // get next turn
                TurnInfo turnInfo = game.GetNextTurnInfo();
                LogTurnInfo(turnInfo, game.CurrentPlayer);

                // check if game ended
                if (game.CurrentGamePhase == GamePhase.Finished) {
                    Debug.Log("Game ended.");
                    break;
                }

                // create verifier for the current player
                var gameInfo = game.GameState.GetGameInfo();
                var playerInfos = game.GetPlayerInfos();
                var currentPlayerInfo = game.PlayerStates[game.CurrentPlayer].GetPlayerInfo();
                var verifier = new ActionVerifier(gameInfo, currentPlayerInfo, turnInfo);

                // try to get action from player
                GameAction? action;
                try {
                    action = await game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier, cancellationToken);
                    if (action == null) {
                        LogPlayerGetActionReturnedNull(game.CurrentPlayer.Name);
                    }
                }
                catch (OperationCanceledException) {
                    // explicitly rethrow the exception to cancel the game
                    throw;
                }
                catch (AggregateException e) {
                    LogPlayerGetActionThrownException(game.CurrentPlayer.Name, e.InnerException.Message);
                    action = null;
                }
                catch (Exception e) {
                    LogPlayerGetActionThrownException(game.CurrentPlayer.Name, e.Message);
                    action = null;
                }

                // if action is not null --> verify it
                if (action != null) {
                    var result = verifier.Verify(action);

                    // if fail --> set action to null
                    if (result is VerificationFailure fail) {
                        LogPlayerProvidedInvalidAction(game.CurrentPlayer.Name, action, fail);
                        action = null;
                    }
                }

                // if action is null --> assign default action
                if (action == null) {
                    action = GetDefaultAction(game.CurrentGamePhase);
                    LogDefaultFunctionAssignment(game.CurrentPlayer.Name, action);
                }

                // action is now valid --> process it
                LogPlayerProvidedValidAction(game.CurrentPlayer.Name, action);
                if (game.CurrentPlayer is AIPlayerBase aiPlayer) {
                    await action.AcceptAsync(_aiPlayerAnimator, cancellationToken);
                }
                await game.ProcessActionAsync(action, cancellationToken);

                // if not finishing touches --> add finished puzzles to summary for the final screen
                if (game.CurrentGamePhase != GamePhase.FinishingTouches) {
                    while (game.TryGetNextPuzzleFinishedBy(game.CurrentPlayer, out var finishedPuzzleInfo)) {
                        LogPlayerFinishedPuzzle(game.CurrentPlayer.Name, finishedPuzzleInfo);
                        GameSummary.AddFinishedPuzzle(game.CurrentPlayer, finishedPuzzleInfo.Puzzle);
                    }
                }

                // if finishing touches --> add tetromino to summary for the final screen
                if (game.CurrentGamePhase == GamePhase.FinishingTouches && action is PlaceTetrominoAction a) {
                    GameSummary.AddFinishingTouchesTetromino(game.CurrentPlayer, a.Shape);
                }
            }

            GameAction GetDefaultAction(GamePhase currentGamePhase)
            {
                return (currentGamePhase == GamePhase.FinishingTouches)
                            ? new EndFinishingTouchesAction()
                            : new DoNothingAction();
            }
        }


        /// <summary>
        /// Prepares the <see cref="GameSummary"/> by adding final results and unfinished puzzles.
        /// </summary>
        private void PrepareGameEndStats(GameCore game)
        {
            // check if the game didn't end already
            if (GameErrorHandler.GameEndedWithError) {
                return;
            }

            // add final results
            GameSummary.FinalResults = game.GetPlayerRankings();

            // add info about leftover tetrominos
            foreach (Player player in game.Players) {
                var info = game.PlayerStates[player].GetPlayerInfo();
                GameSummary.SetNumLeftoverTetrominos(player, info.NumTetrominosOwned.Sum());
            }

            // add info about unfinished puzzles
            foreach (Player player in game.Players) {
                var state = game.PlayerStates[player];
                foreach (Puzzle puzzle in state.GetUnfinishedPuzzles()) {
                    GameSummary.AddUnfinishedPuzzle(player, puzzle);
                }
            }
        }

        /// <summary>
        /// Creates the GameEndedBox, which has a button to go to the final results screen.
        /// Also disables the pause logic.
        /// </summary>
        private void GoToFinalResultsScreen()
        {
            if (_gameEndedBoxPrefab == null) {
                return;  // safety check
            }

            // if game ended with error, we aren't going to final results
            if (GameErrorHandler.GameEndedWithError) {
                return;
            }

            Instantiate(_gameEndedBoxPrefab);
            GameManager.CanGameBePaused = false;
        }

        private void LogTurnInfo(TurnInfo turnInfo, Player currentPlayer)
        {
            Debug.Log($"Current player: {currentPlayer.Name} ({currentPlayer.GetType().Name}), {turnInfo}");
        }

        private void LogPlayerGetActionThrownException(string playerName, string message)
        {
            Debug.LogWarning($"{playerName} failed to provide an action with error: {message}.");
        }

        private void LogPlayerGetActionReturnedNull(string playerName)
        {
            Debug.LogWarning($"{playerName} provided no action.");
        }

        private void LogPlayerProvidedInvalidAction(string playerName, GameAction action, VerificationFailure fail)
        {
            Debug.LogWarning($"{playerName} provided an invalid {action}\nVerification result:\n{fail.GetType()}: {fail.Message}\n");
        }

        private void LogDefaultFunctionAssignment(string playerName, GameAction action)
        {
            Debug.LogWarning($"{playerName} provided no action. Defaulting to {action}");
        }

        private void LogPlayerProvidedValidAction(string playerName, GameAction action)
        {
            Debug.Log($"{playerName} provided a valid {action}");
        }

        private void LogPlayerFinishedPuzzle(string playerName, FinishedPuzzleInfo puzzleInfo)
        {
            var logText = new StringBuilder();
            logText.AppendLine($"{playerName} completed puzzle with ID={puzzleInfo.Puzzle.Id}");
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

        private static class GameErrorHandler
        {
            #region Fields

            private static ErrorAlertBox? _errorAlertBoxPrefab;

            #endregion

            #region Properties

            public static bool GameEndedWithError { get; private set; } = false;

            #endregion

            #region Methods

            public static void Setup(ErrorAlertBox errorAlertBoxPrefab)
            {
                GameEndedWithError = false;
                _errorAlertBoxPrefab = errorAlertBoxPrefab;
            }

            public static void FatalErrorOccurred(string message)
            {
                if (GameEndedWithError) {
                    return;
                }
                GameEndedWithError = true;

                Debug.LogError(message);

                if (_errorAlertBoxPrefab != null) {
                    GameManager.CanGameBePaused = false;
                    Instantiate(_errorAlertBoxPrefab);
                }
            }

            #endregion
        }
    }
}
