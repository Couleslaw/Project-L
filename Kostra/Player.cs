using Kostra;
using System.Text.Json.Serialization;

namespace Kostra {
    // AI hrace bude potreba jinak animovat
    enum PlayerType { Human, AI };

    abstract class Player {
        private static uint _idCounter = 0;
        public uint Id { get; } = _idCounter++;
        public abstract PlayerType Type { get; }
        public abstract Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier);
        public abstract Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions);
    }

    class HumanPlayer : Player {
        public override PlayerType Type => PlayerType.Human;
        private TaskCompletionSource<VerifiableAction> _getActionCompletionSource = new();
        private TaskCompletionSource<TetrominoShape> _getRewardCompletionSource = new();

        // This method will be called by Unity when the player clicks a button
        public void SetAction(VerifiableAction action)  => _getActionCompletionSource.SetResult(action);
        public override async Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier) {
            _getActionCompletionSource = new();
            return await _getActionCompletionSource.Task;
        }
        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions)
        {
            _getRewardCompletionSource = new();
            return await _getRewardCompletionSource.Task;
        }
    }

    abstract class AIPlayerBase : Player {
        public override PlayerType Type => PlayerType.AI;

        // AI players might have some state that needs to be initialized -> deserialize from file...
        public abstract void Init(string? filePath);
        public abstract VerifiableAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier);
        public override async Task<VerifiableAction> GetActionAsync(GameState.GameInfo gameInfo, PlayerState.PlayerInfo[] playerInfos, TurnInfo turnInfo, ActionVerifier verifier) {
            PlayerState.PlayerInfo? myState = null;
            List<PlayerState.PlayerInfo> enemyStates = new();
            for (int i = 0; i < playerInfos.Length; i++) {
                if (playerInfos[i].PlayerId == Id) {
                    myState = playerInfos[i];
                }
                else {
                    enemyStates.Add(playerInfos[i]);
                }
            }
            if (myState == null) {
                throw new ArgumentException($"PlayerState for player {Id} not found!");
            }

            return await Task.Run(() => GetAction(gameInfo, myState, enemyStates, turnInfo, verifier));
        }
        public abstract TetrominoShape GetReward(List<TetrominoShape> rewardOptions );
        public override async Task<TetrominoShape> GetRewardAsync(List<TetrominoShape> rewardOptions)
        {
            return await Task.Run(() => GetReward(rewardOptions));
        }
    }
}

