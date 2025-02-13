namespace Kostra {
    internal class Program {
        static void Main(string[] args) {
            // initialize a new game
            GameState gameState = new GameStateBuilder().Build();
            Player[] players = [
                new HumanPlayer(),
                new HumanPlayer(),
                new HumanPlayer()
            ];
            SimpleAIPlayer simpleAIPlayer = new SimpleAIPlayer();

            // create game core and action processors
            var game = new GameCore(gameState, players, shufflePlayers: false);
            var signaler = game.TurnManager.GetSignaler();

            Dictionary<uint, GameActionProcessor> actionProcessors = new();
            for (int i = 0; i < players.Length; i++)
            {
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

                VerifiableAction action;
                try
                {
                    action = game.GetPlayerWithId(playerId).GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }
                catch (Exception)
                {
                    action = simpleAIPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }

                if (action.Status == ActionStatus.Unverified)
                {
                    action.GetVerifiedBy(verifier);
                }

                if (action.Status == ActionStatus.FailedVerification)
                {
                    action = simpleAIPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;
                }

                action.Accept(actionProcessors[playerId]);
            }

            // final results
            var results = game.GetFinalResults();
        }

        private static ActionVerifier GetActionVerifier(GameCore game, TurnInfo turnInfo, uint playerId)
        {
            var gameInfo = game.GameState.GetGameInfo();
            var playerInfo = game.PlayerStates.First(playerState => playerState.PlayerId == playerId).GetPlayerInfo();
            return new ActionVerifier(gameInfo, playerInfo, turnInfo);
        }
    }
}



public static class IListExtensions {
    // Fisher–Yates shuffle
    public static void Shuffle<T>(this IList<T> list) {
        Random rng = new Random();
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
