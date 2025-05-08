using ProjectL.Data;
using ProjectLCore.GamePieces;
using System.Linq;
using UnityEngine;

#nullable enable

/// <summary>
/// Represents a puzzle in the game.
/// </summary>
public class PuzzleWithGraphics : Puzzle
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PuzzleWithGraphics"/> class.
    /// </summary>
    /// <param name="image">The binary image representing the puzzle.</param>
    /// <param name="rewardScore">The score the player will receive for completing the puzzle.</param>
    /// <param name="rewardTetromino">The tetromino the player will receive for completing the puzzle.</param>
    /// <param name="isBlack">Indicates whether the puzzle is black or white</param>
    /// <param name="puzzleNumber">The order number of this puzzle. The file containing the graphics for this puzzle should have the name <c>color-number.png</c> where color is <c>black</c> or <c>white</c> and number is <paramref name="puzzleNumber"/>.</param>
    public PuzzleWithGraphics(BinaryImage image, int rewardScore, TetrominoShape rewardTetromino, bool isBlack, uint puzzleNumber)
        : base(image, rewardScore, rewardTetromino, isBlack, puzzleNumber)
    {
        ColorImage = new ColorImage(image);
    }

    #endregion

    #region Properties

    /// <summary>
    /// A <see cref="ColorImage"/> representing the puzzle.
    /// Specifies the color of each cell in the puzzle.
    /// </summary>
    public ColorImage ColorImage { get; private set; }

    #endregion

    #region Methods

    public bool TryGetSprite(out Sprite? sprite)
    {
        return ResourcesLoader.TryGetPuzzleSprite(this, PuzzleSpriteType.Borderless, out sprite);
    }

    /// <summary>
    /// Places the given tetromino into the puzzle.
    /// </summary>
    /// <param name="tetromino">The shape of the tetromino.</param>
    /// <param name="position">The position of the tetromino.</param>
    public override void AddTetromino(TetrominoShape tetromino, BinaryImage position)
    {
        base.AddTetromino(tetromino, position);
        ColorImage = ColorImage.AddImage((ColorImage.Color)tetromino, position);
    }

    public override void RemoveTetromino(TetrominoShape tetromino, BinaryImage position)
    {
        base.RemoveTetromino(tetromino, position);
        ColorImage = ColorImage.AddImage(ColorImage.Color.Empty, position);
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>A deep copy of this instance.</returns>
    public override Puzzle Clone()
    {
        PuzzleWithGraphics clone = (PuzzleWithGraphics)MemberwiseClone();
        clone._usedTetrominos = _usedTetrominos.ToArray(); // copy array
        return clone;
    }

    #endregion
}
