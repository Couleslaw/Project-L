#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectLCore.GameLogic;

    public interface IPuzzleZoneCard
    {
        #region Methods

        public void Init(bool isBlack);

        public void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo);

        public PuzzleZoneManager.TemporarySpriteReplacer CreateCardHighlighter();

        public PuzzleZoneManager.TemporarySpriteReplacer CreateCardDimmer();

        #endregion
    }
}
