namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GameManagers;
    using ProjectLCore.Players;

    /// <summary>
    /// Contains all the information about a game of <b>Project L</b>.
    /// </summary>
    public class GameCore
    {
        #region Constants

        /// <summary> The maximum number of players allowed. </summary>
        public const int MaxPlayers = 4;

        #endregion

        #region Constructors

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
            if (players.Count > MaxPlayers) {
                throw new ArgumentException("Too many players");
            }

            // capture game state
            GameState = gameState;

            // shuffle players if needed
            Players = players.ToArray();
            if (shufflePlayers) {
                Players.Shuffle();
            }

            // create player states
            PlayerStates = new PlayerState[NumPlayers];
            for (int i = 0; i < NumPlayers; i++) {
                PlayerStates[i] = new PlayerState(playerId: Players[i].Id);
            }

            // create turn manager
            TurnManager = new(GetPlayerOrder());

            // creates an array of player IDs order as how they are in the Player[] array
            uint[] GetPlayerOrder()
            {
                uint[] order = new uint[NumPlayers];
                for (int i = 0; i < NumPlayers; i++) {
                    order[i] = Players[i].Id;
                }
                return order;
            }
        }

        #endregion

        #region Properties

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

        #endregion

        #region Methods

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
            foreach (var player in Players) {
                if (player.Id == id) {
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
            foreach (var playerState in PlayerStates) {
                if (playerState.PlayerId == id) {
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
            foreach (var playerState in PlayerStates) {
                foreach (var puzzle in playerState.GetUnfinishedPuzzles()) {
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
            for (int i = 1; i < NumPlayers; i++) {
                if (PlayerStates[i] == PlayerStates[i - 1]) {
                    result[PlayerStates[i]] = result[PlayerStates[i - 1]];
                }
                else {
                    result[PlayerStates[i]] = i + 1;
                }
            }

            return result;
        }

        #endregion
    }
}
