using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kostra
{
    class ActionVerifier(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo)
    {
        private readonly GameState.GameInfo _gameInfo = gameInfo;
        private readonly PlayerState.PlayerInfo _playerInfo = playerInfo;
        private readonly TurnInfo _turnInfo = turnInfo;

        public VerificationStatus Verify(VerifiableAction action)
        {
            // if FinishingTouches --> only EndFinishingTouchesAction and PlaceAction are allowed
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches)
            {
                if (action is not EndFinishingTouchesAction && action is not PlaceTetrominoAction)
                {
                    return new InvalidActionDuringFinishingTouchesFail(action.GetType());
                }
            }
            // if not FinishingTouches --> EndFinishingTouhces is not allowed
            else
            {
                if (action is EndFinishingTouchesAction)
                {
                    return new InvalidEndFinishingTouchesActionUseFail(_turnInfo.GamePhase);
                }
            }

            return action switch
            {
                TakeBasicTetrominoAction a => VerifyTakeBasicTetrominoAction(a),
                ChangeTetrominoAction a => VerifyChangeTetrominoAction(a),
                EndFinishingTouchesAction a => VerifyEndFinishingTouchesAction(a),
                RecycleAction a => VerifyRecycleAction(a),
                TakePuzzleAction a => VerifyTakePuzzleAction(a),
                PlaceTetrominoAction a => VerifyPlaceTetrominoAction(a),
                MasterAction a => VerifyMasterAction(a),
                _ => throw new InvalidOperationException("Unknown action type")
            };
        }

        public VerificationStatus VerifyTakeBasicTetrominoAction(TakeBasicTetrominoAction action)
        {
            if (_gameInfo.NumTetrominosLeft[(int)TetrominoShape.O1] == 0)
            {
                return new TetrominoNotInSharedReserveFail(TetrominoShape.O1);
            }
            return new VerificationSuccess();
        }
        public VerificationStatus VerifyChangeTetrominoAction(ChangeTetrominoAction action)
        {
            // check if the player has the old tetromino
            if (_playerInfo.NumTetrominosOwned[(int)action.OldTetromino] == 0)
            {
                return new TetrimonoNotInPersonalSupplyFail(action.OldTetromino);
            }
            // check if the player can trade the old tetromino for the new one
            var validChanges = RewardManager.GetUpgradeOptions(_gameInfo.NumTetrominosLeft, action.OldTetromino);
            if (!validChanges.Contains(action.NewTetromino))
            {
                return new InvalidTetrimonoChangeFail(action.OldTetromino, action.NewTetromino);
            }

            return new VerificationSuccess();
        }

        public VerificationStatus VerifyEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            return new VerificationSuccess();
        }


        public VerificationStatus VerifyRecycleAction(RecycleAction action)
        {
            // if recycle white puzzles --> check white puzzle count
            if (action.Option == RecycleAction.Options.White)
            {
                if (_gameInfo.AvailableWhitePuzzles.Length != action.Order.Count)
                {
                    return new NumberOfRecycledPuzzlesMismatchFail(_gameInfo.AvailableWhitePuzzles.Length, action.Order.Count, RecycleAction.Options.White);
                }
            }

            // if recycle black puzzles --> check black puzzle count
            if (action.Option == RecycleAction.Options.Black)
            {
                if (_gameInfo.AvailableBlackPuzzles.Length != action.Order.Count)
                {
                    return new NumberOfRecycledPuzzlesMismatchFail(_gameInfo.AvailableBlackPuzzles.Length, action.Order.Count, RecycleAction.Options.Black);
                }
            }

            // check if all puzzles are in the correct row
            var puzzlesToCheck = action.Option == RecycleAction.Options.White ? _gameInfo.AvailableWhitePuzzles : _gameInfo.AvailableBlackPuzzles;
            foreach (Puzzle puzzle in puzzlesToCheck)
            {
                if (!action.Order.Contains(puzzle.Id))
                {
                    return new PuzzleNotInRowFail(puzzle.Id, action.Option);
                }
            }

            return new VerificationSuccess();
        }


        public VerificationStatus VerifyTakePuzzleAction(TakePuzzleAction action)
        {
            switch (action.Option)
            {
                case TakePuzzleAction.Options.TopWhite:
                    return _gameInfo.NumWhitePuzzlesLeft == 0
                        ? new PuzzleDeckIsEmptyFail(TakePuzzleAction.Options.TopWhite)
                        : new VerificationSuccess();
                case TakePuzzleAction.Options.TopBlack:
                    return _gameInfo.NumBlackPuzzlesLeft == 0
                        ? new PuzzleDeckIsEmptyFail(TakePuzzleAction.Options.TopBlack)
                        : new VerificationSuccess();
                case TakePuzzleAction.Options.Normal:
                    if (action.PuzzleId is null) return new PuzzleIdIsNullFail();

                    // find the puzzle
                    foreach (Puzzle puzzle in _gameInfo.AvailableWhitePuzzles.Concat(_gameInfo.AvailableBlackPuzzles))
                    {
                        if (puzzle.Id == action.PuzzleId)
                        {
                            // if EndOfTheGame is triggered a player can take only 1 black puzzle per round
                            if (_turnInfo.GamePhase == GamePhase.EndOfTheGame && _turnInfo.TookBlackPuzzle && puzzle.IsBlack)
                            {
                                return new PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail();
                            }
                            return new VerificationSuccess();
                        }
                    }
                    // no puzzle matching ID found
                    return new PuzzleNotAvailableFail(action.PuzzleId.Value);
                default:
                    throw new InvalidOperationException("Unknown take puzzle option");
            }
        }
        public VerificationStatus VerifyPlaceTetrominoAction(PlaceTetrominoAction action)
        {
            // check if player has the tetromino
            if (_playerInfo.NumTetrominosOwned[(int)action.Tetromino] == 0)
            {
                return new TetrimonoNotInPersonalSupplyFail(action.Tetromino);
            }
            // check if the submitted configuration is valid for the shape
            if (!TetrominoManager.CompareShapeToImage(action.Tetromino, action.Position))
            {
                return new InvalidTetrominoConfigurationFail(action.Tetromino, action.Position);
            }
            // check if the player has the puzzle
            Puzzle? puzzle = null;
            foreach (Puzzle p in _playerInfo.UnfinishedPuzzles)
            {
                if (p.Id == action.PuzzleId)
                {
                    puzzle = p;
                    break;
                }
            }
            if (puzzle is null)
            {
                return new PlayerDoesntHavePuzzleFail(action.PuzzleId);
            }

            // check if the tetromino can be placed there
            if (!puzzle.CanPlaceTetromino(action.Position))
            {
                return new CannotPlaceTetrominoFail(action.PuzzleId, action.Position);
            }

            // CHECK WHAT THE GAMEPHASE IT AND MARK THE ACTION
            action.FinishingTouches = _turnInfo.GamePhase == GamePhase.FinishingTouches;

            return new VerificationSuccess();
        }
        public VerificationStatus VerifyMasterAction(MasterAction action)
        {
            // each placement must be to a different puzzle
            List<uint> puzzleIds = new();
            foreach (PlaceTetrominoAction placement in action.TetrominoPlacements)
            {
                if (puzzleIds.Contains(placement.PuzzleId))
                {
                    return new MasterActionUniquePlacementFail();
                }
                puzzleIds.Add(placement.PuzzleId);
            }
            // player has to have all of the tetrominos
            int[] usedTetrominos = new int[TetrominoManager.NumShapes];
            foreach (PlaceTetrominoAction placement in action.TetrominoPlacements)
            {
                usedTetrominos[(int)placement.Tetromino]++;
            }
            for (int i = 0; i < TetrominoManager.NumShapes; i++)
            {
                if (_playerInfo.NumTetrominosOwned[i] < usedTetrominos[i])
                {
                    return new MasterActionNotEnoughTetrominosFail((TetrominoShape)i, _playerInfo.NumTetrominosOwned[i], usedTetrominos[i]);
                }
            }

            // each placement must be valid 
            foreach (PlaceTetrominoAction placement in action.TetrominoPlacements)
            {
                VerificationStatus status = VerifyPlaceTetrominoAction(placement);
                if (status is VerificationFailure)
                {
                    return status;
                }
            }
            return new VerificationSuccess();
        }
    }

    // verification status messages

    abstract class VerificationStatus { }
    class VerificationSuccess : VerificationStatus { }
    abstract class VerificationFailure : VerificationStatus
    {
        public abstract string Message { get; }
    }


    class TetrominoNotInSharedReserveFail(TetrominoShape shape) : VerificationFailure
    {
        public TetrominoShape Shape => shape;
        public override string Message => $"Tetromino {shape} not in shared reserve";
    }
    class InvalidTetrimonoChangeFail(TetrominoShape oldShape, TetrominoShape newShape) : VerificationFailure
    {
        public TetrominoShape OldTetromino => oldShape;
        public TetrominoShape NewTetromino => newShape;
        public override string Message => $"Cannot change {oldShape} for {newShape}";
    }
    class TetrimonoNotInPersonalSupplyFail(TetrominoShape shape) : VerificationFailure
    {
        public TetrominoShape Shape => shape;
        public override string Message => $"Tetromino {shape} not in personal supply";
    }
    class NumberOfRecycledPuzzlesMismatchFail(int expected, int actual, RecycleAction.Options color) : VerificationFailure
    {
        public int Expected => expected;
        public int Actual => actual;
        public RecycleAction.Options RecyclingColor => color;
        public override string Message => $"There are {expected} puzzles of color '{color}', got {actual}";
    }
    class PuzzleNotInRowFail(uint id, RecycleAction.Options color) : VerificationFailure
    {
        public uint Id => id;
        public RecycleAction.Options Color => color;
        public override string Message => $"Puzzle with id {id} is not the {color} row";
    }
    class PuzzleNotAvailableFail(uint id) : VerificationFailure
    {
        public uint Id => id;
        public override string Message => $"Puzzle with id {id} is not available";
    }
    class PuzzleIdIsNullFail : VerificationFailure
    {
        public override string Message => "Puzzle id is null";
    }
    class PuzzleDeckIsEmptyFail(TakePuzzleAction.Options color) : VerificationFailure
    {
        public TakePuzzleAction.Options Color => color;
        public override string Message => $"{Color} puzzle deck is empty";
    }
    class InvalidTetrominoConfigurationFail(TetrominoShape shape, BinaryImage configuration) : VerificationFailure
    {
        public TetrominoShape Shape => shape;
        public BinaryImage Configuration => configuration;
        public override string Message => $"Invalid configuration for tetromino {shape}.";
    }
    class PlayerDoesntHavePuzzleFail(uint id) : VerificationFailure
    {
        public uint Id => id;
        public override string Message => $"Player doesn't have puzzle with id {id}";
    }
    class CannotPlaceTetrominoFail(uint puzzleId, BinaryImage position) : VerificationFailure
    {
        public uint PuzzleId => puzzleId;
        public BinaryImage Position => position;
        public override string Message => $"Cannot place tetromino on puzzle {puzzleId} at given position";
    }
    class MasterActionUniquePlacementFail : VerificationFailure
    {
        public override string Message => "Each placement must be to a different puzzle";
    }
    class MasterActionNotEnoughTetrominosFail(TetrominoShape shape, int owned, int used) : VerificationFailure
    {
        public TetrominoShape Shape => shape;
        public int Owned => owned;
        public int Used => used;
        public override string Message => $"Player doesn't have enough {shape} tetrominos. Owned: {owned}, used: {used}";
    }
    class InvalidActionDuringFinishingTouchesFail(Type actionType) : VerificationFailure
    {
        public Type ActionType => actionType;
        public override string Message => $"Invalid action during finishing touches: {actionType.Name}";
    }
    class InvalidEndFinishingTouchesActionUseFail(GamePhase phase) : VerificationFailure
    {
        public GamePhase Phase => phase;
        public override string Message => $"EndFinishingTouchesAction cannot be used druing the '{phase}' gamephase";
    }
    class PlayerAlreadyTookBlackPuzzleInEndOfTheGameFail : VerificationFailure
    {
        public override string Message => "Players can take only 1 black puzzle per round during the EndOfTheGame phase";
    }
}

