namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameActions.Verification;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A base class for asynchronously processing actions using the visitor pattern.
    /// Each action should be verified by an <see cref="ActionVerifier"/> before being processed.
    /// </summary>
    /// <seealso cref="GameAction"/>
    /// <seealso cref="ActionVerifier"/>
    public abstract class AsyncActionProcessorBase
    {
        #region Methods

        /// <summary>
        /// Processes the given <see cref="GameAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ProcessActionAsync(GameAction action, CancellationToken cancellationToken = default)
        {
            switch (action) {
                case EndFinishingTouchesAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                case TakePuzzleAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                case RecycleAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                case TakeBasicTetrominoAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                case ChangeTetrominoAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                case PlaceTetrominoAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                case MasterAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                case DoNothingAction a:
                    await ProcessActionAsync(a, cancellationToken);
                    break;
                default:
                    throw new NotImplementedException($"Processing action of type {action.GetType()} is not implemented.");
            }
        }

        /// <summary>
        /// Processes the given <see cref="EndFinishingTouchesAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(EndFinishingTouchesAction action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the given <see cref="TakePuzzleAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(TakePuzzleAction action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the given <see cref="RecycleAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the given <see cref="TakeBasicTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the given <see cref="ChangeTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the given <see cref="PlaceTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the given <see cref="MasterAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(MasterAction action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes the given <see cref="DoNothingAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ProcessActionAsync(DoNothingAction action, CancellationToken cancellationToken = default);

        #endregion
    }
}
