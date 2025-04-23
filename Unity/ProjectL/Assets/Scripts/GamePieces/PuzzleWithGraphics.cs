using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using ProjectLCore.GamePieces;
using TMPro;

public class PuzzleWithGraphics : Puzzle
{
    public ColorImage ColorImage { get; private set; }

    public PuzzleWithGraphics(BinaryImage image, int rewardScore, TetrominoShape rewardTetromino, bool isBlack, uint puzzleNumber) 
        : base(image, rewardScore, rewardTetromino, isBlack, puzzleNumber)
    {
        ColorImage = new ColorImage(image);
    }

    public override void AddTetromino(TetrominoShape tetromino, BinaryImage position)
    {
        base.AddTetromino(tetromino, position);
        ColorImage = ColorImage.AddImage((ColorImage.Color)tetromino, position);
    }

    public override Puzzle Clone()
    {
        PuzzleWithGraphics clone = (PuzzleWithGraphics)MemberwiseClone();
        clone._usedTetrominos = _usedTetrominos.ToArray(); // copy array
        return clone;
    }
}
