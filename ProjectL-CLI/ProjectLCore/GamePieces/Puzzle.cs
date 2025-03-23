namespace ProjectLCore.GamePieces
{
    using ProjectLCore.GameManagers;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a puzzle in the game.
    /// </summary>
    /// <param name="image">The binary image representing the puzzle.</param>
    /// <param name="rewardScore">The score the player will receive for completing the puzzle.</param>
    /// <param name="rewardTetromino">The tetromino the player will receive for completing the puzzle.</param>
    /// <param name="isBlack">Indicates whether the puzzle is black or white</param>
    /// <param name="puzzleNumber">The order number of this puzzle. The file containing the graphics for this puzzle should have the name <c>color-number.png</c> where color is <c>black</c> or <c>white</c> and number is <paramref name="puzzleNumber"/>.</param>
    public class Puzzle(BinaryImage image, int rewardScore, TetrominoShape rewardTetromino, bool isBlack, int puzzleNumber)
    {
        #region Fields

        /// <summary>
        /// Contains information about the number of tetrominos of each shape used on the puzzle.
        /// </summary>
        private int[] _usedTetrominos = new int[TetrominoManager.NumShapes];

        #endregion

        #region Properties

        /// <summary>
        /// The unique identifier of the puzzle.
        /// </summary>
        public uint Id { get; } = (uint)(isBlack, puzzleNumber).GetHashCode();

        /// <summary>
        /// Specifies whether the puzzle is black or white.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if this instance is black; <see langword="false"/> if it is white.
        /// </value>
        public bool IsBlack { get; } = isBlack;

        /// <summary>
        /// The score the player gets for completing the puzzle.
        /// </summary>
        public int RewardScore { get; } = rewardScore;

        /// <summary>
        /// The tetromino the player gets for completing the puzzle.
        /// </summary>
        public TetrominoShape RewardTetromino { get; } = rewardTetromino;

        /// <summary>
        /// A binary image representing the puzzle.
        /// Specifies which cells of the puzzle need to be filled in.
        /// </summary>
        public BinaryImage Image { get; private set; } = image;

        /// <summary>
        /// The number of cells which need to be filled in.
        /// </summary>
        public int NumEmptyCells => Image.CountEmptyCells();

        /// <summary>
        /// Indicates whether this puzzle has been completed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if this puzzle has been completed; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsFinished => NumEmptyCells == 0;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the given tetromino can be placed on the puzzle.
        /// </summary>
        /// <param name="tetromino">The position of the tetromino.</param>
        /// <returns>
        ///   <see langword="true"/> if the tetromino can be placed; <see langword="false"/> otherwise.
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
            Image |= position;
        }

        /// <summary>
        /// Enumerates all tetrominos placed on the puzzle.
        /// </summary>
        /// <returns>An enumeration of the tetrominos placed on the puzzle.</returns>
        public IEnumerable<TetrominoShape> GetUsedTetrominos()
        {
            for (int shape = 0; shape < TetrominoManager.NumShapes; shape++) {
                for (int j = 0; j < _usedTetrominos[shape]; j++) {
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
            Puzzle clone = new(Image, RewardScore, RewardTetromino, IsBlack, puzzleNumber);
            clone._usedTetrominos = _usedTetrominos.ToArray(); // copy array
            return clone;
        }

        #endregion
    }
}
