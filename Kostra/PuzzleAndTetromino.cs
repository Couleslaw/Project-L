namespace Kostra
{
    enum TetrominoShape { O1, O2, I2, I3, I4, L2, L3, Z, T }

    interface IPuzzleInfo
    {
        public uint Id { get; }
        public bool IsBlack { get; }
        public int RewardScore { get; }
        public TetrominoShape RewardTetromino { get; }
        public bool IsFinished { get; }
        public int NumEmptyCells { get; }
        public List<TetrominoShape> GetUsedTetrominos();
        public bool DoesTetrominoFit(Tetromino Tetromino);
    }

    class Puzzle
    {
        // id
        private static uint _idCounter = 0;
        public uint Id { get; } = _idCounter++;
        public static void ResetIdCounter() => _idCounter = 0;

        // puzzle parameters
        public bool IsBlack { get; }
        public const int PuzzleSize = 5;
        public int RewardScore { get; }
        public TetrominoShape RewardTetromino { get; }

        // binary representation of the puzzle image
        private int _binaryImage = 0;
        public int NumEmptyCells { get; private set; }
        public bool IsFinished => NumEmptyCells == 0;


        public Puzzle(int binaryImage, int score, TetrominoShape reward, bool isBlack)
        {
            if (binaryImage < 0 || binaryImage >= 1u << PuzzleSize * PuzzleSize)
            {
                throw new ArgumentException($"Puzzle must be {PuzzleSize}x{PuzzleSize}");
            }
            _binaryImage = binaryImage;
            RewardScore = score;
            RewardTetromino = reward;
            IsBlack = isBlack;

            // count empty cells
            NumEmptyCells = 0;
            for (int i = 0; i < PuzzleSize * PuzzleSize; i++)
            {
                if ((_binaryImage & (1u << i)) == 0)
                {
                    NumEmptyCells++;
                }
            }
        }

        private List<TetrominoShape> _usedTetrominos = new();
        public void AddTetromino(TetrominoShape tetromino)
        {
            _usedTetrominos.Add(tetromino);
            NumEmptyCells -= TetrominoLevelManager.GetShapeLevel(tetromino);
        }

        // public get --> copy as protection against modification by AI players
        public List<TetrominoShape> GetUsedTetrominos() => new(_usedTetrominos);

        public bool DoesTetrominoFit(int tetrominoBinaryImage)
        {
            return (tetrominoBinaryImage & _binaryImage) == 0;
        }
        public bool DoesTetrominoFit(ITetrominoInfo tetromino)
        {
            return DoesTetrominoFit(tetromino.BinaryImage);
        }
    }




    interface ITetrominoInfo
    {
        public TetrominoShape Shape { get; }
        public int BinaryImage { get; set; }
        public List<int> GetAllUniqueConfigurations();
    }

    class Tetromino : ITetrominoInfo
    {
        public TetrominoShape Shape { get; }
        public int BinaryImage { get; set; }

        private static int MoveUp(int image)
        {
            if ((image & 0b11111) == 0)
            {
                image >>= 5;
            }
            return image;
        }
        private static int MoveDown(int image)
        {
            if ((image & (0b11111 << 20)) == 0)
            {
                image <<= 5;
            }
            return image;
        }
        private static int MoveRight(int image)
        {
            if ((image & 0b1000010000100001000010000) == 0)
            {
                image <<= 1;
            }
            return image;
        }
        private static int MoveLeft(int image)
        {
            if ((image & 0b100001000010000100001) == 0)
            {
                image >>= 1;
            }
            return image;
        }
        private static int RotateRight(int image)
        {
            int newImage = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    // get the bit on the (i,j) position and move it using the rotation matrix
                    newImage |= ((image >> (5 * i + j) & 1) << (5 * j + 4 - i));
                }
            }
            return newImage;
        }
        private static int FlipHorizontally(int image)
        {
            int newImage = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    newImage |= ((image >> (5 * i + j) & 1) << (5 * i + 4 - j));
                }
            }
            return newImage;
        }
        private static int MoveImageToTopLeftCorner(int image)
        {
            while (true)
            {
                int newImage = MoveUp(image);
                if (newImage == image)
                {
                    break;
                }
                image = newImage;
            }
            while (true)
            {
                int newImage = MoveLeft(image);
                if (newImage == image)
                {
                    break;
                }
                image = newImage;
            }
            return image;
        }


        private List<int> _baseConfigurations;

        public Tetromino(TetrominoShape shape, int binaryImage)
        {
            Shape = shape;

            _baseConfigurations = new();

            for (int i = 0; i < 4; i++)
            {
                _baseConfigurations.Add(MoveImageToTopLeftCorner(binaryImage));
                binaryImage = RotateRight(binaryImage);
            }
            binaryImage = FlipHorizontally(binaryImage);
            for (int i = 0; i < 4; i++)
            {
                _baseConfigurations.Add(MoveImageToTopLeftCorner(binaryImage));
                binaryImage = RotateRight(binaryImage);
            }

            _baseConfigurations = _baseConfigurations.Distinct().ToList();

            BinaryImage = _baseConfigurations[0];
        }

        public List<int> GetAllUniqueConfigurations()
        {
            List<int> result = new List<int>();

            foreach (var baseConfig in _baseConfigurations)
            {
                // moving down
                int imageDown = baseConfig;
                while (true)
                {
                    // moving right
                    int imageRight = imageDown;
                    while (true)
                    {
                        result.Add(imageRight);
                        int newImageRight = MoveRight(imageRight);
                        if (newImageRight == imageRight)
                        {
                            break;
                        }
                        imageRight = newImageRight;
                    }
                    int newImageDown = MoveDown(imageDown);
                    if (newImageDown == imageDown)
                    {
                        break;
                    }
                    imageDown = newImageDown;
                }
            }
            return result;
        }
    }

    static class TetrominoLevelManager
    {
        public const int MinLevel = 1;
        public const int MaxLevel = 4;

        private static int[] _levels = new int[Enum.GetValues(typeof(TetrominoShape)).Length];
        private static List<TetrominoShape>[] _shapesByLevel = new List<TetrominoShape>[MaxLevel - MinLevel + 1];
        static TetrominoLevelManager()
        {  // class ctor
            _levels[(int)TetrominoShape.O1] = 1;
            _levels[(int)TetrominoShape.O2] = 4;
            _levels[(int)TetrominoShape.I2] = 2;
            _levels[(int)TetrominoShape.I3] = 3;
            _levels[(int)TetrominoShape.I4] = 4;
            _levels[(int)TetrominoShape.L2] = 3;
            _levels[(int)TetrominoShape.L3] = 4;
            _levels[(int)TetrominoShape.Z] = 4;
            _levels[(int)TetrominoShape.T] = 4;

            for (int i = 0; i < _levels.Length; i++)
            {
                _shapesByLevel[_levels[i] - MinLevel].Add((TetrominoShape)i);
            }
        }
        public static int GetShapeLevel(TetrominoShape shape) => _levels[(int)shape];
        public static List<TetrominoShape> GetShapesWithLevel(int level) => _shapesByLevel[level - MinLevel];
    }
}