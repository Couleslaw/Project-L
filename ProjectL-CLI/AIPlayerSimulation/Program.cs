namespace AIPlayerSimulation
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Text;
    using static ProjectLCore.GameLogic.GameState;
    using static ProjectLCore.GameLogic.PlayerState;

    internal class Program
    {
        #region Constants

        internal const uint FirstPlayerId = 0;

        internal const string PuzzleFilePath = "puzzles.txt";

        internal const string AIPlayerFilePath = "aiplayers.ini";

        #endregion

        #region Fields

        internal readonly static string LargeSeparator = new String('X', 110);

        internal readonly static string SmallSeparator = new String('-', 95);

        internal static int RoundCount = 0;

        internal static bool IsInteractive = true;

        internal static bool ShouldClearConsole = true;

        internal static bool ExitingGame = false;

        #endregion

        #region Methods

        internal static void Main(string[] args)
        {
            // get program parameters
            var simParams = ParamParser.GetSimulationParamsFromStdIn();
            IsInteractive = simParams.IsInteractive;
            ShouldClearConsole = simParams.ShouldClearConsole;

            // initialize a new game
            Console.Clear();
            Console.WriteLine("Loading game state from file...\n");

            GameState? gameState = LoadGameStateFromFile(PuzzleFilePath, simParams);
            if (gameState == null) {
                return;
            }

            // create players
            List<Player>? players = LoadPlayers(simParams);
            if (players == null) {
                return;
            }

            // initialize players
            Console.WriteLine("Initializing players...\n");
            foreach (Player player in players) {
                if (player is AIPlayerBase aiPlayer) {
                    Task initTask = aiPlayer.InitAsync(players.Count, gameState.GetAllPuzzlesInGame());
                    // handle possible exception
                    initTask.ContinueWith(t => {
                        if (t.Exception != null) {
                            ExitGame($"Initialization of player {player.Name} failed: {t.Exception.InnerException?.Message}");
                        }
                    });
                }
            }

            // create game core
            Console.WriteLine("\nInitializing game...");
            var game = new GameCore(gameState, players, shufflePlayers: false);
            game.InitializeGame();
            Console.WriteLine("Done! Starting game...\n");

            // game loop
            while (true) {
                // get next turn info
                TurnInfo turnInfo = game.GetNextTurnInfo();

                // check if game ended
                if (game.CurrentGamePhase == GamePhase.Finished) {
                    Console.WriteLine("Game ended! Clearing the playing board...\n");
                    game.FinalizeGame();
                    break;
                }

                // print turn info
                PrintGameScreenSeparator();
                PrintTurnInfo(game.CurrentPlayer, turnInfo);

                // create verifier for the current player
                var gameInfo = gameState.GetGameInfo();
                var playerInfos = game.GetPlayerInfos();
                var currentPlayerInfo = game.PlayerStates[game.CurrentPlayer].GetPlayerInfo();
                var verifier = new ActionVerifier(gameInfo, currentPlayerInfo, turnInfo);

                // print game screen
                PrintGameScreen(gameInfo, playerInfos, game);

                // get action from player
                GameAction? action;
                try {
                    action = game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }
                catch (Exception e) {
                    PrintPlayerProvidedNoAction(game.CurrentPlayer, e.Message);
                    continue;
                }

                // verify the action
                var result = verifier.Verify(action);
                if (result is VerificationFailure fail) {
                    PrintPlayerProvidedInvalidAction(action, fail, game.CurrentPlayer);
                    continue;
                }

                // process valid action
                PrintPlayerProvidedValidAction(action, game.CurrentPlayer);
                game.ProcessAction(action);

                // check if player completed any puzzles
                if (game.CurrentGamePhase != GamePhase.FinishingTouches) {
                    while (game.TryGetNextPuzzleFinishedBy(game.CurrentPlayer, out var finishedPuzzleInfo)) {
                        PrintFinishedPuzzleInfo(finishedPuzzleInfo, game.CurrentPlayer);
                    }
                }
            }

            // print final results
            var results = game.GetPlayerRankings();
            PrintGameScreenSeparator();
            PrintFinalResults(results, game);
        }

        internal static void ExitGame(string message="")
        {
            if (ExitingGame == true)
                return;
            ExitingGame = true;

            Console.WriteLine(message);
            Console.WriteLine("Press 'Enter' to exit game");
            Console.ReadLine();
            Environment.Exit(0);
        }

        internal static GameState? LoadGameStateFromFile(string filePath, SimulationParams simParams)
        {
            try {
                return GameState.CreateFromFile<Puzzle>("puzzles.txt", simParams.NumInitialTetrominos, simParams.NumWhitePuzzles, simParams.NumBlackPuzzles);
            }
            catch (Exception e) {
                ExitGame("Failed to load game state from file: " + e.Message);
                return null;
            }
        }

        internal static List<Player>? LoadPlayers(SimulationParams simParams)
        {
            try {
                return ParamParser.GetPlayersFromStdIn(simParams.NumPlayers, AIPlayerFilePath);
            }
            catch (Exception e) {
                ExitGame($"Failed to create players: {e.Message}");
                return null;
            }
        }

        internal static void PrintGameScreenSeparator()
        {
            if (ShouldClearConsole) {
                Console.Clear();
            }
            else {
                Console.WriteLine($"{LargeSeparator}\n{LargeSeparator}\n");
            }
        }

        internal static void PrintTurnInfo(Player currentPlayer, TurnInfo turnInfo)
        {
            // check if new round
            if (currentPlayer.Id == FirstPlayerId && turnInfo.NumActionsLeft == TurnManager.NumActionsInTurn) {
                RoundCount++;
            }
            // print turn info
            int actionNum = TurnManager.NumActionsInTurn - turnInfo.NumActionsLeft + 1;
            Console.WriteLine($"Round: {RoundCount}, Current player: {currentPlayer.Name} ({currentPlayer.GetType().Name}), Action: {actionNum}");
            Console.WriteLine($"TurnInfo: GamePhase={turnInfo.GamePhase}, LastRound={turnInfo.LastRound}, TookBlackPuzzle={turnInfo.TookBlackPuzzle}, UsedMaster={turnInfo.UsedMasterAction}");
        }

        internal static void PrintFinishedPuzzleInfo(FinishedPuzzleInfo puzzleInfo, Player currentPlayer)
        {
            Console.WriteLine($"{currentPlayer.Name} completed puzzle with ID={puzzleInfo.Puzzle.Id}");
            Console.WriteLine($"   Returned pieces: {GetUsedTetrominos()}");
            Console.WriteLine($"   Reward: {puzzleInfo.SelectedReward}");
            Console.WriteLine($"   Points: {puzzleInfo.Puzzle.RewardScore}");
            if (IsInteractive) {
                Console.WriteLine("\nPress 'Enter' to continue.");
                Console.ReadLine();
                if (ExitingGame) {
                    Environment.Exit(0);
                }
            }

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

        internal static void PrintGameScreen(GameInfo gameInfo, PlayerInfo[] playerInfos, GameCore game)
        {
            Console.WriteLine(SmallSeparator);
            Console.Write(gameInfo);
            foreach (var playerInfo in playerInfos) {
                Player player = game.GetPlayerWithId(playerInfo.PlayerId);
                Console.WriteLine(SmallSeparator);
                Console.Write($"{player.Name}. ");
                Console.Write(playerInfo);
            }
            Console.WriteLine(LargeSeparator);
            Console.WriteLine();
        }

        internal static void PrintPlayerProvidedNoAction(Player player, string message)
        {
            Console.WriteLine($"{player.Name} failed to provide an action with error: {message}.\nSkipping action...");
            if (IsInteractive) {
                Console.WriteLine("Press 'Enter' to continue.");
                Console.ReadLine();
                if (ExitingGame) {
                    Environment.Exit(0);
                }
            }
        }

        internal static void PrintPlayerProvidedInvalidAction(GameAction action, VerificationFailure fail, Player player)
        {
            Console.WriteLine($"{player.Name} provided an invalid {action.GetType()}. Verification result:\n{fail.GetType()}: {fail.Message}\n");
            Console.WriteLine("Skipping action...");
            if (IsInteractive) {
                Console.WriteLine("Press 'Enter' to continue.");
                Console.ReadLine();
                if (ExitingGame) {
                    Environment.Exit(0);
                }
            }
        }

        internal static void PrintPlayerProvidedValidAction(GameAction action, Player player)
        {
            Console.WriteLine($"{player.Name} used a {action}\n");
            if (IsInteractive) {
                Console.WriteLine("Press 'Enter' to process action.");
                Console.ReadLine();
                if (ExitingGame) {
                    Environment.Exit(0);
                }
            }
        }

        internal static void PrintFinalResults(Dictionary<Player, int> results, GameCore game)
        {
            Console.WriteLine("Getting final results...\n");
            Console.WriteLine(SmallSeparator);

            foreach (var item in results) {
                var info = game.PlayerStates[item.Key].GetPlayerInfo();
                Console.WriteLine($" {item.Value}. | {item.Key.Name,-8} | Score: {info.Score,2}, Number of finished puzzles: {info.FinishedPuzzlesIds.Count,2}, Number of leftover tetrominos: {info.NumTetrominosOwned.Sum(),2}");
            }
            Console.WriteLine(SmallSeparator);

            ExitGame();
        }

        #endregion
    }
}
