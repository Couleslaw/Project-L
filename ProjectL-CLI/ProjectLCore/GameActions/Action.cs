namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using ProjectLCore.GameActions.Verification;
    using System.Text;

    /// <summary>
    /// Represents the verification status of an action.
    /// </summary>
    public enum ActionStatus
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
    public interface IAction
    {
        #region Methods

        /// <summary>
        /// Accepts the specified visitor by calling the appropriate method.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public void Accept(IActionProcessor visitor);

        #endregion
    }

    /// <summary>
    /// Represents an action which can be verified.
    /// All subclasses which inherit from this class should be <strong>immutable</strong>. This ensures that the action can not be changed after it has been created and therefore it's <see cref="Status"/> can be trusted.
    /// </summary>
    /// <seealso cref="IAction"/>
    public abstract class VerifiableAction : IAction
    {
        #region Properties

        /// <summary>
        /// Represents the verification status of the action. 
        /// Every action starts as unverified and can be verified by a verifier.
        /// </summary>
        public ActionStatus Status { get; private set; } = ActionStatus.Unverified;

        #endregion

        #region Methods

        /// <summary>
        /// Accepts the specified visitor by calling the appropriate method.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public abstract void Accept(IActionProcessor visitor);

        /// <summary>
        /// Accepts a verifier, updates the verification status and return the result of the verification.
        /// </summary>
        /// <param name="verifier">The verifier.</param>
        /// <returns>The result of the verification</returns>
        public VerificationResult GetVerifiedBy(ActionVerifier verifier)
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
    public class DoNothingAction : VerifiableAction
    {
        #region Methods

        /// <summary>  Converts to string. States that this is a <see cref="DoNothingAction"/>.</summary>
        /// <returns> A <see cref="System.String" /> that represents this instance.  </returns>
        public override string ToString() => $"{nameof(DoNothingAction)}";

        /// <summary>
        /// Does nothing, but implements the visitor pattern.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public override void Accept(IActionProcessor visitor)
        {
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of ending a player's turn during <see cref="GamePhase.FinishingTouches"/>
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    public class EndFinishingTouchesAction : VerifiableAction
    {
        #region Methods

        /// <summary>  Converts to string. States that this is a <see cref="EndFinishingTouchesAction"/>.</summary>
        /// <returns> A <see cref="System.String" /> that represents this instance.  </returns>
        public override string ToString() => $"{nameof(EndFinishingTouchesAction)}";

        /// <summary>
        /// Accepts the specified visitor by calling <see cref="IActionProcessor.ProcessEndFinishingTouchesAction"/>.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
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
    /// <param name="option">From where the player wants to take the puzzle.</param>
    /// <param name="puzzleId">The ID of the specific puzzle to take, if <paramref name="option"/> is <see cref="TakePuzzleAction.Options.Normal"/>. Should be null otherwise.</param>
    /// <seealso cref="VerifiableAction" />
    public class TakePuzzleAction(TakePuzzleAction.Options option, uint? puzzleId = null) : VerifiableAction
    {
        /// <summary>
        /// Possible options for taking a puzzle.
        /// </summary>
        public enum Options
        {
            /// <summary> Take the top puzzle from the white deck. </summary>
            TopWhite,
            /// <summary> Take the top puzzle from the black deck. </summary>
            TopBlack,
            /// <summary> Take a specific puzzle from one of the puzzle rows. </summary>
            Normal
        }

        #region Properties

        /// <summary>
        /// From where the player wants to take the puzzle.
        /// </summary>
        public Options Option => option;

        /// <summary>
        /// The ID of the specific puzzle to take, if <see cref="Option"/> is <see cref="Options.Normal"/>.
        /// Should be null otherwise.
        /// </summary>
        public uint? PuzzleId => puzzleId;

        #endregion

        #region Methods

        /// <summary>
        /// Converts to string. States the action the player wants to take. E.g. "Take top white puzzle".
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string action = Option switch {
                Options.TopWhite => "Take top white puzzle",
                Options.TopBlack => "Take top black puzzle",
                Options.Normal => $"Take puzzle with ID={PuzzleId}",
                _ => "Invalid option"
            };
            return $"{nameof(TakePuzzleAction)}: {action}";
        }

        /// <summary>
        /// Accepts the specified visitor by calling <see cref="IActionProcessor.ProcessTakePuzzleAction"/>.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
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
    /// <param name="order">The order in which the puzzles will be put to the bottom of the deck. Smaller index means that the puzzle will be recycled earlier.</param>
    /// <param name="option">The color of the row to recycle.</param>
    /// <seealso cref="VerifiableAction" />
    public class RecycleAction(List<uint> order, RecycleAction.Options option) : VerifiableAction
    {
        /// <summary>
        /// Player can choose to recycle the white or the black row.
        /// </summary>
        public enum Options
        {
            /// <summary> Recycle the white row. </summary>
            White,
            /// <summary> Recycle the black row. </summary>
            Black
        }

        #region Properties

        /// <summary>
        /// The color of the row to recycle.
        /// </summary>
        public Options Option => option;

        /// <summary>
        /// Return the order in which the puzzles will be put to the bottom of the deck.
        /// Smaller index means that the puzzle will be recycled earlier.
        /// </summary>
        public IReadOnlyList<uint> Order { get; } = order.AsReadOnly();

        #endregion

        #region Methods

        /// <summary>
        /// Converts to string. States the color of the row to recycle and the order in which the puzzles will be recycled.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string color = Option == Options.White ? "white" : "black";
            string orderString = string.Join(", ", Order);
            return $"{nameof(RecycleAction)}: Recycle {color} row in order: {orderString}";
        }

        /// <summary>
        /// Accepts the specified visitor by calling <see cref="IActionProcessor.ProcessRecycleAction"/>.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
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
    public abstract class TetrominoAction : VerifiableAction
    {
    }

    /// <summary>
    /// Represents the action of taking a <see cref="TetrominoShape.O1"/> tetromino from the shared reserve.
    /// </summary>
    /// <seealso cref="VerifiableAction" />
    public class TakeBasicTetrominoAction : TetrominoAction
    {
        #region Methods

        /// <summary>Converts to string. States that this is a <see cref="TakeBasicTetrominoAction"/>.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"{nameof(TakeBasicTetrominoAction)}";

        /// <summary>
        /// Accepts the specified visitor by calling <see cref="IActionProcessor.ProcessTakeBasicTetrominoAction"/>.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessTakeBasicTetrominoAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of changing a tetromino for a different one.
    /// </summary>
    /// <param name="oldTetromino">The tetromino the player is returning to the shared reserve.</param>
    /// <param name="newTetromino">The tetromino the player is taking from the shared reserve.</param>
    /// <seealso cref="VerifiableAction"/>
    public class ChangeTetrominoAction(TetrominoShape oldTetromino, TetrominoShape newTetromino) : TetrominoAction
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

        /// <summary>Converts to string. Specifies the old and new tetrominos.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"{nameof(ChangeTetrominoAction)}: {OldTetromino}  --->  {NewTetromino}";

        /// <summary>
        /// Accepts the specified visitor by calling <see cref="IActionProcessor.ProcessChangeTetrominoAction"/>.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessChangeTetrominoAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the action of placing a tetromino on a puzzle.
    /// </summary>
    /// <param name="puzzleId">The ID of the puzzle on which the player wants to place the tetromino.</param>
    /// <param name="shape">The shape of the tetromino the player wants to place.</param>
    /// <param name="position">The position on the puzzle where the player wants to place the tetromino.</param>
    /// <seealso cref="VerifiableAction" />
    public class PlaceTetrominoAction(uint puzzleId, TetrominoShape shape, BinaryImage position) : VerifiableAction
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

        /// <summary>Converts to string. Specifies the shape, puzzle ID and position of the tetromino.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var imageReader = new StringReader(position.ToString());
            StringBuilder action = new($"{nameof(PlaceTetrominoAction)}: Place shape {shape} on puzzle with ID={puzzleId} on position\n");
            for (int lineNum = 0; lineNum < 5; lineNum++) {
                action.Append("  ").Append(imageReader.ReadLine()).Append("   ");
                if (lineNum == 1) {
                    action.Append($"Shape: {shape}");
                }
                if (lineNum == 2) {
                    action.Append($"ID: {puzzleId}");
                }
                action.AppendLine();
            }
            return action.ToString();
        }

        /// <summary>
        /// Accepts the specified visitor by calling <see cref="IActionProcessor.ProcessPlaceTetrominoAction"/>.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessPlaceTetrominoAction(this);
        }

        #endregion
    }

    /// <summary>
    /// Represents the use of the Master Action.
    /// </summary>
    /// <param name="tetrominoPlacements">The tetrominos placed with the Master Action.</param>
    /// <seealso cref="VerifiableAction" />
    public class MasterAction(List<PlaceTetrominoAction> tetrominoPlacements) : VerifiableAction
    {
        #region Properties

        /// <summary>
        /// The tetrominos placed with the Master Action.
        /// </summary>
        public IReadOnlyList<PlaceTetrominoAction> TetrominoPlacements { get; } = tetrominoPlacements.AsReadOnly();

        #endregion

        #region Methods

        /// <summary> Converts to string. States that this is a <see cref="MasterAction"/> and lists the tetromino placements. </summary>
        /// <returns> A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            string action = $"{nameof(MasterAction)} with placements:\n";
            for (int i = 0; i < TetrominoPlacements.Count; i++) {
                action += $"{i + 1}. " + TetrominoPlacements[i].ToString();
            }
            return action;
        }

        /// <summary>
        /// Accepts the specified visitor by calling <see cref="IActionProcessor.ProcessMasterAction"/>.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public override void Accept(IActionProcessor visitor)
        {
            visitor.ProcessMasterAction(this);
        }

        #endregion
    }
}
