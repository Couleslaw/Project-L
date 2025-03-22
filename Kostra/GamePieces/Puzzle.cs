using Kostra.GameManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kostra.GamePieces
{
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
}
