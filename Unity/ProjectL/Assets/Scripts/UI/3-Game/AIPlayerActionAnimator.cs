#nullable enable

namespace ProjectL.UI.GameScene.Actions
{
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.GameScene.Zones.PieceZone;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SocialPlatforms.GameCenter;

    public class AIPlayerActionAnimator : AsyncActionProcessorBase, IPlayerStatePuzzleFinishedAsyncListener
    {
        protected override async Task ProcessActionAsync(EndFinishingTouchesAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            using (new ActionZonesManager.SimulateButtonPressDisposable(ActionZonesManager.Button.EndFinishingTouches)) {
                await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(TakePuzzleAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            using (new ActionZonesManager.SimulateButtonPressDisposable(ActionZonesManager.Button.TakePuzzle)) {
                await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            using (new ActionZonesManager.SimulateButtonPressDisposable(ActionZonesManager.Button.Recycle)) {
                await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            using (new ActionZonesManager.SimulateButtonPressDisposable(ActionZonesManager.Button.TakeBasicTetromino)) {
                await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            using (new ActionZonesManager.SimulateButtonPressDisposable(ActionZonesManager.Button.ChangeTetromino)) {
                await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            DraggableTetromino tetromino = TetrominoButtonsManager.Instance.SpawnTetromino(action.Shape);
            Vector2 center = PlayerZoneManager.Instance.GetPlacementPositionFor(action);
            await tetromino.AnimateAIPlayerPlaceActionAsync(center, action, cancellationToken);
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            tetromino.ReturnToCollection();
        }

        protected override async Task ProcessActionAsync(MasterAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            using (new ActionZonesManager.SimulateButtonPressDisposable(ActionZonesManager.Button.MasterAction)) {
                await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(DoNothingAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);
        }

        async Task IPlayerStatePuzzleFinishedAsyncListener.OnPuzzleFinishedAsync(int index, FinishedPuzzleInfo info, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return;
            }
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
            
            var puzzleSlot = PlayerZoneManager.Instance.GetCurrentPlayerPuzzleOnIndex(index);
            using (puzzleSlot.CreateTemporaryPuzzleHighlighter()) {
                await GameAnimationManager.WaitForAnimationDelayFraction(1f, cancellationToken);

                if (info.RewardOptions == null || info.SelectedReward == null) {
                    return;
                }

                using (new ActionZonesManager.SimulateButtonPressDisposable(ActionZonesManager.Button.SelectReward)) {
                    await TetrominoButtonsManager.Instance.AnimateSelectRewardAsync(info.RewardOptions, info.SelectedReward.Value, cancellationToken);
                }
            }
            await GameAnimationManager.WaitForAnimationDelayFraction(0.5f, cancellationToken);
        }
    }
}
