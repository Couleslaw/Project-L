namespace Kostra {
    interface IAction {
        public void Accept(IActionProcessor visitor);
    }

    class EndFinishingTouchesAction : IAction
    {
        public void Accept(IActionProcessor visitor)
        {
            visitor.ProcessEndFinishingTouchesAction(this);
        }
    }
    class TakeBasicTetrominoAction : IAction
    {
        public void Accept(IActionProcessor visitor)
        {
            visitor.ProcessTakeBasicTetrominoAction(this);
        }
    }
    class ChangeTetrominoAction : IAction {
        public TetrominoShape ReturnedTetromino { get; init; }
        public TetrominoShape TakenTetromino { get; init; }
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessChangeTetrominoAction(this);
        }
    }
    class PlaceTetrominoAction : IAction {
        public uint PuzzleId { get; init; }
        public TetrominoShape Tetromino { get; init; }
        public BinaryImage Position { get; init; }
        public bool FinishingTouches { get; init; } = false;
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessPlaceTetrominoAction(this);
        }
    }

    // TODO vyresit jak vybrat odmenu za puzzle

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
        public Queue<uint> Order { get; init; } = new();  // queue for puzzle ids
        public void Accept(IActionProcessor visitor) {
            visitor.ProcessRecycleAction(this);
        }
    }
    class MasterAction : IAction {
        public List<PlaceTetrominoAction> TetrominoPlacements { get; init; } = new();
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
        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action);
    }

    // one of these should be created for each player
    // they share the gamestate and the turnmanager
    class GameActionProcessor(GameCore game, uint playerId, TurnManager.Signals signaller) : IActionProcessor {
        private readonly GameState _gameState = game.GameState;
        private readonly Player _player = game.GetPlayerWithId(playerId);
        private readonly PlayerState _playerState = game.GetPlayerStateWithId(playerId);
        private readonly TurnManager.Signals _gameEventSignaller = signaller;

        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action) {
            _gameState.RemoveTetromino(TetrominoShape.O1);
            _playerState.AddTetromino(TetrominoShape.O1);
        }
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action) {
            _playerState.RemoveTetromino(action.ReturnedTetromino);
            _playerState.AddTetromino(action.TakenTetromino);
        }
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action) {
            Puzzle? puzzle = _gameState.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }
            puzzle.AddTetromino(action.Tetromino, action.Position);

            if (action.FinishingTouches)
            {
                _playerState.Score -= 1;
                if (puzzle.IsFinished)
                {
                    _playerState.FinishPuzzleWithId(puzzle.Id);
                }
                return;
            }

            if (puzzle.IsFinished) {
                _playerState.Score += puzzle.RewardScore;

                var rewardOptions = RewardManager.GetRewardOptions(_gameState.NumTetronimosByShape, puzzle.RewardTetromino);
                TetrominoShape reward;

                if (rewardOptions.Count == 0)
                {
                    throw new InvalidOperationException("No reward options");
                }
                else if (rewardOptions.Count == 1)
                {
                    reward = rewardOptions[0];
                }
                else
                {
                    reward = _player.GetRewardAsync(rewardOptions).Result;
                }

                _playerState.AddTetromino(reward);
                _gameState.RemoveTetromino(reward);

                _playerState.FinishPuzzleWithId(puzzle.Id);
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
                    puzzle = _gameState.TakeTopWhitePuzzle();
                    break;
                case TakePuzzleAction.Options.TopBlack:
                    puzzle = _gameState.TakeTopBlackPuzzle();
                    break;
                case TakePuzzleAction.Options.Normal:
                    puzzle = _gameState.GetPuzzleWithId(action.PuzzleId!.Value);
                    if (puzzle is null) break;

                    _gameState.RemovePuzzleWithId(action.PuzzleId!.Value);
                    _gameState.RefillPuzzles();
                    break;
            }
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }

            _playerState.PlaceNewPuzzle(puzzle!);
        }
        public void ProcessRecycleAction(RecycleAction action) { 
            while (action.Order.Count > 0)
            {
                uint id = action.Order.Dequeue();
                Puzzle? puzzle = _gameState.GetPuzzleWithId(id);
                if (puzzle is null)
                {
                    throw new InvalidOperationException("Puzzle not found");
                }
                _gameState.RemovePuzzleWithId(id);
                _gameState.PutPuzzleToTheBottomOfDeck(puzzle);
            }
        }

        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            _gameEventSignaller.PlayerEndedFinishingTouches();
        }
    }
}