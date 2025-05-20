#nullable enable

namespace ProjectL.UI.GameScene.Actions
{
    using ProjectL.UI.Animation;
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.GameScene.Zones.PieceZone;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.GameScene.Zones.PuzzleZone;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAIPlayerActionAnimator<T> where T : GameAction
    {
        #region Methods

        Task Animate(T action, CancellationToken cancellationToken);

        #endregion
    }

    public class AIPlayerActionAnimationManager : GraphicsManager<AIPlayerActionAnimationManager>,
        IAsyncActionProcessor,
        IPlayerStatePuzzleFinishedAsyncListener,
        IGameStatePuzzleAsyncListener
    {
        #region Constants

        private const float _initialDelay = 0.6f;

        #endregion

        #region Fields

        private IAIPlayerActionAnimator<TakePuzzleAction>? _takePuzzleAnimator;

        private IAIPlayerActionAnimator<RecycleAction>? _recycleAnimator;

        private IAIPlayerActionAnimator<TakeBasicTetrominoAction>? _takeBasicTetrominoAnimator;

        private IAIPlayerActionAnimator<ChangeTetrominoAction>? _changeTetrominoActionAnimator;

        private IAIPlayerActionAnimator<SelectRewardAction>? _selectRewardActionAnimator;

        #endregion

        #region Methods

        public override void Init(GameCore game)
        {
            game.GameState.AddListener((IGameStatePuzzleAsyncListener)this);
            foreach (var player in game.Players) {
                if (player is AIPlayerBase) {
                    game.PlayerStates[player].AddListener((IPlayerStatePuzzleFinishedAsyncListener)this);
                }
            }

            _takePuzzleAnimator = PuzzleZoneManager.Instance;
            _recycleAnimator = PuzzleZoneManager.Instance;
            _takeBasicTetrominoAnimator = PieceZoneManager.Instance;
            _changeTetrominoActionAnimator = PieceZoneManager.Instance;
            _selectRewardActionAnimator = PieceZoneManager.Instance;
        }

        async Task IPlayerStatePuzzleFinishedAsyncListener.OnPuzzleFinishedAsync(FinishedPuzzleInfo info, CancellationToken cancellationToken)
        {
            if (_selectRewardActionAnimator == null) {
                return;
            }
            await AnimationManager.WaitForScaledDelay(0.5f, cancellationToken);

            // highlight completed puzzle
            var puzzleSlot = PlayerZoneManager.Instance.GetPuzzleWithId(info.Puzzle.Id)!;
            using (puzzleSlot.CreateTemporaryPuzzleHighlighter()) {
                await AnimationManager.WaitForScaledDelay(1f, cancellationToken);

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
            await AnimationManager.WaitForScaledDelay(0.6f, cancellationToken);
        }

        /// <summary>
        /// Animates the specified action.
        /// </summary>
        /// <param name="action">The action to animate.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the animation to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">The task was canceled.</exception>
        async Task IAsyncActionProcessor.ProcessActionAsync(GameAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

        private async Task ProcessActionAsync(EndFinishingTouchesAction action, CancellationToken cancellationToken)
        {
            await AnimationManager.WaitForScaledDelay(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.EndFinishingTouches)) {
                await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
            }
        }

        private async Task ProcessActionAsync(TakePuzzleAction action, CancellationToken cancellationToken)
        {
            if (_takePuzzleAnimator == null) {
                return;
            }
            await AnimationManager.WaitForScaledDelay(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.TakePuzzle)) {
                await AnimationManager.WaitForScaledDelay(0.5f, cancellationToken);
                await _takePuzzleAnimator.Animate(action, cancellationToken);
            }
        }

        private async Task ProcessActionAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            if (_recycleAnimator == null) {
                return;
            }
            await AnimationManager.WaitForScaledDelay(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.Recycle)) {
                await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
                await _recycleAnimator.Animate(action, cancellationToken);
            }
        }

        private async Task ProcessActionAsync(TakeBasicTetrominoAction action, CancellationToken cancellationToken)
        {
            if (_takeBasicTetrominoAnimator == null) {
                return;
            }
            await AnimationManager.WaitForScaledDelay(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.TakeBasicTetromino)) {
                await _takeBasicTetrominoAnimator.Animate(action, cancellationToken);
            }
        }

        private async Task ProcessActionAsync(ChangeTetrominoAction action, CancellationToken cancellationToken)
        {
            if (_changeTetrominoActionAnimator == null) {
                return;
            }
            await AnimationManager.WaitForScaledDelay(_initialDelay, cancellationToken);
            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.ChangeTetromino)) {
                await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
                await _changeTetrominoActionAnimator.Animate(action, cancellationToken);
            }
        }

        private async Task ProcessActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            await AnimationManager.WaitForScaledDelay(1.5f * _initialDelay, cancellationToken);

            var tetromino = await AnimatePlaceMovement(action, cancellationToken);
        }

        private async Task<DraggableTetromino> AnimatePlaceMovement(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            var tetromino = PieceZoneManager.Instance.GetPlaceTetrominoActionAnimator(action.Shape);
            await tetromino.Animate(action, cancellationToken);
            return (DraggableTetromino)tetromino;
        }

        private async Task ProcessActionAsync(MasterAction action, CancellationToken cancellationToken)
        {
            await AnimationManager.WaitForScaledDelay(_initialDelay, cancellationToken);

            List<Task<DraggableTetromino>> placeTasks = new();

            using (new ActionZonesManager.SimulateButtonClickDisposable(ActionZonesManager.Button.MasterAction)) {
                await AnimationManager.WaitForScaledDelay(0.5f, cancellationToken);

                foreach (PlaceTetrominoAction placeAction in action.TetrominoPlacements) {
                    cancellationToken.ThrowIfCancellationRequested();

                    placeTasks.Add(AnimatePlaceMovement(placeAction, cancellationToken));
                    await AnimationManager.WaitForScaledDelay(0.3f, cancellationToken);
                }
                await Task.WhenAll(placeTasks);
            }
        }

        private async Task ProcessActionAsync(DoNothingAction action, CancellationToken cancellationToken)
        {
            await AnimationManager.WaitForScaledDelay(_initialDelay, cancellationToken);
        }

        #endregion
    }
}
