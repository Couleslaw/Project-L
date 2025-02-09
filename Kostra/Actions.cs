namespace Kostra {

    // TODO: zvazit, zda je Visitor pattern vhodny pro tuto situaci
    // + dobre pro rozšiřitelnost - přidávání nových akcí, změna pravidel
    // - ne všechny procesory akcí budou potřebovat všechny akce
    // třeba GameStateActionProcessor nepotřebuje TakeBasicTetrominoAction

    interface IAction {
        public void Accept(IActionProcessor visitor);
    }

    class EndFinishingTouchesAction : IAction { }
    // TODO: alternativa: player.GetAction vrati null
    // nekde se musi zavolat TurnManeger.Signals.PlayerEndedFinishingTouches

    class TakeBasicTetrominoAction : IAction {
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessTakeBasicTetrominoAction(this);
        }
    }
    class ChangeTetrominoAction : IAction {
        public TetrominoShape OldTetromino { get; init; }
        public TetrominoShape NewTetromino { get; init; }
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessChangeTetrominoAction(this);
        }
    }
    class PlaceTetrominoAction : IAction {
        public ITetromino Tetromino { get; init; }
        public Position Position { get; init; }
        public uint PuzzleId { get; init; }
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessPlaceTetrominoAction(this);
        }
    }
    class TakePuzzleAction : IAction {
        public enum Options { TopWhite, TopBlack, Normal }
        public Options Option { get; init; }
        public uint? PuzzleId { get; init; } = null;
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessTakePuzzleAction(this);
        }
    }
    class RecycleAction : IAction {
        public enum Options { White, Black }
        public Options Option { get; init; }
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessRecycleAction(this);
        }
    }
    class MasterAction : IAction {
        public List<PlaceTetrominoAction> TetrominoPlacements { get; init; }
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessMasterAction(this);
        }
    }

    interface IActionProcessor {
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action);
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action);
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action);
        public void ProcessTakePuzzleAction(TakePuzzleAction action);
        public void ProcessRecycleAction(RecycleAction action);
        public void ProcessMasterAction(MasterAction action);
    }
    class GameStateActionProcessor(GameState gameState, TurnManager.Signals signaller) : IActionProcessor {
        private readonly GameState _gameState = gameState;
        private readonly TurnManager.Signals _gameEventSignaller = signaller;

        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action) { }
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action) { }
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action) { }
        public void ProcessMasterAction(MasterAction action) { }

        // IMPORTANT: This method should be called AFTER PlayerStateActionProcessor.ProcessTakePuzzleAction
        public void ProcessTakePuzzleAction(TakePuzzleAction action) {
            switch (action.Option) {
                case TakePuzzleAction.Options.TopWhite:
                    break;
                case TakePuzzleAction.Options.TopBlack:
                    if (_gameState.NumBlackPuzzlesLeft == 0) {
                        _gameEventSignaller.NoCardsLeftInBlackDeck();
                    }
                    break;
                case TakePuzzleAction.Options.Normal:
                    _gameState.RemovePuzzleWithId(action.PuzzleId!.Value);
                    break;
            }
        }
        public void ProcessRecycleAction(RecycleAction action) {
            if (action.Option == RecycleAction.Options.White) {
                _gameState.RecycleWhitePuzzles();
            }
            else {
                _gameState.RecycleBlackPuzzles();
            }
        }
    }
    class PlayerStateActionProcessor(PlayerState playerState, IPuzzleProvider puzzleProvider, TurnManager.Signals signaller) : IActionProcessor {
        private readonly PlayerState _playerState = playerState;
        private readonly IPuzzleProvider _puzzleProvider = puzzleProvider;
        private readonly TurnManager.Signals _gameEventSignaller = signaller;

        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action) {
            _playerState.AddTetromino(TetrominoShape.O1);
        }
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action) {
            _playerState.RemoveTetromino(action.OldTetromino);
            _playerState.AddTetromino(action.NewTetromino);
        }
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action) {
            Puzzle? puzzle = _puzzleProvider.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }
            if (!action.Tetromino.CanBePlacedIn(puzzle, action.Position)) {
                throw new InvalidOperationException("Tetromino cannot be placed at this position");
            }
            action.Tetromino.PlaceIn(puzzle, action.Position);

            if (puzzle.IsFinished) {
                _playerState.Score += puzzle.RewardScore;
                _playerState.AddTetromino(puzzle.RewardTetromino);
                _playerState.RemovePuzzleWithId(puzzle.Id);
            }
        }
        public void ProcessMasterAction(MasterAction action) {
            foreach (var placement in action.TetrominoPlacements) {
                ProcessPlaceTetrominoAction(placement);
            }
        }
        public void ProcessTakePuzzleAction(TakePuzzleAction action) {
            Puzzle? puzzle = null;
            switch (action.Option) {
                case TakePuzzleAction.Options.TopWhite:
                    puzzle = _puzzleProvider.TakeTopWhitePuzzle();
                    break;
                case TakePuzzleAction.Options.TopBlack:
                    puzzle = _puzzleProvider.TakeTopBlackPuzzle();
                    break;
                case TakePuzzleAction.Options.Normal:
                    puzzle = _puzzleProvider.GetPuzzleWithId(action.PuzzleId!.Value);
                    break;
            }
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }

            _playerState.PlaceNewPuzzle(puzzle!);
        }
        public void ProcessRecycleAction(RecycleAction action) { }
    }


    // TODO: tohle se zda neefektivní, protože Draw() neví, co přesně se změnilo --> vykreslí všechno znovu
    // lepší by bylo mít nějaký způsob, jak zjistit, co se změnilo a jen to vykreslit
    // GameStateWithGraphics : GameState
    // metody, co něco mění virtuální a při override zavolat base metodu a pak vykreslit změny
    // plus v graficke verzi musi byt nejake prodlevy/animace, aby to vypadalo dobre

    class GameStateGraphicsProcessor(GameState gameState) : IActionProcessor {
        private readonly GameState.Graphics _gameStateGraphics = new(gameState);

        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action) { }
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action) { }
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action) { }
        public void ProcessMasterAction(MasterAction action) { }
        public void ProcessTakePuzzleAction(TakePuzzleAction action) {
            _gameStateGraphics.Draw();
        }
        public void ProcessRecycleAction(RecycleAction action) {
            _gameStateGraphics.Draw();
        }
    }
    class PlayerStateGraphicsProcessor(PlayerState playerState) : IActionProcessor {
        private readonly PlayerState.Graphics _playerStateGraphics = new(playerState);

        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action) {
            _playerStateGraphics.Draw();
        }
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action) {
            _playerStateGraphics.Draw();
        }
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action) {
            _playerStateGraphics.Draw();
        }
        public void ProcessMasterAction(MasterAction action) {
            _playerStateGraphics.Draw();
        }
        public void ProcessTakePuzzleAction(TakePuzzleAction action) {
            _playerStateGraphics.Draw();
        }
        public void ProcessRecycleAction(RecycleAction action) { }
    }
}