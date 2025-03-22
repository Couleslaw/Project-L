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
            GameState gameState = GameState.CreateFromFile("puzzles.txt", numInitialTetrominos);

            Player[] players = [
                new HumanPlayer(),
                new HumanPlayer(),
                new HumanPlayer()
            ];

            SimpleAIPlayer simpleAIPlayer = new SimpleAIPlayer();
            simpleAIPlayer.Init(players.Length, null);

            // create game core and action processors
            var game = new GameCore(gameState, players, shufflePlayers: false);
            game.InitializeGame();
            var signaler = game.TurnManager.GetSignaler();

            Dictionary<uint, GameActionProcessor> actionProcessors = new();
            for (int i = 0; i < players.Length; i++) {
                uint playerId = game.Players[i].Id;
                actionProcessors[playerId] = new GameActionProcessor(game, playerId, signaler);
            }

            // game loop
            while (true) {
                // get next turn, if game ended, break
                TurnInfo turnInfo = game.GetNextTurnInfo();

                if (game.CurrentGamePhase == GamePhase.Finished) {
                    game.GameEnded();
                    break;
                }

                // get action from current player and process it
                uint playerId = game.CurrentPlayerId;

                var gameInfo = gameState.GetGameInfo();
                var playerInfos = game.PlayerStates.Select(playerState => playerState.GetPlayerInfo()).ToArray();
                var verifier = GetActionVerifier(game, turnInfo, playerId);

                VerifiableAction? action;
                try {
                    action = game.GetPlayerWithId(playerId).GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }
                catch (Exception) {
                    action = null;
                }

                if (action is not null && action.Status == ActionStatus.Unverified) {
                    action.GetVerifiedBy(verifier);
                }

                if (action is null || action.Status == ActionStatus.FailedVerification) {
                    action = simpleAIPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }

                action.Accept(actionProcessors[playerId]);
            }

            // final results
            var results = game.GetFinalResults();
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
