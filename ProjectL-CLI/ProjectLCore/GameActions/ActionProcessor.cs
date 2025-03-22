namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;

    /// <summary>
    /// An interface for processing actions using the visitor pattern.
    /// Each action should be verified before being processed.
    /// </summary>
    /// <seealso cref="IAction"/>
    public interface IActionProcessor
    {
        #region Methods

        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action);

        public void ProcessTakePuzzleAction(TakePuzzleAction action);

        public void ProcessRecycleAction(RecycleAction action);

        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action);

        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action);

        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action);

        public void ProcessMasterAction(MasterAction action);

        #endregion
    }

    /// <summary>
    /// A class for processing player actions in the game. One instance should be created for each player.
    /// The class is responsible for updating the game state based on the player's actions.
    /// It isn't responsible for verifying the actions. The actions should be verified by <see cref="ActionVerifier"/> before being processed.
    /// </summary>
    /// <seealso cref="IActionProcessor" />
    /// <param name="game">The current game.</param>
    /// <param name="playerId">The ID of the player the processor is for.</param>
    /// <param name="signaler">A <see cref="TurnManager.Signaler"/> to send signals when processing actions.</param>
    public class GameActionProcessor(GameCore game, uint playerId, TurnManager.Signaler signaler) : IActionProcessor
    {
        #region Fields

        private readonly GameState _gameState = game.GameState;

        private readonly Player _player = game.GetPlayerWithId(playerId);

        private readonly PlayerState _playerState = game.GetPlayerStateWithId(playerId);

        #endregion

        #region Methods

        /// <summary>
        /// Processes the end finishing touches action.
        /// </summary>
        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            signaler.PlayerEndedFinishingTouches();
        }

        /// <summary>
        /// Processes the take puzzle action.
        ///   <list type="number">
        ///     <item> Removes the puzzle from the <see cref="GameState"/> (throws an exception if the puzzle is not found)</item>
        ///     <item>Adds the puzzle to the <see cref="PlayerState"/> </item>
        ///   </list>
        /// Signals if the player took a black puzzle. Also signals if the black deck is empty.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="InvalidOperationException">Puzzle not found</exception>
        public void ProcessTakePuzzleAction(TakePuzzleAction action)
        {
            Puzzle? puzzle = null;
            switch (action.Option) {
                case TakePuzzleAction.Options.TopWhite: {
                    puzzle = _gameState.TakeTopWhitePuzzle();
                    break;
                }
                case TakePuzzleAction.Options.TopBlack: {
                    puzzle = _gameState.TakeTopBlackPuzzle();
                    break;
                }
                case TakePuzzleAction.Options.Normal: {
                    puzzle = _gameState.GetPuzzleWithId(action.PuzzleId!.Value);
                    if (puzzle is null) {
                        break;
                    }
                    _gameState.RemovePuzzleWithId(action.PuzzleId!.Value);
                    _gameState.RefillPuzzles();
                    break;
                }
            }

            // check if the puzzle was found
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }

            // signal if the player took a black puzzle
            if (puzzle.IsBlack) {
                signaler.PlayerTookBlackPuzzle();
            }

            // signal if the black deck is empty
            if (_gameState.NumBlackPuzzlesLeft == 0) {
                signaler.BlackDeckIsEmpty();
            }

            // add the puzzle to the player's state
            _playerState.PlaceNewPuzzle(puzzle!);
        }

        /// <summary>
        /// Processes the recycle action. Raises an exception if the puzzle is not found.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="InvalidOperationException">Puzzle not found</exception>
        public void ProcessRecycleAction(RecycleAction action)
        {
            foreach (var id in action.Order) {
                Puzzle? puzzle = _gameState.GetPuzzleWithId(id);
                if (puzzle is null) {
                    throw new InvalidOperationException("Puzzle not found");
                }
                _gameState.RemovePuzzleWithId(id);
                _gameState.PutPuzzleToTheBottomOfDeck(puzzle);
            }
            _gameState.RefillPuzzles();
        }

        /// <summary>
        /// Processes the take basic tetromino action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action)
        {
            _gameState.RemoveTetromino(TetrominoShape.O1);
            _playerState.AddTetromino(TetrominoShape.O1);
        }

        /// <summary>
        /// Processes the change tetromino action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action)
        {
            // remove old tetromino
            _playerState.RemoveTetromino(action.OldTetromino);
            _gameState.AddTetromino(action.OldTetromino);
            // add new tetromino
            _playerState.AddTetromino(action.NewTetromino);
            _gameState.RemoveTetromino(action.NewTetromino);
        }

        /// <summary>
        /// Processes the place tetromino action. Adds the tetromino to the puzzle. If the puzzle is finished, the player gets a reward and the used tetrominos back.
        /// During <see cref="GamePhase.FinishingTouches"/>, the player doesn't get any rewards and this action costs 1 score.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <exception cref="InvalidOperationException">Puzzle not found</exception>
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action)
        {
            Puzzle? puzzle = _gameState.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("Puzzle not found");
            }
            puzzle.AddTetromino(action.Shape, action.Position);

            // handle FinishingTouches separately
            if (game.CurrentGamePhase == GamePhase.FinishingTouches) {
                _playerState.Score -= 1;
                if (puzzle.IsFinished) {
                    _playerState.FinishPuzzleWithId(puzzle.Id);
                }
                return;
            }

            // that's all we have to do if the puzzle isn't finished yet
            if (!puzzle.IsFinished) {
                return;
            }

            // if the puzzle is finished --> reward the player
            _playerState.Score += puzzle.RewardScore;

            TetrominoShape? reward = GetPuzzleReward(puzzle);
            if (reward is not null) {
                _playerState.AddTetromino(reward.Value);
                _gameState.RemoveTetromino(reward.Value);
            }

            // return the used pieces to the player
            foreach (var tetromino in puzzle.GetUsedTetrominos()) {
                _playerState.AddTetromino(tetromino);
            }

            // remove the puzzle from the player's state
            _playerState.FinishPuzzleWithId(puzzle.Id);
        }

        /// <summary>
        /// Processes the master action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ProcessMasterAction(MasterAction action)
        {
            signaler.PlayerUsedMasterAction();

            foreach (var placement in action.TetrominoPlacements) {
                ProcessPlaceTetrominoAction(placement);
            }
        }

        /// <summary>
        /// Gets the reward for completing a puzzle. If there are multiple options, the player gets to choose.
        /// </summary>
        /// <param name="puzzle">The puzzle the reward is for.</param>
        private TetrominoShape? GetPuzzleReward(Puzzle puzzle)
        {
            var rewardOptions = RewardManager.GetRewardOptions(_gameState.NumTetrominosLeft, puzzle.RewardTetromino);

            // if there are no reward options, the player doesn't get anything
            if (rewardOptions.Count == 0) {
                return null;
            }
            // if there is only one option, return it
            if (rewardOptions.Count == 1) {
                return rewardOptions[0];
            }
            // if there are multiple options, let the player choose
            TetrominoShape reward = _player.GetRewardAsync(rewardOptions, puzzle.Clone()).Result;
            // if the chosen reward isn't valid, pick the first one
            if (!rewardOptions.Contains(reward)) {
                reward = rewardOptions[0];
            }
            return reward;
        }

        #endregion
    }
}
