namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;

    /// <summary>
    /// Represents the result of a verification of a <see cref="VerifiableAction"/> by a <see cref="ActionVerifier"/>.
    /// </summary>
    public abstract class VerificationResult
    {
    }

    /// <summary>
    /// Represents a successful verification of a <see cref="VerifiableAction"/> by a <see cref="ActionVerifier"/>.
    /// </summary>
    /// <seealso cref="VerificationResult" />
    public class VerificationSuccess : VerificationResult
    {
    }

    /// <summary>
    /// Represents a failed verification of a <see cref="VerifiableAction"/> by a <see cref="ActionVerifier"/>.
    /// Derived classes should provide a description of the failure in the <see cref="Message"/> property.
    /// </summary>
    /// <seealso cref="VerificationResult" />
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
    /// There are no tetrominos of type <paramref name="shape"/> left in the shared reserve.
    /// </summary>
    /// <param name="shape">The type of the missing tetromino.</param>
    /// <seealso cref="VerificationFailure"/>
    public class TetrominoNotInSharedReserveFail(TetrominoShape shape) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The type of the missing tetromino.
        /// </summary>
        public TetrominoShape Shape => shape;

        /// <summary>
        /// A description of the failure. Specifies the type of the missing tetromino.
        /// </summary>
        public override string Message => $"Tetromino {shape} not in shared reserve";

        #endregion
    }

    /// <summary>
    /// It is not possible to change a tetromino of type <paramref name="oldShape"/> for a tetromino of type <paramref name="newShape"/>.
    /// </summary>
    /// <param name="oldShape">The shape of the tetromino the player wants to return to the shared reserve.</param>
    /// <param name="newShape">The shape of the tetromino the player wants to take from the shared reserve.</param>
    /// <seealso cref="VerificationFailure" />
    public class InvalidTetrominoChangeFail(TetrominoShape oldShape, TetrominoShape newShape) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The shape of the tetromino the player wants to return to the shared reserve.
        /// </summary>
        public TetrominoShape OldTetromino => oldShape;

        /// <summary>
        /// The shape of the tetromino the player wants to take from the shared reserve.
        /// </summary>
        public TetrominoShape NewTetromino => newShape;

        /// <summary>
        /// A description of the failure. Specifies the shapes of the tetrominos the player wants to change.
        /// </summary>
        public override string Message => $"Cannot change {oldShape} for {newShape}";

        #endregion
    }

    /// <summary>
    /// The player doesn't have any tetrominos of type <paramref name="shape"/> in their personal supply.
    /// </summary>
    /// <param name="shape">The shape of the missing tetromino.</param>
    /// <seealso cref="VerificationFailure"/>
    public class TetrominoNotInPersonalSupplyFail(TetrominoShape shape) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The shape of the tetromino.
        /// </summary>
        public TetrominoShape Shape => shape;

        /// <summary>
        /// A description of the failure. Specifies the type of the missing tetromino.
        /// </summary>
        public override string Message => $"Tetromino {shape} not in personal supply";

        #endregion
    }

    /// <summary>
    /// The player tried to recycle the puzzle row with color <paramref name="color"/>, but the number of puzzles to recycle doesn't match the number of puzzles in the row.
    /// </summary>
    /// <param name="expected">The number of puzzles in the row.</param>
    /// <param name="actual">The number of puzzle IDs in the <see cref="RecycleAction.Order"/>.</param>
    /// <param name="color">The color of the row the player tried to recycle.</param>
    /// <seealso cref="VerificationFailure"/>
    public class NumberOfRecycledPuzzlesMismatchFail(int expected, int actual, RecycleAction.Options color) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The number of puzzles in the row.
        /// </summary>
        public int Expected => expected;

        /// <summary>
        /// The number of puzzle IDs in the <see cref="RecycleAction.Order"/>.
        /// </summary>
        public int Actual => actual;

        /// <summary>
        /// The color of the row the player tried to recycle.
        /// </summary>
        public RecycleAction.Options RecyclingColor => color;

        /// <summary>
        /// A description of the failure. Specifies the color of the row, the number of puzzles in the row and the number of puzzles in the <see cref="RecycleAction.Order"/>.
        /// </summary>
        public override string Message => $"There are {expected} puzzles of color '{color}', got {actual}";

        #endregion
    }

    /// <summary>
    /// The player tried to recycle an empty row.
    /// </summary>
    /// <param name="color">The color of the row the player tried to recycle.</param>
    /// <seealso cref="VerificationFailure" />
    public class EmptyRowRecycleFail(RecycleAction.Options color) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The color of the row the player tried to recycle.
        /// </summary>
        public RecycleAction.Options Color => color;

        /// <summary>
        /// A description of the failure. Specifies the color of the row the player tried to recycle.
        /// </summary>
        public override string Message => $"{color} row is empty";

        #endregion
    }

    /// <summary>
    /// There is an ID in <see cref="RecycleAction.Order"/> which doesn't match any puzzle in the row with color <paramref name="color"/>.
    /// </summary>
    /// <param name="id">The ID of the puzzle which doesn't match any puzzle in the row.</param>
    /// <param name="color">The color of the row the player tried to recycle.</param>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleNotInRowFail(uint id, RecycleAction.Options color) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle that doesn't match any puzzle in the row with color <see cref="Color"/>.
        /// </summary>
        public uint Id => id;

        /// <summary>
        /// The color of the row the player tried to recycle.
        /// </summary>
        public RecycleAction.Options Color => color;

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle and the color of the row the player tried to recycle.
        /// </summary>
        public override string Message => $"Puzzle with id {id} is not the {color} row";

        #endregion
    }

    /// <summary>
    /// The player tried to take a puzzle which isn't available.
    /// </summary>
    /// <param name="id">The ID of the puzzle the player tried to take.</param>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleNotAvailableFail(uint id) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle he tried to take.
        /// </summary>
        public uint Id => id;

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle the player tried to take.
        /// </summary>
        public override string Message => $"Puzzle with id {id} is not available";

        #endregion
    }

    /// <summary>
    /// The player specified <see cref="TakePuzzleAction.Options.Normal"/> but <see cref="TakePuzzleAction.PuzzleId"/> was <see langword="null"/>.
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
    /// </summary>
    /// <param name="color">The color of the deck. Either <see cref="TakePuzzleAction.Options.TopBlack"/> or <see cref="TakePuzzleAction.Options.TopWhite"/>.</param>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleDeckIsEmptyFail(TakePuzzleAction.Options color) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The color of the deck. Either <see cref="TakePuzzleAction.Options.TopBlack"/> or <see cref="TakePuzzleAction.Options.TopWhite"/>.
        /// </summary>
        public TakePuzzleAction.Options Color => color;

        /// <summary>
        /// A description of the failure. Specifies the color of the deck which is empty.
        /// </summary>
        public override string Message => $"{Color} puzzle deck is empty";

        #endregion
    }

    /// <summary>
    /// The given <paramref name="configuration"/> is invalid for a tetromino of type <paramref name="shape"/>.
    /// </summary>
    /// <param name="shape">The type of the tetromino.</param>
    /// <param name="configuration">The configuration of the tetromino.</param>
    /// <seealso cref="VerificationFailure"/>
    public class InvalidTetrominoConfigurationFail(TetrominoShape shape, BinaryImage configuration) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The type of the tetromino.
        /// </summary>
        public TetrominoShape Shape => shape;

        /// <summary>
        /// The configuration of the tetromino.
        /// </summary>
        public BinaryImage Configuration => configuration;

        /// <summary>
        /// A description of the failure. Specifies the type of the tetromino and its configuration.
        /// </summary>
        public override string Message => $"Invalid configuration for tetromino of type {shape}:\n{configuration}";

        #endregion
    }

    /// <summary>
    /// The player doesn't have the puzzle with the given ID.
    /// </summary>
    /// <param name="id">The ID of the puzzle.</param>
    /// <seealso cref="VerificationFailure" />
    public class PlayerDoesntHavePuzzleFail(uint id) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle.
        /// </summary>
        public uint Id => id;

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle the player doesn't have.
        /// </summary>
        public override string Message => $"Player doesn't have puzzle with id {id}";

        #endregion
    }

    /// <summary>
    /// The tetromino specified by <paramref name="position"/> cannot be placed into the puzzle with the given ID.
    /// </summary>
    /// <param name="puzzleId">The ID of the puzzle.</param>
    /// <param name="position">The configuration of the tetromino.</param>
    /// <seealso cref="VerificationFailure" />
    public class CannotPlaceTetrominoFail(uint puzzleId, BinaryImage position) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle
        /// </summary>
        public uint PuzzleId => puzzleId;

        /// <summary>
        /// The configuration of the tetromino.
        /// </summary>
        public BinaryImage Position => position;

        /// <summary>
        /// A description of the failure. Specifies the ID of the puzzle and the configuration of the tetromino.
        /// </summary>
        public override string Message => $"Cannot place tetromino on puzzle {puzzleId} at given position";

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
    /// Two of the placements specified by a Master action are to the same puzzle.
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
    /// <param name="shape">The tetromino type the player doesn't have enough tetrominos of.</param>
    /// <param name="owned">The number of tetrominos type <paramref name="shape"/> the player owns.</param>
    /// <param name="used">The number of tetrominos of type <paramref name="shape"/> needed by the <see cref="MasterAction"/>.</param>
    /// <seealso cref="VerificationFailure"/>
    public class MasterActionNotEnoughTetrominosFail(TetrominoShape shape, int owned, int used) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The tetromino type the player doesn't have enough tetrominos of.
        /// </summary>
        public TetrominoShape Shape => shape;

        /// <summary>
        /// The number of tetromino of type <see cref="Shape"/> the player owns.
        /// </summary>
        public int Owned => owned;

        /// <summary>
        /// The number of tetrominos of type <see cref="Shape"/> needed to perform the master action.
        /// </summary>
        public int Used => used;

        /// <summary>
        /// A description of the failure. Specifies the type of the tetromino, the number of tetrominos the player owns and the number of tetrominos needed by the <see cref="MasterAction"/>.
        /// </summary>
        public override string Message => $"Player doesn't have enough {shape} tetrominos. Owned: {owned}, used: {used}";

        #endregion
    }

    /// <summary>
    /// The player used an invalid action when <see cref="GameCore.CurrentGamePhase"/> was <see cref="GamePhase.FinishingTouches"/>. 
    /// The only allowed actions are <see cref="PlaceTetrominoAction"/> and <see cref="EndFinishingTouchesAction"/>.
    /// </summary>
    /// <param name="actionType">The type of the action used.</param>
    /// <seealso cref="VerificationFailure" />
    public class InvalidActionDuringFinishingTouchesFail(Type actionType) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The type of the action used.
        /// </summary>
        public Type ActionType => actionType;

        /// <summary>
        /// A description of the failure. Specifies the type of the action used.
        /// </summary>
        public override string Message => $"Invalid action during finishing touches: {actionType.Name}";

        #endregion
    }

    /// <summary>
    /// The player used the <see cref="EndFinishingTouchesAction"/> during a different phase than <see cref="GamePhase.FinishingTouches"/>.
    /// </summary>
    /// <param name="phase">The game phase the player used the action in.</param>
    /// <seealso cref="VerificationFailure" />
    public class InvalidEndFinishingTouchesActionUseFail(GamePhase phase) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The game phase the player used the action in.
        /// </summary>
        public GamePhase Phase => phase;

        /// <summary>
        /// A description of the failure. Specifies the game phase the player used the action in.
        /// </summary>
        public override string Message => $"{nameof(EndFinishingTouchesAction)} cannot be used during the '{phase}' game phase";

        #endregion
    }

    /// <summary>
    /// The player tried to take a second black puzzle in the same turn during <see cref="GamePhase.EndOfTheGame"/>.
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
