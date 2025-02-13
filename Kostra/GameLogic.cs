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
        int ActionsLeft,        // how many actions has the current player left in this turn
        GamePhase GamePhase,    // what is the current game phase
        bool UsedMasterAction,  // did the player use the Master action this turn?
        bool TookBlackPuzzle,   // did the player take a black puzzle this turn?
        bool LastRound          // is this the last round of the game?
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
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches || _turnInfo.GamePhase == GamePhase.Finished) {
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

        /// <summary> Creates a new signaler for this instance. </summary>
        public Signals GetSignaler() => new Signals(this);

        /// <summary>
        /// Signal the given <see cref="TurnManager"/> about the events that happened during the turn.
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

    /// <summary>
    /// Contains all the information about a game of Project L.
    /// </summary>
    class GameCore
    {
        /// <summary> The maximum number of players allowed. </summary>
        public const int MaxPlayers = 4;

        /// <summary> The number of players playing the game.  </summary>
        public int NumPlayers => Players.Length;

        /// <summary> The current <see cref="GamePhase"/>. </summary>
        public GamePhase CurrentGamePhase { get; private set; } = GamePhase.Normal;

        /// <summary> The ID of the player who's turn it currently is. </summary>
        public uint CurrentPlayerId { get; private set; }

        /// <summary> Information about the shared resources in the game. </summary>
        public GameState GameState { get; }

        /// <summary> The players playing the game. </summary>
        public Player[] Players { get; }

        /// <summary> Information about the resources of each player. </summary>
        public PlayerState[] PlayerStates { get; }

        /// <summary> Manages who's turn it currently is and what is the game phase. </summary>
        public TurnManager TurnManager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCore"/> class.
        /// </summary>
        /// <param name="gameState">State of the game.</param>
        /// <param name="players">The players.</param>
        /// <param name="shufflePlayers">If set to <c>true</c> then the players are shuffled to randomize turn order.</param>
        /// <exception cref="ArgumentException">Too many players. <c>players.Count &gt; <see cref="MaxPlayers"/></c></exception>
        public GameCore(GameState gameState, ICollection<Player> players, bool shufflePlayers)
        {
            // check the number of players
            if (players.Count > MaxPlayers)
            {
                throw new ArgumentException("Too many players");
            }

            // capture game state
            GameState = gameState;

            // shuffle players if needed
            Players = players.ToArray();
            if (shufflePlayers)
            {
                Players.Shuffle();
            }

            // create player states
            PlayerStates = new PlayerState[NumPlayers];
            for (int i = 0; i < NumPlayers; i++)
            {
                PlayerStates[i] = new PlayerState(playerId: Players[i].Id);
            }

            // create turn manager
            TurnManager = new(GetPlayerOrder());

            // creates an array of player IDs order as how they are in the Player[] array
            uint[] GetPlayerOrder()
            {
                uint[] order = new uint[NumPlayers];
                for (int i = 0; i < NumPlayers; i++)
                {
                    order[i] = Players[i].Id;
                }
                return order;
            }
        }

        /// <summary>
        /// Returns information about the next turn and updates <see cref="CurrentPlayerId"/> and <see cref="CurrentGamePhase"/>.
        /// </summary>
        public TurnInfo GetNextTurnInfo()
        {
            TurnInfo info = TurnManager.NextTurn();
            CurrentGamePhase = info.GamePhase;
            CurrentPlayerId = TurnManager.CurrentPlayerId;
            return info;
        }

        /// <summary>
        /// Returns the player matching the ID. Throws an exception if no such player exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentException">Player not found</exception>
        public Player GetPlayerWithId(uint id)
        {
            foreach (var player in Players)
            {
                if (player.Id == id)
                {
                    return player;
                }
            }
            throw new ArgumentException("Player not found");
        }

        /// <summary>
        /// Returns the <see cref="PlayerState"/> of the player matching the ID. Throws an exception if no such player exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentException">Player state not found</exception>
        public PlayerState GetPlayerStateWithId(uint id)
        {
            foreach (var playerState in PlayerStates)
            {
                if (playerState.PlayerId == id)
                {
                    return playerState;
                }
            }
            throw new ArgumentException("Player state not found");
        }

        /// <summary>
        /// Finishes up internal game state and prepares for evaluating the results of the game.
        /// Should be called after <see cref="CurrentGamePhase"/> changes to <see cref="GamePhase.Finished"/>.
        /// </summary>
        /// <returns></returns>
        public void GameEnded()
        {
            // remove points for unfinished puzzles
            foreach (var playerState in PlayerStates)
            {
                foreach (var puzzle in playerState.GetUnfinishedPuzzles())
                {
                    playerState.Score -= puzzle.RewardScore;
                }
            }
        }

        /// <summary>
        /// Determines the results of the game. <see cref="GameEnded"/> should be called before calling this function.
        /// </summary>
        /// <returns>
        /// A dictionary containing the result order for each player. Player with order 1 wins. It is possible for multiple players to have the same order.
        /// </returns>
        /// <seealso cref="PlayerState.CompareTo(PlayerState?)"/>
        public Dictionary<PlayerState, int> GetFinalResults()
        {
            // determine the order of players by score, completed puzzles and leftover tetrominos
            // lover index means better position
            Array.Sort(PlayerStates);

            // (PlayerState, order)
            // if PlayerState1 == PlayerState2, then order1 == order2
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