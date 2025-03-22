namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;

    /// <summary>
    /// Represents the result of the verification.
    /// </summary>
    public abstract class VerificationStatus
    {
    }

    /// <summary>
    /// Represents a successful verification.
    /// </summary>
    /// <seealso cref="VerificationStatus" />
    public class VerificationSuccess : VerificationStatus
    {
    }

    /// <summary>
    /// Represents a failed verification and provides a message describing the failure. Derived classes should provide more context.
    /// </summary>
    /// <seealso cref="VerificationStatus" />
    public abstract class VerificationFailure : VerificationStatus
    {
        #region Properties

        /// <summary>
        /// Message describing the failure.
        /// </summary>
        public abstract string Message { get; }

        #endregion
    }

    /// <summary>
    /// There are no tetrominos of this shape left in the shared reserve.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class TetrominoNotInSharedReserveFail(TetrominoShape shape) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The shape of the tetromino.
        /// </summary>
        public TetrominoShape Shape => shape;

        public override string Message => $"Tetromino {shape} not in shared reserve";

        #endregion
    }

    /// <summary>
    /// It is not possible to change the old tetromino for the new one.
    /// </summary>
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

        public override string Message => $"Cannot change {oldShape} for {newShape}";

        #endregion
    }

    /// <summary>
    /// The player doesn't have any tetrominos of the given shape.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class TetrominoNotInPersonalSupplyFail(TetrominoShape shape) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The shape of the tetromino.
        /// </summary>
        public TetrominoShape Shape => shape;

        public override string Message => $"Tetromino {shape} not in personal supply";

        #endregion
    }

    /// <summary>
    /// The player tried to recycle the puzzles of the given color, but the number of puzzles to recycle doesn't match the number of puzzles in the row.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
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
        /// The color of the row to recycle.
        /// </summary>
        public RecycleAction.Options RecyclingColor => color;

        public override string Message => $"There are {expected} puzzles of color '{color}', got {actual}";

        #endregion
    }

    /// <summary>
    /// The player tried to recycle an empty row.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class EmptyRowRecycleFail(RecycleAction.Options color) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The color of the row to recycle.
        /// </summary>
        public RecycleAction.Options Color => color;

        public override string Message => $"{color} row is empty";

        #endregion
    }

    /// <summary>
    /// There is an ID in <see cref="RecycleAction.Order"/> which doesn't match any puzzle in specified row.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleNotInRowFail(uint id, RecycleAction.Options color) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle to recycle
        /// </summary>
        public uint Id => id;

        /// <summary>
        /// The color of the row to recycle
        /// </summary>
        public RecycleAction.Options Color => color;

        public override string Message => $"Puzzle with id {id} is not the {color} row";

        #endregion
    }

    /// <summary>
    /// The player tried to take a puzzle which isn't available.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleNotAvailableFail(uint id) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle he tried to take.
        /// </summary>
        public uint Id => id;

        public override string Message => $"Puzzle with id {id} is not available";

        #endregion
    }

    /// <summary>
    /// The player specified <see cref="TakePuzzleAction.Options.Normal"/> but <see cref="TakePuzzleAction.PuzzleId"/> was <c>null</c>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleIdIsNullFail : VerificationFailure
    {
        #region Properties

        public override string Message => "Puzzle id is null";

        #endregion
    }

    /// <summary>
    /// The player specified <see cref="TakePuzzleAction.Options.TopBlack"/> or <see cref="TakePuzzleAction.Options.TopWhite"/> but the specified deck is empty.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PuzzleDeckIsEmptyFail(TakePuzzleAction.Options color) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The color of the deck.
        /// </summary>
        public TakePuzzleAction.Options Color => color;

        public override string Message => $"{Color} puzzle deck is empty";

        #endregion
    }

    /// <summary>
    /// The given configuration doesn't match the given shape.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class InvalidTetrominoConfigurationFail(TetrominoShape shape, BinaryImage configuration) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The given tetromino shape.
        /// </summary>
        public TetrominoShape Shape => shape;

        /// <summary>
        /// The given tetromino configuration.
        /// </summary>
        public BinaryImage Configuration => configuration;

        public override string Message => $"Invalid configuration for tetromino {shape}.";

        #endregion
    }

    /// <summary>
    /// The player doesn't have the puzzle with this ID.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PlayerDoesntHavePuzzleFail(uint id) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle.
        /// </summary>
        public uint Id => id;

        public override string Message => $"Player doesn't have puzzle with id {id}";

        #endregion
    }

    /// <summary>
    /// The specified tetromino cannot be placed into the puzzle with the given ID.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class CannotPlaceTetrominoFail(uint puzzleId, BinaryImage position) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The ID of the puzzle
        /// </summary>
        public uint PuzzleId => puzzleId;

        /// <summary>
        /// The given tetromino configuration.
        /// </summary>
        public BinaryImage Position => position;

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

        public override string Message => "Each placement must be to a different puzzle";

        #endregion
    }

    /// <summary>
    /// The player doesn't have enough tetrominos needed by a <see cref="MasterAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class MasterActionNotEnoughTetrominosFail(TetrominoShape shape, int owned, int used) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The tetromino shape in question.
        /// </summary>
        public TetrominoShape Shape => shape;

        /// <summary>
        /// The number of tetromino of this shape the player owns.
        /// </summary>
        public int Owned => owned;

        /// <summary>
        /// The number of tetrominos of this shape needed to perform the master action.
        /// </summary>
        public int Used => used;

        public override string Message => $"Player doesn't have enough {shape} tetrominos. Owned: {owned}, used: {used}";

        #endregion
    }

    /// <summary>
    /// The player used an invalid action during <see cref="GamePhase.FinishingTouches"/>. 
    /// The only allowed actions are <see cref="PlaceTetrominoAction"/> and <see cref="EndFinishingTouchesAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class InvalidActionDuringFinishingTouchesFail(Type actionType) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The type of the action used.
        /// </summary>
        public Type ActionType => actionType;

        public override string Message => $"Invalid action during finishing touches: {actionType.Name}";

        #endregion
    }

    /// <summary>
    /// The player used the <see cref="EndFinishingTouchesAction"/> during a different phase than <see cref="GamePhase.FinishingTouches"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class InvalidEndFinishingTouchesActionUseFail(GamePhase phase) : VerificationFailure
    {
        #region Properties

        /// <summary>
        /// The game phase the player used the action in.
        /// </summary>
        public GamePhase Phase => phase;

        public override string Message => $"EndFinishingTouchesAction cannot be used druing the '{phase}' gamephase";

        #endregion
    }

    /// <summary>
    /// The player tried to take a second black puzzle in the same turn during <see cref="GamePhase.EndOfTheGame"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    public class PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail : VerificationFailure
    {
        #region Properties

        public override string Message => "Players can take only 1 black puzzle per round during the EndOfTheGame phase";

        #endregion
    }
}
