namespace ProjectLCore.GameActions.Verification
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Verifies the validity of actions made by a player in the context of the current game state.
    /// </summary>
    /// <seealso cref="IAction"/>
    /// <seealso cref="VerificationResult"/>
    /// <seealso cref="GameActionProcessor"/>
    public class ActionVerifier
    {
        #region Fields

        private readonly GameState.GameInfo _gameInfo;

        private readonly PlayerState.PlayerInfo _playerInfo;

        private readonly TurnInfo _turnInfo;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionVerifier"/> class.
        /// </summary>
        /// <param name="gameInfo">Information about the current state of the game.</param>
        /// <param name="playerInfo">Information about the tetrominos and puzzles owned by the player who makes the actions.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        public ActionVerifier(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
        {
            _gameInfo = gameInfo;
            _playerInfo = playerInfo;
            _turnInfo = turnInfo;
        }

        #endregion

        #region Methods

        /// <summary> Verifies the given <see cref="IAction"/>. </summary>
        /// <param name="action">The action to verify.</param>
        /// <returns>The result of the verification. 
        /// <see cref="VerificationSuccess"/> if the action is valid. 
        /// In case the action is invalid, returns a <see cref="VerificationFailure"/> describing the first issue encountered.
        /// </returns>
        public VerificationResult Verify(IAction action)
        {
            // if FinishingTouches --> only EndFinishingTouchesAction and PlaceAction are allowed
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches) {
                if (action is not EndFinishingTouchesAction && action is not PlaceTetrominoAction) {
                    return new InvalidActionDuringFinishingTouchesFail(action.GetType());
                }
            }

            return action switch {
                DoNothingAction => new VerificationSuccess(),
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

        /// <summary>Verifies the given <see cref="EndFinishingTouchesAction"/>.</summary>
        /// <param name="action">The action to verify.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="VerificationSuccess"/> if <see cref="GameCore.CurrentGamePhase"/> is <see cref="GamePhase.FinishingTouches"/>.</item>
        ///     <item><see cref="InvalidEndFinishingTouchesActionUseFail"/> otherwise.</item>
        ///   </list>
        /// </returns>
        private VerificationResult VerifyEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches) {
                return new VerificationSuccess();
            }
            return new InvalidEndFinishingTouchesActionUseFail(_turnInfo.GamePhase);
        }

        /// <summary>Verifies the given <see cref="TakePuzzleAction"/>.</summary>
        /// <param name="action">The action to verify.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item> <see cref="PuzzleDeckIsEmptyFail"/> if the player is taking a puzzle from the top of a deck but it is empty.</item>
        ///     <item> <see cref="PuzzleIdIsNullFail"/> if the player wants a specific puzzle but the ID is <see langword="null"/>.</item>
        ///     <item> <see cref="PuzzleNotAvailableFail"/> if the player wants a specific puzzle but the ID doesn't match any of the available puzzles.</item>
        ///     <item> <see cref="PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail"/> if the player wants to take a black puzzle when <see cref="GameCore.CurrentGamePhase"/> is <see cref="GamePhase.EndOfTheGame"/>, but he already took one this turn.</item>
        ///     <item> <see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        private VerificationResult VerifyTakePuzzleAction(TakePuzzleAction action)
        {
            switch (action.Option) {
                case TakePuzzleAction.Options.TopWhite: {
                    return _gameInfo.NumWhitePuzzlesLeft == 0
                        ? new PuzzleDeckIsEmptyFail(TakePuzzleAction.Options.TopWhite)
                        : new VerificationSuccess();
                }
                case TakePuzzleAction.Options.TopBlack: {
                    return _gameInfo.NumBlackPuzzlesLeft == 0
                        ? new PuzzleDeckIsEmptyFail(TakePuzzleAction.Options.TopBlack)
                        : new VerificationSuccess();
                }
                case TakePuzzleAction.Options.Normal: {
                    if (action.PuzzleId is null)
                        return new PuzzleIdIsNullFail();

                    // find the puzzle
                    foreach (Puzzle puzzle in _gameInfo.AvailableWhitePuzzles.Concat(_gameInfo.AvailableBlackPuzzles)) {
                        if (puzzle.Id == action.PuzzleId) {
                            // if EndOfTheGame is triggered a player can take only 1 black puzzle per turn
                            if (_turnInfo.GamePhase == GamePhase.EndOfTheGame && _turnInfo.TookBlackPuzzle && puzzle.IsBlack) {
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

        /// <summary>Verifies the given <see cref="RecycleAction"/>.</summary>
        /// <param name="action">The action to verify.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="EmptyRowRecycleFail"/> if the player want to recycle an empty row.</item>
        ///     <item><see cref="NumberOfRecycledPuzzlesMismatchFail"/> if the number of puzzles in the row the player wants to recycle doesn't match the number of puzzle IDs given in the recycling order.</item>
        ///     <item><see cref="PuzzleNotInRowFail"/> if a puzzle ID from the recycling order isn't found in the row the player is recycling.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        private VerificationResult VerifyRecycleAction(RecycleAction action)
        {
            // if recycle white puzzles --> check white puzzle count
            if (action.Option == RecycleAction.Options.White) {
                if (_gameInfo.AvailableWhitePuzzles.Length == 0) {
                    return new EmptyRowRecycleFail(RecycleAction.Options.White);
                }
                if (_gameInfo.AvailableWhitePuzzles.Length != action.Order.Count) {
                    return new NumberOfRecycledPuzzlesMismatchFail(_gameInfo.AvailableWhitePuzzles.Length, action.Order.Count, RecycleAction.Options.White);
                }
            }

            // if recycle black puzzles --> check black puzzle count
            if (action.Option == RecycleAction.Options.Black) {
                if (_gameInfo.AvailableBlackPuzzles.Length == 0) {
                    return new EmptyRowRecycleFail(RecycleAction.Options.Black);
                }
                if (_gameInfo.AvailableBlackPuzzles.Length != action.Order.Count) {
                    return new NumberOfRecycledPuzzlesMismatchFail(_gameInfo.AvailableBlackPuzzles.Length, action.Order.Count, RecycleAction.Options.Black);
                }
            }

            // check if all puzzles are in the correct row
            var puzzlesToCheck = action.Option == RecycleAction.Options.White ? _gameInfo.AvailableWhitePuzzles : _gameInfo.AvailableBlackPuzzles;
            foreach (Puzzle puzzle in puzzlesToCheck) {
                if (!action.Order.Contains(puzzle.Id)) {
                    return new PuzzleNotInRowFail(puzzle.Id, action.Option);
                }
            }

            return new VerificationSuccess();
        }

        /// <summary>Verifies the given <see cref="TakeBasicTetrominoAction"/>.</summary>
        /// <param name="action">The action to verify.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="BasicTetrominoNotInSharedReserveFail"/> if there are no <see cref="TetrominoShape.O1"/> tetrominos left in the shared reserve.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        private VerificationResult VerifyTakeBasicTetrominoAction(TakeBasicTetrominoAction action)
        {
            if (_gameInfo.NumTetrominosLeft[(int)TetrominoShape.O1] == 0) {
                return new BasicTetrominoNotInSharedReserveFail();
            }
            return new VerificationSuccess();
        }

        /// <summary>Verifies the given <see cref="ChangeTetrominoAction"/>.</summary>
        /// <param name="action">The action to verify.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="TetrominoNotInPersonalSupplyFail"/> if the player doesn't have the old tetromino.</item>
        ///     <item><see cref="InvalidTetrominoChangeFail"/> if the player can't trade the old tetromino for the new one. This includes the scenario when the new tetromino is the same as the old one.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        /// <seealso cref="RewardManager.GetUpgradeOptions"/>
        private VerificationResult VerifyChangeTetrominoAction(ChangeTetrominoAction action)
        {
            // check if the two tetrominos are different
            if (action.OldTetromino == action.NewTetromino) {
                return new InvalidTetrominoChangeFail(action.OldTetromino, action.NewTetromino);
            }
            // check if the player has the old tetromino
            if (_playerInfo.NumTetrominosOwned[(int)action.OldTetromino] == 0) {
                return new TetrominoNotInPersonalSupplyFail(action.OldTetromino);
            }
            // check if the player can trade the old tetromino for the new one
            var validChanges = RewardManager.GetUpgradeOptions(_gameInfo.NumTetrominosLeft, action.OldTetromino);
            if (!validChanges.Contains(action.NewTetromino)) {
                return new InvalidTetrominoChangeFail(action.OldTetromino, action.NewTetromino);
            }

            return new VerificationSuccess();
        }

        /// <summary>Verifies the given <see cref="PlaceTetrominoAction"/>.</summary>
        /// <param name="action">The action to verify.</param>
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
        private VerificationResult VerifyPlaceTetrominoAction(PlaceTetrominoAction action)
        {
            // check if player has the tetromino
            if (_playerInfo.NumTetrominosOwned[(int)action.Shape] == 0) {
                return new TetrominoNotInPersonalSupplyFail(action.Shape);
            }
            // check if the submitted configuration is valid for the shape
            if (!TetrominoManager.CompareShapeToImage(action.Shape, action.Position)) {
                return new InvalidTetrominoConfigurationFail(action.Shape, action.Position);
            }
            // check if the player has the puzzle
            Puzzle? puzzle = null;
            foreach (Puzzle p in _playerInfo.UnfinishedPuzzles) {
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

        /// <summary>Verifies the given <see cref="MasterAction"/>.</summary>
        /// <param name="action">The action to verify.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><see cref="MasterActionAlreadyUsedFail"/> if the player already used the Master action in this turn.</item>
        ///     <item><see cref="MasterActionUniquePlacementFail"/> if two placements are to the same puzzle.</item>
        ///     <item><see cref="MasterActionNotEnoughTetrominosFail"/> if the player doesn't have the tetrominos he wants to place.</item>
        ///     <item>Any <see cref="VerificationFailure"/> which can occur when verifying a <see cref="PlaceTetrominoAction"/> with <see cref="VerifyPlaceTetrominoAction"/>.</item>
        ///     <item><see cref="VerificationSuccess"/> otherwise.</item>
        ///   </list>
        /// </returns>
        /// <seealso cref="PlaceTetrominoAction"/>
        /// <seealso cref="VerifyPlaceTetrominoAction(PlaceTetrominoAction)"/>
        private VerificationResult VerifyMasterAction(MasterAction action)
        {
            // check if master action was already used
            if (_turnInfo.UsedMasterAction) {
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
                if (_playerInfo.NumTetrominosOwned[i] < usedTetrominos[i]) {
                    return new MasterActionNotEnoughTetrominosFail((TetrominoShape)i, _playerInfo.NumTetrominosOwned[i], usedTetrominos[i]);
                }
            }

            // each placement must be valid 
            foreach (PlaceTetrominoAction placement in action.TetrominoPlacements) {
                VerificationResult status = VerifyPlaceTetrominoAction(placement);
                if (status is VerificationFailure) {
                    return status;
                }
            }
            return new VerificationSuccess();
        }

        #endregion
    }
}
