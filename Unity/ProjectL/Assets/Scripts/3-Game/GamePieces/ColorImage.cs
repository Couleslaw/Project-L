namespace ProjectLCore.GamePieces
{
    using System;

    /// <summary>
    /// Represents a 5x5 image where each cells has a certain <see cref="Color"/>.
    /// </summary>
    public struct ColorImage
    {
        #region Fields

        private readonly Color[] _image;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorImage"/> struct based on a <see cref="BinaryImage"/>.
        /// Cells which are filled in the binary image are set to <see cref="Color.fill"/>, while empty cells are set to <see cref="Color.empty"/>.
        /// </summary>
        /// <param name="image">The binary image used to initialize the color image.</param>
        public ColorImage(BinaryImage image)
        {
            _image = new Color[25];
            for (int i = 0; i < 25; i++) {
                _image[i] = image[i] ? Color.fill : Color.empty;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a binary image to the current color image, overriding the color of cells specified in the binary image with the given color.
        /// </summary>
        /// <param name="color">The color to apply.</param>
        /// <param name="image">The binary image to add to the current color image.</param>
        /// <returns>A new <see cref="ColorImage"/> with the binary image applied.</returns>
        public ColorImage AddImage(Color color, BinaryImage image)
        {
            ColorImage newImage = this;
            for (int i = 0; i < 25; i++) {
                if (image[i])
                    newImage._image[i] = color;
            }
            return newImage;
        }

        #endregion

        /// <summary>
        /// Represents a color.
        /// </summary>
        public struct Color : IEquatable<Color>
        {
            #region Fields

            /// <summary>
            /// Predefined color representing an empty state.
            /// </summary>
            public static readonly Color empty = new Color(int.MinValue);

            /// <summary>
            /// Predefined color representing a filled state.
            /// </summary>
            public static readonly Color fill = new Color(int.MaxValue);

            private int _color;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Color"/> struct with a specified integer value.
            /// </summary>
            /// <param name="color">The integer value representing the color.</param>
            public Color(int color) => _color = color;

            /// <summary>
            /// Initializes a new instance of the <see cref="Color"/> struct based on a <see cref="TetrominoShape"/>.
            /// </summary>
            /// <param name="shape">The shape to derive the color from.</param>
            public Color(TetrominoShape shape) => _color = (int)shape;

            #endregion

            #region Methods

            /// <summary>
            /// Determines whether the current color is equal to another color.
            /// </summary>
            /// <param name="other">The other color to compare with.</param>
            /// <returns><see langword="true"/> if the colors are equal; otherwise <see langword="false"/>.</returns>
            public bool Equals(Color other) => this._color == other._color;

            #endregion

            /// <summary>
            /// Explicitly converts a <see cref="TetrominoShape"/> to a <see cref="Color"/>.
            /// </summary>
            /// <param name="shape">The shape to convert.</param>
            public static explicit operator Color(TetrominoShape shape) => new Color((int)shape);

            public static implicit operator UnityEngine.Color(Color color)
            {
                return color._color switch {
                    (int)TetrominoShape.O1 => new(251f / 255f, 243f / 255f, 52f / 255f),
                    (int)TetrominoShape.O2 => new(237f / 255f, 20f / 255f, 91f / 255f),
                    (int)TetrominoShape.I2 => new(0f / 255f, 174f / 255f, 77f / 255f),
                    (int)TetrominoShape.I3 => new(0f / 255f, 124f / 255f, 197f / 255f),
                    (int)TetrominoShape.I4 => new(143f / 255f, 71f / 255f, 155f / 255f),
                    (int)TetrominoShape.L2 => new(254f / 255f, 183f / 255f, 14f / 255f),
                    (int)TetrominoShape.L3 => new(0f / 255f, 171f / 255f, 203f / 255f),
                    (int)TetrominoShape.Z => new(243f / 255f, 111f / 255f, 34f / 255f),
                    (int)TetrominoShape.T => new(236f / 255f, 0f / 255f, 141f / 255f),
                    _ => UnityEngine.Color.clear
                };
            }
        }
    }
}
