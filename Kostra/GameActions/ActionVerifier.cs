namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Verifies the validity of actions in the context of the current game state.
    /// </summary>
    /// <param name="gameInfo">The current game state.</param>
    /// <param name="playerInfo">The current player state.</param>
    /// <param name="turnInfo">Info about the current turn.</param>
    internal class ActionVerifier(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
    {
        #region Methods

        /// <summary>
        /// Verifies the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The result of the verification. 
        /// <see cref="VerificationSuccess"/> if the action is valid. 
        /// In case the action is invalid, returns a <see cref="VerificationFailure"/> describing the first wrong thing encountered.</returns>
        public VerificationStatus Verify(VerifiableAction action)
        {
            // if FinishingTouches --> only EndFinishingTouchesAction and PlaceAction are allowed
            if (turnInfo.GamePhase == GamePhase.FinishingTouches) {
                if (action is not EndFinishingTouchesAction && action is not PlaceTetrominoAction) {
                    return new InvalidActionDuringFinishingTouchesFail(action.GetType());
                }
            }

            return action switch {
                DoNothingAction a => new VerificationSuccess(),
                EndFinishingTouchesAction a => VerifyEndFinishingTouchesAction(a),
                TakePuzzleAction a => VerifyTakePuzzleAction(a),
                RecycleAction a => VerifyRecycleAction(a),
                TakeBasicTetrominoAction a => VerifyTakeBasicTetrominoAction(a),
                ChangeTetrominoAction a => VerifyChangeTetrominoAction(a),
                PlaceTetrominoAction a => VerifyPlaceTetrominoAction(a),
                MasterAction a => VerifyMasterAction(a),
                _ => throw new InvalidOperationException("Unknown action type")
            };
        }

        /// <summary>Verifies the end finishing touches action.</summary>
        /// <param name="action">The action</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="VerificationSuccess"/> if the action is used during the <see cref="GamePhase.FinishingTouches"/> game phase.</item>
        ///     <item><see cref="InvalidEndFinishingTouchesActionUseFail"/> otherwise.</item>
        ///   </list>
        /// </returns>
        private VerificationStatus VerifyEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            if (turnInfo.GamePhase == GamePhase.FinishingTouches) {
                return new VerificationSuccess();
            }
            return new InvalidEndFinishingTouchesActionUseFail(turnInfo.GamePhase);
        }

        /// <summary>Verifies the take puzzle action.</summary>
        /// <param name="action">The action</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item> <see cref="PuzzleDeckIsEmptyFail"/> if the player is taking a puzzle from the top of a deck but it is empty.</item>
        ///     <item> <see cref="PuzzleIdIsNullFail"/> if the player wants a specific puzzle but the ID is null.</item>
        ///     <item> <see cref="PuzzleNotAvailableFail"/> if the player wants a specific puzzle but the ID doesn't match any of the available puzzles.</item>
        ///     <item> <see cref="PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail"/> if its <see cref="GamePhase.EndOfTheGame"/> and the player want to take a black puzzle but he already took one this turn.</item>
        ///     <item> <see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        /// <exception cref="InvalidOperationException">Unknown TakePuzzleAction option</exception>
        private VerificationStatus VerifyTakePuzzleAction(TakePuzzleAction action)
        {
            switch (action.Option) {
                case TakePuzzleAction.Options.TopWhite: {
                    return gameInfo.NumWhitePuzzlesLeft == 0
                        ? new PuzzleDeckIsEmptyFail(TakePuzzleAction.Options.TopWhite)
                        : new VerificationSuccess();
                }
                case TakePuzzleAction.Options.TopBlack: {
                    return gameInfo.NumBlackPuzzlesLeft == 0
                        ? new PuzzleDeckIsEmptyFail(TakePuzzleAction.Options.TopBlack)
                        : new VerificationSuccess();
                }
                case TakePuzzleAction.Options.Normal: {
                    if (action.PuzzleId is null)
                        return new PuzzleIdIsNullFail();

                    // find the puzzle
                    foreach (Puzzle puzzle in gameInfo.AvailableWhitePuzzles.Concat(gameInfo.AvailableBlackPuzzles)) {
                        if (puzzle.Id == action.PuzzleId) {
                            // if EndOfTheGame is triggered a player can take only 1 black puzzle per turn
                            if (turnInfo.GamePhase == GamePhase.EndOfTheGame && turnInfo.TookBlackPuzzle && puzzle.IsBlack) {
                                return new PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail();
                            }
                            return new VerificationSuccess();
                        }
                    }
                    // no puzzle matching ID found
                    return new PuzzleNotAvailableFail(action.PuzzleId.Value);
                }
                default: {
                    // should never happen
                    throw new InvalidOperationException("Unknown take puzzle option");
                }
            }
        }

        /// <summary>Verifies the recycle action.</summary>
        /// <param name="action">The action</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="EmptyRowRecycleFail"/> if the player want to recycle an empty row.</item>
        ///     <item><see cref="NumberOfRecycledPuzzlesMismatchFail"/> if the number of puzzles in the row the player wants to recycle doesn't match the number of puzzle IDs given in the recycling order.</item>
        ///     <item><see cref="PuzzleNotInRowFail"/> if a puzzle ID from the recycling order isn't found in the row the player is recycling.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        private VerificationStatus VerifyRecycleAction(RecycleAction action)
        {
            // if recycle white puzzles --> check white puzzle count
            if (action.Option == RecycleAction.Options.White) {
                if (gameInfo.AvailableWhitePuzzles.Length == 0) {
                    return new EmptyRowRecycleFail(RecycleAction.Options.White);
                }
                if (gameInfo.AvailableWhitePuzzles.Length != action.Order.Count) {
                    return new NumberOfRecycledPuzzlesMismatchFail(gameInfo.AvailableWhitePuzzles.Length, action.Order.Count, RecycleAction.Options.White);
                }
            }

            // if recycle black puzzles --> check black puzzle count
            if (action.Option == RecycleAction.Options.Black) {
                if (gameInfo.AvailableBlackPuzzles.Length == 0) {
                    return new EmptyRowRecycleFail(RecycleAction.Options.Black);
                }
                if (gameInfo.AvailableBlackPuzzles.Length != action.Order.Count) {
                    return new NumberOfRecycledPuzzlesMismatchFail(gameInfo.AvailableBlackPuzzles.Length, action.Order.Count, RecycleAction.Options.Black);
                }
            }

            // check if all puzzles are in the correct row
            var puzzlesToCheck = action.Option == RecycleAction.Options.White ? gameInfo.AvailableWhitePuzzles : gameInfo.AvailableBlackPuzzles;
            foreach (Puzzle puzzle in puzzlesToCheck) {
                if (!action.Order.Contains(puzzle.Id)) {
                    return new PuzzleNotInRowFail(puzzle.Id, action.Option);
                }
            }

            return new VerificationSuccess();
        }

        /// <summary>Verifies the take basic tetromino action.</summary>
        /// <param name="action">The action</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="TetrominoNotInSharedReserveFail"/> if there are no <see cref="TetrominoShape.O1"/> tetrominos left in the shared reserve.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        private VerificationStatus VerifyTakeBasicTetrominoAction(TakeBasicTetrominoAction action)
        {
            if (gameInfo.NumTetrominosLeft[(int)TetrominoShape.O1] == 0) {
                return new TetrominoNotInSharedReserveFail(TetrominoShape.O1);
            }
            return new VerificationSuccess();
        }

        /// <summary>Verifies the change tetromino action.</summary>
        /// <param name="action">The action</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="TetrominoNotInPersonalSupplyFail"/> if the player doesn't have the old tetromino.</item>
        ///     <item><see cref="InvalidTetrominoChangeFail"/> if the player can't trade the old tetromino for the new one.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        /// <seealso cref="RewardManager.GetUpgradeOptions(IReadOnlyList{int}, TetrominoShape)"/>
        private VerificationStatus VerifyChangeTetrominoAction(ChangeTetrominoAction action)
        {
            // check if the player has the old tetromino
            if (playerInfo.NumTetrominosOwned[(int)action.OldTetromino] == 0) {
                return new TetrominoNotInPersonalSupplyFail(action.OldTetromino);
            }
            // check if the player can trade the old tetromino for the new one
            var validChanges = RewardManager.GetUpgradeOptions(gameInfo.NumTetrominosLeft, action.OldTetromino);
            if (!validChanges.Contains(action.NewTetromino)) {
                return new InvalidTetrominoChangeFail(action.OldTetromino, action.NewTetromino);
            }

            return new VerificationSuccess();
        }

        /// <summary>Verifies the place tetromino action.</summary>
        /// <param name="action">The action</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="TetrominoNotInPersonalSupplyFail"/> if the player doesn't have the tetromino.</item>
        ///     <item><see cref="InvalidTetrominoConfigurationFail"/> if the submitted configuration is invalid for the shape.</item>
        ///     <item><see cref="PlayerDoesntHavePuzzleFail"/> if the player doesn't have the puzzle.</item>
        ///     <item><see cref="CannotPlaceTetrominoFail"/> if the tetromino can't be placed there.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        /// <seealso cref="TetrominoManager.CompareShapeToImage(TetrominoShape, BinaryImage)"/>
        /// <seealso cref="Puzzle.CanPlaceTetromino(BinaryImage)"/>
        private VerificationStatus VerifyPlaceTetrominoAction(PlaceTetrominoAction action)
        {
            // check if player has the tetromino
            if (playerInfo.NumTetrominosOwned[(int)action.Shape] == 0) {
                return new TetrominoNotInPersonalSupplyFail(action.Shape);
            }
            // check if the submitted configuration is valid for the shape
            if (!TetrominoManager.CompareShapeToImage(action.Shape, action.Position)) {
                return new InvalidTetrominoConfigurationFail(action.Shape, action.Position);
            }
            // check if the player has the puzzle
            Puzzle? puzzle = null;
            foreach (Puzzle p in playerInfo.UnfinishedPuzzles) {
                if (p.Id == action.PuzzleId) {
                    puzzle = p;
                    break;
                }
            }
            if (puzzle is null) {
                return new PlayerDoesntHavePuzzleFail(action.PuzzleId);
            }

            // check if the tetromino can be placed there
            if (!puzzle.CanPlaceTetromino(action.Position)) {
                return new CannotPlaceTetrominoFail(action.PuzzleId, action.Position);
            }

            return new VerificationSuccess();
        }

        /// <summary>Verifies the Master action.</summary>
        /// <param name="action">The action</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="MasterActionAlreadyUsedFail"/> if the player already used the Master action in this turn.</item>
        ///     <item><see cref="MasterActionUniquePlacementFail"/> if two placements are to the same puzzle.</item>
        ///     <item><see cref="MasterActionNotEnoughTetrominosFail"/> if the player doesn't have the tetrominos he wants to place.</item>
        ///     <item>Any verification fail which can occur during <see cref="VerifyPlaceTetrominoAction"/></item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        /// <seealso cref="VerifyPlaceTetrominoAction(PlaceTetrominoAction)"/>
        private VerificationStatus VerifyMasterAction(MasterAction action)
        {
            // check if master action was already used
            if (turnInfo.UsedMasterAction) {
                return new MasterActionAlreadyUsedFail();
            }
            // each placement must be to a different puzzle
            List<uint> puzzleIds = new();
            foreach (PlaceTetrominoAction placement in action.TetrominoPlacements) {
                if (puzzleIds.Contains(placement.PuzzleId)) {
                    return new MasterActionUniquePlacementFail();
                }
                puzzleIds.Add(placement.PuzzleId);
            }
            // player has to have all of the tetrominos
            int[] usedTetrominos = new int[TetrominoManager.NumShapes];
            foreach (PlaceTetrominoAction placement in action.TetrominoPlacements) {
                usedTetrominos[(int)placement.Shape]++;
            }
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                if (playerInfo.NumTetrominosOwned[i] < usedTetrominos[i]) {
                    return new MasterActionNotEnoughTetrominosFail((TetrominoShape)i, playerInfo.NumTetrominosOwned[i], usedTetrominos[i]);
                }
            }

            // each placement must be valid 
            foreach (PlaceTetrominoAction placement in action.TetrominoPlacements) {
                VerificationStatus status = VerifyPlaceTetrominoAction(placement);
                if (status is VerificationFailure) {
                    return status;
                }
            }
            return new VerificationSuccess();
        }

        #endregion
    }

    /* ---------- VERIFICATION STATUS MESSAGES ---------- */

    /// <summary>
    /// Represents the result of the verification.
    /// </summary>
    internal abstract class VerificationStatus
    {
    }

    /// <summary>
    /// Represents a successful verification.
    /// </summary>
    /// <seealso cref="VerificationStatus" />
    internal class VerificationSuccess : VerificationStatus
    {
    }

    /// <summary>
    /// Represents a failed verification and provides a message describing the failure. Derived classes should provide more context.
    /// </summary>
    /// <seealso cref="VerificationStatus" />
    internal abstract class VerificationFailure : VerificationStatus
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
    internal class TetrominoNotInSharedReserveFail(TetrominoShape shape) : VerificationFailure
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
    internal class InvalidTetrominoChangeFail(TetrominoShape oldShape, TetrominoShape newShape) : VerificationFailure
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
    internal class TetrominoNotInPersonalSupplyFail(TetrominoShape shape) : VerificationFailure
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
    internal class NumberOfRecycledPuzzlesMismatchFail(int expected, int actual, RecycleAction.Options color) : VerificationFailure
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
    internal class EmptyRowRecycleFail(RecycleAction.Options color) : VerificationFailure
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
    internal class PuzzleNotInRowFail(uint id, RecycleAction.Options color) : VerificationFailure
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
    internal class PuzzleNotAvailableFail(uint id) : VerificationFailure
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
    internal class PuzzleIdIsNullFail : VerificationFailure
    {
        #region Properties

        public override string Message => "Puzzle id is null";

        #endregion
    }

    /// <summary>
    /// The player specified <see cref="TakePuzzleAction.Options.TopBlack"/> or <see cref="TakePuzzleAction.Options.TopWhite"/> but the specified deck is empty.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    internal class PuzzleDeckIsEmptyFail(TakePuzzleAction.Options color) : VerificationFailure
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
    internal class InvalidTetrominoConfigurationFail(TetrominoShape shape, BinaryImage configuration) : VerificationFailure
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
    internal class PlayerDoesntHavePuzzleFail(uint id) : VerificationFailure
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
    internal class CannotPlaceTetrominoFail(uint puzzleId, BinaryImage position) : VerificationFailure
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
    internal class MasterActionAlreadyUsedFail : VerificationFailure
    {
        #region Properties

        public override string Message => "Master action already used in this turn";

        #endregion
    }

    /// <summary>
    /// Two of the placements specified by a Master action are to the same puzzle.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    internal class MasterActionUniquePlacementFail : VerificationFailure
    {
        #region Properties

        public override string Message => "Each placement must be to a different puzzle";

        #endregion
    }

    /// <summary>
    /// The player doesn't have enough tetrominos needed by a <see cref="MasterAction"/>.
    /// </summary>
    /// <seealso cref="VerificationFailure" />
    internal class MasterActionNotEnoughTetrominosFail(TetrominoShape shape, int owned, int used) : VerificationFailure
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
    internal class InvalidActionDuringFinishingTouchesFail(Type actionType) : VerificationFailure
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
    internal class InvalidEndFinishingTouchesActionUseFail(GamePhase phase) : VerificationFailure
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
    internal class PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail : VerificationFailure
    {
        #region Properties

        public override string Message => "Players can take only 1 black puzzle per round during the EndOfTheGame phase";

        #endregion
    }
}
