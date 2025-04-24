using ProjectLCore.GamePieces;
using System;

/// <summary>
/// Represents a 5x5 image where each cells has a certain <see cref="Color"/>.
/// </summary>
public struct ColorImage
{
    #region Fields

    public Color[] _image;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorImage"/> struct based on a <see cref="BinaryImage"/>.
    /// Cells which are filled in the binary image are set to <see cref="Color.Fill"/>, while empty cells are set to <see cref="Color.Empty"/>.
    /// </summary>
    /// <param name="image">The binary image used to initialize the color image.</param>
    public ColorImage(BinaryImage image)
    {
        _image = new Color[25];
        for (int i = 0; i < 25; i++) {
            _image[i] = image[i] ? Color.Fill : Color.Empty;
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

        #region Properties

        /// <summary>
        /// Predefined color representing an empty state.
        /// </summary>
        public static Color Empty { get; } = new Color(int.MinValue);

        /// <summary>
        /// Predefined color representing a filled state.
        /// </summary>
        public static Color Fill { get; } = new Color(int.MaxValue);

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
    }
}
