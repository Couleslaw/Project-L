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

    public class AIPlayerActionCreationAnimator : AsyncActionProcessorBase, 
        IPlayerStatePuzzleFinishedAsyncListener,
        IGameStatePuzzleAsyncListener
    {
        const float _initialDelay = 0.6f;
        private IAIPlayerActionAnimator<TakePuzzleAction>? _takePuzzleAnimator;
        private IAIPlayerActionAnimator<RecycleAction>? _recycleAnimator;
        private IAIPlayerActionAnimator<TakeBasicTetrominoAction>? _takeBasicTetrominoAnimator;
        private IAIPlayerActionAnimator<ChangeTetrominoAction>? _changeTetrominoActionAnimator;
        private IAIPlayerActionAnimator<SelectRewardAction>? _selectRewardActionAnimator;


        public void Init(GameCore game)
        {
            game.GameState.AddListener(this);
            _takePuzzleAnimator = PuzzleZoneManager.Instance;
            _recycleAnimator = PuzzleZoneManager.Instance;
            _takeBasicTetrominoAnimator = PieceZoneManager.Instance;
            _changeTetrominoActionAnimator = PieceZoneManager.Instance;
            _selectRewardActionAnimator = PieceZoneManager.Instance;
        }

        protected override async Task ProcessActionAsync(EndFinishingTouchesAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.EndFinishingTouches)) {
                await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(TakePuzzleAction action, CancellationToken cancellationToken)
        {
            if (_takePuzzleAnimator == null) {
                return;
            }
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.TakePuzzle)) {
                await GameAnimationManager.WaitForScaledDelayAsync(0.5f, cancellationToken);
                await _takePuzzleAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            if (_recycleAnimator == null) {
                return;
            }
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.Recycle)) {
                await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);
                await _recycleAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            if (_takeBasicTetrominoAnimator == null) {
                return;
            }
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.TakeBasicTetromino)) {
                await _takeBasicTetrominoAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            if (_changeTetrominoActionAnimator == null) {
                return;
            }
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.ChangeTetromino)) {
                await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);
                await _changeTetrominoActionAnimator.Animate(action, cancellationToken);
            }
        }

        protected override async Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
            
            IAIPlayerActionAnimator<PlaceTetrominoAction> tetromino = PieceZoneManager.Instance.SpawnTetromino(action.Shape);
            await tetromino.Animate(action, cancellationToken);
        }

        protected override async Task ProcessActionAsync(MasterAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.MasterAction)) {
                await GameAnimationManager.WaitForScaledDelayAsync(0.5f, cancellationToken);
                foreach (PlaceTetrominoAction placeAction in action.TetrominoPlacements) {
                    await ProcessActionAsync(placeAction, cancellationToken);
                }
            }
        }

        protected override async Task ProcessActionAsync(DoNothingAction action, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForScaledDelayAsync(_initialDelay, cancellationToken);
        }

        async Task IPlayerStatePuzzleFinishedAsyncListener.OnPuzzleFinishedAsync(int index, FinishedPuzzleInfo info, CancellationToken cancellationToken)
        {
            if (_selectRewardActionAnimator == null) {
                return;
            }
            await GameAnimationManager.WaitForScaledDelayAsync(0.5f, cancellationToken);
            
            // highlight completed puzzle
            var puzzleSlot = PlayerZoneManager.Instance.GetCurrentPlayerPuzzleOnIndex(index);
            using (puzzleSlot.CreateTemporaryPuzzleHighlighter()) {
                await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);

                // if there is reward --> animate selection
                if (info.RewardOptions != null && info.SelectedReward != null) {
                    SelectRewardAction action = new SelectRewardAction(info.RewardOptions, info.SelectedReward.Value);
                    using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.SelectReward)) {
                        await _selectRewardActionAnimator.Animate(action, cancellationToken);
                    }
                }
            }
        }

        async Task IGameStatePuzzleAsyncListener.OnPuzzleRefilledAsync(int index, CancellationToken cancellationToken)
        {
            await GameAnimationManager.WaitForScaledDelayAsync(0.6f, cancellationToken);
        }
    }
}
