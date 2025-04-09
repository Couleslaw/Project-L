namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
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

        #region Fields

        /// <summary> Action processors for each player. </summary>
        private readonly Dictionary<Player, GameActionProcessor> _actionProcessors = new();

        /// <summary> Manages who's turn it currently is and what is the game phase. </summary>
        private readonly TurnManager _turnManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCore"/> class.
        /// </summary>
        /// <param name="gameState">State of the game.</param>
        /// <param name="players">The players.</param>
        /// <param name="shufflePlayers">If set to <see langword="true"/> then the players are shuffled to randomize turn order.</param>
        /// <exception cref="ArgumentException">Too many players. <c>players.Count &gt; <see cref="MaxPlayers"/></c></exception>
        public GameCore(GameState gameState, ICollection<Player> players, bool shufflePlayers)
        {
            // check the number of players
            if (players.Count > MaxPlayers) {
                throw new ArgumentException("Too many players");
            }

            // capture game state
            GameState = gameState;

            // capture players and shuffle them if needed
            Players = players.ToArray();
            if (shufflePlayers) {
                Players.Shuffle();
            }

            // create turn manager and set the first player
            _turnManager = new TurnManager(Players.Select(p => p.Id).ToArray());
            CurrentPlayer = GetPlayerWithId(_turnManager.CurrentPlayerId);

            // create player states and action processors
            var signaler = _turnManager.GetSignaler();
            foreach (var player in players) {
                PlayerStates[player] = new PlayerState(player.Id);
                _actionProcessors[player] = new GameActionProcessor(this, player, signaler);
            }
        }

        #endregion

        #region Properties

        /// <summary> The current <see cref="GamePhase"/>. </summary>
        public GamePhase CurrentGamePhase { get; private set; } = GamePhase.Normal;

        /// <summary> The player who's turn it currently is. </summary>
        public Player CurrentPlayer { get; private set; }

        /// <summary> Information about the shared resources in the game. </summary>
        public GameState GameState { get; }

        /// <summary> The players playing the game. </summary>
        public Player[] Players { get; }

        /// <summary> Information about the resources of each player. </summary>
        public Dictionary<Player, PlayerState> PlayerStates { get; } = new();

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the game by giving every player a <see cref="TetrominoShape.O1"/> and <see cref="TetrominoShape.I2"/> tetromino from the shared reserve.
        /// And filling the black and white puzzle rows with puzzles from the decks.
        /// </summary>
        public void InitializeGame()
        {
            // give all players their initial tetrominos
            foreach (var playerState in PlayerStates.Values) {
                playerState.AddTetromino(TetrominoShape.O1);
                playerState.AddTetromino(TetrominoShape.I2);
                GameState.RemoveTetromino(TetrominoShape.O1);
                GameState.RemoveTetromino(TetrominoShape.I2);
            }

            // fill puzzle rows with puzzles
            GameState.RefillPuzzles();
        }

        /// <summary>
        /// Finishes up internal game state and prepares for evaluating the results of the game.
        /// Should be called after <see cref="CurrentGamePhase"/> changes to <see cref="GamePhase.Finished"/>.
        /// </summary>
        public void GameEnded()
        {
            // remove points for unfinished puzzles
            foreach (var playerState in PlayerStates.Values) {
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
        public Dictionary<Player, int> GetFinalResults()
        {
            // determine the order of players by score, completed puzzles and leftover tetrominos
            // lover index means better position
            Array.Sort(Players, (p1, p2) => PlayerStates[p1].CompareTo(PlayerStates[p2]));

            // if PlayerState1 == PlayerState2, then order1 == order2
            var order = new Dictionary<Player, int>();
            order[Players[0]] = 1;

            for (int i = 1; i < Players.Length; i++) {
                if (PlayerStates[Players[i]] == PlayerStates[Players[i - 1]]) {
                    order[Players[i]] = order[Players[i - 1]];
                }
                else {
                    order[Players[i]] = order[Players[i - 1]] + 1;
                }
            }

            return order;
        }

        /// <summary>
        /// Prepares the next turn and updates <see cref="CurrentPlayer"/> and <see cref="CurrentGamePhase"/>.
        /// </summary>
        /// <returns>Information about the next turn.</returns>
        /// <seealso cref="TurnManager.NextTurn"/>
        public TurnInfo GetNextTurnInfo()
        {
            TurnInfo info = _turnManager.NextTurn();
            CurrentGamePhase = info.GamePhase;
            CurrentPlayer = GetPlayerWithId(_turnManager.CurrentPlayerId);
            return info;
        }

        /// <summary>
        /// Gets a <see cref="FinishedPuzzleInfo"/> from the <see cref="GameActionProcessor.FinishedPuzzlesQueue"/> of the given <paramref name="player"/>.
        /// The return value indicates whether there was something in the queue or not.
        /// </summary>
        /// <param name="player">The player, who's completed puzzles this method will check.</param>
        /// <param name="finishedPuzzleInfo">Contains information about the last unprocessed finished puzzle if there is one; otherwise <see langword="default"/>.</param>
        /// <returns><see langword="true"/> if there is an unprocessed finished puzzle; otherwise <see langword="false"/>.</returns>
        /// <seealso cref="GameActionProcessor.FinishedPuzzlesQueue"/>
        public bool TryGetNextPuzzleFinishedBy(Player player, out FinishedPuzzleInfo finishedPuzzleInfo)
        {
            if (_actionProcessors[player].FinishedPuzzlesQueue.Count > 0) {
                finishedPuzzleInfo = _actionProcessors[player].FinishedPuzzlesQueue.Dequeue();
                return true;
            }
            finishedPuzzleInfo = default;
            return false;
        }

        /// <summary>
        /// Returns the <see cref="Player"/> matching the given ID. Throws an exception if no such player exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The player with the given ID.</returns>
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
        /// Creates a read-only <see cref="PlayerState.PlayerInfo"/> wrapper for each <see cref="PlayerState"/> in <see cref="PlayerStates"/>.
        /// </summary>
        /// <returns> The array of <see cref="PlayerState.PlayerInfo"/> objects.</returns>
        public PlayerState.PlayerInfo[] GetPlayerInfos()
        {
            return PlayerStates.Select(playerState => playerState.Value.GetPlayerInfo()).ToArray();
        }

        /// <summary>
        /// Adjusts the <see cref="GameState"/> and the <see cref="PlayerState"/> of the current player based on the given action.
        /// Doesn't check if the action is valid. Use an <see cref="ActionVerifier"/> to check if the action is valid before calling this function.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ProcessAction(IAction action)
        {
            action.Accept(_actionProcessors[CurrentPlayer]);
        }

        #endregion
    }
}
