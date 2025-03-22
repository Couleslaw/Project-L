using Kostra.GameManagers;
using Kostra.GamePieces;

namespace Kostra.GameActions {
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
    /// Represents an action which can be verified.
    /// All subclasses which inherit from this class should be <strong>immutable</strong>. This ensures that the action can not be changed after it has been created and therefore it's <see cref="Status"/> can be trusted.
    /// </summary>
    /// <seealso cref="IAction" />
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
    /// <seealso cref="VerifiableAction" />
    class DoNothingAction : VerifiableAction
    {
        public override void Accept(IActionProcessor visitor) { /*do nothing*/ }
    }

    /// <summary>
    /// Represents the action of ending a player's turn during <see cref="GamePhase.FinishingTouches"/>
    /// </summary>
    /// <seealso cref="VerifiableAction" />
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
    /// <seealso cref="VerifiableAction" />
    class TakePuzzleAction(TakePuzzleAction.Options option, uint? puzzleId=null) : VerifiableAction {
        public enum Options { TopWhite, TopBlack, Normal }
        /// <summary>
        /// From where the player wants to take the puzzle.
        /// </summary>
        public Options Option => option;
        /// <summary>
        /// The ID of the specific puzzle to take, if <see cref="Option"/> is <see cref="Options.Normal"/>
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
    /// <seealso cref="VerifiableAction" />
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
    /// The base class for <see cref="TakeBasicTetrominoAction"/> and <see cref="ChangeTetrominoAction"/> because they are technically the same action, just with different parameters.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    abstract class TetrominoAction : VerifiableAction { }

    /// <summary>
    /// Represents the action of taking a <see cref="TetrominoShape.O1"/> tetromino from the shared reserve.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    class TakeBasicTetrominoAction : TetrominoAction
    {
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessTakeBasicTetrominoAction(this);
        }
    }

    /// <summary>
    /// Represents the action of changing a tetromino for a different one.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    class ChangeTetrominoAction(TetrominoShape oldTetromino, TetrominoShape newTetromino) : TetrominoAction
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
    /// <seealso cref="VerifiableAction" />
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
    /// <seealso cref="VerifiableAction" />
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
}