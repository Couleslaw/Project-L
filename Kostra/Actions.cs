namespace Kostra {
    /// <summary>
    /// Represents the verification status of an action.
    /// </summary>
    enum ActionStatus 
    {
        /// <summary>
        /// The action has been verified and is valid.
        /// </summary>
        Verified,
        /// <summary>
        /// The action hasn't been verified yet.
        /// </summary>
        Unverified,
        /// <summary>
        /// The action has been verified and is invalid.
        /// </summary>
        FailedVerification
    };

    /// <summary>
    /// Interface for the visitor pattern.
    /// </summary>
    /// <seealso cref="IActionProcessor"/>
    interface IAction 
    {
        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        public void Accept(IActionProcessor visitor);
    }

    /// <summary>
    /// An interface for processing actions using the visitor pattern.
    /// Each action should be verified before being processed.
    /// </summary>
    /// <seealso cref="IAction"/>
    interface IActionProcessor
    {
        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action);
        public void ProcessTakePuzzleAction(TakePuzzleAction action);
        public void ProcessRecycleAction(RecycleAction action);
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action);
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action);
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action);
        public void ProcessMasterAction(MasterAction action);
    }

    /// <summary>
    /// Represents an action which can be verified.
    /// All subclasses which inherit from this class should be <strong>immutable</strong>. This ensures that the action can not be changed after it has been created and therefore it's <see cref="VerifiableAction.Status"/> can be trusted.
    /// </summary>
    /// <seealso cref="Kostra.IAction" />
    abstract class VerifiableAction : IAction
    {
        public abstract void Accept(IActionProcessor visitor);
        /// <summary>
        /// Represents the verification status of the action. 
        /// Every action starts as unverified and can be verified by a verifier.
        /// </summary>
        public ActionStatus Status { get; private set; } = ActionStatus.Unverified;

        /// <summary>
        /// Accepts a verifier, updates the verification status and return the result of the verification.
        /// </summary>
        /// <returns>The result of the verification</returns>
        public VerificationStatus GetVerifiedBy(ActionVerifier verifier)
        {
            var result = verifier.Verify(this);
            Status = result is VerificationSuccess ? ActionStatus.Verified : ActionStatus.FailedVerification;
            return result;
        }
    }

    /// <summary>
    /// Last resort action for AI players, they should never actually need to use it. It will always be accepted unless the game phase is <see cref="GamePhase.FinishingTouches"/>.
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class DoNothingAction : VerifiableAction
    {
        public override void Accept(IActionProcessor visitor) { /*do nothing*/ }
    }

    /// <summary>
    /// Represents the action of ending a player's turn during <see cref="GamePhase.FinishingTouches"/>
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class EndFinishingTouchesAction : VerifiableAction
    {
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessEndFinishingTouchesAction(this);
        }
    }

    /// <summary>
    /// Represents the action of taking a puzzle.
    /// Players can take puzzles from the top of the white deck, top of the black deck or a specific puzzle in one of the rows.
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class TakePuzzleAction(TakePuzzleAction.Options option, uint? puzzleId=null) : VerifiableAction {
        public enum Options { TopWhite, TopBlack, Normal }
        /// <summary>
        /// From where the player wants to take the puzzle.
        /// </summary>
        public Options Option => option;
        /// <summary>
        /// The ID of the specific puzzle to take, if <see cref="TakePuzzleAction.Option"/> is <see cref="TakePuzzleAction.Options.Normal"/>
        /// Should be null otherwise.
        /// </summary>
        public uint? PuzzleId => puzzleId;
        public override void Accept(IActionProcessor visitor) {
            visitor.ProcessTakePuzzleAction(this);
        }
    }

    /// <summary>
    /// Represents the action of recycling puzzles.
    /// The player chooses a row to recycle. The puzzles from the row will be put to the bottom of the deck in the order specified by the player. The puzzle row is then refilled.
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class RecycleAction(List<uint> order, RecycleAction.Options option) : VerifiableAction
    {
        public enum Options { White, Black }
        /// <summary>
        /// The color of the row to recycle.
        /// </summary>
        public Options Option => option;

        private List<uint> _order = order;
        /// <summary>
        /// Return the order in which the puzzles will be put to the bottom of the deck.
        /// Smaller index means that the puzzle will be recycled earlier.
        /// </summary>
        public IReadOnlyList<uint> Order => _order.AsReadOnly();
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessRecycleAction(this);
        }
    }

    /// <summary>
    /// Represents the action of taking a <see cref="TetrominoShape.O1"/> tetromino from the shared reserve.
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class TakeBasicTetrominoAction : VerifiableAction
    {
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessTakeBasicTetrominoAction(this);
        }
    }

    /// <summary>
    /// Represents the action of changing a tetromino for a different one.
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class ChangeTetrominoAction(TetrominoShape oldTetromino, TetrominoShape newTetromino) : VerifiableAction 
    {
        /// <summary>
        /// The tetromino the player is returning to the shared reserve.
        /// </summary>
        public TetrominoShape OldTetromino => oldTetromino;
        /// <summary>
        /// The tetromino the player is taking from the shared reserve.
        /// </summary>
        public TetrominoShape NewTetromino => newTetromino;
        public override void Accept(IActionProcessor visitor) {
            visitor.ProcessChangeTetrominoAction(this);

        }
    }

    /// <summary>
    /// Represents the action of placing a tetromino on a puzzle.
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class PlaceTetrominoAction(uint puzzleId, TetrominoShape shape, BinaryImage position) : VerifiableAction
    {
        /// <summary>
        /// The ID of the puzzle on which the player wants to place the tetromino.
        /// </summary>
        public uint PuzzleId => puzzleId;
        /// <summary>
        /// The shape of the tetromino the player wants to place.
        /// </summary>
        public TetrominoShape Shape => shape;
        /// <summary>
        /// The position on the puzzle where the player wants to place the tetromino.
        /// </summary>
        public BinaryImage Position => position;
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessPlaceTetrominoAction(this);
        }
    }

    /// <summary>
    /// Represents the use of the Master Action.
    /// </summary>
    /// <seealso cref="Kostra.VerifiableAction" />
    class MasterAction(List<PlaceTetrominoAction> tetrominoPlacements) : VerifiableAction {

        private List<PlaceTetrominoAction> _tetrominoPlacements = tetrominoPlacements;
        /// <summary>
        /// The tetrominos placed with the Master Action.
        /// </summary>
        public IReadOnlyList<PlaceTetrominoAction> TetrominoPlacements => _tetrominoPlacements.AsReadOnly();
        public override void Accept(IActionProcessor visitor) {
            visitor.ProcessMasterAction(this);
        }
    }


    /// <summary>
    /// A class for processing player actions in the game. One instance should be created for each player.
    /// </summary>
    /// <seealso cref="Kostra.IActionProcessor" />
    class GameActionProcessor(GameCore game, uint playerId, TurnManager.Signals signaler) : IActionProcessor {
        private readonly GameState _gameState = game.GameState;
        private readonly Player _player = game.GetPlayerWithId(playerId);
        private readonly PlayerState _playerState = game.GetPlayerStateWithId(playerId);

        /// <summary>
        /// Processes the end finishing touches action.
        /// </summary>
        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            signaler.PlayerEndedFinishingTouches();
        }

        /// <summary>
        ///   <para>
        /// Processes the take puzzle action.
        /// </para>
        ///   <list type="number">
        ///     <item>
        /// Removes the puzzle from the <see cref="GameState"/> (throws an exception if the puzzle is not found)</item>
        ///     <item>Adds the puzzle to the <see cref="PlayerState"/>
        /// </item>
        ///   </list>
        ///   <para>
        /// Signals if the player took a black puzzle. Also signals if the black deck is empty.</para>
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="System.InvalidOperationException">Puzzle not found</exception>
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

            // signal if the player took a black puzzle
            if (puzzle.IsBlack)
            {
                signaler.PlayerTookBlackPuzzle();
            }
            // signal if the black deck is empty
            if (_gameState.NumBlackPuzzlesLeft == 0)
            {
                signaler.BlackDeckIsEmpty();
            }
            _playerState.PlaceNewPuzzle(puzzle!);
        }

        /// <summary>
        /// Processes the recycle action. Raises an exception if the puzzle is not found.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="System.InvalidOperationException">Puzzle not found</exception>
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
            _gameState.RefillPuzzles();
        }

        /// <summary>
        /// Processes the take basic tetromino action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action) {
            _gameState.RemoveTetromino(TetrominoShape.O1);
            _playerState.AddTetromino(TetrominoShape.O1);
        }

        /// <summary>
        /// Processes the change tetromino action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action) {
            // remove old tetromino
            _playerState.RemoveTetromino(action.OldTetromino);
            _gameState.AddTetromino(action.OldTetromino);
            // add new tetromino
            _playerState.AddTetromino(action.NewTetromino);
            _gameState.RemoveTetromino(action.NewTetromino);
        }

        /// <summary>
        /// Processes the place tetromino action. Adds the tetromino to the puzzle. If the puzzle is finished, the player gets a reward and the used tetrominos back.
        /// During <see cref="GamePhase.FinishingTouches"/>, the player doesn't get any rewards and this action costs 1 score.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="System.InvalidOperationException">Puzzle not found</exception>
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action) {
            Puzzle? puzzle = _gameState.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }
            puzzle.AddTetromino(action.Shape, action.Position);

            // handle FinishingTouches separately
            if (game.CurrentGamePhase == GamePhase.FinishingTouches)
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

                // give the player the reward tetromino
                var rewardOptions = RewardManager.GetRewardOptions(_gameState.NumTetrominosLeft, puzzle.RewardTetromino);

                // if there are no reward options, the player doesn't get anything

                if (rewardOptions.Count >= 1)
                {
                    TetrominoShape reward;

                    if (rewardOptions.Count == 1)
                    {
                        reward = rewardOptions[0];
                    }
                    else
                    {
                        reward = _player.GetRewardAsync(rewardOptions).Result;
                        // if the chosen reward isn't valid, pick the first one
                        if (!rewardOptions.Contains(reward))
                        {
                            reward = rewardOptions[0];
                        }
                    }

                    // give player his reward
                    _playerState.AddTetromino(reward);
                    _gameState.RemoveTetromino(reward);
                }

                // return the used pieces to the player
                foreach (var tetromino in puzzle.GetUsedTetrominos())
                {
                    _playerState.AddTetromino(tetromino);
                }

                // remove the puzzle from the player's state
                _playerState.FinishPuzzleWithId(puzzle.Id);
            }
        }

        /// <summary>
        /// Processes the master action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ProcessMasterAction(MasterAction action) {
            signaler.PlayerUsedMasterAction();

            foreach (var placement in action.TetrominoPlacements) {
                ProcessPlaceTetrominoAction(placement);
            }
        }
    }
}