using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kostra
{
    static class RewardManager
    {
        // returns the possible shapes the player can get as a reward for finishing a puzzle
        // usually he can get only the shape on the card
        // but if there are no left he can chose a shape of the next available level
        public static List<TetrominoShape> GetRewardOptions(IReadOnlyList<int> numTetrominosLeft, TetrominoShape shape)
        {
            if (numTetrominosLeft.Count != TetrominoManager.NumShapes)
            {
                throw new ArgumentException("Invalid numTetrominosLeft length");
            }

            if (numTetrominosLeft[(int)shape] > 0)
            {
                return new List<TetrominoShape> { shape };
            }

            var result = new List<TetrominoShape>();
            for (int level = TetrominoManager.GetLevelOf(shape); level <= TetrominoManager.MaxLevel; level++)
            {
                foreach (var s in TetrominoManager.GetShapesWithLevel(level))
                {
                    if (numTetrominosLeft[(int)s] > 0) result.Add(s);
                }
                if (result.Count > 0) return result;
            }
            // should never happen
            throw new InvalidOperationException("No tetrominos left");
        }

        // returns the possible shapes the player can get through the ChangeTetromino action
        // level(newshape) <= level(oldshape) + 1
        // if there are no shapes with level(oldshape)+1 available, he can choose from the next available level
        public static List<TetrominoShape> GetUpgradeOptions(IReadOnlyList<int> numTetrominosLeft, TetrominoShape shape)
        {
            if (numTetrominosLeft.Count != TetrominoManager.NumShapes)
            {
                throw new ArgumentException("Invalid numTetrominosLeft length");
            }

            int oldLevel = TetrominoManager.GetLevelOf(shape);
            var result = new List<TetrominoShape>();
            // first try to find shapes with level(oldshape)+1
            for (int level = oldLevel+1; level <= TetrominoManager.MaxLevel; level++)
            {
                foreach (var s in TetrominoManager.GetShapesWithLevel(level))
                {
                    if (numTetrominosLeft[(int)s] > 0) result.Add(s);
                }
                if (result.Count > 0) return result;
            }
            // now add all shapes of level <= oldLevel
            for (int level = TetrominoManager.MinLevel; level <= oldLevel; level++)
            {
                foreach (var s in TetrominoManager.GetShapesWithLevel(level))
                {
                    if (numTetrominosLeft[(int)s] > 0) result.Add(s);
                }
            }

            if (result.Count > 0) return result;
            
            // should never happen
            throw new InvalidOperationException("No tetrominos left");
        }
    }
}