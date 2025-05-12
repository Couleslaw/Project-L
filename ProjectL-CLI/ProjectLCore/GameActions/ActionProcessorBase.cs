namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameActions.Verification;
    using System;

    /// <summary>
    /// Defines an interface for processing game actions.
    /// </summary>
    public interface IActionProcessor
    {
        /// <summary>
        /// Processes the given <see cref="GameAction"/>.
        /// </summary>
        /// <param name="action">The game action to process.</param>
        public void ProcessAction(GameAction action);
    }

    /// <summary>
    /// A base class for processing actions using the visitor pattern.
    /// Each action should be verified by an <see cref="ActionVerifier"/> before being processed.
    /// </summary>
    /// <seealso cref="GameAction"/>
    /// <seealso cref="ActionVerifier"/>
    /// <seealso cref="GameActionProcessor"/>
    public abstract class ActionProcessorBase : IActionProcessor
    {
        #region Methods

        /// <summary>
        /// Processes the given <see cref="GameAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessAction(GameAction action)
        {
            switch (action) {
                case EndFinishingTouchesAction a:
                    ProcessAction(a);
                    break;
                case TakePuzzleAction a:
                    ProcessAction(a);
                    break;
                case RecycleAction a:
                    ProcessAction(a);
                    break;
                case TakeBasicTetrominoAction a:
                    ProcessAction(a);
                    break;
                case ChangeTetrominoAction a:
                    ProcessAction(a);
                    break;
                case PlaceTetrominoAction a:
                    ProcessAction(a);
                    break;
                case MasterAction a:
                    ProcessAction(a);
                    break;
                case DoNothingAction a:
                    ProcessAction(a);
                    break;
                default:
                    throw new NotImplementedException($"Processing action of type {action.GetType()} is not implemented.");
            }
        }

        /// <summary>
        /// Processes the given <see cref="EndFinishingTouchesAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(EndFinishingTouchesAction action);

        /// <summary>
        /// Processes the given <see cref="TakePuzzleAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(TakePuzzleAction action);

        /// <summary>
        /// Processes the given <see cref="RecycleAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(RecycleAction action);

        /// <summary>
        /// Processes the given <see cref="TakeBasicTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(TakeBasicTetrominoAction action);

        /// <summary>
        /// Processes the given <see cref="ChangeTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(ChangeTetrominoAction action);

        /// <summary>
        /// Processes the given <see cref="PlaceTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(PlaceTetrominoAction action);

        /// <summary>
        /// Processes the given <see cref="MasterAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(MasterAction action);

        /// <summary>
        /// Processes the given <see cref="DoNothingAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected abstract void ProcessAction(DoNothingAction action);

        #endregion
    }
}
