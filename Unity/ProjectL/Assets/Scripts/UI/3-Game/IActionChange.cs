#nullable enable

namespace ProjectL.UI.GameScene.Actions.Constructing
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;

    public interface IActionChange<out T> where T : GameAction
    {
    }


    public class TakePuzzleActionChange : IActionChange<TakePuzzleAction>
    {
        public TakePuzzleAction? Action { get; }
        public TakePuzzleActionChange(TakePuzzleAction? action)
        {
            Action = action;
        }
    }

    public class RecycleActionChange : IActionChange<RecycleAction>
    {
        public bool IsSelected { get; }
        public RecycleAction.Options Color { get; }
        public uint PuzzleId { get; }
        public RecycleActionChange(Puzzle puzzle, bool isSelected)
        {
            IsSelected = isSelected;
            Color = puzzle.IsBlack ? RecycleAction.Options.Black : RecycleAction.Options.White;
            PuzzleId = puzzle.Id;
        }
    }

    public class TakeBasicTetrominoActionChange : IActionChange<TakeBasicTetrominoAction>
    {
        public bool IsSelected { get; }
        public TakeBasicTetrominoActionChange(bool isSelected)
        {
            IsSelected = isSelected;
        }
    }

    public class ChangeTetrominoActionChange : IActionChange<ChangeTetrominoAction>
    {
        public enum Options { Old, New }
        public Options Option { get; }
        public TetrominoShape? Shape { get; }
        public ChangeTetrominoActionChange(TetrominoShape? shape, Options option)
        {
            Shape = shape;
            Option = option;
        }
    }


    public class PlaceTetrominoActionChange : IActionChange<PlaceTetrominoAction>
    {
        public enum Options { Placed, Removed }
        public Options Option { get; }
        public PlaceTetrominoAction Placement { get; }
        public PlaceTetrominoActionChange(PlaceTetrominoAction action, Options option)
        {
            Placement = action;
            Option = option;
        }
    }

    public class SelectRewardActionChange : IActionChange<SelectRewardAction>
    {
        public TetrominoShape? SelectedReward { get; }
        public SelectRewardActionChange(TetrominoShape? selectedReward)
        {
            SelectedReward = selectedReward;
        }
    }
}
