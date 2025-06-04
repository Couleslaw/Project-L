#nullable enable

namespace ProjectL.GameScene.PuzzleZone
{
    using ProjectL.Animation;
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.GameScene.ActionZones;
    using ProjectL.Sound;
    using ProjectL.Utils;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public enum PuzzleZoneMode
    {
        Disabled,
        ReadyToTakePuzzle,
        TakePuzzle,
        Recycle
    }

    public class PuzzleZoneManager : GraphicsManager<PuzzleZoneManager>,
        ICurrentTurnListener, IGameStatePuzzleListener,
        IActionCreationController,
        IHumanPlayerActionCreator<TakePuzzleAction>, IHumanPlayerActionCreator<RecycleAction>,
        IAIPlayerActionAnimator<TakePuzzleAction>, IAIPlayerActionAnimator<RecycleAction>
    {

        [Header("Puzzle columns")]
        [SerializeField] private PuzzlesColumn? _whiteColumn;
        [SerializeField] private PuzzlesColumn? _blackColumn;

        [SerializeField] private Button? _requestTakePuzzleButton;

        private TurnInfo _currentTurnInfo;

        private event Action<IActionModification<TakePuzzleAction>>? TakePuzzleModifiedEventHandler;
        event Action<IActionModification<TakePuzzleAction>>? IHumanPlayerActionCreator<TakePuzzleAction>.ActionModifiedEventHandler {
            add => TakePuzzleModifiedEventHandler += value;
            remove => TakePuzzleModifiedEventHandler -= value;
        }

        private event Action<IActionModification<RecycleAction>>? RecycleModifiedEventHandler;
        event Action<IActionModification<RecycleAction>>? IHumanPlayerActionCreator<RecycleAction>.ActionModifiedEventHandler {
            add => RecycleModifiedEventHandler += value;
            remove => RecycleModifiedEventHandler -= value;
        }

        public void ReportTakePuzzleChange(TakePuzzleActionModification change) => TakePuzzleModifiedEventHandler?.Invoke(change);
        public void ReportRecycleChange(RecycleActionModification change)
        {
            RecycleModifiedEventHandler?.Invoke(change);
            if (change.IsSelected) {
                if (change.Color == RecycleAction.Options.White) {
                    _blackColumn!.RemoveFromRecycle();
                }
                else {
                    _whiteColumn!.RemoveFromRecycle();
                }
            }
        }

        public static void AddToRadioButtonGroup(Button button)
        {
            string groupName = nameof(PuzzleZoneManager);
            RadioButtonsGroup.RegisterButton(button, groupName);
        }

        public static void RemoveFromRadioButtonGroup(Button button)
        {
            RadioButtonsGroup.UnregisterButton(button);
        }

        public override void Init(GameCore game)
        {
            if (_whiteColumn == null || _blackColumn == null || _requestTakePuzzleButton == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            game.GameState.AddListener((IGameStatePuzzleListener)this);
            game.AddListener((ICurrentTurnListener)this);

            HumanPlayerActionCreationManager.Instance.AddListener<TakePuzzleAction>(this);
            HumanPlayerActionCreationManager.Instance.AddListener<RecycleAction>(this);

            _whiteColumn.Init(isBlack: false, game.GameState.NumWhitePuzzlesLeft);
            _blackColumn.Init(isBlack: true, game.GameState.NumBlackPuzzlesLeft);

            HumanPlayerActionCreationManager.RegisterController(this);

            _requestTakePuzzleButton.onClick.AddListener(ActionZonesManager.Instance.ManuallyClickTakePuzzleButton);
            EnableRequestTakePuzzleButton(false);
        }

        void IActionCreationController.SetPlayerMode(PlayerMode mode)
        {
            if (mode == PlayerMode.NonInteractive) {
                SetMode(PuzzleZoneMode.Disabled);
            }
        }

        void IActionCreationController.SetActionMode(ActionMode mode)
        {
            if (mode == ActionMode.ActionCreation) {
                SetMode(PuzzleZoneMode.ReadyToTakePuzzle);
            }
            else {
                SetMode(PuzzleZoneMode.Disabled);
            }
        }

        private void SetMode(PuzzleZoneMode mode)
        {
            _whiteColumn!.SetMode(mode, _currentTurnInfo);
            _blackColumn!.SetMode(mode, _currentTurnInfo);

            EnableRequestTakePuzzleButton(mode == PuzzleZoneMode.ReadyToTakePuzzle);
        }

        private void EnableRequestTakePuzzleButton(bool enable) => _requestTakePuzzleButton!.image.raycastTarget = enable;

        void IGameStatePuzzleListener.OnWhitePuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_whiteColumn != null) {
                _whiteColumn[index]?.SetPuzzle(puzzle);
            }
        }

