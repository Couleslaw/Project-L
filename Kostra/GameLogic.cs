namespace Kostra {

    /// <summary>
    /// Represents the current phase of the game
    /// </summary>
    enum GamePhase {
        /// <summary>
        /// Standard phase of the game in which players take actions.
        /// </summary>
        Normal,

        /// <summary> 
        /// The EndOfTheGame phase is triggered when there are no more black puzzles in the black deck.
        /// </summary>
        EndOfTheGame,
        
        /// <summary>
        /// The FinishingTouches phase is triggered after the last round of the game.
        /// </summary>
        FinishingTouches,
        
        /// <summary>
        /// The game is finishing after all players use the <see cref="EndFinishingTouchesAction"/>.
        /// </summary>
        Finished
    }

    /// <summary>
    /// Represents the information about the current turn.
    /// </summary>
    record struct TurnInfo(
        int ActionsLeft,         
        GamePhase GamePhase, 
        bool UsedMasterAction, 
        bool TookBlackPuzzle, 
        bool LastRound
        );

    /// <summary>
    /// Takes care of the order of players, the game phase and the current turn.
    /// </summary>
    class TurnManager(uint[] playerIds) {
        private readonly int _numPlayers = playerIds.Length;
        private readonly uint[] _playersIds = playerIds;

        /// <summary>
        /// The number of actions a player has each turn.
        /// </summary>
        public const int NumActionsInTurn = 3;
        private int _currentPlayerOrder = 0;

        /// <summary>
        /// Gets the current player's ID
        /// </summary>
        public uint CurrentPlayerId => _playersIds[_currentPlayerOrder];

        /// <summary>
        /// True if this is the turn of the last player. 
        /// </summary>
        private bool IsEndOfRound => _currentPlayerOrder == _numPlayers - 1;

        /// <summary>
        /// Internal representation if the current turn.
        /// </summary>
        private TurnInfo _turnInfo = new(ActionsLeft: NumActionsInTurn, GamePhase.Normal, UsedMasterAction: false, TookBlackPuzzle: false, LastRound: false);

        /// <summary>
        /// Resets the internal turn state for the next player.
        /// </summary>
        private void SetNextPlayer() {
            _currentPlayerOrder = (_currentPlayerOrder + 1) % _numPlayers;
            _turnInfo.ActionsLeft = NumActionsInTurn;
            _turnInfo.UsedMasterAction = false;
            _turnInfo.TookBlackPuzzle = false;
        }

        /// <summary>
        /// Changes the game phase from <see cref="GamePhase.EndOfTheGame"/> to <see cref="GamePhase.FinishingTouches"/> if the conditions are met.
        /// After <see cref="GamePhase.EndOfTheGame"/> is triggered, the players finish their current round and then play one last round. 
        /// <see cref="GamePhase.FinishingTouches"/> start after that.
        /// </summary>
        private void ChangeGamePhaseIfNeeded()
        {
            if (_turnInfo.GamePhase == GamePhase.EndOfTheGame)
            {
                if (_turnInfo.LastRound == false)
                {
                    _turnInfo.LastRound = true;
                }
                else
                {
                    _turnInfo.GamePhase = GamePhase.FinishingTouches;
                }
            }
        }

        /// <summary>
        /// Adjusts the internal turn state to represent the next turn.
        /// </summary>
        /// <returns>Information about the next turn.</returns>
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


        /// <summary>
        /// Used to signal the given <see cref="TurnManager"/> about the events that happened during the turn.
        /// </summary>
        public class Signals(TurnManager turnManager) {
            /// <summary>
            /// Signals that the current player took a black puzzle.
            /// Players can take only up to 1 black puzzle per turn once <see cref="GamePhase.EndOfTheGame"/> is triggered.
            /// </summary>
            public void PlayerTookBlackPuzzle()
            {
                turnManager._turnInfo.TookBlackPuzzle = true;
            }

            /// <summary>
            /// Signals that the black deck is empty.
            /// This triggers the <see cref="GamePhase.EndOfTheGame"/> phase, if it is not already triggered.
            /// </summary>
            public void BlackDeckIsEmpty() {
                if (turnManager._turnInfo.GamePhase == GamePhase.Normal)
                {
                    turnManager._turnInfo.GamePhase = GamePhase.EndOfTheGame;
                }
            }

            /// <summary>
            /// Signals that the current player used <see cref="MasterAction"/>
            /// Players can use <see cref="MasterAction"/> only once per turn.
            /// </summary>
            public void PlayerUsedMasterAction() {
                turnManager._turnInfo.UsedMasterAction = true;
            }

            /// <summary>
            /// Signals that the current player use <see cref="EndFinishingTouchesAction"/>.
            /// The game ends once all players do this.
            /// </summary>
            public void PlayerEndedFinishingTouches() {
                turnManager.SetNextPlayer();
                if (turnManager._currentPlayerOrder == 0) {
                    turnManager._turnInfo.GamePhase = GamePhase.Finished;
                }
            }
        }
    }

    class GameCore
    {
        public const int MaxPlayers = 4;
        public int NumPlayers => Players.Length;
        public GamePhase CurrentGamePhase { get; set; } = GamePhase.Normal;

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

            // determine the order of players by score, completed puzzles and leftover tetrominos
            // lover index means better position
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