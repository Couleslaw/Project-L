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
        public static List<TetrominoShape> GetRewardOptions(int[] numTetrominosLeft, TetrominoShape shape)
        {
            if (numTetrominosLeft[(int)shape] > 0)
            {
                return new List<TetrominoShape> { shape };
            }

            var result = new List<TetrominoShape>();
            for (int level = TetrominoManager.GetLevelOf(shape); level <= TetrominoManager.MaxLevel; level++)
            {
                foreach (var s in TetrominoManager.GetShapesOfLevel(level))
                {
                    if (numTetrominosLeft[(int)s] > 0)
                    {
                        result.Add(s);
                    }
                }
                if (result.Count > 0)
                {
                    return result;
                }
            }
            // should never happen
            throw new InvalidOperationException("No tetrominos left");
        }
    }
}
