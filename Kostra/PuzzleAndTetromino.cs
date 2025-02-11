using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Kostra
{
    /// <summary>
    /// Reprezents a 5x5 binary image. The image is stored as an integer, where each bit represents a cell in the image.
    /// The top left corner is viewed the least significant bit. We go down row by row from left to right.
    /// 
    /// <example><code>
    /// 
    /// #####         11111
    /// ##.##         11011  
    /// ##..#  ---->  11001  ---->  0b10011_10001_10011_11011_11111
    /// #...#         10001    
    /// ##..#         11001
    /// 
    /// </code></example>
    /// </summary>
    public readonly struct BinaryImage : IEquatable<BinaryImage>
    {
        private readonly int _image;
        public static BinaryImage EmptyImage => new(0);
        public static BinaryImage FullImage => new((1 << 26) - 1);

        public BinaryImage(int image)
        {
            if (image < 0 || image >= 1 << 25)
            {
                throw new ArgumentException("Binary image must be 5x5");
            }
            _image = image;
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    sb.Append((_image >> (5 * i + j) & 1) == 1 ? '#' : '.');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }


        // Implement IEquatable

        public bool Equals(BinaryImage other)
        {
            return _image == other._image;
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is BinaryImage bi)
            {
                return Equals(bi);
            }
            return false;
        }
        public override int GetHashCode() => _image.GetHashCode();
        public static bool operator ==(BinaryImage left, BinaryImage right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(BinaryImage left, BinaryImage right)
        {
            return !left.Equals(right);
        }

        // Useful bitwise operators

        public static BinaryImage operator &(BinaryImage left, BinaryImage right)
        {
            return new(left._image & right._image);
        }
        public static BinaryImage operator |(BinaryImage left, BinaryImage right)
        {
            return new(left._image | right._image);
        }
        public static BinaryImage operator ~(BinaryImage image)
        {
            return new(~image._image);
        }

        public int CountFilledCells()
        {
            int count = 0;
            for (int i = 0; i < 25; i++)
            {
                count += (_image >> i) & 1;
            }
            return count;
        }
        public int CountEmptyCells()
        {
            return 25 - CountFilledCells();
        }

        // Image transformations
        public BinaryImage MoveUp()
        {
            int newImage = _image;
            if ((_image & 0b11111) == 0)
            {
                newImage >>= 5;
            }
            return new(newImage);
        }
        public BinaryImage MoveDown()
        {
            int newImage = _image;
            if ((_image & (0b11111 << 20)) == 0)
            {
                newImage <<= 5;
            }
            return new(newImage);
        }
        public BinaryImage MoveRight()
        {
            int newImage = _image;
            if ((_image & 0b1000010000100001000010000) == 0)
            {
                newImage <<= 1;
            }
            return new(newImage);
        }
        public BinaryImage MoveLeft()
        {
            int newImage = _image;
            if ((_image & 0b100001000010000100001) == 0)
            {
                newImage >>= 1;
            }
            return new(newImage);
        }
        public BinaryImage RotateRight()
        {
            int newImage = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    // (i,j) -> (j, 4-i)
                    newImage |= ((_image >> (5 * i + j) & 1) << (5 * j + 4 - i));
                }
            }
            return new(newImage);
        }
        public BinaryImage RotateLeft()
        {
            int newImage = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    // (i,j) -> (4-j, i)
                    newImage |= ((_image >> (5 * i + j) & 1) << (5 * (4 - j) + i));
                }
            }
            return new(newImage);
        }
        public BinaryImage FlipHorizontally()
        {
            int newImage = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    // (i,j) -> (i, 4-j)
                    newImage |= ((_image >> (5 * i + j) & 1) << (5 * i + 4 - j));
                }
            }
            return new(newImage);
        }
        public BinaryImage FlipVertically()
        {
            int newImage = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    // (i,j) -> (4-i, j)
                    newImage |= ((_image >> (5 * i + j) & 1) << (5 * (4 - i) + j));
                }
            }
            return new(newImage);
        }
        public BinaryImage MoveImageToTopLeftCorner()
        {
            int newImage = _image;
            // move image up
            while ((newImage & 0b11111) == 0)
            {
                newImage >>= 5;
            }
            // move image left
            while ((newImage & 0b100001000010000100001) == 0)
            {
                newImage >>= 1;
            }
            return new(newImage);
        }
    }

    public enum TetrominoShape { O1, O2, I2, I3, I4, L2, L3, Z, T }

    public class Puzzle
    {
        // id
        private static uint _idCounter = 0;
        public uint Id { get; } = _idCounter++;

        // puzzle parameters
        public bool IsBlack { get; }
        public int RewardScore { get; }
        public TetrominoShape RewardTetromino { get; }

        // binary representation of the puzzle image
        public BinaryImage Image { get; private set; }
        public int NumEmptyCells { get; private set; }
        public bool IsFinished => NumEmptyCells == 0;


        public Puzzle(BinaryImage binaryImage, int score, TetrominoShape reward, bool isBlack)
        {
            Image = binaryImage;
            RewardScore = score;
            RewardTetromino = reward;
            IsBlack = isBlack;

            NumEmptyCells = Image.CountEmptyCells();
        }


        // index by shape to get number of used tetrominos of that shape
        private int[] _usedTetrominos = new int[TetrominoManager.NumShapes];

        public bool CanPlaceTetromino(BinaryImage tetromino) => (Image & tetromino) == BinaryImage.EmptyImage;
        public void AddTetromino(TetrominoShape tetromino, BinaryImage tetrominoImage)
        {
            _usedTetrominos[(int)tetromino]++;
            NumEmptyCells -= TetrominoManager.GetLevelOf(tetromino);
            Image |= tetrominoImage;
        }
        public IEnumerable<TetrominoShape> GetUsedTetrominos()
        {
            for (int shape = 0; shape < TetrominoManager.NumShapes; shape++)
            {
                for (int j = 0; j < _usedTetrominos[shape]; j++)
                {
                    yield return (TetrominoShape)shape;
                }
            }
        }
    
        public Puzzle Clone()
        {
            Puzzle clone = new(Image, RewardScore, RewardTetromino, IsBlack);
            clone.NumEmptyCells = NumEmptyCells;
            clone._usedTetrominos = _usedTetrominos.ToArray(); // copy array
            return clone;
        }
    }

    public static class TetrominoManager
    {
        public static int NumShapes = Enum.GetValues(typeof(TetrominoShape)).Length;

        public const int MinLevel = 1;
        public const int MaxLevel = 4;

        private static readonly int[] _levels = new int[NumShapes];
        private static readonly List<TetrominoShape>[] _shapesByLevel = new List<TetrominoShape>[MaxLevel - MinLevel + 1];
        private static readonly BinaryImage[] _binaryImages = new BinaryImage[NumShapes];
        private static readonly List<BinaryImage>[] _baseConfigurations = new List<BinaryImage>[NumShapes];

        static TetrominoManager()
        {  // class ctor

            // initialize tetromino images
            _binaryImages[(int)TetrominoShape.O1] = new(0b1);
            _binaryImages[(int)TetrominoShape.O2] = new(0b1100011);
            _binaryImages[(int)TetrominoShape.I2] = new(0b11);
            _binaryImages[(int)TetrominoShape.I3] = new(0b111);
            _binaryImages[(int)TetrominoShape.I4] = new(0b1111);
            _binaryImages[(int)TetrominoShape.L2] = new(0b100011);
            _binaryImages[(int)TetrominoShape.L3] = new(0b100111);
            _binaryImages[(int)TetrominoShape.Z] = new(0b11000011);
            _binaryImages[(int)TetrominoShape.T] = new(0b1000111);

            // level of tetromino = number of 1s in its binary image
            for (int i = 0; i < NumShapes; i++)
            {
                _levels[i] = _binaryImages[i].CountFilledCells();
            }

            // create list of shapes for each level
            for (int i = 0; i < NumShapes; i++)
            {
                int index = _levels[i] - MinLevel;
                if (_shapesByLevel[index] is null)
                {
                    _shapesByLevel[index] = new();
                }
                _shapesByLevel[index].Add((TetrominoShape)i);
            }

            // generate all base configurations for each shape
            for (int i = 0; i < NumShapes; i++)
            {
                _baseConfigurations[i] = GetBaseConfigurationsOf((TetrominoShape)i);
            }
        }

        public static BinaryImage GetImageOf(TetrominoShape shape) => _binaryImages[(int)shape];
        public static int GetLevelOf(TetrominoShape shape) => _levels[(int)shape];
        public static List<TetrominoShape> GetShapesWithLevel(int level) => _shapesByLevel[level - MinLevel];
        public static bool CompareShapeToImage(TetrominoShape shape, BinaryImage image)
        {
            // checks if the images is a valid configuration of the shape
            BinaryImage baseConf = image.MoveImageToTopLeftCorner();
            return _baseConfigurations[(int)shape].Contains(baseConf);
        }

        private static List<BinaryImage> GetBaseConfigurationsOf(TetrominoShape shape)
        {
            List<BinaryImage> conf = new();
            BinaryImage image = _binaryImages[(int)shape];

            for (int i = 0; i < 4; i++)
            {
                conf.Add(image.MoveImageToTopLeftCorner());
                image = image.RotateRight();
            }
            image = image.FlipHorizontally();
            for (int i = 0; i < 4; i++)
            {
                conf.Add(image.MoveImageToTopLeftCorner());
                image = image.RotateRight();
            }

            return conf.Distinct().ToList();
        }

        private static List<BinaryImage>[] _allConfigurationsCache = new List<BinaryImage>[NumShapes];
        public static List<BinaryImage> GetAllUniqueConfigurationsOf(TetrominoShape shape)
        {
            // check cache first
            if (_allConfigurationsCache[(int)shape] is not null)
            {
                return _allConfigurationsCache[(int)shape];
            }

            // generate all unique configurations of the shape

            List<BinaryImage> result = new();

            foreach (BinaryImage image in _baseConfigurations[(int)shape])
            {
                BinaryImage downImage = image;
                // moving down
                while (true)
                {
                    BinaryImage rightImage = downImage;
                    // moving right
                    result.Add(rightImage);
                    while (rightImage != rightImage.MoveRight())
                    {
                        rightImage = rightImage.MoveRight();
                        result.Add(rightImage);
                    }

                    if (downImage == downImage.MoveDown())
                    {
                        break;
                    }
                    downImage = downImage.MoveDown();
                }
            }

            _allConfigurationsCache[(int)shape] = result;
            return result;
        }
    }
}