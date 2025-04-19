namespace ProjectLCore.GameActions.Verification
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;

    /// <summary>
    /// Represents the result of a verification of a <see cref="IAction"/> by a <see cref="ActionVerifier"/>.
    /// </summary>
    /// <seealso cref="VerificationSuccess"/>
    /// <seealso cref="VerificationFailure"/>
    public abstract class VerificationResult
    {
    }

    /// <summary>
    /// Represents a successful verification of a <see cref="IAction"/> by a <see cref="ActionVerifier"/>.
    /// </summary>
    /// <seealso cref="VerificationResult" />
    /// <seealso cref="VerificationFailure" />
    public class VerificationSuccess : VerificationResult
    {
    }

    /// <summary>
    /// Represents a failed verification of a <see cref="IAction"/> by a <see cref="ActionVerifier"/>.
    /// Derived classes should provide a description of the failure in the <see cref="Message"/> property.
    /// </summary>
    /// <seealso cref="VerificationResult" />
    /// <seealso cref="VerificationSuccess" />
    public abstract class VerificationFailure : VerificationResult
    {
        #region Properties

        /// <summary>
        /// A description of the failure.
        /// </summary>
        public abstract string Message { get; }

        #endregion
    }

    /// <summary>
    /// The player tried to take a basic tetromino but there are none left in the shared reserve.
    /// This failure can be produced by the <see cref="TakeBasicTetrominoAction"/>.
    /// /// </summary>
    /// <seealso cref="VerificationFailure"/>
    public class BasicTetrominoNotInSharedReserveFail : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// A description of the failure. States that there are no basic tetrominos left in the shared reserve.
        /// </summary>
        public override string Message => $"There are no basic tetrominos left in the shared reserve";

        #endregion
    }

    /// <summary>
    /// The player tried to change an old tetromino for a new one, but this trade is not possible.
    /// This includes the scenario when the new tetromino is the same as the old one.
    /// This failure can be produced by the <see cref="ChangeTetrominoAction"/>.
    /// </summary>

    /// <seealso cref="VerificationFailure" />
    public class InvalidTetrominoChangeFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTetrominoChangeFail"/> class.
        /// </summary>
        /// <param name="oldShape">The shape of the tetromino the player wants to return to the shared reserve.</param>
        /// <param name="newShape">The shape of the tetromino the player wants to take from the shared reserve.</param>
        public InvalidTetrominoChangeFail(TetrominoShape oldShape, TetrominoShape newShape)
        {
            OldTetromino = oldShape;
            NewTetromino = newShape;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The shape of the tetromino the player wants to return to the shared reserve.
        /// </summary>
        public TetrominoShape OldTetromino { get; }

        /// <summary>
        /// The shape of the tetromino the player wants to take from the shared reserve.
        /// </summary>
        public TetrominoShape NewTetromino { get; }

        /// <summary>
        /// A description of the failure. Specifies the shapes of the tetrominos the player wants to change.
        /// </summary>
        public override string Message => $"Cannot change {OldTetromino} for {NewTetromino}";

        #endregion
    }

    /// <summary>
    /// The player tried to use a tetromino but doesn't have any of this shape in their personal supply.
    /// This failure can be produced by the <see cref="ChangeTetrominoAction"/>, <see cref="PlaceTetrominoAction"/> or <see cref="MasterAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure"/>
    public class TetrominoNotInPersonalSupplyFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TetrominoNotInPersonalSupplyFail"/> class.
        /// </summary>
        /// <param name="shape">The shape of the missing tetromino.</param>
        public TetrominoNotInPersonalSupplyFail(TetrominoShape shape)
        {
            Shape = shape;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The shape of the tetromino.
        /// </summary>
        public TetrominoShape Shape { get; }

        /// <summary>
        /// A description of the failure. Specifies the type of the missing tetromino.
        /// </summary>
        public override string Message => $"Tetromino {Shape} not in personal supply";

        #endregion
    }

    /// <summary>
    /// The player tried to recycle a puzzle row, but the number of puzzles to recycle doesn't match the number of puzzles in the row.
    /// This failure can be produced by the <see cref="RecycleAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure"/>
    public class NumberOfRecycledPuzzlesMismatchFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberOfRecycledPuzzlesMismatchFail"/> class.
        /// </summary>
        /// <param name="expected">The number of puzzles in the row.</param>
        /// <param name="actual">The number of puzzle IDs in the <see cref="RecycleAction.Order"/>.</param>
        /// <param name="color">The color of the row the player tried to recycle.</param>
        public NumberOfRecycledPuzzlesMismatchFail(int expected, int actual, RecycleAction.Options color)
        {
            Expected = expected;
            Actual = actual;
            RecycleColor = color;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The number of puzzles in the row.
        /// </summary>
        public int Expected { get; }

        /// <summary>
        /// The number of puzzle IDs in the <see cref="RecycleAction.Order"/>.
        /// </summary>
        public int Actual { get; }

        /// <summary>
        /// The color of the row the player tried to recycle.
        /// </summary>
        public RecycleAction.Options RecycleColor { get; }

        /// <summary>
        /// A description of the failure. Specifies the color of the row, the number of puzzles in the row and the number of puzzles in the <see cref="RecycleAction.Order"/>.
        /// </summary>
        public override string Message => $"There are {Expected} puzzles of color '{RecycleColor}', got {Actual}";

        #endregion
    }

    /// <summary>
    /// The player tried to recycle an empty row.
    /// This failure can be produced by the <see cref="RecycleAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class EmptyRowRecycleFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyRowRecycleFail"/> class.
        /// </summary>
        /// <param name="color">The color of the row the player tried to recycle.</param>
        public EmptyRowRecycleFail(RecycleAction.Options color)
        {
            RecycleColor = color;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The color of the row the player tried to recycle.
        /// </summary>
        public RecycleAction.Options RecycleColor { get; }

        /// <summary>
        /// A description of the failure. Specifies the color of the row the player tried to recycle.
        /// </summary>
        public override string Message => $"{RecycleColor} row is empty";

        #endregion
    }

    /// <summary>
    /// The player tried to recycle a puzzle row, but no puzzle in the row has the <see cref="Id"/> specified in the <see cref="RecycleAction.Order"/>.
    /// This failure can be produced by the <see cref="RecycleAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleNotInRowFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleNotInRowFail"/> class.
        /// </summary>
        /// <param name="id">The ID of the puzzle which doesn't match any puzzle in the row.</param>
        /// <param name="color">The color of the row the player tried to recycle.</param>
        public PuzzleNotInRowFail(uint id, RecycleAction.Options color)
        {
            Id = id;
            Color = color;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the puzzle that doesn't match any puzzle in the row with color <see cref="Color"/>.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// The color of the row the player tried to recycle.
        /// </summary>
        public RecycleAction.Options Color { get; }

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle and the color of the row the player tried to recycle.
        /// </summary>
        public override string Message => $"Puzzle with id {Id} is not the {Color} row";

        #endregion
    }

    /// <summary>
    /// The player tried to take the puzzle with ID <see cref="Id"/>, but no available puzzle has this ID.
    /// This failure can be produced by the <see cref="TakePuzzleAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleNotAvailableFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleNotAvailableFail"/> class.
        /// </summary>
        /// <param name="id">The ID of the puzzle the player tried to take.</param>
        public PuzzleNotAvailableFail(uint id)
        {
            Id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the puzzle he tried to take.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle the player tried to take.
        /// </summary>
        public override string Message => $"Puzzle with id {Id} is not available";

        #endregion
    }

    /// <summary>
    /// The player specified <see cref="TakePuzzleAction.Options.Normal"/> but <see cref="TakePuzzleAction.PuzzleId"/> was <see langword="null"/>.
    /// This failure can be produced by the <see cref="TakePuzzleAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure"/>
    public class PuzzleIdIsNullFail : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// A description of the failure. States that the <see cref="TakePuzzleAction.PuzzleId"/> was <see langword="null"/>.
        /// </summary>
        public override string Message => "Puzzle id is null";

        #endregion
    }

    /// <summary>
    /// The player specified <see cref="TakePuzzleAction.Options.TopBlack"/> or <see cref="TakePuzzleAction.Options.TopWhite"/> but the corresponding deck is empty.
    /// This failure can be produced by the <see cref="TakePuzzleAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleDeckIsEmptyFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleDeckIsEmptyFail"/> class.
        /// </summary>
        /// <param name="color">The color of the deck. Either <see cref="TakePuzzleAction.Options.TopBlack"/> or <see cref="TakePuzzleAction.Options.TopWhite"/>.</param>
        public PuzzleDeckIsEmptyFail(TakePuzzleAction.Options color)
        {
            Color = color;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The color of the deck. Either <see cref="TakePuzzleAction.Options.TopBlack"/> or <see cref="TakePuzzleAction.Options.TopWhite"/>.
        /// </summary>
        public TakePuzzleAction.Options Color { get; }

        /// <summary>
        /// A description of the failure. Specifies the color of the deck which is empty.
        /// </summary>
        public override string Message => $"{Color} puzzle deck is empty";

        #endregion
    }

    /// <summary>
    /// The player tried to place a tetromino, but the given configuration doesn't match its shape.
    /// This failure can be produced by the <see cref="PlaceTetrominoAction"/> or <see cref="MasterAction"/>.
    /// </summary>

    /// <seealso cref="VerificationFailure"/>
    public class InvalidTetrominoConfigurationFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTetrominoConfigurationFail"/> class.
        /// </summary>
        /// <param name="shape">The type of the tetromino.</param>
        /// <param name="configuration">The configuration of the tetromino.</param>
        public InvalidTetrominoConfigurationFail(TetrominoShape shape, BinaryImage configuration)
        {
            Shape = shape;
            Configuration = configuration;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The type of the tetromino.
        /// </summary>
        public TetrominoShape Shape { get; }

        /// <summary>
        /// The configuration of the tetromino.
        /// </summary>
        public BinaryImage Configuration { get; }

        /// <summary>
        /// A description of the failure. Specifies the type of the tetromino and its configuration.
        /// </summary>
        public override string Message => $"Invalid configuration for tetromino of type {Shape}:\n{Configuration}";

        #endregion
    }

    /// <summary>
    /// The player tried to place a tetromino into the puzzle with ID <see cref="Id"/>, but the player doesn't have a puzzle with this ID.
    /// This failure can be produced by the <see cref="PlaceTetrominoAction"/> or <see cref="MasterAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PlayerDoesntHavePuzzleFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerDoesntHavePuzzleFail"/> class.
        /// </summary>
        /// <param name="id">The ID of the puzzle.</param>
        public PlayerDoesntHavePuzzleFail(uint id)
        {
            Id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the puzzle.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle the player doesn't have.
        /// </summary>
        public override string Message => $"Player doesn't have puzzle with id {Id}";

        #endregion
    }

    /// <summary>
    /// The player tried to place a tetromino into a puzzle, but the tetromino cannot be placed there.
    /// This failure can be produced by the <see cref="PlaceTetrominoAction"/> or <see cref="MasterAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class CannotPlaceTetrominoFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotPlaceTetrominoFail"/> class.
        /// </summary>
        /// <param name="puzzleId">The ID of the puzzle.</param>
        /// <param name="configuration">The configuration of the tetromino.</param>
        public CannotPlaceTetrominoFail(uint puzzleId, BinaryImage configuration)
        {
            PuzzleId = puzzleId;
            Configuration = configuration;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the puzzle
        /// </summary>
        public uint PuzzleId { get; }

        /// <summary>
        /// The configuration of the tetromino.
        /// </summary>
        public BinaryImage Configuration { get; }

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle and the configuration of the tetromino.
        /// </summary>
        public override string Message => $"Cannot place tetromino on puzzle {PuzzleId} at given position";

        #endregion
    }

    /// <summary>
    /// The player has already used the <see cref="MasterAction"/> in this turn.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class MasterActionAlreadyUsedFail : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// A description of the failure. States that the player has already used the <see cref="MasterAction"/> in this turn.
        /// </summary>
        public override string Message => "Master action already used in this turn";

        #endregion
    }

    /// <summary>
    /// The player tried to place two tetrominos into the same puzzle using the <see cref="MasterAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class MasterActionUniquePlacementFail : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// A description of the failure. States that each placement must be to a different puzzle.
        /// </summary>
        public override string Message => "Each placement must be to a different puzzle";

        #endregion
    }

    /// <summary>
    /// The player doesn't have enough tetrominos needed by a <see cref="MasterAction"/>.
    /// </summary>
    /// <example>
    /// The player tried to make the following placements:
    /// <list type="table">
    ///     <listheader><term> Puzzle ID</term><term> Used tetromino</term></listheader>
    ///     <item><description> 13</description><description> <see cref="TetrominoShape.L3"/></description></item>
    ///     <item><description> 9</description><description> <see cref="TetrominoShape.O2"/></description></item>
    ///     <item><description> 27</description><description> <see cref="TetrominoShape.L3"/></description></item>
    /// </list>
    /// But the player has only one <see cref="TetrominoShape.L3"/> tetromino, while two are needed.
    /// </example>
    /// <seealso cref="VerificationFailure"/>
    public class MasterActionNotEnoughTetrominosFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterActionNotEnoughTetrominosFail"/> class.
        /// </summary>
        /// <param name="shape">The tetromino type the player doesn't have enough tetrominos of.</param>
        /// <param name="owned">The number of tetrominos type <paramref name="shape"/> the player owns.</param>
        /// <param name="used">The number of tetrominos of type <paramref name="shape"/> needed by the <see cref="MasterAction"/>.</param>
        public MasterActionNotEnoughTetrominosFail(TetrominoShape shape, int owned, int used)
        {
            Shape = shape;
            Owned = owned;
            Used = used;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The tetromino type the player doesn't have enough tetrominos of.
        /// </summary>
        public TetrominoShape Shape { get; }

        /// <summary>
        /// The number of tetromino of type <see cref="Shape"/> the player owns.
        /// </summary>
        public int Owned { get; }

        /// <summary>
        /// The number of tetrominos of type <see cref="Shape"/> needed to perform the master action.
        /// </summary>
        public int Used { get; }

        /// <summary>
        /// A description of the failure. Specifies the type of the tetromino, the number of tetrominos the player owns and the number of tetrominos needed by the <see cref="MasterAction"/>.
        /// </summary>
        public override string Message => $"Player doesn't have enough {Shape} tetrominos. Owned: {Owned}, used: {Used}";

        #endregion
    }

    /// <summary>
    /// The player used an invalid action when <see cref="GameCore.CurrentGamePhase"/> was <see cref="GamePhase.FinishingTouches"/>. 
    /// The only allowed actions are <see cref="PlaceTetrominoAction"/> and <see cref="EndFinishingTouchesAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class InvalidActionDuringFinishingTouchesFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidActionDuringFinishingTouchesFail"/> class.
        /// </summary>
        /// <param name="actionType">The type of the action used.</param>
        public InvalidActionDuringFinishingTouchesFail(Type actionType)
        {
            ActionType = actionType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The type of the action used.
        /// </summary>
        public Type ActionType { get; }

        /// <summary>
        /// A description of the failure. Specifies the type of the action used.
        /// </summary>
        public override string Message => $"Invalid action during finishing touches: {ActionType.Name}";

        #endregion
    }

    /// <summary>
    /// The player used the <see cref="EndFinishingTouchesAction"/> during a different phase than <see cref="GamePhase.FinishingTouches"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class InvalidEndFinishingTouchesActionUseFail : VerificationFailure
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEndFinishingTouchesActionUseFail"/> class.
        /// </summary>
        /// <param name="phase">The game phase the player used the action in.</param>
        public InvalidEndFinishingTouchesActionUseFail(GamePhase phase)
        {
            Phase = phase;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The game phase the player used the action in.
        /// </summary>
        public GamePhase Phase { get; }

        /// <summary>
        /// A description of the failure. Specifies the game phase the player used the action in.
        /// </summary>
        public override string Message => $"{nameof(EndFinishingTouchesAction)} cannot be used during the '{Phase}' game phase";

        #endregion
    }

    /// <summary>
    /// The player tried to take a second black puzzle in the same turn during <see cref="GamePhase.EndOfTheGame"/>.
    /// This failure can be produced by the <see cref="TakePuzzleAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// A description of the failure. States that players can take only one black puzzle per turn during the <see cref="GamePhase.EndOfTheGame"/> game phase.
        /// </summary>
        public override string Message => $"Players can take only one black puzzle per turn during the {GamePhase.EndOfTheGame} game phase";

        #endregion
    }
}
