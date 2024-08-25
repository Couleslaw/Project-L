using System.Text.Json.Serialization;

namespace Kostra {
    // AI hrace bude potreba jinak animovat
    enum PlayerType { Human, AI };

    abstract class Player {
        private static uint _idCounter = 0;
        public uint Id { get; } = _idCounter++;
        public static void ResetIdCounter() => _idCounter = 0;
        public abstract PlayerType Type { get; }
        public abstract Task<IAction> GetActionAsync(GameState gameState, PlayerState[] playerStates, TurnInfo turnInfo);
    }

    class HumanPlayer : Player {
        public override PlayerType Type => PlayerType.Human;
        private TaskCompletionSource<IAction> _getActionCompletionSource = new();

        // This method will be called by Unity when the player clicks a button
        public void SetAction(IAction action)  => _getActionCompletionSource.SetResult(action);
        public override async Task<IAction> GetActionAsync(GameState gameState, PlayerState[] playerStates, TurnInfo turnInfo) {
            _getActionCompletionSource = new();
            return await _getActionCompletionSource.Task;
        }
    }

    abstract class AIPlayerBase : Player {
        public override PlayerType Type => PlayerType.AI;

        // AI players might have some state that needs to be initialized -> deserialize from file...
        public abstract void Init(string filePath);
        public abstract IAction GetAction(GameState gameState, PlayerState myState, List<PlayerState> enemyStates, TurnInfo turnInfo);
        public override async Task<IAction> GetActionAsync(GameState gameState, PlayerState[] playerStates, TurnInfo turnInfo) {
            PlayerState? myState = null;
            List<PlayerState> enemyStates = new();
            for (int i = 0; i < playerStates.Length; i++) {
                if (playerStates[i].PlayerId == Id) {
                    myState = playerStates[i];
                }
                else {
                    enemyStates.Add(playerStates[i]);
                }
            }
            if (myState == null) {
                throw new ArgumentException($"PlayerState for player {Id} not found!");
            }

            return await Task.Run(() => GetAction(gameState, myState, enemyStates, turnInfo));
        }
    }
}