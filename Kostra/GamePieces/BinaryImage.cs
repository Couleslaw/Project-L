using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Kostra.GamePieces
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
        /// <exception cref="ArgumentException">Binary image must be 5x5</exception>
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
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    sb.Append((_image >> 5 * i + j & 1) == 1 ? '#' : '.');
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
            if ((_image & 0b11111 << 20) == 0)
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
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
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
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
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
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
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
}