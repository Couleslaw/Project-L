namespace AIPlayerSimulation
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using AIPlayerExample;

    internal class Program
    {
        internal static void Main(string[] args)
        {
            // initialize a new game
            int numInitialTetrominos = 15;
            Console.WriteLine("Loading game state from file...");
            GameState gameState = GameState.CreateFromFile("puzzles.txt", numInitialTetrominos);

            Player[] players = [
                new SimpleAIPlayer() {Name="First"},
                new SimpleAIPlayer() {Name="Second"},
            ];

            Console.WriteLine("Initializing players...");
            foreach (Player player in players) {
                if (player is AIPlayerBase aiPlyer) {
                    aiPlyer.Init(players.Length, null);
                }
            }

            // create game core and action processors
            Console.WriteLine("Creating a GameCore object and initializing game...");
            var game = new GameCore(gameState, players, shufflePlayers: false);
            var signaler = game.TurnManager.GetSignaler();
            game.InitializeGame();

            Console.WriteLine("Creating action processors...");
            Dictionary<uint, GameActionProcessor> actionProcessors = new();
            for (int i = 0; i < players.Length; i++) {
                uint playerId = game.Players[i].Id;
                actionProcessors[playerId] = new GameActionProcessor(game, playerId, signaler);
            }

            Console.WriteLine("Done! Starting game...\n");

            // game loop
            int roundCount = 0;
            uint firstPlayerId = 0;
            while (true) {
                // get next turn, if game ended, break
                TurnInfo turnInfo = game.GetNextTurnInfo();
                if (turnInfo.NumActionsLeft == 2 && game.CurrentPlayerId == firstPlayerId) {
                    roundCount++;
                }
                

                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX\n");
                Console.WriteLine($"Round: {roundCount}, Current player: {game.CurrentPlayerId}, Action: {3-turnInfo.NumActionsLeft}");
                Console.WriteLine($"TurnInfo: GamePhase={turnInfo.GamePhase}, LastRound={turnInfo.LastRound}, TookBlackPuzzle={turnInfo.TookBlackPuzzle}, UsedMaster={turnInfo.UsedMasterAction}");

                if (game.CurrentGamePhase == GamePhase.Finished) {
                    Console.WriteLine("Game ended! Clearing the playing board...");
                    game.GameEnded();
                    break;
                }

                // get action from current player and process it
                uint playerId = game.CurrentPlayerId;

                var gameInfo = gameState.GetGameInfo();
                var playerInfos = game.PlayerStates.Select(playerState => playerState.GetPlayerInfo()).ToArray();
                var verifier = GetActionVerifier(game, turnInfo, playerId);

                Console.WriteLine("------------------------------------------------------------------------------------------");
                Console.Write(gameInfo);
                Console.WriteLine("------------------------------------------------------------------------------------------");
                foreach (var playerInfo in playerInfos) {
                    Console.Write(playerInfo);
                    Console.WriteLine("------------------------------------------------------------------------------------------");
                }

                VerifiableAction? action;
                try {
                    action = game.GetPlayerWithId(playerId).GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }
                catch (Exception) {
                    action = null;
                }

                if (action == null) {
                    Console.WriteLine("Player failed to provide a action. Skipping action...");
                }

                if (action != null && action.Status == ActionStatus.Unverified) {
                    Console.WriteLine($"Player provided an unverified {action.GetType()}. Verifying... ");
                    action.GetVerifiedBy(verifier);
                }

                if (action != null && action.Status == ActionStatus.FailedVerification) {
                    var result = verifier.Verify(action);
                    if (result is VerificationFailure fail) {
                        Console.WriteLine($"Player provided an invalid {action.GetType()}. Verification result:\n{fail.GetType()}: {fail.Message}\n");
                        Console.WriteLine("Skipping action...");
                    }
                }

                if (action == null || action.Status == ActionStatus.FailedVerification) {
                    Console.WriteLine("Press 'Enter' to continue.");
                    Console.ReadLine();
                    continue;
                }

                Console.WriteLine($"The player used a {action.GetType()}:");
                Console.WriteLine(action);
                Console.WriteLine("Press 'Enter' to process action.");
                Console.ReadLine();

                action.Accept(actionProcessors[playerId]);
            }

            // final results
            Console.WriteLine("Getting final results...");
            var results = game.GetFinalResults();
            var order = results.OrderBy(pair => pair.Value).Select(pair => pair.Key);
            foreach (var key in order) {
                Console.WriteLine($"Player {key}: {results[key]}");
            }
            Console.WriteLine("\n The game is finished. Press 'Enter' to exit.");
            Console.ReadLine();
        }

        /// <summary>
        /// Creates an action verifier for the given player
        /// </summary>
        /// <param name="game">Info about the game</param>
        /// <param name="turnInfo">Info about current turn</param>
        /// <param name="playerId">ID of the current player</param>
        private static ActionVerifier GetActionVerifier(GameCore game, TurnInfo turnInfo, uint playerId)
        {
            var gameInfo = game.GameState.GetGameInfo();
            var playerInfo = game.PlayerStates.First(playerState => playerState.PlayerId == playerId).GetPlayerInfo();
            return new ActionVerifier(gameInfo, playerInfo, turnInfo);
        }
    }
}
