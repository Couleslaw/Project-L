namespace ProjectLCore.GameManagers
{
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides information about the different tetromino shapes and their configurations.
    /// </summary>
    public static class TetrominoManager
    {
        #region Constants

        /// <summary> The minimum level a tetromino can have.  </summary>
        public const int MinLevel = 1;

        /// <summary>  The maximum level a tetromino can have. </summary>
        public const int MaxLevel = 4;

        #endregion

        #region Fields

        /// <summary> Contains the level of each tetromino shape.  </summary>
        private static readonly int[] _levels;

        /// <summary>  For each possible level, contains a list of tetromino shapes with that level.  </summary>
        private static readonly List<TetrominoShape>[] _shapesByLevel = new List<TetrominoShape>[MaxLevel - MinLevel + 1];

        /// <summary> Contains the <see cref="BinaryImage"/> representation for each tetromino shape. </summary>
        private static readonly BinaryImage[] _binaryImages;

        /// <summary> 
        /// Contains a list of all base configurations for each tetromino shape.
        /// A base configuration is a position in the top left corner of the image, which can be achieved by transforming the image found in <see cref="_binaryImages"/>.
        /// </summary>
        private static readonly List<BinaryImage>[] _baseConfigurations;

        private static readonly List<BinaryImage>[] _allConfigurationsCache;

        #endregion

        #region Constructors

        static TetrominoManager()
        {
            // initialize tetromino images
            _binaryImages = new BinaryImage[NumShapes];
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
            _levels = new int[NumShapes];
            for (int i = 0; i < NumShapes; i++) {
                _levels[i] = _binaryImages[i].CountFilledCells();
            }

            // create list of shapes for each level
            for (int i = 0; i < NumShapes; i++) {
                int index = _levels[i] - MinLevel;
                if (_shapesByLevel[index] is null) {
                    _shapesByLevel[index] = new();
                }
                _shapesByLevel[index].Add((TetrominoShape)i);
            }

            // generate all base configurations for each shape
            _baseConfigurations = new List<BinaryImage>[NumShapes];
            _allConfigurationsCache = new List<BinaryImage>[NumShapes];
            for (int i = 0; i < NumShapes; i++) {
                _baseConfigurations[i] = GetBaseConfigurationsOf((TetrominoShape)i);
            }
        }

        #endregion

        #region Properties

        /// <summary> The number of different tetromino shapes in the game. </summary>
        public static int NumShapes { get; } = Enum.GetValues(typeof(TetrominoShape)).Length;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the standard <see cref="BinaryImage"/> representation of the given tetromino shape.
        /// </summary>
        /// <param name="shape">Specifies the tetromino type.</param>
        /// <returns>The <see cref="BinaryImage"/> representation of the shape.</returns>
        public static BinaryImage GetImageOf(TetrominoShape shape) => _binaryImages[(int)shape];

        /// <summary>
        /// Gets the level of the given tetromino <paramref name="shape"/>.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns>The level of the shape.</returns>
        public static int GetLevelOf(TetrominoShape shape) => _levels[(int)shape];

        /// <summary>
        /// Finds all tetromino shapes with the given <paramref name="level"/>.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>A list containing the shapes.</returns>
        public static List<TetrominoShape> GetShapesWithLevel(int level) => _shapesByLevel[level - MinLevel];

        /// <summary>
        /// Checks is the given <paramref name="image"/> is a valid configuration of the given <paramref name="shape"/>.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="image">The image.</param>
        /// <returns><see langword="true"/> if the given configuration is valid; otherwise <see langword="false"/>.</returns>
        public static bool CompareShapeToImage(TetrominoShape shape, BinaryImage image)
        {
            // checks if the images is a valid configuration of the shape
            BinaryImage baseConf = image.MoveImageToTopLeftCorner();
            return _baseConfigurations[(int)shape].Contains(baseConf);
        }

        /// <summary>
        /// Generates all the possible unique configurations of the given <paramref name="shape"/>.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns>A list containing the configurations.</returns>
        public static List<BinaryImage> GetAllConfigurationsOf(TetrominoShape shape)
        {
            // check cache first
            if (_allConfigurationsCache[(int)shape] is not null) {
                return _allConfigurationsCache[(int)shape];
            }

            // generate all unique configurations of the shape

            List<BinaryImage> result = new();

            foreach (BinaryImage image in _baseConfigurations[(int)shape]) {
                BinaryImage downImage = image;
                // moving down
                while (true) {
                    BinaryImage rightImage = downImage;
                    // moving right
                    result.Add(rightImage);
                    while (rightImage != rightImage.MoveRight()) {
                        rightImage = rightImage.MoveRight();
                        result.Add(rightImage);
                    }

                    if (downImage == downImage.MoveDown()) {
                        break;
                    }
                    downImage = downImage.MoveDown();
                }
            }

            _allConfigurationsCache[(int)shape] = result;
            return result;
        }

        /// <summary>
        /// Generates all base configurations of the given <paramref name="shape"/>.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns>A list containing the configurations.</returns>
        private static List<BinaryImage> GetBaseConfigurationsOf(TetrominoShape shape)
        {
            List<BinaryImage> conf = new();
            BinaryImage image = _binaryImages[(int)shape];

            for (int i = 0; i < 4; i++) {
                conf.Add(image.MoveImageToTopLeftCorner());
                image = image.RotateRight();
            }
            image = image.FlipHorizontally();
            for (int i = 0; i < 4; i++) {
                conf.Add(image.MoveImageToTopLeftCorner());
                image = image.RotateRight();
            }

            return conf.Distinct().ToList();
        }

        #endregion
    }
}
