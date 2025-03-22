using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kostra.GamePieces
{
    /// <summary>
    /// Represents a specific tetromino shape.
    /// </summary>
    public enum TetrominoShape
    {
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
}
