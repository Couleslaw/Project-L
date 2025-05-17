#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectL.UI.Sound;
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
        TakePuzzle,
        Recycle
    }

    public interface IPuzzleZoneCard
    {
        #region Methods

        public void Init(bool isBlack);

        public void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo);

        public PuzzleZoneManager.TemporarySpriteReplacer CreateCardHighlighter();

        public PuzzleZoneManager.TemporarySpriteReplacer CreateCardDimmer();

        #endregion
    }

    public class PuzzleZoneManager : GraphicsManager<PuzzleZoneManager>,
        ICurrentTurnListener, IGameStatePuzzleListener,
        IGameActionController,
        IHumanPlayerActionListener<TakePuzzleAction>, IHumanPlayerActionListener<RecycleAction>,
        IAIPlayerActionAnimator<TakePuzzleAction>, IAIPlayerActionAnimator<RecycleAction>

    {

        [Header("Puzzle columns")]
        [SerializeField] private PuzzleColumn? _whiteColumn;
        [SerializeField] private PuzzleColumn? _blackColumn;

        private TurnInfo _currentTurnInfo;

        private event Action<IActionChange<TakePuzzleAction>>? TakePuzzleStateChangedEventHandler;
        event Action<IActionChange<TakePuzzleAction>>? IHumanPlayerActionListener<TakePuzzleAction>.StateChangedEventHandler {
            add => TakePuzzleStateChangedEventHandler += value;
            remove => TakePuzzleStateChangedEventHandler -= value;
        }

        private event Action<IActionChange<RecycleAction>>? RecycleStateChangedEventHandler;
        event Action<IActionChange<RecycleAction>>? IHumanPlayerActionListener<RecycleAction>.StateChangedEventHandler {
            add => RecycleStateChangedEventHandler += value;
            remove => RecycleStateChangedEventHandler -= value;
        }

        public void ReportTakePuzzleChange(TakePuzzleActionChange change) => TakePuzzleStateChangedEventHandler?.Invoke(change);
        public void ReportRecycleChange(RecycleActionChange change) => RecycleStateChangedEventHandler?.Invoke(change);

        public static void AddToRadioButtonGroup(Button button, Action? onSelect, Action? onCancel)
        {
            string groupName = nameof(PuzzleZoneManager);
            RadioButtonsGroup.RegisterButton(button, groupName, onSelect, onCancel);
        }

        public static void RemoveFromRadioButtonGroup(Button button)
        {
            RadioButtonsGroup.UnregisterButton(button);
        }

        public override void Init(GameCore game)
        {
            if (_whiteColumn == null || _blackColumn == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            game.GameState.AddListener(this);
            game.AddListener(this);

            HumanPlayerActionCreator.Instance.AddListener<TakePuzzleAction>(this);
            HumanPlayerActionCreator.Instance.AddListener<RecycleAction>(this);

            _whiteColumn.Init(isBlack: false);
            _blackColumn.Init(isBlack: true);

            HumanPlayerActionCreator.RegisterController(this);
        }

        void IGameActionController.SetPlayerMode(PlayerMode mode)
        {
            SetMode(PuzzleZoneMode.Disabled);
        }

        void IGameActionController.SetActionMode(ActionMode mode)
        {
            SetMode(PuzzleZoneMode.Disabled);
        }

        private void SetMode(PuzzleZoneMode mode)
        {
            _whiteColumn!.SetMode(mode, _currentTurnInfo);
            _blackColumn!.SetMode(mode, _currentTurnInfo);
        }

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

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionRequested() => SetMode(PuzzleZoneMode.TakePuzzle);

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionCanceled() => SetMode(PuzzleZoneMode.Disabled);

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionConfirmed() => SetMode(PuzzleZoneMode.Disabled);

        void IHumanPlayerActionListener<RecycleAction>.OnActionRequested() => SetMode(PuzzleZoneMode.Recycle);

        void IHumanPlayerActionListener<RecycleAction>.OnActionCanceled() => SetMode(PuzzleZoneMode.Disabled);

        void IHumanPlayerActionListener<RecycleAction>.OnActionConfirmed() => SetMode(PuzzleZoneMode.Disabled);

        async Task IAIPlayerActionAnimator<TakePuzzleAction>.Animate(TakePuzzleAction action, CancellationToken cancellationToken)
        {
            if (_whiteColumn == null || _blackColumn == null) {
                return;
            }

            float delay = 0.7f;

            // dim all cards
            using (_whiteColumn.CreateColumnDimmer()) {
                using (_blackColumn.CreateColumnDimmer()) {

                    // wait a bit
                    await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    // select the taken puzzle card
                    switch (action.Option) {
                        case TakePuzzleAction.Options.TopWhite:
                            using (_whiteColumn.DeckCard.CreateCardHighlighter()) {
                                await GameAnimationManager.WaitForScaledDelayAsync(delay, cancellationToken);
                            }
                            break;
                        case TakePuzzleAction.Options.TopBlack:
                            using (_blackColumn.DeckCard.CreateCardHighlighter()) {
                                await GameAnimationManager.WaitForScaledDelayAsync(delay, cancellationToken);
                            }
                            break;
                        case TakePuzzleAction.Options.Normal:
                            if (TryGetPuzzleCardWithId(action.PuzzleId!.Value, out var puzzleCard)) {
                                using (puzzleCard!.CreateCardHighlighter()) {
                                    await GameAnimationManager.WaitForScaledDelayAsync(delay, cancellationToken);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        async Task IAIPlayerActionAnimator<RecycleAction>.Animate(RecycleAction action, CancellationToken cancellationToken)
        {
            if (_whiteColumn == null || _blackColumn == null) {
                return;
            }

            // dim all cards - except the deck cards
            using (_whiteColumn.CreateColumnDimmer(shouldDimCoverCard: false)) {
                using (_blackColumn.CreateColumnDimmer(shouldDimCoverCard: false)) {

                    // wait a bit
                    await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    PuzzleColumn column = action.Option == RecycleAction.Options.White ? _whiteColumn : _blackColumn;

                    List<uint> puzzleIds = new();
                    foreach (uint puzzleId in action.Order) {
                        cancellationToken.ThrowIfCancellationRequested();

                        puzzleIds.Add(puzzleId);
                        using (column.CreatePuzzleHighlighter(puzzleIds)) {
                            await GameAnimationManager.WaitForScaledDelayAsync(1f, cancellationToken);
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

        public class TemporarySpriteReplacer : IDisposable
        {
            private Button? _button;
            private SpriteState _originalSpriteState;

            public TemporarySpriteReplacer(Button button, Sprite? tempSprite)
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
