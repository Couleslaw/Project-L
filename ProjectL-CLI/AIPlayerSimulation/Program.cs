namespace AIPlayerSimulation
{
    using AIPlayerExample;
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

        #endregion

        #region Fields

        internal readonly static string LargeSeparator = new String('X', 110);

        internal readonly static string SmallSeparator = new String('-', 95);

        internal static readonly string[] PlayerNames = { "Alice", "Bob", "Charlie", "David" };

        internal static int RoundCount = 0;

        internal static bool IsInteractive = true;

        internal static bool ShouldClearConsole = true;

        #endregion

        #region Methods

        internal static void Main(string[] args)
        {
            // get program parameters
            var simParams = SimulationParams.GetSimulationParamsFromStdIn();
            IsInteractive = simParams.IsInteractive;
            ShouldClearConsole = simParams.ShouldClearConsole;

            // initialize a new game
            Console.Clear();
            Console.WriteLine("Loading game state from file...");
            var gameState = GameState.CreateFromFile("puzzles.txt", simParams.NumInitialTetrominos, simParams.NumWhitePuzzles, simParams.NumBlackPuzzles);

            // create players
            Console.WriteLine("Initializing players...");
            Player[] players = new Player[simParams.NumPlayers];
            for (int i = 0; i < simParams.NumPlayers; i++) {
                players[i] = new SimpleAIPlayer() { Name = PlayerNames[i] };
                ((SimpleAIPlayer)players[i]).InitAsync(players.Length, gameState.GetAllPuzzlesInGame());
            }

            // create game core
            Console.WriteLine("Creating a GameCore object and initializing game...");
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
                    game.GameEnded();
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
                IAction? action;
                try {
                    action = game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }
                catch (Exception) {
                    action = null;
                }

                // check if the player provided a action
                if (action == null) {
                    PrintPlayerProvidedNoAction(game.CurrentPlayer);
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
                    while (game.TryGetUnprocessedFinishedPuzzle(out var finishedPuzzleInfo)) {
                        PrintFinishedPuzzleInfo(finishedPuzzleInfo, game.CurrentPlayer);
                    }
                }
            }

            // print final results
            var results = game.GetFinalResults();
            PrintGameScreenSeparator();
            PrintFinalResults(results, game);
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
            if (currentPlayer.Id == FirstPlayerId && turnInfo.NumActionsLeft == TurnManager.NumActionsInTurn - 1) {
                RoundCount++;
            }
            // print turn info
            Console.WriteLine($"Round: {RoundCount}, Current player: {currentPlayer.Name}, Action: {3 - turnInfo.NumActionsLeft}");
            Console.WriteLine($"TurnInfo: GamePhase={turnInfo.GamePhase}, LastRound={turnInfo.LastRound}, TookBlackPuzzle={turnInfo.TookBlackPuzzle}, UsedMaster={turnInfo.UsedMasterAction}");
        }

        internal static void PrintFinishedPuzzleInfo(TurnManager.FinishedPuzzleInfo puzzleInfo, Player currentPlayer) 
        {
            Console.WriteLine($"{currentPlayer.Name} completed puzzle with ID={puzzleInfo.Puzzle.Id}");

            Console.WriteLine($"   Returned pieces: {GetUsedTetrominos()}");
            Console.WriteLine($"   Reward: {puzzleInfo.SelectedReward}");
            Console.WriteLine($"   Points: {puzzleInfo.Puzzle.RewardScore}");
            Console.WriteLine("\nPress 'Enter' to continue.");
            Console.ReadLine();

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

        internal static void PrintPlayerProvidedNoAction(Player player)
        {
            Console.WriteLine($"{player.Name} failed to provide a action. Skipping action...");
            if (IsInteractive) {
                Console.WriteLine("Press 'Enter' to continue.");
                Console.ReadLine();
            }
        }

        internal static void PrintPlayerProvidedInvalidAction(IAction action, VerificationFailure fail, Player player)
        {
            Console.WriteLine($"{player.Name} provided an invalid {action.GetType()}. Verification result:\n{fail.GetType()}: {fail.Message}\n");
            Console.WriteLine("Skipping action...");
            if (IsInteractive) {
                Console.WriteLine("Press 'Enter' to continue.");
                Console.ReadLine();
            }
        }

        internal static void PrintPlayerProvidedValidAction(IAction action, Player player)
        {
            Console.WriteLine($"{player.Name} used a {action}\n");
            if (IsInteractive) {
                Console.WriteLine("Press 'Enter' to process action.");
                Console.ReadLine();
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

            Console.WriteLine("\n The game is finished. Press 'Enter' to exit.");
            Console.ReadLine();
        }

        #endregion
    }
}
