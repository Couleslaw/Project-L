#nullable enable

namespace ProjectLCore.GamePieces
{
    public interface IColorPuzzleListener
    {
        #region Methods

        void OnTetrominoPlaced(ColorImage.Color color, BinaryImage position);

        void OnTetrominoRemoved(ColorImage.Color color, BinaryImage position);

        #endregion
    }
}
