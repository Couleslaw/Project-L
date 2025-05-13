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
        const float _initialDelay = 0.6f;

        protected override async Task ProcessActionAsync(EndFinishingTouchesAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.EndFinishingTouches)) {
                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(TakePuzzleAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.TakePuzzle)) {
                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.Recycle)) {
                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.TakeBasicTetromino)) {
                await TetrominoButtonsManager.Instance.AnimateTakeBasicTetrominoActionAsync(cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.ChangeTetromino)) {
                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);
                await TetrominoButtonsManager.Instance.AnimateChangeTetrominoActionAsync(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            DraggableTetromino tetromino = TetrominoButtonsManager.Instance.SpawnTetromino(action.Shape);
            Vector2 center = PlayerZoneManager.Instance.GetPlacementPositionFor(action);
            await tetromino.AnimateAIPlayerPlaceActionAsync(center, action, cancellationToken);
            await GameAnimationManager.WaitForSecondsAsync(0.5f, cancellationToken);
            tetromino.ReturnToCollectionAfterMilliseconds(20);
        }

        protected override async Task ProcessActionAsync(MasterAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.MasterAction)) {
                await GameAnimationManager.WaitForSecondsAsync(0.5f, cancellationToken);
                foreach (PlaceTetrominoAction placeAction in action.TetrominoPlacements) {
                    await ProcessActionAsync(placeAction, cancellationToken);
                }
            }
        }

        protected override async Task ProcessActionAsync(DoNothingAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
        }

        async Task IPlayerStatePuzzleFinishedAsyncListener.OnPuzzleFinishedAsync(int index, FinishedPuzzleInfo info, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return;
            }
            await GameAnimationManager.WaitForSecondsAsync(0.5f, cancellationToken);
            
            // highlight completed puzzle
            var puzzleSlot = PlayerZoneManager.Instance.GetCurrentPlayerPuzzleOnIndex(index);
            using (puzzleSlot.CreateTemporaryPuzzleHighlighter()) {
                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);

                // if there is reward --> animate selection
                if (info.RewardOptions != null && info.SelectedReward != null) {
                    using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.SelectReward)) {
                        await TetrominoButtonsManager.Instance.AnimateSelectRewardAsync(info.RewardOptions, info.SelectedReward.Value, cancellationToken);
                    }
                }
            }
        }
    }
}
