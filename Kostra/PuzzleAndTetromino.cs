namespace Kostra {
    enum Color { Empty, Fill, Yellow, Green, LightOrange, DarkOrange, LightBlue, DarkBlue, Red, Purple, Pink }
    enum TetrominoShape { O1, O2, I2, I3, I4, L2, L3, Z, T }

    class Puzzle {
        // id
        private static uint _idCounter = 0;
        public uint Id { get; } = _idCounter++;
        public static void ResetIdCounter() => _idCounter = 0;

        // puzzle parameters
        public const int PuzzleSize = 5;
        public int Score { get; }
        public TetrominoShape RewardTetromino { get; }
        public Color[,] PuzzleMatrix { get; }
        public int NumEmptyCells { get; private set; }
        public bool IsFinished => NumEmptyCells == 0;


        public Puzzle(Color[,] puzzle, int score, TetrominoShape reward) {
            if (puzzle.GetLength(0) != PuzzleSize || puzzle.GetLength(1) != PuzzleSize) {
                throw new ArgumentException($"Puzzle must be {PuzzleSize}x{PuzzleSize}");
            }
            PuzzleMatrix = puzzle;
            Score = score;
            RewardTetromino = reward;
            NumEmptyCells = PuzzleMatrix.Cast<Color>().Count(c => c == Color.Empty);
        }

        private Dictionary<TetrominoShape, int> _usedTetrominos = new();
        public void AddTetromino(TetrominoShape tetromino) {
            _usedTetrominos[tetromino] = _usedTetrominos.GetValueOrDefault(tetromino, 0) + 1;
            NumEmptyCells -= TetrominoLevelManager.GetShapeLevel(tetromino);
        }
        public Dictionary<TetrominoShape, int> GetUsedTetrominos() => _usedTetrominos;
    }


    record struct Position(int X, int Y);
    enum Rotation { Forward, Left, Backward, Right };

    // pro AI na zjisteni moznych tahu - Square nema cenu rotovat
    enum TetrominoType { Square, Line, ComplexNoFlip, ComplesFlip }

    interface ITetromino {
        public Color Color { get; }
        public TetrominoType Type { get; }
        public TetrominoShape Shape { get; }
        public Position Center { get; }
        public Rotation Rotation { get; }
        public bool IsFlipped { get; }
        public void Flip();
        public void SetRotation(Rotation rotation);
        public void RotateLeft();
        public void RotateRight();
        public bool CanBePlacedIn(Puzzle puzzle, Position pos);
        public void PlaceIn(Puzzle puzzle, Position pos);
    }

    abstract class TetrominoBase : ITetromino {
        public abstract TetrominoType Type { get; }
        public abstract TetrominoShape Shape { get; }
        public abstract Color Color { get; }
        public abstract Position Center { get; }
        public bool IsFlipped { get; private set; } = false;
        public Rotation Rotation { get; set; } = Rotation.Forward;
        public abstract void Flip();
        public abstract void SetRotation(Rotation rotation);
        public void RotateLeft() => SetRotation((Rotation)(((int)Rotation + 1) % 4));
        public void RotateRight() => SetRotation((Rotation)(((int)Rotation + 3) % 4));

        protected bool[,] _tetrominoMatrix;
        public bool CanBePlacedIn(Puzzle puzzle, Position pos) {
            throw new NotImplementedException();
        }
        public void PlaceIn(Puzzle puzzle, Position pos) {
            throw new NotImplementedException();
        }
    }

    static class TetrominoLevelManager {
        public const int MinLevel = 1;
        public const int MaxLevel = 4;

        private static int[] _levels = new int[Enum.GetValues(typeof(TetrominoShape)).Length];
        private static List<TetrominoShape>[] _shapesByLevel = new List<TetrominoShape>[MaxLevel - MinLevel + 1];
        static TetrominoLevelManager() {  // class ctor
            _levels[(int)TetrominoShape.O1] = 1;
            _levels[(int)TetrominoShape.O2] = 4;
            _levels[(int)TetrominoShape.I2] = 2;
            _levels[(int)TetrominoShape.I3] = 3;
            _levels[(int)TetrominoShape.I4] = 4;
            _levels[(int)TetrominoShape.L2] = 3;
            _levels[(int)TetrominoShape.L3] = 4;
            _levels[(int)TetrominoShape.Z] = 4;
            _levels[(int)TetrominoShape.T] = 4;

            for (int i = 0; i < _levels.Length; i++) {
                _shapesByLevel[_levels[i] - MinLevel].Add((TetrominoShape)i);
            }
        }
        public static int GetShapeLevel(TetrominoShape shape) => _levels[(int)shape];
        public static List<TetrominoShape> GetShapesWithLevel(int level) => _shapesByLevel[level - MinLevel];
    }
}