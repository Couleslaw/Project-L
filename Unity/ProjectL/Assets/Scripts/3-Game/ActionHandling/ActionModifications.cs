#nullable enable

namespace ProjectL.GameScene.ActionHandling
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;

    public interface IActionModification<out T> where T : GameAction
    {
    }

    public class TakePuzzleActionModification : IActionModification<TakePuzzleAction>
    {
        #region Constructors

        public TakePuzzleActionModification(TakePuzzleAction? action)
        {
            Action = action;
        }

        #endregion

        #region Properties

        public TakePuzzleAction? Action { get; }

        #endregion
    }

    public class RecycleActionModification : IActionModification<RecycleAction>
    {
        #region Constructors

        public RecycleActionModification(Puzzle puzzle, bool isSelected)
        {
            IsSelected = isSelected;
            Color = puzzle.IsBlack ? RecycleAction.Options.Black : RecycleAction.Options.White;
            PuzzleId = puzzle.Id;
        }

        #endregion

        #region Properties

        public bool IsSelected { get; }

        public RecycleAction.Options Color { get; }

        public uint PuzzleId { get; }

        #endregion
    }

    public class TakeBasicTetrominoActionModification : IActionModification<TakeBasicTetrominoAction>
    {
        #region Constructors

        public TakeBasicTetrominoActionModification(bool isSelected)
        {
            IsSelected = isSelected;
        }

        #endregion

        #region Properties

        public bool IsSelected { get; }

        #endregion
    }

    public class ChangeTetrominoActionModification : IActionModification<ChangeTetrominoAction>
    {
        #region Constructors

        public ChangeTetrominoActionModification(TetrominoShape? oldTetromino, TetrominoShape? newTetromino)
        {
            OldTetromino = oldTetromino;
            NewTetromino = newTetromino;
        }

        #endregion

        #region Properties

        public TetrominoShape? OldTetromino { get; }

        public TetrominoShape? NewTetromino { get; }

        #endregion
    }

    public class PlaceTetrominoActionModification : IActionModification<PlaceTetrominoAction>
    {
        #region Constructors

        public PlaceTetrominoActionModification(PlaceTetrominoAction action, Options option)
        {
            Placement = action;
            Option = option;
        }

        #endregion

        public enum Options { Placed, Removed }

        #region Properties

        public Options Option { get; }

        public PlaceTetrominoAction Placement { get; }

        #endregion
    }

    public class SelectRewardActionModification : IActionModification<SelectRewardAction>
    {
        #region Constructors

        public SelectRewardActionModification(TetrominoShape? selectedReward)
        {
            SelectedReward = selectedReward;
        }

        #endregion

        #region Properties

        public TetrominoShape? SelectedReward { get; }

        #endregion
    }
}
