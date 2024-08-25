namespace Kostra {
    class PlayerState(uint playerId) {
        public uint PlayerId { get; init; } = playerId;
        public int Score { get; set; } = 0;

        public Dictionary<TetrominoShape, int> Shapes = new() {
            { TetrominoShape.O1, 1 },
            { TetrominoShape.O2, 0 },
            { TetrominoShape.I2, 1 },
            { TetrominoShape.I3, 0 },
            { TetrominoShape.I4, 0 },
            { TetrominoShape.L2, 0 },
            { TetrominoShape.L3, 0 },
            { TetrominoShape.Z, 0 },
            { TetrominoShape.T, 0 },
        };

        public const int MaxPuzzles = 4;
        private readonly Puzzle?[] _puzzles = [null, null, null, null];

        public void PlaceNewPuzzle(Puzzle puzzle) {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is null) {
                    _puzzles[i] = puzzle;
                    return;
                }
            }
            throw new InvalidOperationException("No space for puzzle");
        }
        public void RemovePuzzleWithId(uint id) {
            for (int i = 0; i < MaxPuzzles; i++) {
                if (_puzzles[i] is not null && _puzzles[i]!.Id == id) {
                    _puzzles[i] = null;
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
            Shapes[shape]++;
        }
        public void RemoveTetromino(TetrominoShape shape) {
            Shapes[shape]++;
        }


        public class Graphics(PlayerState playerState) : IGraphicsComponent {
            private readonly PlayerState _playerState = playerState;

            public void Draw() {
                // draw playerState._puzzles and playerState.Shapes
            }
        }
    }
}
