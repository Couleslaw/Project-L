#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.Sound;
    using ProjectL.UI.Animation;
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

        [SerializeField] private Button? _requestTakePuzzleButton;

        private TurnInfo _currentTurnInfo;

        private event Action<IActionModification<TakePuzzleAction>>? TakePuzzleModifiedEventHandler;
        event Action<IActionModification<TakePuzzleAction>>? IHumanPlayerActionListener<TakePuzzleAction>.ActionModifiedEventHandler {
            add => TakePuzzleModifiedEventHandler += value;
            remove => TakePuzzleModifiedEventHandler -= value;
        }

        private event Action<IActionModification<RecycleAction>>? RecycleModifiedEventHandler;
        event Action<IActionModification<RecycleAction>>? IHumanPlayerActionListener<RecycleAction>.ActionModifiedEventHandler {
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
            if (_whiteColumn == null || _blackColumn == null || _requestTakePuzzleButton == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            game.GameState.AddListener((IGameStatePuzzleListener)this);
            game.AddListener((ICurrentTurnListener)this);

            HumanPlayerActionCreator.Instance.AddListener<TakePuzzleAction>(this);
            HumanPlayerActionCreator.Instance.AddListener<RecycleAction>(this);

            _whiteColumn.Init(isBlack: false);
            _blackColumn.Init(isBlack: true);

            HumanPlayerActionCreator.RegisterController(this);

            _requestTakePuzzleButton.onClick.AddListener(ActionZonesManager.Instance.ManuallyClickTakePuzzleButton);
            EnableRequestTakePuzzleButton(false);
        }

        void IGameActionController.SetPlayerMode(PlayerMode mode)
        {
            if (mode == PlayerMode.NonInteractive) {
                SetMode(PuzzleZoneMode.Disabled);
            }
        }

        void IGameActionController.SetActionMode(ActionMode mode)
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

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionRequested() => SetMode(PuzzleZoneMode.TakePuzzle);

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionCanceled() => SetMode(PuzzleZoneMode.ReadyToTakePuzzle);

        void IHumanPlayerActionListener<TakePuzzleAction>.OnActionConfirmed() => SetMode(PuzzleZoneMode.Disabled);

        void IHumanPlayerActionListener<RecycleAction>.OnActionRequested() => SetMode(PuzzleZoneMode.Recycle);

        void IHumanPlayerActionListener<RecycleAction>.OnActionCanceled() => SetMode(PuzzleZoneMode.ReadyToTakePuzzle);

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
                    await AnimationManager.WaitForScaledDelay(delay, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    // select the taken puzzle card
                    switch (action.Option) {
                        case TakePuzzleAction.Options.TopWhite:
                            using (_whiteColumn.DeckCard.CreateCardHighlighter()) {
                                await AnimationManager.WaitForScaledDelay(delay, cancellationToken);
                            }
                            break;
                        case TakePuzzleAction.Options.TopBlack:
                            using (_blackColumn.DeckCard.CreateCardHighlighter()) {
                                await AnimationManager.WaitForScaledDelay(delay, cancellationToken);
                            }
                            break;
                        case TakePuzzleAction.Options.Normal:
                            if (TryGetPuzzleCardWithId(action.PuzzleId!.Value, out var puzzleCard)) {
                                using (puzzleCard!.CreateCardHighlighter()) {
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

        async Task IAIPlayerActionAnimator<RecycleAction>.Animate(RecycleAction action, CancellationToken cancellationToken)
        {
            if (_whiteColumn == null || _blackColumn == null) {
                return;
            }

            // dim all cards - except the deck cards
            using (_whiteColumn.CreateColumnDimmer(shouldDimCoverCard: false)) {
                using (_blackColumn.CreateColumnDimmer(shouldDimCoverCard: false)) {

                    // wait a bit
                    await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    PuzzleColumn column = action.Option == RecycleAction.Options.White ? _whiteColumn : _blackColumn;

                    List<uint> puzzleIds = new();
                    foreach (uint puzzleId in action.Order) {
                        cancellationToken.ThrowIfCancellationRequested();

                        puzzleIds.Add(puzzleId);
                        using (column.CreatePuzzleHighlighter(puzzleIds)) {
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
