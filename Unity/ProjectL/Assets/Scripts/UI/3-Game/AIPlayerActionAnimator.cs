#nullable enable

namespace ProjectL.UI.GameScene
{
    using ProjectL.UI.GameScene.Zones.PieceZone;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectLCore.GameActions;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SocialPlatforms.GameCenter;

    public class AIPlayerActionAnimator : AsyncActionProcessorBase
    {
        protected override async Task ProcessActionAsync(EndFinishingTouchesAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(TakePuzzleAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            DraggableTetromino tetromino = TetrominoButtonsManager.Instance.SpawnTetromino(action.Shape);
            Vector2 center = PlayerZoneManager.Instance.GetPlacementPositionFor(action);
            await tetromino.AnimateAIPlayerPlaceActionAsync(center, action, cancellationToken);
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            tetromino.Discard();
        }

        protected override async Task ProcessActionAsync(MasterAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        protected override async Task ProcessActionAsync(DoNothingAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }
    }
}
