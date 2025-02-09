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

            // create game core, turn manager and processor manager
            var game = new GameCore(gameState, players, shufflePlayers: false);
            var turnManager = new TurnManager(game.GetPlayerOrder());

            var signaller = new TurnManager.Signals(turnManager);
            Dictionary<uint, GameActionProcessor> actionProcessors = new();
            for (int i = 0; i < players.Length; i++)
            {
                uint playerId = game.Players[i].Id;
                actionProcessors[playerId] = new GameActionProcessor(game, playerId, signaller);
            }

            // game loop
            while (true) {
                // get next turn, if game ended, break
                TurnInfo turnInfo = turnManager.NextTurn();
                if (turnInfo.GamePhase == GamePhase.Finished) {
                    break;
                }

                // get action from current player and process it
                // TODO: THIS SHOULD RETURN PLAYER ID
                uint playerId = turnManager.CurrentPlayerId;

                var GameInfo = new GameState.GameInfo(game.GameState);
                var PlayerInfos = game.PlayerStates.Select(playerState => new PlayerState.PlayerInfo(playerState)).ToArray();

                IAction action = game.GetPlayerWithId(playerId).GetActionAsync(GameInfo, PlayerInfos, turnInfo).Result;
                action.Accept(actionProcessors[playerId]);
            }

            // game ended
            var results = game.GameEnded();
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
