namespace Kostra {
    enum GamePhase { Normal, EndOfTheGame, FinishingTouches, Finished }

    record struct TurnInfo(int ActionsLeft, GamePhase GamePhase, bool UsedMasterAction, bool TookBlackPuzzle);

    class TurnManager(uint[] playerIds) {
        private readonly int _numPlayers = playerIds.Length;
        private readonly uint[] _playersIds = playerIds;

        public const int NumActionsInTurn = 3;
        private int _currentPlayerOrder = 0;
        public uint CurrentPlayerId => _playersIds[_currentPlayerOrder];
        private bool IsEndOfRound => _currentPlayerOrder == _numPlayers - 1;

        private TurnInfo _turnInfo = new(ActionsLeft: NumActionsInTurn, GamePhase.Normal, UsedMasterAction: false, TookBlackPuzzle: false);



        private void SetNextPlayer() {
            _currentPlayerOrder = (_currentPlayerOrder + 1) % _numPlayers;
            _turnInfo.ActionsLeft = NumActionsInTurn;
            _turnInfo.UsedMasterAction = false;
            _turnInfo.TookBlackPuzzle = false;
        }

        private bool _lastRound = false;
        private void ChangeGamePhaseIfNeeded()
        {
            if (_turnInfo.GamePhase == GamePhase.EndOfTheGame)
            {
                if (_lastRound == false)
                {
                    _lastRound = true;
                }
                else
                {
                    _turnInfo.GamePhase = GamePhase.FinishingTouches;
                }
            }
        }

        public TurnInfo NextTurn() {
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches) {
                return _turnInfo;
            }

            if (_turnInfo.ActionsLeft > 0) {
                _turnInfo.ActionsLeft--;
                return _turnInfo;
            }

            // turn of a new player
            // after EndOfTheGame is triggered, finish current round and then play 1 last round

            if (IsEndOfRound) {
                ChangeGamePhaseIfNeeded();
            }
            SetNextPlayer();

            return _turnInfo;
        }

        public class Signals(TurnManager turnManager) {
            private TurnManager _turnManager = turnManager;

            public void PlayerTookBlackPuzzle()
            {
                _turnManager._turnInfo.TookBlackPuzzle = true;
            }
            public void NoCardsLeftInBlackDeck() {
                _turnManager._turnInfo.GamePhase = GamePhase.EndOfTheGame;
            }
            public void PlayerUsedMasterAction() {
                _turnManager._turnInfo.UsedMasterAction = true;
            }
            public void PlayerEndedFinishingTouches() {
                _turnManager.SetNextPlayer();
                if (_turnManager._currentPlayerOrder == 0) {
                    _turnManager._turnInfo.GamePhase = GamePhase.Finished;
                }
            }
        }
    }

    class GameCore
    {
        public const int MaxPlayers = 4;
        public int NumPlayers => Players.Length;

        public GameState GameState { get; }
        public Player[] Players { get; }
        public PlayerState[] PlayerStates { get; }

        public GameCore(GameState gameState, IList<Player> players, bool shufflePlayers)
        {
            if (players.Count > MaxPlayers)
            {
                throw new ArgumentException("Too many players");
            }
            GameState = gameState;

            Players = new Player[players.Count];
            players.CopyTo(Players, 0);
            if (shufflePlayers)
            {
                Players.Shuffle();
            }

            PlayerStates = new PlayerState[NumPlayers];
            for (int i = 0; i < NumPlayers; i++)
            {
                PlayerStates[i] = new PlayerState(playerId: Players[i].Id);
            }
        }

        public uint[] GetPlayerOrder()
        {
            uint[] order = new uint[NumPlayers];
            for (int i = 0; i < NumPlayers; i++)
            {
                order[i] = Players[i].Id;
            }
            return order;
        }

        public Player GetPlayerWithId(uint id)
        {
            foreach (var player in Players)
            {
                if (player.Id == id)
                {
                    return player;
                }
            }
            throw new InvalidOperationException("Player not found");
        }

        public PlayerState GetPlayerStateWithId(uint id)
        {
            foreach (var playerState in PlayerStates)
            {
                if (playerState.PlayerId == id)
                {
                    return playerState;
                }
            }
            throw new InvalidOperationException("Player state not found");
        }

        public Dictionary<PlayerState, int> GameEnded()
        {
            // remove points for unfinished puzzles
            foreach (var playerState in PlayerStates)
            {
                foreach (var puzzle in playerState.GetUnfinishedPuzzles())
                {
                    playerState.Score -= puzzle.RewardScore;
                }
            }

            // determine the order of players by score
            Array.Sort(PlayerStates);
            // (PlayerState, order)
            // if two PlayerState1 == PlayerState2, then order1 == order2
            var result = new Dictionary<PlayerState, int>();
            result[PlayerStates[0]] = 1;
            for (int i = 1; i < NumPlayers; i++)
            {
                if (PlayerStates[i] == PlayerStates[i - 1])
                {
                    result[PlayerStates[i]] = result[PlayerStates[i - 1]];
                }
                else
                {
                    result[PlayerStates[i]] = i + 1;
                }
            }

            return result;
        }
    }
}