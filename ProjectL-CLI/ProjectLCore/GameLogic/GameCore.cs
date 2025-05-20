namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

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

        /// <summary>
        /// <see langword="true"/> if <see cref="InitializeGame"/> was called; otherwise <see langword="false"/>.
        /// </summary>
        private bool _didInitialize = false;

        /// <summary>
        /// <see langword="true"/> if <see cref="FinalizeGame"/> was called; otherwise <see langword="false"/>.
        /// </summary>
        private bool _didFinalize = false;

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

        #region Events

        private event Action<TurnInfo>? OnTurnChangedEventHandler;

        private event Action<Player>? CurrentPlayerChangedEventHandler;

        #endregion

        #region Properties

        /// <summary> Information about the current turn. </summary>
        public TurnInfo CurrentTurn { get; private set; }

        /// <summary> The current <see cref="GamePhase"/>. </summary>
        public GamePhase CurrentGamePhase => CurrentTurn.GamePhase;

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
        /// Subscribes the turn listener to the events of this <see cref="GameCore"/>.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <seealso cref="RemoveListener(ICurrentTurnListener)"/>
        public void AddListener(ICurrentTurnListener listener)
        {
            if (listener == null) {
                return;
            }
            OnTurnChangedEventHandler += listener.OnCurrentTurnChanged;
        }

        /// <summary>
        /// Unsubscribes the turn listener from the events of this <see cref="GameCore"/>.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <seealso cref="AddListener(ICurrentTurnListener)"/>
        public void RemoveListener(ICurrentTurnListener listener)
        {
            if (listener == null) {
                return;
            }
            OnTurnChangedEventHandler -= listener.OnCurrentTurnChanged;
        }

        /// <summary>
        /// Subscribes the player listener to the events of this <see cref="GameCore"/>.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <seealso cref="RemoveListener(ICurrentPlayerListener)"/>
        public void AddListener(ICurrentPlayerListener listener)
        {
            if (listener == null) {
                return;
            }
            CurrentPlayerChangedEventHandler += listener.OnCurrentPlayerChanged;
        }

        /// <summary>
        /// Unsubscribes the player listener from the events of this <see cref="GameCore"/>.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <seealso cref="AddListener(ICurrentPlayerListener)"/>
        public void RemoveListener(ICurrentPlayerListener listener)
        {
            if (listener == null) {
                return;
            }
            CurrentPlayerChangedEventHandler -= listener.OnCurrentPlayerChanged;
        }

        /// <summary>
        /// Gives every player a <see cref="TetrominoShape.O1"/> and <see cref="TetrominoShape.I2"/> tetromino from the shared reserve.
        /// And fills the black and white puzzle rows with puzzles from the decks.
        /// Throws an exception if this method is called more than once.
        /// </summary>
        /// <exception cref="InvalidOperationException">Game already initialized.</exception>
        public void InitializeGame()
        {
            if (_didInitialize) {
                throw new InvalidOperationException("Game already initialized");
            }
            _didInitialize = true;

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
        /// Gives every player a <see cref="TetrominoShape.O1"/> and <see cref="TetrominoShape.I2"/> tetromino from the shared reserve.
        /// Then asynchronously fills the black and white puzzle rows with puzzles from the decks.
        /// Throws an exception if this method is called more than once.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Game already initialized.</exception>
        public async Task InitializeGameAsync(CancellationToken cancellationToken = default)
        {
            if (_didInitialize) {
                throw new InvalidOperationException("Game already initialized");
            }
            _didInitialize = true;

            cancellationToken.ThrowIfCancellationRequested();

            // give all players their initial tetrominos
            foreach (var playerState in PlayerStates.Values) {
                playerState.AddTetromino(TetrominoShape.O1);
                playerState.AddTetromino(TetrominoShape.I2);
                GameState.RemoveTetromino(TetrominoShape.O1);
                GameState.RemoveTetromino(TetrominoShape.I2);
            }

            // fill puzzle rows with puzzles
            await GameState.RefillPuzzlesAsync(cancellationToken);
        }

        /// <summary>
        /// Finishes up internal game state and prepares for evaluating the results of the game.
        /// Should be called after <see cref="CurrentGamePhase"/> changes to <see cref="GamePhase.Finished"/>.
        /// Throws an exception if this method is called more than once.
        /// </summary>
        /// <exception cref="InvalidOperationException">Game already finalized.</exception>
        public void FinalizeGame()
        {
            if (_didFinalize) {
                throw new InvalidOperationException("Game already finalized");
            }
            _didFinalize = true;

            // remove points for unfinished puzzles
            foreach (var playerState in PlayerStates.Values) {
                foreach (var puzzle in playerState.GetUnfinishedPuzzles()) {
                    playerState.Score -= puzzle.RewardScore;
                }
            }
        }

        /// <summary>
        /// Determines the rank of each player based on their score, number of completed puzzles and leftover tetrominos.
        /// If this method is called before <see cref="FinalizeGame"/>, it will not take into account the points lost for unfinished puzzles.
        /// </summary>
        /// <returns>
        /// A dictionary containing the rank for each player. Player with rank 1 wins. It is possible for multiple players to have the same rank.
        /// </returns>
        /// <seealso cref="PlayerState.CompareTo(PlayerState?)"/>
        public Dictionary<Player, int> GetPlayerRankings()
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
        /// <remarks>Notifies listening <see cref="ICurrentPlayerListener"/>s if the current player changed. Then notifies <see cref="ICurrentTurnListener"/>s about the turn change.</remarks>
        /// <returns>Information about the next turn.</returns>
        /// <seealso cref="TurnManager.GetNextTurn"/>
        public TurnInfo GetNextTurnInfo()
        {
            CurrentTurn = _turnManager.GetNextTurn();
            CurrentPlayer = GetPlayerWithId(_turnManager.CurrentPlayerId);

            // first notify about the current player change
            if (CurrentTurn.NumActionsLeft == TurnManager.NumActionsInTurn) {
                CurrentPlayerChangedEventHandler?.Invoke(CurrentPlayer);
            }
            // then about turn change
            OnTurnChangedEventHandler?.Invoke(CurrentTurn);

            return CurrentTurn;
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
        public void ProcessAction(GameAction action)
        {
            action.Accept(_actionProcessors[CurrentPlayer]);
        }

        /// <summary>
        /// Asynchronously adjusts the <see cref="GameState"/> and the <see cref="PlayerState"/> of the current player based on the given action.
        /// Doesn't check if the action is valid. Use an <see cref="ActionVerifier"/> to check if the action is valid before calling this function.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ProcessActionAsync(GameAction action, CancellationToken cancellationToken = default)
        {
            await action.AcceptAsync(_actionProcessors[CurrentPlayer], cancellationToken);
        }

        #endregion
    }
}
