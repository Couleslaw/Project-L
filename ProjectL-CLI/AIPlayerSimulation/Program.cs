namespace AIPlayerSimulation
{
    using AIPlayerExample;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using System.Diagnostics;

    internal class Program
    {
        #region Fields

        internal readonly static string LargeSeparator = new String('X', 110);

        internal readonly static string SmallSeparator = new String('-', 95);

        #endregion

        #region Methods

        internal static void Main(string[] args)
        {
            // get program parameters
            var simParams = SimulationParams.GetSimulationParamsFromStdIn();

            // initialize a new game
            Console.Clear();
            Console.WriteLine("Loading game state from file...");
            GameState gameState = GameState.CreateFromFile("puzzles.txt", simParams.NumInitialTetrominos, simParams.NumWhitePuzzles, simParams.NumBlackPuzzles);

            // create players
            Console.WriteLine("Initializing players...");
            Player[] players = new Player[simParams.NumPlayers];
            for (int i = 0; i < simParams.NumPlayers; i++) {
                players[i] = new SimpleAIPlayer();
                ((SimpleAIPlayer)players[i]).Init(players.Length);
            }

            // create game core
            Console.WriteLine("Creating a GameCore object and initializing game...");
            var game = new GameCore(gameState, players, shufflePlayers: false);
            var signaler = game.TurnManager.GetSignaler();
            game.InitializeGame();

            // create action processors
            Console.WriteLine("Creating action processors...");
            Dictionary<Player, GameActionProcessor> actionProcessors = new();
            foreach (var player in players) {
                actionProcessors[player] = new GameActionProcessor(game, player.Id, signaler);
            }

            Console.WriteLine("Done! Starting game...\n");

            // game loop
            int roundCount = 0;
            uint firstPlayerId = 0;
            while (true) {
                // get next turn, if game ended, break
                TurnInfo turnInfo = game.GetNextTurnInfo();
                if (turnInfo.NumActionsLeft == 2 && game.CurrentPlayer.Id == firstPlayerId) {
                    roundCount++;
                }

                if (simParams.ShouldClearConsole) {
                    Console.Clear();
                }
                else {
                    Console.WriteLine($"{LargeSeparator}\n{LargeSeparator}\n");
                }

                // check if game ended
                if (game.CurrentGamePhase == GamePhase.Finished) {
                    Console.WriteLine("Game ended! Clearing the playing board...");
                    game.GameEnded();
                    break;
                }

                // print turn info
                Console.WriteLine($"Round: {roundCount}, Current player: {game.CurrentPlayer.Id}, Action: {3 - turnInfo.NumActionsLeft}");
                Console.WriteLine($"TurnInfo: GamePhase={turnInfo.GamePhase}, LastRound={turnInfo.LastRound}, TookBlackPuzzle={turnInfo.TookBlackPuzzle}, UsedMaster={turnInfo.UsedMasterAction}");

                // get action from current player
                var gameInfo = gameState.GetGameInfo();
                var playerInfos = game.PlayerStates.Select(playerState => playerState.GetPlayerInfo()).ToArray();
                var currentPlayerInfo = game.GetPlayerStateWithId(game.CurrentPlayer.Id).GetPlayerInfo();
                var verifier = new ActionVerifier(gameInfo, currentPlayerInfo, turnInfo);

                Console.WriteLine(SmallSeparator);
                Console.Write(gameInfo);
                foreach (var playerInfo in playerInfos) {
                    Console.WriteLine(SmallSeparator);
                    Console.Write(playerInfo);
                }
                Console.WriteLine(LargeSeparator);
                Console.WriteLine();

                // time how long getting the action took
                Stopwatch stopwatch = Stopwatch.StartNew();
                IAction? action;
                try {
                    action = game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }
                catch (Exception) {
                    action = null;
                }
                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds >= 1000) {
                    Console.WriteLine($"WARNING! GetAction call took {stopwatch.ElapsedMilliseconds} ms");
                }

                // check if the player provided a action
                if (action == null) {
                    Console.WriteLine("Player failed to provide a action. Skipping action...");
                    if (simParams.IsInteractive) {
                        Console.WriteLine("Press 'Enter' to continue.");
                        Console.ReadLine();
                    }
                    continue;
                }

                // verify the action
                var result = verifier.Verify(action);
                if (result is VerificationFailure fail) {
                    Console.WriteLine($"Player provided an invalid {action.GetType()}. Verification result:\n{fail.GetType()}: {fail.Message}\n");
                    Console.WriteLine("Skipping action...");
                    if (simParams.IsInteractive) {
                        Console.WriteLine("Press 'Enter' to continue.");
                        Console.ReadLine();
                    }
                    continue;
                }

                // process valid action
                Console.WriteLine($"The player used a {action}\n");
                if (simParams.IsInteractive) {
                    Console.WriteLine("Press 'Enter' to process action.");
                    Console.ReadLine();
                }

                action.Accept(actionProcessors[game.CurrentPlayer]);
            }

            // print final results
            Console.WriteLine("Getting final results...\n");
            var results = game.GetFinalResults();
            var order = results.OrderBy(pair => pair.Value).Select(pair => pair.Key);
            Console.WriteLine(SmallSeparator);
            foreach (var key in order) {
                var info = key.GetPlayerInfo();
                Console.WriteLine($" {results[key]}. | Player {info.PlayerId} | Score: {info.Score,2}, Number of finished puzzles: {info.FinishedPuzzlesIds.Count,2}, Number of leftover tetrominos: {info.NumTetrominosOwned.Sum()}");
            }
            Console.WriteLine(SmallSeparator);

            Console.WriteLine("\n The game is finished. Press 'Enter' to exit.");
            Console.ReadLine();
        }

        #endregion
    }
}
