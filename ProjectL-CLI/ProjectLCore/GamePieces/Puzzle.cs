namespace ProjectLCore.GamePieces
{
    using ProjectLCore.GameManagers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a puzzle in the game.
    /// </summary>
    public class Puzzle
    {
        #region Fields

        /// <summary>
        /// The order number of this puzzle. The file containing the graphics for this puzzle should have the name <c>color-number.png</c> where color is <c>black</c> or <c>white</c> and number is <see cref="_puzzleNumber"/>.
        /// </summary>
        private readonly uint _puzzleNumber;

        /// <summary>
        /// Contains information about the number of tetrominos of each shape used on the puzzle.
        /// </summary>
        protected int[] _usedTetrominos = new int[TetrominoManager.NumShapes];

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Puzzle"/> class.
        /// </summary>
        /// <param name="image">The binary image representing the puzzle.</param>
        /// <param name="rewardScore">The score the player will receive for completing the puzzle.</param>
        /// <param name="rewardTetromino">The tetromino the player will receive for completing the puzzle.</param>
        /// <param name="isBlack">Indicates whether the puzzle is black or white</param>
        /// <param name="puzzleNumber">The order number of this puzzle. The file containing the graphics for this puzzle should have the name <c>color-number.png</c> where color is <c>black</c> or <c>white</c> and number is <paramref name="puzzleNumber"/>.</param>
        public Puzzle(BinaryImage image, int rewardScore, TetrominoShape rewardTetromino, bool isBlack, uint puzzleNumber)
        {
            _puzzleNumber = puzzleNumber;
            IsBlack = isBlack;
            RewardScore = rewardScore;
            RewardTetromino = rewardTetromino;
            Image = image;
            Id = PuzzleIDProvider.GetId(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The unique identifier of the puzzle. Two puzzles have the same ID if they have the same color and puzzle number.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Specifies whether the puzzle is black or white.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if this instance is black; <see langword="false"/> if it is white.
        /// </value>
        public bool IsBlack { get; }

        /// <summary>
        /// The score the player gets for completing the puzzle.
        /// </summary>
        public int RewardScore { get; }

        /// <summary>
        /// The tetromino the player gets for completing the puzzle.
        /// </summary>
        public TetrominoShape RewardTetromino { get; }

        /// <summary>
        /// A binary image representing the puzzle.
        /// Specifies which cells of the puzzle need to be filled in.
        /// </summary>
        public BinaryImage Image { get; private set; }

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
        /// Determines whether the given tetromino can be placed into the puzzle.
        /// </summary>
        /// <param name="position">The position of the tetromino.</param>
        /// <returns>
        ///   <see langword="true"/> if the tetromino can be placed; <see langword="false"/> otherwise.
        /// </returns>
        public bool CanPlaceTetromino(BinaryImage position) => (Image & position) == BinaryImage.EmptyImage;

        /// <summary>
        /// Places the given tetromino into the puzzle.
        /// </summary>
        /// <param name="tetromino">The shape of the tetromino.</param>
        /// <param name="position">The position of the tetromino.</param>
        public virtual void AddTetromino(TetrominoShape tetromino, BinaryImage position)
        {
            _usedTetrominos[(int)tetromino]++;
            Image |= position;
        }

        /// <summary>
        /// Enumerates all tetrominos placed into the puzzle.
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
        public virtual Puzzle Clone()
        {
            Puzzle clone = (Puzzle)MemberwiseClone();
            clone._usedTetrominos = _usedTetrominos.ToArray(); // copy array
            return clone;
        }

        #endregion

        private static class PuzzleIDProvider
        {
            #region Fields

            private static uint _idCounter = 0;

            private static Dictionary<Tuple<bool, uint>, uint> _puzzleToId = new();

            #endregion

            #region Methods

            public static uint GetId(Puzzle puzzle)
            {
                Tuple<bool, uint> key = new(puzzle.IsBlack, puzzle._puzzleNumber);
                if (!_puzzleToId.ContainsKey(key)) {
                    _puzzleToId[key] = _idCounter++;
                }
                return _puzzleToId[key];
            }

            #endregion
        }
    }
}
