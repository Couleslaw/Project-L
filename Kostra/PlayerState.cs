namespace Kostra {
    class PlayerState(uint playerId) : IComparable<PlayerState>, IEquatable<PlayerState> {
        public uint PlayerId { get; init; } = playerId;
        public int Score { get; set; } = 0;

        private int[] _numTetrominosOwned = new int[TetrominoManager.NumShapes];

        public static int MaxPuzzles = 4;
        private readonly Puzzle?[] _puzzles = [null, null, null, null];

        private List<uint> _finishedPuzzleIds = new();

        // Fullfile the IComparable interface
        public int CompareTo(PlayerState? other)
        {
            if (other is null)
            {
                return 1;
            }

            // compare by score
            var res = Score.CompareTo(other.Score);
            if (res != 0) return res;

            // more puzzles finished wins
            res = _finishedPuzzleIds.Count.CompareTo(other._finishedPuzzleIds.Count);
            if (res != 0) return res;

            // more leftover tetrominos wins
            return _numTetrominosOwned.Sum().CompareTo(other._numTetrominosOwned.Sum());
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

        // Fullfile the IEquatable interface
        public bool Equals(PlayerState? other)
        {
            return CompareTo(other) == 0;
        }
        public override bool Equals(object? obj)
        {
            if (obj is PlayerState other)
            {
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


        public void PlaceNewPuzzle(Puzzle puzzle) {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is null) {
                    _puzzles[i] = puzzle;
                    return;
                }
            }
            throw new InvalidOperationException("No space for puzzle");
        }
        public void FinishPuzzleWithId(uint id) {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is not null && _puzzles[i]!.Id == id) {
                    _puzzles[i] = null;
                    _finishedPuzzleIds.Add(id);
                    return;
                }
            }
            throw new InvalidOperationException("Puzzle not found");
        }
        public Puzzle? GetPuzzleWithId(uint id) {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is not null && _puzzles[i]!.Id == id) {
                    return _puzzles[i];
                }
            }
            return null;
        }
        public List<Puzzle> GetUnfinishedPuzzles() {
            List<Puzzle> result = new();
            for (int i = 0; i < _puzzles.Length; i++) {
                if (_puzzles[i] is not null) {
                    result.Add(_puzzles[i]!);
                }
            }
            return result;
        }
        public void AddTetromino(TetrominoShape shape) {
            _numTetrominosOwned[(int)shape]++;
        }
        public void RemoveTetromino(TetrominoShape shape) {
            _numTetrominosOwned[(int)shape]++;
        }

        public PlayerInfo GetPlayerInfo()
        {
            return new PlayerInfo(this);
        }
        public class PlayerInfo(PlayerState playerState)
        {
            // wrapper around PlayerState to prevent modification
            public uint PlayerId = playerState.PlayerId;
            public int Score = playerState.Score;
            public IReadOnlyList<int> NumTetrominosOwned = playerState._numTetrominosOwned.AsReadOnly();
            public Puzzle[] UnfinishedPuzzles = playerState.GetUnfinishedPuzzles().Select(p => p.Clone()).ToArray();
            public IReadOnlyList<uint> FinishedPuzzlesIds = playerState._finishedPuzzleIds.AsReadOnly();
        }
    }
}
