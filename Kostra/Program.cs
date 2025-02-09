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
            var turnManager = new TurnManager(game.NumPlayers);
            var processorManager = new ProcessorManager(game.GameState, game.PlayerStates, turnManager);

            // game loop
            while (true) {
                // get next turn, if game ended, break
                TurnInfo turnInfo = turnManager.NextTurn();
                if (turnInfo.GamePhase == GamePhase.Finished) {
                    break;
                }

                // get action from current player and process it
                int index = turnManager.CurrentPlayer;

                IAction action = game.Players[index].GetActionAsync(game.GameState, game.PlayerStates, turnInfo).Result;
                action.Accept(processorManager.GameStateActionProcessor);
                action.Accept(processorManager.PlayerStateActionProcessors[index]);

                // if in Unity, draw the game state and player states
                action.Accept(processorManager.GameStateGraphicsProcessor);
                action.Accept(processorManager.PlayerStateGraphicsProcessors[index]);
            }
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