        void IGameStatePuzzleListener.OnBlackPuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_blackColumn != null) {
                _blackColumn[index]?.SetPuzzle(puzzle);
            }
        }

        void IGameStatePuzzleListener.OnBlackPuzzleDeckChanged(int deckSize)
        {
            if (_blackColumn != null) {
                _blackColumn.DeckCard.SetDeckSize(deckSize);
                SoundManager.Instance?.PlaySoftTapSoundEffect();
            }
        }

        void IGameStatePuzzleListener.OnWhitePuzzleDeckChanged(int deckSize)
        {
            if (_whiteColumn != null) {
                _whiteColumn.DeckCard.SetDeckSize(deckSize);
                SoundManager.Instance?.PlaySoftTapSoundEffect();
            }
        }

        void ICurrentTurnListener.OnCurrentTurnChanged(TurnInfo currentTurnInfo)
        {
            _currentTurnInfo = currentTurnInfo;
        }

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionRequested() => SetMode(PuzzleZoneMode.TakePuzzle);

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionCanceled() => SetMode(PuzzleZoneMode.ReadyToTakePuzzle);

        void IHumanPlayerActionCreator<TakePuzzleAction>.OnActionConfirmed() => SetMode(PuzzleZoneMode.Disabled);

        void IHumanPlayerActionCreator<RecycleAction>.OnActionRequested() => SetMode(PuzzleZoneMode.Recycle);

        void IHumanPlayerActionCreator<RecycleAction>.OnActionCanceled() => SetMode(PuzzleZoneMode.ReadyToTakePuzzle);

        void IHumanPlayerActionCreator<RecycleAction>.OnActionConfirmed() => SetMode(PuzzleZoneMode.Disabled);

        async Task IAIPlayerActionAnimator<TakePuzzleAction>.AnimateAsync(TakePuzzleAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_whiteColumn == null || _blackColumn == null) {
                return;
            }

            float delay = 0.7f;

            // dim all cards
            using (_whiteColumn.GetDisposableColumnDimmer()) {
                using (_blackColumn.GetDisposableColumnDimmer()) {

                    // wait a bit
                    await AnimationManager.WaitForScaledDelay(delay, cancellationToken);

                    // select the taken puzzle card
                    switch (action.Option) {
                        case TakePuzzleAction.Options.TopWhite:
                            using (_whiteColumn.DeckCard.GetDisposableCardHighlighter()) {
                                await AnimationManager.WaitForScaledDelay(delay, cancellationToken);
                            }
                            break;
                        case TakePuzzleAction.Options.TopBlack:
                            using (_blackColumn.DeckCard.GetDisposableCardHighlighter()) {
                                await AnimationManager.WaitForScaledDelay(delay, cancellationToken);
                            }
                            break;
                        case TakePuzzleAction.Options.Normal:
                            if (TryGetPuzzleCardWithId(action.PuzzleId!.Value, out var puzzleCard)) {
                                using (puzzleCard!.GetDisposableCardHighlighter()) {
                                    await AnimationManager.WaitForScaledDelay(delay, cancellationToken);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        async Task IAIPlayerActionAnimator<RecycleAction>.AnimateAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_whiteColumn == null || _blackColumn == null) {
                return;
            }

            // dim all cards - except the deck cards
            using (_whiteColumn.GetDisposableColumnDimmer(shouldDimCoverCard: false)) {
                using (_blackColumn.GetDisposableColumnDimmer(shouldDimCoverCard: false)) {

                    // wait a bit
                    await AnimationManager.WaitForScaledDelay(1f, cancellationToken);

                    PuzzlesColumn column = action.Option == RecycleAction.Options.White ? _whiteColumn : _blackColumn;

                    List<uint> puzzleIds = new();
                    foreach (uint puzzleId in action.Order) {
                        puzzleIds.Add(puzzleId);
                        using (column.GetDisposablePuzzleHighlighter(puzzleIds)) {
                            await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
                        }
                    }
                }
            }
        }

        private bool TryGetPuzzleCardWithId(uint puzzleId, out PuzzleCard? puzzleCard)
        {
            if (_whiteColumn!.TryGetPuzzleCardWithId(puzzleId, out puzzleCard)) {
                return true;
            }
            if (_blackColumn!.TryGetPuzzleCardWithId(puzzleId, out puzzleCard)) {
                return true;
            }
            puzzleCard = null;
            return false;
        }

        public class DisposableSpriteReplacer : IDisposable
        {
            private Button? _button;
            private SpriteState _originalSpriteState;

            public DisposableSpriteReplacer(Button button, Sprite? tempSprite)
            {
                if (button.transition != Selectable.Transition.SpriteSwap || tempSprite == null) {
                    return;
                }
                _button = button;
                _originalSpriteState = button.spriteState;
                button.spriteState = new SpriteState {
                    highlightedSprite = tempSprite,
                    pressedSprite = tempSprite,
                    selectedSprite = tempSprite,
                    disabledSprite = tempSprite
                };
            }

            public void Dispose()
            {
                if (_button != null && _button.transition == Selectable.Transition.SpriteSwap) {
                    _button.spriteState = _originalSpriteState;
                }
            }
        }
    }
}
