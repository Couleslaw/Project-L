namespace AIPlayerSimulation
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Diagnostics;
    using System.Text;

    internal class Program
    {
        #region Constants

        private const uint FirstPlayerId = 0;

        private const string PuzzleFilePath = "puzzles.txt";

        #endregion

        #region Fields

        private readonly static string LargeSeparator = new String('X', 110);

        private readonly static string SmallSeparator = new String('-', 95);

        private static int RoundCount = 0;

        private static bool IsInteractive = true;

        private static bool ShouldClearConsole = true;

        private static bool ExitingGame = false;

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

            GameState? gameState = LoadGameStateFromFile(PuzzleFilePath, simParams);
            if (gameState == null) {
                return;
            }

            // create players
            var playersWithTypes = LoadPlayers(simParams);
            if (playersWithTypes == null) {
                return;
            }

            // initialize players
            Console.WriteLine();
            InitializePlayers(playersWithTypes, gameState);

            // create game core
            Console.WriteLine("\nInitializing game...");
            List<Player> players = playersWithTypes.Keys.Cast<Player>().ToList();
            var game = new GameCore(gameState, players, shufflePlayers: false);
            game.InitializeGame();

            // start game loop
            Console.WriteLine("Done! Starting game...\n");
            GameLoop(game);

            // print final results
            game.FinalizeGame();
            var results = game.GetPlayerRankings();
            PrintGameScreenSeparator();
            PrintFinalResults(results, game);
        }

        private static void InitializePlayers(Dictionary<AIPlayerBase, PlayerTypeInfo> playersWithTypes, GameState gameState)
        {
            List<AIPlayerBase> players = playersWithTypes.Keys.ToList();

            // initialize players
            foreach (AIPlayerBase aiPlayer in players) {
                string? iniPath = playersWithTypes[aiPlayer].InitPath;
                string fileStr = iniPath != null ? $", (ini file: {iniPath})" : string.Empty;

                Console.WriteLine($"Initializing player {aiPlayer.Name}{iniPath}...");
                Task initTask = aiPlayer.InitAsync(playersWithTypes.Count, gameState.GetAllPuzzlesInGame(), iniPath);

                // wait for initialization to finish
                try {
                    initTask.GetAwaiter().GetResult();
                    Console.WriteLine($"Player {aiPlayer.Name} initialized successfully.");
                }
                catch (AggregateException e) {
                    ExitGame($"Initialization of player {aiPlayer.Name} failed: {e.InnerException?.Message}");
                }
                catch (Exception e) {
                    ExitGame($"Initialization of player {aiPlayer.Name} failed: {e.Message}");
                }
            }
        }

        private static void GameLoop(GameCore game)
        {
            GameState gameState = game.GameState;

            // game loop
            while (true) {
                // get next turn info
                TurnInfo turnInfo = game.GetNextTurnInfo();

                // check if game ended
                if (game.CurrentGamePhase == GamePhase.Finished) {
                    Console.WriteLine("Game ended! Clearing the playing board...\n");
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
                    action = ((AIPlayerBase)game.CurrentPlayer).GetAction(gameInfo, playerInfos, turnInfo, verifier);
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
        }

        private static void ExitGame(string message = "")
        {
            if (ExitingGame == true)
                return;
            ExitingGame = true;

            Console.WriteLine(message);
            Console.WriteLine("Press 'Enter' to exit game");
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static GameState? LoadGameStateFromFile(string filePath, SimulationParams simParams)
        {
            Console.WriteLine("Loading game state...");
            try {
                return GameState.CreateFromFile<Puzzle>("puzzles.txt", simParams.NumInitialTetrominos, simParams.NumWhitePuzzles, simParams.NumBlackPuzzles);
            }
            catch (Exception e) {
                ExitGame("Failed to load game state from file: " + e.Message);
                return null;
            }
        }

        private static Dictionary<AIPlayerBase, PlayerTypeInfo>? LoadPlayers(SimulationParams simParams)
        {
            Console.WriteLine("Loading AI player types...\n");
            try {
                return ParamParser.GetPlayersFromStdIn(simParams.NumPlayers);
            }
            catch (Exception e) {
                ExitGame($"Failed to create players: {e.Message}");
                return null;
            }
        }

        private static void PrintGameScreenSeparator()
        {
            if (ShouldClearConsole) {
                Console.Clear();
            }
            else {
                Console.WriteLine($"{LargeSeparator}\n{LargeSeparator}\n");
            }
        }

        private static void PrintTurnInfo(Player currentPlayer, TurnInfo turnInfo)
        {
            // check if new round
            if (currentPlayer.Id == FirstPlayerId && turnInfo.NumActionsLeft == TurnManager.NumActionsInTurn) {
                RoundCount++;
            }
            // print turn info
            int actionNum = TurnManager.NumActionsInTurn - turnInfo.NumActionsLeft + 1;
            Console.WriteLine($"Round: {RoundCount}, Current player: {currentPlayer.Name} ({currentPlayer.GetType().Name}), Action: {actionNum}");
            Console.WriteLine(turnInfo.ToString());
        }

        private static void PrintFinishedPuzzleInfo(FinishedPuzzleInfo puzzleInfo, Player currentPlayer)
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

        private static void PrintGameScreen(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, GameCore game)
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

        private static void PrintPlayerProvidedNoAction(Player player, string message)
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

        private static void PrintPlayerProvidedInvalidAction(GameAction action, VerificationFailure fail, Player player)
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

        private static void PrintPlayerProvidedValidAction(GameAction action, Player player)
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

        private static void PrintFinalResults(Dictionary<Player, int> results, GameCore game)
        {
            Console.WriteLine("Getting final results...\n");
            Console.WriteLine(SmallSeparator);

            foreach (var item in results) {
                var info = game.PlayerStates[item.Key].GetPlayerInfo();
                Console.WriteLine($" {item.Value}. | {item.Key.Name,-10} | Score: {info.Score,2}, Number of finished puzzles: {info.FinishedPuzzlesIds.Count,2}, Number of leftover tetrominos: {info.NumTetrominosOwned.Sum(),2}");
            }
            Console.WriteLine(SmallSeparator);

            ExitGame();
        }

        #endregion
    }
}
