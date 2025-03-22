namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;

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
        #region Methods

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        public void Accept(IActionProcessor visitor);

        #endregion
    }

    /// <summary>
    /// Represents an action which can be verified.
    /// All subclasses which inherit from this class should be <strong>immutable</strong>. This ensures that the action can not be changed after it has been created and therefore it's <see cref="Status"/> can be trusted.
    /// </summary>
    /// <seealso cref="IAction" />
    internal abstract class VerifiableAction : IAction
    {
        #region Properties

        /// <summary>
        /// Represents the verification status of the action. 
        /// Every action starts as unverified and can be verified by a verifier.
        /// </summary>
        public ActionStatus Status { get; private set; } = ActionStatus.Unverified;

        #endregion

        #region Methods

        public abstract void Accept(IActionProcessor visitor);

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

        #endregion
    }

    /// <summary>
    /// Last resort action for AI players, they should never actually need to use it. It will always be accepted unless the game phase is <see cref="GamePhase.FinishingTouches"/>.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class DoNothingAction : VerifiableAction
    {
        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of ending a player's turn during <see cref="GamePhase.FinishingTouches"/>
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class EndFinishingTouchesAction : VerifiableAction
    {
        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessEndFinishingTouchesAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of taking a puzzle.
    /// Players can take puzzles from the top of the white deck, top of the black deck or a specific puzzle in one of the rows.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class TakePuzzleAction(TakePuzzleAction.Options option, uint? puzzleId = null) : VerifiableAction
    {
        /// <summary>
        /// Possible options for taking a puzzle.
        /// </summary>
        public enum Options { TopWhite, TopBlack, Normal }

        #region Properties

        /// <summary>
        /// From where the player wants to take the puzzle.
        /// </summary>
        public Options Option => option;

        /// <summary>
        /// The ID of the specific puzzle to take, if <see cref="Option"/> is <see cref="Options.Normal"/>
        /// Should be null otherwise.
        /// </summary>
        public uint? PuzzleId => puzzleId;

        #endregion

        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessTakePuzzleAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of recycling puzzles.
    /// The player chooses a row to recycle. The puzzles from the row will be put to the bottom of the deck in the order specified by the player. The puzzle row is then refilled.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class RecycleAction(List<uint> order, RecycleAction.Options option) : VerifiableAction
    {
        #region Fields

        private List<uint> _order = order;

        #endregion

        public enum Options { White, Black }

        #region Properties

        /// <summary>
        /// The color of the row to recycle.
        /// </summary>
        public Options Option => option;

        /// <summary>
        /// Return the order in which the puzzles will be put to the bottom of the deck.
        /// Smaller index means that the puzzle will be recycled earlier.
        /// </summary>
        public IReadOnlyList<uint> Order => _order.AsReadOnly();

        #endregion

        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessRecycleAction(this);
        }

        #endregion
    }

    /// <summary>
    /// The base class for <see cref="TakeBasicTetrominoAction"/> and <see cref="ChangeTetrominoAction"/> because they are technically the same action, just with different parameters.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal abstract class TetrominoAction : VerifiableAction
    {
    }

    /// <summary>
    /// Represents the action of taking a <see cref="TetrominoShape.O1"/> tetromino from the shared reserve.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class TakeBasicTetrominoAction : TetrominoAction
    {
        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessTakeBasicTetrominoAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of changing a tetromino for a different one.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class ChangeTetrominoAction(TetrominoShape oldTetromino, TetrominoShape newTetromino) : TetrominoAction
    {
        #region Properties

        /// <summary>
        /// The tetromino the player is returning to the shared reserve.
        /// </summary>
        public TetrominoShape OldTetromino => oldTetromino;

        /// <summary>
        /// The tetromino the player is taking from the shared reserve.
        /// </summary>
        public TetrominoShape NewTetromino => newTetromino;

        #endregion

        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessChangeTetrominoAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of placing a tetromino on a puzzle.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class PlaceTetrominoAction(uint puzzleId, TetrominoShape shape, BinaryImage position) : VerifiableAction
    {
        #region Properties

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

        #endregion

        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessPlaceTetrominoAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the use of the Master Action.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    internal class MasterAction(List<PlaceTetrominoAction> tetrominoPlacements) : VerifiableAction
    {
        #region Fields

        private readonly List<PlaceTetrominoAction> _tetrominoPlacements = tetrominoPlacements;

        #endregion

        #region Properties

        /// <summary>
        /// The tetrominos placed with the Master Action.
        /// </summary>
        public IReadOnlyList<PlaceTetrominoAction> TetrominoPlacements => _tetrominoPlacements.AsReadOnly();

        #endregion

        #region Methods

        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessMasterAction(this);
        }

        #endregion
    }
}
