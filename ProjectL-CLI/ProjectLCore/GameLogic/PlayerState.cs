namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// Represents the resources and progress of a single <see cref="Player"/>.
    ///   <list type="bullet">
    ///     <item>His current score.</item>
    ///     <item>Tetrominos he has.</item>
    ///     <item>Puzzles he is working on.</item>
    ///     <item>Puzzles he has completed.</item>
    ///   </list>
    /// </summary>
    /// <seealso cref="PlayerInfo"/>
    /// <seealso cref="GameState"/>
    public class PlayerState : IComparable<PlayerState>, IEquatable<PlayerState>
    {
        #region Constants

        /// <summary>  The maximum number of puzzles a player can be working on at the same time. </summary>
        public const int MaxPuzzles = 4;

        #endregion

        #region Fields

        private readonly Puzzle?[] _puzzles = { null, null, null, null };

        /// <summary> Contains the number of tetrominos owned by the player for each shape. </summary>
        private int[] _numTetrominosOwned = new int[TetrominoManager.NumShapes];

        /// <summary> Contains the ids of the puzzles that the player has already completed. </summary>
        private List<uint> _finishedPuzzleIds = new();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerState"/> class.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        public PlayerState(uint playerId)
        {
            PlayerId = playerId;
        }

        #endregion

        #region Events

        private event Action<int, FinishedPuzzleInfo>? PuzzleFinished;

        private event Action<int, Puzzle>? PuzzleAdded;

        private event Action<TetrominoShape, int>? TetrominosCollectionChanged;

        #endregion

        #region Properties

        /// <summary> Unique identifier of the player. </summary>
        public uint PlayerId { get; }

        /// <summary> Current score of the player. </summary>
        public int Score { get; set; } = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Subscribes the puzzle listener to the events of this <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <seealso cref="RemoveListener(IPlayerStatePuzzleListener)"/>
        public void AddListener(IPlayerStatePuzzleListener listener)
        {
            PuzzleFinished += listener.OnPuzzleFinished;
            PuzzleAdded += listener.OnPuzzleAdded;
        }

        /// <summary>
        /// Unsubscribes the puzzle listener from the events of this <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <seealso cref="AddListener(IPlayerStatePuzzleListener)"/>
        public void RemoveListener(IPlayerStatePuzzleListener listener)
        {
            PuzzleFinished -= listener.OnPuzzleFinished;
            PuzzleAdded -= listener.OnPuzzleAdded;
        }

        /// <summary>
        /// Subscribes the tetromino listener to the events of this <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        /// <seealso cref="RemoveListener(IPlayerStateTetrominoListener)"/>
        public void AddListener(IPlayerStateTetrominoListener listener)
        {
            TetrominosCollectionChanged += listener.OnTetrominoCollectionChanged;
        }

        /// <summary>
        /// Unsubscribes the tetromino listener from the events of this <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="listener">The listener to remove.</param>
        /// <seealso cref="AddListener(IPlayerStateTetrominoListener)"/>
        public void RemoveListener(IPlayerStateTetrominoListener listener)
        {
            TetrominosCollectionChanged -= listener.OnTetrominoCollectionChanged;
        }

        /// <summary>
        /// <para>
        /// Compares THIS instance with another <see cref="PlayerState" /> and returns an integer that indicates whether THIS instance precedes, follows, or occurs in the same position in the final scoring order as the other player.
        /// This is used for determining the order of players in the leaderboard.
        /// Meaning that if <c>A&lt;B</c> then A is in a better position than B.
        /// </para>
        /// <para>The player with the highest score wins.</para>
        /// <list type="bullet">
        ///   <item>In case of a tie, the player who has completed the most puzzles wins.</item>
        ///   <item>In case of a tie, the player with the most leftover tetrominos wins.</item>
        ///   <item>If there is still a tie, the payers are placed at the same position. </item>
        /// </list>
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table"><listheader><term> Value</term><term> Meaning</term></listheader><item><description> Less than zero</description><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><description> Zero</description><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><description> Greater than zero</description><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list>
        /// </returns>
        /// <seealso cref="IComparable{T}"/>
        public int CompareTo(PlayerState? other)
        {
            if (other is null) {
                return 1;
            }

            // A < B  iif  A is in the scoring order before B   (A is better than B)
            return -GetPointsResult();

            // A < B  iff  A has less score/puzzles/tetrominos than B
            int GetPointsResult()
            {
                // compare by score
                var res = Score.CompareTo(other.Score);
                if (res != 0)
                    return res;

                // more puzzles finished wins
                res = _finishedPuzzleIds.Count.CompareTo(other._finishedPuzzleIds.Count);
                if (res != 0)
                    return res;

                // more leftover tetrominos wins
                return _numTetrominosOwned.Sum().CompareTo(other._numTetrominosOwned.Sum());
            }
        }

        /// <summary>
        /// Indicates whether the current <see cref="PlayerState"/> is equal to another <see cref="PlayerState"/> of the same type.
        /// </summary>
        /// <param name="other">An <see cref="PlayerState"/> to compare with this <see cref="PlayerState"/>.</param>
        /// <returns>
        ///   <see langword="true" /> if the current <see cref="PlayerState"/> is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        /// <seealso cref="CompareTo"/>
        /// <seealso cref="IEquatable{T}"/>
        public bool Equals(PlayerState? other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="PlayerState"/>.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object is equal to the current <see cref="PlayerState"/>; otherwise, <see langword="false" />.
        /// </returns>
        /// <seealso cref="Equals(PlayerState?)"/>
        public override bool Equals(object? obj)
        {
            if (obj is PlayerState other) {
                return Equals(other);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return PlayerId.GetHashCode();
        }

        /// <summary>
        /// Adds the puzzle to the player's unfinished puzzles.
        /// Also calls the <see cref="IPlayerStatePuzzleListener.OnPuzzleAdded(int, Puzzle)"/> method of all listeners.
        /// </summary>
        /// <param name="puzzle">The puzzle.</param>
        /// <exception cref="InvalidOperationException">No space for puzzle</exception>
        public void PlaceNewPuzzle(Puzzle puzzle)
        {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is null) {
                    _puzzles[i] = puzzle;
                    PuzzleAdded?.Invoke(i, puzzle);
                    return;
                }
            }
            throw new InvalidOperationException("No space for puzzle");
        }

        /// <summary>
        /// Removes the puzzle specified in the <see cref="FinishedPuzzleInfo"/> from the player's unfinished puzzles and adds it to the list of finished puzzles.
        /// Also calls the <see cref="IPlayerStatePuzzleListener.OnPuzzleFinished(int, FinishedPuzzleInfo)"/> method of all listeners.
        /// </summary>
        /// <param name="info">Information about the puzzle.</param>
        /// <exception cref="InvalidOperationException">Puzzle not found</exception>
        public void FinishPuzzle(FinishedPuzzleInfo info)
        {
            uint id = info.Puzzle.Id;
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is not null && _puzzles[i]!.Id == id) {
                    _puzzles[i] = null;
                    _finishedPuzzleIds.Add(id);
                    PuzzleFinished?.Invoke(i, info);
                    return;
                }
            }
            throw new InvalidOperationException("Puzzle not found");
        }

        /// <summary>
        /// Gets the puzzle matching the specified identifier.
        /// </summary>
        /// <param name="id">The puzzle identifier.</param>
        /// <returns>The puzzle with the specified identifier or <see langword="null" /> if the player doesn't have a puzzle matching the <paramref name="id"/>.</returns>
        public Puzzle? GetPuzzleWithId(uint id)
        {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is not null && _puzzles[i]!.Id == id) {
                    return _puzzles[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a list of puzzles the player is currently working on.
        /// </summary>
        /// <returns>A list of puzzles the player is currently working on.</returns>
        public List<Puzzle> GetUnfinishedPuzzles()
        {
            List<Puzzle> result = new();
            for (int i = 0; i < _puzzles.Length; i++) {
                if (_puzzles[i] is not null) {
                    result.Add(_puzzles[i]!);
                }
            }
            return result;
        }

        /// <summary>
        /// Adds the given tetromino to the player's personal collection.
        /// Also calls the <see cref="IPlayerStatePuzzleListener.OnTetrominoCollectionChanged(TetrominoShape, int)"/> method of all listeners.
        /// </summary>
        /// <param name="shape">The tetromino type to add.</param>
        public void AddTetromino(TetrominoShape shape)
        {
            _numTetrominosOwned[(int)shape]++;
            TetrominosCollectionChanged?.Invoke(shape, _numTetrominosOwned[(int)shape]);
        }

        /// <summary>
        /// Removes the tetromino from the player's personal collection.
        /// Also calls the <see cref="IPlayerStatePuzzleListener.OnTetrominoCollectionChanged(TetrominoShape, int)"/> method of all listeners.
        /// </summary>
        /// <param name="shape">The tetromino type to remove.</param>
        public void RemoveTetromino(TetrominoShape shape)
        {
            if (_numTetrominosOwned[(int)shape] == 0) {
                throw new InvalidOperationException("Cannot remove tetromino that has not been placed.");
            }
            _numTetrominosOwned[(int)shape]--;
            TetrominosCollectionChanged?.Invoke(shape, _numTetrominosOwned[(int)shape]);
        }

        /// <summary>
        /// Returns a copy of information about the player wrapped in a <see cref="PlayerInfo" /> object. It prevents modification of the original data.
        /// </summary>
        /// <returns>A copy of information about the player.</returns>
        public PlayerInfo GetPlayerInfo() => new PlayerInfo(this);

        #endregion

        /// <summary>
        /// Provides information about about a <see cref="PlayerState"/> while preventing modification of the original data.
        /// </summary>
        /// <seealso cref="PlayerState"/>
        /// <seealso cref="Player"/>
        public class PlayerInfo
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="PlayerInfo"/> class.
            /// </summary>
            /// <param name="playerState">The player state that will be wrapped.</param>
            public PlayerInfo(PlayerState playerState)
            {
                PlayerId = playerState.PlayerId;
                Score = playerState.Score;
                NumTetrominosOwned = Array.AsReadOnly(playerState._numTetrominosOwned);
                UnfinishedPuzzles = playerState.GetUnfinishedPuzzles().Select(p => p.Clone()).ToArray();
                FinishedPuzzlesIds = playerState._finishedPuzzleIds.AsReadOnly();
            }

            #endregion

            #region Properties

            /// <summary> The unique identifier of the player. </summary>
            public uint PlayerId { get; }

            /// <summary> The current score of the player. </summary>
            public int Score { get; }

            /// <summary> The number of tetrominos owned by the player for each shape. </summary>
            public IReadOnlyList<int> NumTetrominosOwned { get; }

            /// <summary> The puzzles that the player is currently working on. </summary>
            public Puzzle[] UnfinishedPuzzles { get; }

            /// <summary> The Ids of the puzzles that the player has already completed. </summary>
            public IReadOnlyList<uint> FinishedPuzzlesIds { get; }

            #endregion

            #region Methods

            /// <summary>
            /// Converts to string. It shows the player's score, the number of tetrominos owned for each shape, the puzzles the player is working on, and the number of puzzles the player has completed.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                StringBuilder sb = new();
                sb.AppendLine($"ID: {PlayerId}, Score: {Score}, Num finished puzzles: {FinishedPuzzlesIds.Count}");

                // append tetromino info
                sb.Append("Tetrominos:");
                AppendTetrominoInfo(NumTetrominosOwned);

                // add info about unfinished puzzles
                sb.AppendLine().AppendLine();
                if (UnfinishedPuzzles.Length > 0) {
                    AppendPuzzleInfo();
                }
                else {
                    sb.AppendLine("  The player doesn't have any puzzles.");
                }

                return sb.AppendLine().ToString();

                void AppendTetrominoInfo(IReadOnlyList<int> tetrominos)
                {
                    bool first = true;
                    for (int i = 0; i < tetrominos.Count; i++) {
                        if (tetrominos[i] == 0) {
                            continue;
                        }
                        if (first) {
                            first = false;
                        }
                        else {
                            sb.Append(',');
                        }
                        sb.Append($"  {(TetrominoShape)i}: {tetrominos[i]}");
                    }
                }

                void AppendPuzzleInfo()
                {
                    StringReader[] puzzleReaders = UnfinishedPuzzles.Select(p => p.Image.ToString()).Select(p => new StringReader(p)).ToArray();

                    for (int i = 0; i < UnfinishedPuzzles.Length; i++) {
                        if (i > 0) {
                            sb.AppendLine();
                        }
                        Puzzle puzzle = UnfinishedPuzzles[i];

                        // make a reader for the image
                        var reader = new StringReader(puzzle.Image.ToString());
                        for (int lineNum = 0; lineNum < 5; lineNum++) {
                            // add the image line
                            sb.Append("  ").Append(reader.ReadLine()).Append("   ");
                            // add reward info to second line
                            if (lineNum == 1) {
                                sb.Append($"Reward: {puzzle.RewardScore},  {puzzle.RewardTetromino}");
                            }
                            // add used tetrominos to third line
                            if (lineNum == 2) {
                                sb.Append($"Used tetrominos:");
                                var usedTetrominoCounts = new int[TetrominoManager.NumShapes];
                                foreach (TetrominoShape shape in puzzle.GetUsedTetrominos()) {
                                    usedTetrominoCounts[(int)shape]++;
                                }
                                AppendTetrominoInfo(usedTetrominoCounts);
                            }
                            // add puzzle id to fourth line
                            if (lineNum == 3) {
                                sb.Append($"ID: {puzzle.Id}");
                            }
                            sb.AppendLine();
                        }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <seealso cref="CompareTo"/>
        public static bool operator >(PlayerState left, PlayerState right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <seealso cref="CompareTo"/>
        public static bool operator <(PlayerState left, PlayerState right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <seealso cref="CompareTo"/>
        public static bool operator >=(PlayerState left, PlayerState right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <seealso cref="CompareTo"/>
        public static bool operator <=(PlayerState left, PlayerState right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <seealso cref="Equals(PlayerState?)"/>
        public static bool operator ==(PlayerState left, PlayerState right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        /// <seealso cref="Equals(PlayerState?)"/>
        public static bool operator !=(PlayerState left, PlayerState right)
        {
            return !left.Equals(right);
        }
    }
}
