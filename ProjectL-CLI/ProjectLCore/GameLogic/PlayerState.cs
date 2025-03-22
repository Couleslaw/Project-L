namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;

    /// <summary>
    /// Represents the resources and progress of a single player.
    ///   <list type="bullet">
    ///     <item>His current score.</item>
    ///     <item>Tetrominos he has.</item>
    ///     <item>Puzzles he is working on.</item>
    ///     <item>Puzzles he has completed.</item>
    ///   </list>
    /// </summary>
    /// <param name="playerId">The unique identifier of the player.</param>
    public class PlayerState(uint playerId) : IComparable<PlayerState>, IEquatable<PlayerState>
    {
        #region Constants

        /// <summary>  The maximum number of puzzles a player can be working on at the same time. </summary>
        public const int MaxPuzzles = 4;

        #endregion

        #region Fields

        private readonly Puzzle?[] _puzzles = [null, null, null, null];

        /// <summary> Contains the number of tetrominos owned by the player for each shape. </summary>
        private int[] _numTetrominosOwned = new int[TetrominoManager.NumShapes];

        /// <summary> Contains the ids of the puzzles that the player has already completed. </summary>
        private List<uint> _finishedPuzzleIds = new();

        #endregion

        #region Properties

        /// <summary> Unique identifier of the player. </summary>
        public uint PlayerId { get; init; } = playerId;

        /// <summary> Current score of the player. </summary>
        public int Score { get; set; } = 0;

        #endregion

        #region Methods

        #region Implement IComparable 

        /// <summary>
        /// <para>
        /// Compares THIS instance with another <see cref="PlayerState" /> and returns an integer that indicates whether THIS instance precedes, follows, or occurs in the same position in the final scoring order as the other player.
        /// This is used for determining the order of players in the leaderboard.
        /// Meaning that if <c>A&lt;B</c> then A is in a better position than B.
        /// </para>
        /// <para>The player with the highest score wins.</para>
        ///   <list type="bullet">
        ///     <item>In case of a tie, the player who has completed the most puzzles wins.</item>
        ///     <item>In case of a tie, the player with the most leftover tetrominos wins.</item>
        ///     <item>If there is still a tie, the payers are placed at the same position. </item>
        ///   </list>
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table"><listheader><term> Value</term><term> Meaning</term></listheader><item><description> Less than zero</description><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><description> Zero</description><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><description> Greater than zero</description><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
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

        // Define the is greater than operator.
        public static bool operator >(PlayerState operand1, PlayerState operand2)
        {
            return operand1.CompareTo(operand2) > 0;
        }

        // Define the is less than operator.
        public static bool operator <(PlayerState operand1, PlayerState operand2)
        {
            return operand1.CompareTo(operand2) < 0;
        }

        // Define the is greater than or equal to operator.
        public static bool operator >=(PlayerState operand1, PlayerState operand2)
        {
            return operand1.CompareTo(operand2) >= 0;
        }

        // Define the is less than or equal to operator.
        public static bool operator <=(PlayerState operand1, PlayerState operand2)
        {
            return operand1.CompareTo(operand2) <= 0;
        }

        #endregion

        #region Implement IEquatable
        public bool Equals(PlayerState? other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj is PlayerState other) {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return PlayerId.GetHashCode();
        }

        // Define the is equal to operator.
        public static bool operator ==(PlayerState operand1, PlayerState operand2)
        {
            return operand1.Equals(operand2);
        }

        // Define the is not equal to operator.
        public static bool operator !=(PlayerState operand1, PlayerState operand2)
        {
            return !operand1.Equals(operand2);
        }

        #endregion

        #region Puzzles

        /// <summary>
        /// Adds the puzzle to the player's unfinished puzzles.
        /// </summary>
        /// <param name="puzzle">The puzzle.</param>
        /// <exception cref="InvalidOperationException">No space for puzzle</exception>
        public void PlaceNewPuzzle(Puzzle puzzle)
        {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is null) {
                    _puzzles[i] = puzzle;
                    return;
                }
            }
            throw new InvalidOperationException("No space for puzzle");
        }

        /// <summary>
        /// Removes the puzzle with the specified identifier from the player's unfinished puzzles and adds it to the list of finished puzzles.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="InvalidOperationException">Puzzle not found</exception>
        public void FinishPuzzleWithId(uint id)
        {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is not null && _puzzles[i]!.Id == id) {
                    _puzzles[i] = null;
                    _finishedPuzzleIds.Add(id);
                    return;
                }
            }
            throw new InvalidOperationException("Puzzle not found");
        }

        /// <summary>
        /// Returns the puzzle matching the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
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
        /// Returns the puzzles that the player is currently working on.
        /// </summary>
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

        #endregion

        #region Tetrominos

        /// <summary>
        /// Adds the given tetromino to the player's personal collection.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public void AddTetromino(TetrominoShape shape)
        {
            _numTetrominosOwned[(int)shape]++;
        }

        /// <summary>
        /// Removes the tetromino from the player's personal collection.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public void RemoveTetromino(TetrominoShape shape)
        {
            _numTetrominosOwned[(int)shape]++;
        }

        #endregion

        /// <summary>
        /// Returns a copy of information about the player wrapped in a <see cref="PlayerInfo" /> object.
        /// </summary>
        public PlayerInfo GetPlayerInfo() => new PlayerInfo(this);

        #endregion

        /// <summary>
        /// Provides information about about a player while preventing modification of the original data.
        /// </summary>
        public class PlayerInfo(PlayerState playerState)
        {
            #region Fields

            /// <summary> The unique identifier of the player. </summary>
            public uint PlayerId = playerState.PlayerId;

            /// <summary> The current score of the player. </summary>
            public int Score = playerState.Score;

            /// <summary> The number of tetrominos owned by the player for each shape. </summary>
            public IReadOnlyList<int> NumTetrominosOwned = playerState._numTetrominosOwned.AsReadOnly();

            /// <summary> The puzzles that the player is currently working on. </summary>
            public Puzzle[] UnfinishedPuzzles = playerState.GetUnfinishedPuzzles().Select(p => p.Clone()).ToArray();

            /// <summary> The Ids of the puzzles that the player has already completed. </summary>
            public IReadOnlyList<uint> FinishedPuzzlesIds = playerState._finishedPuzzleIds.AsReadOnly();

            #endregion
        }

        
    }
}
