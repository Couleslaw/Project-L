#nullable enable

namespace ProjectL.UI.GameScene.Actions
{
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.GameScene.Zones.PieceZone;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.GameScene.Zones.PuzzleZone;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SocialPlatforms.GameCenter;

    public interface IAIPlayerActionAnimator<T> where T : GameAction
    {
        Task Animate(T action, CancellationToken cancellationToken);
    }

    public class AIPlayerActionAnimator : AsyncActionProcessorBase, IPlayerStatePuzzleFinishedAsyncListener
    {
        const float _initialDelay = 0.6f;
        private readonly IAIPlayerActionAnimator<TakePuzzleAction> _takePuzzleAnimator;
        private readonly IAIPlayerActionAnimator<RecycleAction> _recycleAnimator;
        private readonly IAIPlayerActionAnimator<TakeBasicTetrominoAction> _takeBasicTetrominoAnimator;
        private readonly IAIPlayerActionAnimator<ChangeTetrominoAction> _changeTetrominoActionAnimator;
        private readonly IAIPlayerActionAnimator<SelectRewardAction> _selectRewardActionAnimator;


        public AIPlayerActionAnimator()
        {
            _takePuzzleAnimator = PuzzleZoneManager.Instance;
            _recycleAnimator = PuzzleZoneManager.Instance;
            _takeBasicTetrominoAnimator = TetrominoButtonsManager.Instance;
            _changeTetrominoActionAnimator = TetrominoButtonsManager.Instance;
            _selectRewardActionAnimator = TetrominoButtonsManager.Instance;
        }

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
                await _takePuzzleAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.Recycle)) {
                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);
                await _recycleAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.TakeBasicTetromino)) {
                await _takeBasicTetrominoAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.ChangeTetromino)) {
                await GameAnimationManager.WaitForSecondsAsync(1f, cancellationToken);
                await _changeTetrominoActionAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForSecondsAsync(_initialDelay, cancellationToken);
            
            IAIPlayerActionAnimator<PlaceTetrominoAction> tetromino = TetrominoButtonsManager.Instance.SpawnTetromino(action.Shape);
            await tetromino.Animate(action, cancellationToken);
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
                    SelectRewardAction action = new SelectRewardAction(info.RewardOptions, info.SelectedReward.Value);
                    using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.SelectReward)) {
                        await _selectRewardActionAnimator.Animate(action, cancellationToken);
                    }
                }
            }
        }
    }
}
