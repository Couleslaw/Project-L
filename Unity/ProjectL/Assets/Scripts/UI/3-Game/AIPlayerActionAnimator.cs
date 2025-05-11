#nullable enable

namespace ProjectL.UI.GameScene
{
    using ProjectLCore.GameActions;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class AIPlayerActionAnimator : AsyncActionProcessorBase
    {
        protected override async Task ProcessActionAsync(EndFinishingTouchesAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(TakePuzzleAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(MasterAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(DoNothingAction action, CancellationToken cancellationToken = default)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }
    }
}
