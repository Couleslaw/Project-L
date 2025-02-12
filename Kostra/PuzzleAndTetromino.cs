using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Kostra
{
    /// <summary>
    /// Represents a 5x5 binary image. The image is stored as an integer, where each bit represents a cell in the image.
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
        /// <summary> The internal representation of the image. </summary>
        private readonly int _image;

        /// <summary> The image which has all cells empty. </summary>
        public static BinaryImage EmptyImage => new(0);

        /// <summary> The image which has all cells filled in. </summary>
        public static BinaryImage FullImage => new((1 << 26) - 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryImage"/> struct.
        /// </summary>
        /// <param name="image">The encoding of the image.</param>
        /// <exception cref="System.ArgumentException">Binary image must be 5x5</exception>
        public BinaryImage(int image)
        {
            if (image < 0 || image >= 1 << 25)
            {
                throw new ArgumentException("Binary image must be 5x5");
            }
            _image = image;
        }

        /// <summary>
        /// Converts to string. '#' represents filled cell, '.' represents empty cell.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
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


        /*------------ Implement IEquatable ------------*/

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

        /*------------- Useful bitwise operators ------------*/

        /// <summary>
        /// Implements the operator &amp;.
        /// </summary>
        /// <returns>
        /// The intersection of the two images.
        /// </returns>
        public static BinaryImage operator &(BinaryImage left, BinaryImage right)
        {
            return new(left._image & right._image);
        }

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <returns>
        /// The union of the two images.
        /// </returns>
        public static BinaryImage operator |(BinaryImage left, BinaryImage right)
        {
            return new(left._image | right._image);
        }

        /// <summary>
        /// Implements the operator ~.
        /// </summary>
        /// <returns>
        /// The complement of the image.
        /// </returns>
        public static BinaryImage operator ~(BinaryImage image)
        {
            return new(~image._image);
        }

        /// <summary>
        /// Counts the filled cells.
        /// </summary>
        /// <returns>The number of filled in cells of the image.</returns>
        public int CountFilledCells()
        {
            int count = 0;
            for (int i = 0; i < 25; i++)
            {
                count += (_image >> i) & 1;
            }
            return count;
        }

        /// <summary>
        /// Counts the empty cells.
        /// </summary>
        /// <returns>The number of empty cells of the image.</returns>
        public int CountEmptyCells()
        {
            return 25 - CountFilledCells();
        }

        /*----------------- Image transformations -----------------*/

        /// <summary>
        /// Attempts to move the cells of the image up by one cell. If there is a filled cell in the top row, no transformation is done.
        /// </summary>
        /// <returns>The transformed image.</returns>
        public BinaryImage MoveUp()
        {
            int newImage = _image;
            if ((_image & 0b11111) == 0)
            {
                newImage >>= 5;
            }
            return new(newImage);
        }

        /// <summary>
        /// Attempts to move the cells of the image down by one cell. If there is a filled cell in the bottom row, no transformation is done.
        /// </summary>
        /// <returns>The transformed image.</returns>
        public BinaryImage MoveDown()
        {
            int newImage = _image;
            if ((_image & (0b11111 << 20)) == 0)
            {
                newImage <<= 5;
            }
            return new(newImage);
        }

        /// <summary>
        /// Attempts to move the cells of the image right by one cell. If there is a filled cell in the right column, no transformation is done.
        /// </summary>
        /// <returns>The transformed image.</returns>
        public BinaryImage MoveRight()
        {
            int newImage = _image;
            if ((_image & 0b1000010000100001000010000) == 0)
            {
                newImage <<= 1;
            }
            return new(newImage);
        }

        /// <summary>
        /// Attempts to move the cells of the image left by one cell. If there is a filled cell in the left column, no transformation is done.
        /// </summary>
        /// <returns>The transformed image.</returns>
        public BinaryImage MoveLeft()
        {
            int newImage = _image;
            if ((_image & 0b100001000010000100001) == 0)
            {
                newImage >>= 1;
            }
            return new(newImage);
        }

        /// <summary>
        /// Rotates the image 90 degrees to the right.
        /// </summary>
        /// <returns>The transformed image.</returns>
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

        /// <summary>
        /// Rotates the image 90 degrees to the left.
        /// </summary>
        /// <returns>The transformed image.</returns>
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

        /// <summary>
        /// Flips the image horizontally.
        /// </summary>
        /// <returns>The transformed image.</returns>
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

        /// <summary>
        /// Flips the image vertically.
        /// </summary>
        /// <returns>The transformed image.</returns>
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

        /// <summary>
        /// Moves the filled in cells to the top left corner of the image.
        /// </summary>
        /// <returns>The transformed image.</returns>
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

    /// <summary>
    /// Represents a specific tetromino shape.
    /// </summary>
    public enum TetrominoShape {
        /// <summary>
        /// The 1x1 square tetromino.
        /// </summary>
        O1,

        /// <summary>
        /// The 2x2 square tetromino.
        /// </summary>
        O2,

        /// <summary>
        /// The 1x2 line tetromino.
        /// </summary>
        I2,

        /// <summary>
        /// The 1x3 line tetromino.
        /// </summary>
        I3,

        /// <summary>
        /// The 1x4 line tetromino.
        /// </summary>
        I4,

        /// <summary>
        /// The L shaped tetromino of length 2. Looks like <see cref="I2"/> with <see cref="O1"/> attached to the right.
        /// </summary>
        L2,

        /// <summary>
        /// The L shaped tetromino of length 3. Looks like <see cref="I3"/> with <see cref="O1"/> attached to the right.
        /// </summary>
        L3,

        /// <summary>
        /// The Z shaped tetromino. Looks like two <see cref="I2"/> tetrominos attached to each other, the bottom one shifted by one cell to the right.
        /// </summary>
        Z,

        /// <summary>
        /// The T shaped tetromino. Likes like <see cref="I3"/> with <see cref="O1"/> attacked to the middle cell.
        /// </summary>
        T
    }

    /// <summary>
    /// Represents a puzzle in the game.
    /// </summary>
    public class Puzzle
    {
        // id

        private static uint _idCounter = 0;

        /// <summary>
        /// The unique identifier of the puzzle.
        /// </summary>
        public uint Id { get; } = _idCounter++;

        // puzzle parameters

        /// <summary>
        /// Specifies whether the puzzle is black or white.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is black; <c>false</c> if it is white.
        /// </value>
        public bool IsBlack { get; }

        /// <summary>
        /// Specifies the score the player gets for completing the puzzle.
        /// </summary>
        public int RewardScore { get; }

        /// <summary>
        /// Specifies the tetromino the player gets for completing the puzzle.
        /// </summary>
        public TetrominoShape RewardTetromino { get; }

        // binary representation of the puzzle image
        // 
        /// <summary>
        /// Specifies which cells of the puzzle need to be filled in.
        /// </summary>
        public BinaryImage Image { get; private set; }

        /// <summary>
        /// The number of cells which need to be filled in.
        /// </summary>
        public int NumEmptyCells { get; private set; }

        /// <summary>
        /// Indicates whether this puzzle has been completed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this puzzle has been completed; otherwise, <c>false</c>.
        /// </value>
        public bool IsFinished => NumEmptyCells == 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Puzzle"/> class.
        /// </summary>
        /// <param name="binaryImage">The binary image representing the puzzle.</param>
        /// <param name="score">The score the player will receive for completing the puzzle.</param>
        /// <param name="reward">The tetromino the player will receive for completing the puzzle.</param>
        /// <param name="isBlack">Indicates whether the puzzle is black or white</param>
        public Puzzle(BinaryImage binaryImage, int score, TetrominoShape reward, bool isBlack)
        {
            Image = binaryImage;
            RewardScore = score;
            RewardTetromino = reward;
            IsBlack = isBlack;

            NumEmptyCells = Image.CountEmptyCells();
        }


        /// <summary>
        /// Contains information about the number of tetrominos of each shape used on the puzzle.
        /// </summary>
        private int[] _usedTetrominos = new int[TetrominoManager.NumShapes];

        /// <summary>
        /// Determines whether the given tetromino can be placed on the puzzle.
        /// </summary>
        /// <param name="tetromino">The position of the tetromino.</param>
        /// <returns>
        ///   <c>true</c> if the tetromino can be placed; <c>false</c> otherwise.
        /// </returns>
        public bool CanPlaceTetromino(BinaryImage tetromino) => (Image & tetromino) == BinaryImage.EmptyImage;

        /// <summary>
        /// Places the given tetromino on the puzzle.
        /// </summary>
        /// <param name="tetromino">The shape of the tetromino.</param>
        /// <param name="position">The position of the tetromino.</param>
        public void AddTetromino(TetrominoShape tetromino, BinaryImage position)
        {
            _usedTetrominos[(int)tetromino]++;
            NumEmptyCells -= TetrominoManager.GetLevelOf(tetromino);
            Image |= position;
        }

        /// <summary>
        /// Enumerates all tetrominos placed on the puzzle.
        /// </summary>
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

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A deep copy of this instance.</returns>
        public Puzzle Clone()
        {
            Puzzle clone = new(Image, RewardScore, RewardTetromino, IsBlack);
            clone.NumEmptyCells = NumEmptyCells;
            clone._usedTetrominos = _usedTetrominos.ToArray(); // copy array
            return clone;
        }
    }

    /// <summary>
    /// Provides information about the different tetromino shapes and their configurations.
    /// </summary>
    public static class TetrominoManager
    {
        /// <summary> The number tetromino shape in the game. </summary>
        public static int NumShapes => Enum.GetValues(typeof(TetrominoShape)).Length;

        /// <summary> The minimum level a tetromino can have.  </summary>
        public const int MinLevel = 1;

        /// <summary>  The maximum level a tetromino can have. </summary>
        public const int MaxLevel = 4;

        /// <summary> Contains the level of each tetromino shape.  </summary>
        private static readonly int[] _levels = new int[NumShapes];

        /// <summary>  For each possible level, contains a list of tetromino shapes with that level.  </summary>
        private static readonly List<TetrominoShape>[] _shapesByLevel = new List<TetrominoShape>[MaxLevel - MinLevel + 1];

        /// <summary> Contains the <see cref="BinaryImage"/> representation for each tetromino shape. </summary>
        private static readonly BinaryImage[] _binaryImages = new BinaryImage[NumShapes];

        /// <summary> 
        /// Contains a list of all base configurations for each tetromino shape.
        /// A base configuration is a position in the top left corner of the image, which can be achieved by transforming the image found in <see cref="_binaryImages"/>.
        /// </summary>
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

        /// <summary>
        /// Returns the <see cref="BinaryImage"/> representation of the given tetromino shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public static BinaryImage GetImageOf(TetrominoShape shape) => _binaryImages[(int)shape];

        /// <summary>
        /// Returns the level of the given tetromino shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public static int GetLevelOf(TetrominoShape shape) => _levels[(int)shape];

        /// <summary>
        /// Finds all tetromino shapes with the given level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>A list containing the shapes.</returns>
        public static List<TetrominoShape> GetShapesWithLevel(int level) => _shapesByLevel[level - MinLevel];

        /// <summary>
        /// Checks is the given image is a valid configuration of the given shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="image">The image.</param>
        /// <returns><c>true</c> if the given configuration is valid; otherwise <c>false</c>.</returns>
        public static bool CompareShapeToImage(TetrominoShape shape, BinaryImage image)
        {
            // checks if the images is a valid configuration of the shape
            BinaryImage baseConf = image.MoveImageToTopLeftCorner();
            return _baseConfigurations[(int)shape].Contains(baseConf);
        }

        /// <summary>
        /// Generates all base configurations of the given shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns>A list containing the configurations.</returns>
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

        /// <summary>
        /// Generates all the possible unique configurations of the given shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns>A list containing the configurations.</returns>
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