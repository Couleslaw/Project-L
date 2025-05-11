namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an action that a player can take during their turn. Together with <see cref="ActionProcessorBase" /> it implements the visitor pattern.
    /// The validity of every action should be checked by an <see cref="ActionVerifier" /> before being processed.
    /// </summary>
    /// <seealso cref="ActionProcessorBase" />
    /// <seealso cref="ActionVerifier" />
    public abstract class GameAction
    {
        #region Methods

        /// <summary>
        /// Accepts the specified visitor by calling the appropriate method.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        public void Accept(ActionProcessorBase visitor)
        {
            visitor.ProcessAction(this);
        }

        /// <summary>
        /// Asynchronously accepts the specified visitor by calling the appropriate method.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AcceptAsync(AsyncActionProcessorBase visitor, CancellationToken cancellationToken = default)
        {
            await visitor.ProcessActionAsync(this, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }

    /// <summary>
    /// Last resort action for AI players, they should never actually need to use it. It will always be accepted unless the game phase is <see cref="GamePhase.FinishingTouches"/>.
    /// </summary>
    /// <seealso cref="GameAction" />
    public class DoNothingAction : GameAction
    {
        #region Methods

        /// <summary>  Converts to string. States that this is a <see cref="DoNothingAction"/>.</summary>
        /// <returns> A <see cref="System.String" /> that represents this instance.  </returns>
        public override string ToString() => $"{nameof(DoNothingAction)}";

        #endregion
    }

    /// <summary>
    /// Represents the action of ending a player's turn during <see cref="GamePhase.FinishingTouches"/>
    /// </summary>
    /// <seealso cref="GameAction" />
    public class EndFinishingTouchesAction : GameAction
    {
        #region Methods

        /// <summary>  Converts to string. States that this is a <see cref="EndFinishingTouchesAction"/>.</summary>
        /// <returns> A <see cref="System.String" /> that represents this instance.  </returns>
        public override string ToString() => $"{nameof(EndFinishingTouchesAction)}";

        #endregion
    }

    /// <summary>
    /// Represents the action of taking a puzzle.
    /// Players can take puzzles from the top of the white deck, top of the black deck or a specific puzzle in one of the rows.
    /// </summary>
    /// <seealso cref="GameAction" />
    public class TakePuzzleAction : GameAction
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TakePuzzleAction"/> class.
        /// </summary>
        /// <param name="option">From where the player wants to take the puzzle.</param>
        /// <param name="puzzleId">The ID of the specific puzzle to take, if <paramref name="option"/> is <see cref="TakePuzzleAction.Options.Normal"/>. Should be null otherwise.</param>
        public TakePuzzleAction(Options option, uint? puzzleId = null)
        {
            Option = option;
            PuzzleId = puzzleId;
        }

        #endregion

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
        public Options Option { get; }

        /// <summary>
        /// The ID of the specific puzzle to take, if <see cref="Option"/> is <see cref="Options.Normal"/>.
        /// Should be null otherwise.
        /// </summary>
        public uint? PuzzleId { get; }

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

        #endregion
    }

    /// <summary>
    /// Represents the action of recycling puzzles.
    /// The player chooses a row to recycle. The puzzles from the row will be put to the bottom of the deck in the order specified by the player. The puzzle row is then refilled.
    /// </summary>
    /// <seealso cref="GameAction" />
    public class RecycleAction : GameAction
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RecycleAction"/> class.
        /// </summary>
        /// <param name="order">The order in which the puzzles will be put to the bottom of the deck. Smaller index means that the puzzle will be recycled earlier.</param>
        /// <param name="option">The color of the row to recycle.</param>
        public RecycleAction(List<uint> order, Options option)
        {
            Order = order.AsReadOnly();
            Option = option;
        }

        #endregion

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
        public Options Option { get; }

        /// <summary>
        /// Return the order in which the puzzles will be put to the bottom of the deck.
        /// Smaller index means that the puzzle will be recycled earlier.
        /// </summary>
        public IReadOnlyList<uint> Order { get; }

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

        #endregion
    }

    /// <summary>
    /// The base class for <see cref="TakeBasicTetrominoAction"/> and <see cref="ChangeTetrominoAction"/> because they are technically the same action, just with different parameters.
    /// </summary>
    /// <seealso cref="GameAction" />
    public abstract class TetrominoAction : GameAction
    {
    }

    /// <summary>
    /// Represents the action of taking a <see cref="TetrominoShape.O1"/> tetromino from the shared reserve.
    /// </summary>
    /// <seealso cref="GameAction" />
    public class TakeBasicTetrominoAction : TetrominoAction
    {
        #region Methods

        /// <summary>Converts to string. States that this is a <see cref="TakeBasicTetrominoAction"/>.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"{nameof(TakeBasicTetrominoAction)}";

        #endregion
    }

    /// <summary>
    /// Represents the action of changing a tetromino for a different one.
    /// </summary>
    /// <seealso cref="GameAction"/>
    public class ChangeTetrominoAction : TetrominoAction
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeTetrominoAction"/> class.
        /// </summary>
        /// <param name="oldTetromino">The tetromino the player is returning to the shared reserve.</param>
        /// <param name="newTetromino">The tetromino the player is taking from the shared reserve.</param>
        public ChangeTetrominoAction(TetrominoShape oldTetromino, TetrominoShape newTetromino)
        {
            OldTetromino = oldTetromino;
            NewTetromino = newTetromino;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The tetromino the player is returning to the shared reserve.
        /// </summary>
        public TetrominoShape OldTetromino { get; }

        /// <summary>
        /// The tetromino the player is taking from the shared reserve.
        /// </summary>
        public TetrominoShape NewTetromino { get; }

        #endregion

        #region Methods

        /// <summary>Converts to string. Specifies the old and new tetrominos.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString() => $"{nameof(ChangeTetrominoAction)}: {OldTetromino}  --->  {NewTetromino}";

        #endregion
    }

    /// <summary>
    /// Represents the action of placing a tetromino on a puzzle.
    /// </summary>
    /// <seealso cref="GameAction" />
    public class PlaceTetrominoAction : GameAction
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceTetrominoAction"/> class.
        /// </summary>
        /// <param name="puzzleId">The ID of the puzzle on which the player wants to place the tetromino.</param>
        /// <param name="shape">The shape of the tetromino the player wants to place.</param>
        /// <param name="position">The position on the puzzle where the player wants to place the tetromino.</param>
        public PlaceTetrominoAction(uint puzzleId, TetrominoShape shape, BinaryImage position)
        {
            PuzzleId = puzzleId;
            Shape = shape;
            Position = position;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the puzzle on which the player wants to place the tetromino.
        /// </summary>
        public uint PuzzleId { get; }

        /// <summary>
        /// The shape of the tetromino the player wants to place.
        /// </summary>
        public TetrominoShape Shape { get; }

        /// <summary>
        /// The position on the puzzle where the player wants to place the tetromino.
        /// </summary>
        public BinaryImage Position { get; }

        #endregion

        #region Methods

        /// <summary>Converts to string. Specifies the shape, puzzle ID and position of the tetromino.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var imageReader = new StringReader(Position.ToString());
            StringBuilder action = new($"{nameof(PlaceTetrominoAction)}: Place shape {Shape} on puzzle with ID={PuzzleId} on position\n");
            for (int lineNum = 0; lineNum < 5; lineNum++) {
                action.Append("  ").Append(imageReader.ReadLine()).Append("   ");
                if (lineNum == 1) {
                    action.Append($"Shape: {Shape}");
                }
                if (lineNum == 2) {
                    action.Append($"ID: {PuzzleId}");
                }
                action.AppendLine();
            }
            return action.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Represents the use of the Master Action.
    /// </summary>
    /// <seealso cref="GameAction" />
    public class MasterAction : GameAction
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterAction"/> class.
        /// </summary>
        /// <param name="tetrominoPlacements">The tetrominos placed with the Master Action.</param>
        public MasterAction(List<PlaceTetrominoAction> tetrominoPlacements)
        {
            TetrominoPlacements = tetrominoPlacements.AsReadOnly();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The tetrominos placed with the Master Action.
        /// </summary>
        public IReadOnlyList<PlaceTetrominoAction> TetrominoPlacements { get; }

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

        #endregion
    }
}
