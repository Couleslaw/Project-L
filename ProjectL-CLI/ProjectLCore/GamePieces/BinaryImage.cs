namespace ProjectLCore.GamePieces
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System;

    /// <summary>
    /// Represents a 5x5 binary image. The image is stored as an integer, where each bit represents a cell in the image.
    /// The top left corner is viewed the least significant bit. We go down row by row from left to right.
    /// 
    /// <example><code language="none">
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
        #region Fields

        /// <summary> The internal representation of the image. </summary>
        private readonly int _image;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryImage"/> struct using an encoding of the image into an integer. The encoding should be as specified in the <see cref="BinaryImage"/> class documentation.
        /// </summary>
        /// <param name="image">The encoding of the image.</param>
        /// <exception cref="ArgumentException">Binary image must be 5x5</exception>
        public BinaryImage(int image)
        {
            if (image < 0 || image >= 1 << 25) {
                throw new ArgumentException("Binary image must be 5x5");
            }
            _image = image;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryImage"/> struct using a <c>bool[25]</c>. The first 5 elements represent the first row (left to right), the next 5 elements represent the second row, and so on. Filled in cells are represented by <see langword="true"/> and empty cells by <see langword="false"/>.
        /// </summary>
        /// <param name="array">The encoding of the image.</param>
        /// <exception cref="ArgumentException">Binary image must be 5x5</exception>
        public BinaryImage(bool[] array)
        {
            if (array.Length != 25) {
                throw new ArgumentException("Binary image must be 5x5");
            }
            _image = 0;
            for (int i = 0; i < 25; i++) {
                if (array[i]) {
                    _image |= 1 << i;
                }
            }
        }

        #endregion

        #region Properties

        /// <summary> The image which has all cells empty. </summary>
        public static BinaryImage EmptyImage => new(0);

        /// <summary> The image which has all cells filled in. </summary>
        public static BinaryImage FullImage => new((1 << 25) - 1);

        #endregion

        #region Methods

        /// <summary>
        /// Converts to string. '#' represents filled cell, '.' represents empty cell.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new();
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    sb.Append((_image >> 5 * i + j & 1) == 1 ? '#' : '.');
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the cell at the specified index: image[i,j] is equivalent to image[i * 5 + j].
        /// </summary>
        /// <param name="index">Linear index.</param>
        /// <returns><see langword="true"/> if the cell is filled, else <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool this[int index] {
            get {
                if (index < 0 || index >= 25) {
                    throw new ArgumentOutOfRangeException();
                }
                return (_image >> index & 1) == 1;
            }
        }

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        /// <param name="i">Row index.</param>
        /// <param name="j">Column index.</param>
        /// <returns><see langword="true"/> if the cell is filled, else <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool this[int i, int j] {
            get {
                if (i < 0 || i >= 5 || j < 0 || j >= 5) {
                    throw new ArgumentOutOfRangeException();
                }
                return this[i * 5 + j];
            }
        }


        /// <summary>
        /// Indicates whether the current <see cref="BinaryImage"/> is equal to another <see cref="BinaryImage"/>.
        /// Two images are equal if all of their cells are the same.
        /// </summary>
        /// <param name="other">A <see cref="BinaryImage"/> to compare with this <see cref="BinaryImage"/>.</param>
        /// <returns>
        ///   <see langword="true" /> if the current <see cref="BinaryImage"/> is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        /// <seealso cref="IEquatable{T}"/>
        public bool Equals(BinaryImage other)
        {
            return _image == other._image;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="BinaryImage"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="BinaryImage"/>.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object is equal to the current <see cref="BinaryImage"/>; otherwise, <see langword="false" />.
        /// </returns>
        /// <seealso cref="Equals(BinaryImage)"/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is BinaryImage bi) {
                return Equals(bi);
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => _image.GetHashCode();

        /// <summary>
        /// Counts the filled cells.
        /// </summary>
        /// <returns>The number of filled in cells of the image.</returns>
        public int CountFilledCells()
        {
            int count = 0;
            for (int i = 0; i < 25; i++) {
                count += _image >> i & 1;
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

        /// <summary>
        /// Attempts to move the cells of the image up by one cell. If there is a filled cell in the top row, no transformation is done.
        /// </summary>
        /// <returns>The transformed image.</returns>
        public BinaryImage MoveUp()
        {
            int newImage = _image;
            if ((_image & 0b11111) == 0) {
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
            if ((_image & 0b11111 << 20) == 0) {
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
            if ((_image & 0b1000010000100001000010000) == 0) {
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
            if ((_image & 0b100001000010000100001) == 0) {
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
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    // (i,j) -> (j, 4-i)
                    newImage |= (_image >> 5 * i + j & 1) << 5 * j + 4 - i;
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
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    // (i,j) -> (4-j, i)
                    newImage |= (_image >> 5 * i + j & 1) << 5 * (4 - j) + i;
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
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    // (i,j) -> (i, 4-j)
                    newImage |= (_image >> 5 * i + j & 1) << 5 * i + 4 - j;
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
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    // (i,j) -> (4-i, j)
                    newImage |= (_image >> 5 * i + j & 1) << 5 * (4 - i) + j;
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
            // if the image is empty, return the empty image
            if (_image == 0) {
                return this;
            }

            int newImage = _image;
            // move image up
            while ((newImage & 0b11111) == 0) {
                newImage >>= 5;
            }
            // move image left
            while ((newImage & 0b100001000010000100001) == 0) {
                newImage >>= 1;
            }
            return new(newImage);
        }

        #endregion

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object is equal to the current <see cref="BinaryImage"/>; otherwise, <see langword="false" />.
        /// </returns>
        /// <seealso cref="Equals(BinaryImage)"/>
        public static bool operator ==(BinaryImage left, BinaryImage right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The operand on the left.</param>
        /// <param name="right">The operand on the right.</param>
        /// <returns>
        /// <see langword="true" /> if the specified object is not equal to the current <see cref="BinaryImage"/>; otherwise, <see langword="false" />.
        /// </returns>
        /// <seealso cref="Equals(BinaryImage)"/>
        public static bool operator !=(BinaryImage left, BinaryImage right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Implements the operator &amp;. The intersection of two images is the image where a cell is filled in if and only if both images have the cell filled in.
        /// </summary>
        /// <param name="left">The operator on the left.</param>
        /// <param name="right">The operator on the right.</param>
        /// <returns>
        /// The intersection of the two images.
        /// </returns>
        /// <example><code language="none">
        /// #####       ###..        ###..
        /// ##.##       ..#..        .....
        /// ##..#   &amp;   .###.   ==   .#...
        /// #...#       #####        #...#
        /// ##..#       .....        .....
        /// </code></example>
        public static BinaryImage operator &(BinaryImage left, BinaryImage right)
        {
            return new(left._image & right._image);
        }

        /// <summary>
        /// Implements the operator |. The union of two images is the image where a cell is filled in if and only if at least one of the images have the cell filled in.
        /// </summary>
        /// <param name="left">The operator on the left.</param>
        /// <param name="right">The operator on the right.</param>
        /// <returns>
        /// The union of the two images.
        /// </returns>
        /// <example><code language="none">
        /// #####       ###..        #####
        /// ##.##       ..#..        #####
        /// ##..#   |   .....   ==   ##..#
        /// #...#       ###..        ###.#
        /// ##..#       .....        ##..#
        /// </code></example>
        public static BinaryImage operator |(BinaryImage left, BinaryImage right)
        {
            return new(left._image | right._image);
        }

        /// <summary>
        /// Implements the operator ~. The complement of an image is the image where a cell is filled in if and only if the original image has the cell empty.
        /// </summary>
        /// <param name="image">The operator on the image.</param>
        /// <returns>
        /// The complement of the image.
        /// </returns>
        /// <example><code language="none">
        ///   #####        .....
        ///   ##.##        ..#..
        /// ~ ##..#   ==   ..##.   
        ///   #...#        .###.
        ///   ##..#        ..##.
        /// </code></example>
        public static BinaryImage operator ~(BinaryImage image)
        {
            int newImage = ~image._image;
            // remove the bits that are not part of the image
            newImage &= (1 << 25) - 1;
            return new(newImage);
        }
    }
}
