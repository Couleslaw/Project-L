namespace Kostra {
    enum ActionStatus { Verified, Unverified, Invalid };
    interface IAction {
        public void Accept(IActionProcessor visitor);
    }

    abstract class VerifiableAction : IAction
    {
        public abstract void Accept(IActionProcessor visitor);
        public ActionStatus Status { get; private set; } = ActionStatus.Unverified;
        public VerificationStatus Verify(ActionVerifier verifier)
        {
            var result = verifier.Verify(this);
            Status = result is VerificationSuccess ? ActionStatus.Verified : ActionStatus.Invalid;
            return result;
        }
    }

    class DoNothingAction : VerifiableAction
    {
        // for AI players as a last resort, they should never actually need to use it
        // it will always be accepted unless its the FinishingTouches stage
        public override void Accept(IActionProcessor visitor) { /*do nothing*/ }
    }
    class EndFinishingTouchesAction : VerifiableAction
    {
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessEndFinishingTouchesAction(this);
        }
  
    }
    class TakePuzzleAction(TakePuzzleAction.Options option, uint? puzzleId=null) : VerifiableAction {
        public enum Options { TopWhite, TopBlack, Normal }
        public Options Option => option;
        public uint? PuzzleId => puzzleId;
        public override void Accept(IActionProcessor visitor) {
            visitor.ProcessTakePuzzleAction(this);
        }

    }
    class RecycleAction(List<uint> order, RecycleAction.Options option) : VerifiableAction
    {
        public enum Options { White, Black }
        public Options Option => option;

        // lower index ==> put to the bottom of the deck earlier
        private List<uint> _order = order;
        public IReadOnlyList<uint> Order => _order.AsReadOnly();
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessRecycleAction(this);
        }
    }
    class TakeBasicTetrominoAction : VerifiableAction
    {
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessTakeBasicTetrominoAction(this);
        }

    }
    class ChangeTetrominoAction(TetrominoShape oldTetrimono, TetrominoShape newTetromino) : VerifiableAction 
    {
        public TetrominoShape OldTetromino => oldTetrimono;
        public TetrominoShape NewTetromino => newTetromino;
        public override void Accept(IActionProcessor visitor) {
            visitor.ProcessChangeTetrominoAction(this);

        }
       
    }
    class PlaceTetrominoAction(uint puzzleId, TetrominoShape shape, BinaryImage position) : VerifiableAction
    {
        public uint PuzzleId => puzzleId;
        public TetrominoShape Shape => shape;
        public BinaryImage Position => position;
        // verifier will set finishing touches to true if the action is valid and the GamePhase is FinishingTouches
        public bool FinishingTouches { get; set; } = false;
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessPlaceTetrominoAction(this);
        }

    }
    class MasterAction(List<PlaceTetrominoAction> tetrominoPlacements) : VerifiableAction {

        private List<PlaceTetrominoAction> _tetrominoPlacements = tetrominoPlacements;
        // copy the list to prevent action modification after creation
        public IReadOnlyList<PlaceTetrominoAction> TetrominoPlacements => _tetrominoPlacements.AsReadOnly();
        public override void Accept(IActionProcessor visitor) {
            visitor.ProcessMasterAction(this);
        }
    }



    interface IActionProcessor {
        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action);
        public void ProcessTakePuzzleAction(TakePuzzleAction action);
        public void ProcessRecycleAction(RecycleAction action);
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action);
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action);
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action);
        public void ProcessMasterAction(MasterAction action);
    }

    // one of these should be created for each player
    // they share the gamestate and the turnmanager
    class GameActionProcessor(GameCore game, uint playerId, TurnManager.Signals signaller) : IActionProcessor {
        private readonly GameState _gameState = game.GameState;
        private readonly Player _player = game.GetPlayerWithId(playerId);
        private readonly PlayerState _playerState = game.GetPlayerStateWithId(playerId);
        private readonly TurnManager.Signals _gameEventSignaller = signaller;

        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            _gameEventSignaller.PlayerEndedFinishingTouches();
        }
        public void ProcessTakePuzzleAction(TakePuzzleAction action)
        {
            Puzzle? puzzle = null;
            switch (action.Option)
            {
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
            if (puzzle is null)
            {
                throw new InvalidOperationException("Puzzle not found");
            }

            _playerState.PlaceNewPuzzle(puzzle!);
        }
        public void ProcessRecycleAction(RecycleAction action) { 
            foreach (var id in action.Order)
            {
                Puzzle? puzzle = _gameState.GetPuzzleWithId(id);
                if (puzzle is null)
                {
                    throw new InvalidOperationException("Puzzle not found");
                }
                _gameState.RemovePuzzleWithId(id);
                _gameState.PutPuzzleToTheBottomOfDeck(puzzle);
            }
        }
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action) {
            _gameState.RemoveTetromino(TetrominoShape.O1);
            _playerState.AddTetromino(TetrominoShape.O1);
        }
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action) {
            // remove old tetromino
            _playerState.RemoveTetromino(action.OldTetromino);
            _gameState.AddTetromino(action.OldTetromino);
            // add new tetromino
            _playerState.AddTetromino(action.NewTetromino);
            _gameState.RemoveTetromino(action.NewTetromino);
        }
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action) {
            Puzzle? puzzle = _gameState.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }
            puzzle.AddTetromino(action.Shape, action.Position);

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

                var rewardOptions = RewardManager.GetRewardOptions(_gameState.NumTetrominosLeft, puzzle.RewardTetromino);
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
                    // if the chosen reward isnt valid, pick the first one
                    if (!rewardOptions.Contains(reward))
                    {
                        reward = rewardOptions[0];
                    }
                }

                // give player his reward
                _playerState.AddTetromino(reward);
                _gameState.RemoveTetromino(reward);

                // return him the used pieces
                foreach (var tetromino in puzzle.GetUsedTetrominos())
                {
                    _playerState.AddTetromino(tetromino);
                }

                _playerState.FinishPuzzleWithId(puzzle.Id);
            }
        }
        public void ProcessMasterAction(MasterAction action) {
            foreach (var placement in action.TetrominoPlacements) {
                ProcessPlaceTetrominoAction(placement);
            }
        }
    }
}