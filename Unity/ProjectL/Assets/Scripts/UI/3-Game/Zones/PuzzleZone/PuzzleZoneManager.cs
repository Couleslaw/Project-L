#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public enum PuzzleZoneMode
    {
        Disabled,
        TakePuzzle,
        Recycle
    }

    public class PuzzleZoneManager : GraphicsManager<PuzzleZoneManager>,
        ICurrentTurnListener, IGameStatePuzzleListener,
        IGameActionController,
        IHumanPlayerActionListener<TakePuzzleAction>, IHumanPlayerActionListener<RecycleAction>
    {
        #region Fields

        [Header("Puzzle columns")]
        [SerializeField] private PuzzleColumn? _whitePuzzleColumn;

        [SerializeField] private PuzzleColumn? _blackPuzzleColumn;

        [Header("Deck cover cards")]
        [SerializeField] private DeckCoverCard? _whiteDeckCoverCard;

        [SerializeField] private DeckCoverCard? _blackDeckCoverCard;

        private TurnInfo _currentTurnInfo;

        #endregion

        #region Methods

        public static void AddToRadioButtonGroup(Button button)
        {
            string groupName = nameof(PuzzleZoneManager);
            RadioButtonsGroup.RegisterButton(button, groupName, onSelect: ActionCreationManager.Instance.ReportStateChanged);
        }

        public static void RemoveFromRadioButtonGroup(Button button)
        {
            RadioButtonsGroup.UnregisterButton(button);
        }

        public override void Init(GameCore game)
        {
            if (_whitePuzzleColumn == null || _blackPuzzleColumn == null ||
                _whiteDeckCoverCard == null || _blackDeckCoverCard == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            game.GameState.AddListener(this);
            game.AddListener(this);

            ActionCreationManager.Instance.AddListener<TakePuzzleAction>(this);
            ActionCreationManager.Instance.AddListener<RecycleAction>(this);

            _whitePuzzleColumn.Init(isBlack: false);
            _blackPuzzleColumn.Init(isBlack: true);
            _whiteDeckCoverCard.Init(isBlack: false);
            _blackDeckCoverCard.Init(isBlack: true);

            SetMode(PuzzleZoneMode.Disabled);
        }

        public void SetPlayerMode(PlayerMode mode)
        {
            SetMode(PuzzleZoneMode.Disabled);
        }

        public void SetActionMode(ActionMode mode)
        {
            SetMode(PuzzleZoneMode.Disabled);
        }

        private void SetMode(PuzzleZoneMode mode)
        {
            _whiteDeckCoverCard!.SetMode(mode, _currentTurnInfo);
            _whitePuzzleColumn!.SetMode(mode, _currentTurnInfo);
            _blackDeckCoverCard!.SetMode(mode, _currentTurnInfo);
            _blackPuzzleColumn!.SetMode(mode, _currentTurnInfo);
        }

        void IGameStatePuzzleListener.OnWhitePuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_whitePuzzleColumn != null) {
                _whitePuzzleColumn[index] = puzzle;
            }
        }

        void IGameStatePuzzleListener.OnBlackPuzzleRowChanged(int index, Puzzle? puzzle)
        {
            if (_blackPuzzleColumn != null) {
                _blackPuzzleColumn[index] = puzzle;
            }
        }

        void IGameStatePuzzleListener.OnBlackPuzzleDeckChanged(int deckSize)
        {
            _blackDeckCoverCard?.SetDeckSize(deckSize);
        }

        void IGameStatePuzzleListener.OnWhitePuzzleDeckChanged(int deckSize)
        {
            _whiteDeckCoverCard?.SetDeckSize(deckSize);
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

        public TakePuzzleAction? GetTakePuzzleAction()
        {
            throw new NotImplementedException();
        }

        public RecycleAction? GetRecycleAction()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
