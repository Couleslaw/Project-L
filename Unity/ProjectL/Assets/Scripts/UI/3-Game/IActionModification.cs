#nullable enable

namespace ProjectL.UI.GameScene.Actions.Constructing
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;

    public interface IActionModification<out T> where T : GameAction
    {
    }


    public class TakePuzzleActionModification : IActionModification<TakePuzzleAction>
    {
        public TakePuzzleAction? Action { get; }
        public TakePuzzleActionModification(TakePuzzleAction? action)
        {
            Action = action;
        }
    }

    public class RecycleActionModification : IActionModification<RecycleAction>
    {
        public bool IsSelected { get; }
        public RecycleAction.Options Color { get; }
        public uint PuzzleId { get; }
        public RecycleActionModification(Puzzle puzzle, bool isSelected)
        {
            IsSelected = isSelected;
            Color = puzzle.IsBlack ? RecycleAction.Options.Black : RecycleAction.Options.White;
            PuzzleId = puzzle.Id;
        }
    }

    public class TakeBasicTetrominoActionModification : IActionModification<TakeBasicTetrominoAction>
    {
        public bool IsSelected { get; }
        public TakeBasicTetrominoActionModification(bool isSelected)
        {
            IsSelected = isSelected;
        }
    }

    public class ChangeTetrominoActionModification : IActionModification<ChangeTetrominoAction>
    {
        public TetrominoShape? OldTetromino { get; }
        public TetrominoShape? NewTetromino { get; }
        public ChangeTetrominoActionModification(TetrominoShape? oldTetromino, TetrominoShape? newTetromino)
        {
            OldTetromino = oldTetromino;
            NewTetromino = newTetromino;
        }
    }


    public class PlaceTetrominoActionModification : IActionModification<PlaceTetrominoAction>
    {
        public enum Options { Placed, Removed }
        public Options Option { get; }
        public PlaceTetrominoAction Placement { get; }
        public PlaceTetrominoActionModification(PlaceTetrominoAction action, Options option)
        {
            Placement = action;
            Option = option;
        }
    }

    public class SelectRewardActionModification : IActionModification<SelectRewardAction>
    {
        public TetrominoShape? SelectedReward { get; }
        public SelectRewardActionModification(TetrominoShape? selectedReward)
        {
            SelectedReward = selectedReward;
        }
    }
}
