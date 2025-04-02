namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;

    /// <summary>
    /// An interface for processing actions using the visitor pattern.
    /// Each action should be verified by an <see cref="ActionVerifier"/> before being processed.
    /// </summary>
    /// <seealso cref="IAction"/>
    /// <seealso cref="ActionVerifier"/>
    /// <seealso cref="GameActionProcessor"/>
    public interface IActionProcessor
    {
        #region Methods

        /// <summary>
        /// Processes the given <see cref="EndFinishingTouchesAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action);

        /// <summary>
        /// Processes the given <see cref="TakePuzzleAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessTakePuzzleAction(TakePuzzleAction action);

        /// <summary>
        /// Processes the given <see cref="RecycleAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessRecycleAction(RecycleAction action);

        /// <summary>
        /// Processes the given <see cref="TakeBasicTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action);

        /// <summary>
        /// Processes the given <see cref="ChangeTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessChangeTetrominoAction(ChangeTetrominoAction action);

        /// <summary>
        /// Processes the given <see cref="PlaceTetrominoAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action);

        /// <summary>
        /// Processes the given <see cref="MasterAction"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessMasterAction(MasterAction action);

        #endregion
    }

    /// <summary>
    /// Processes actions of one player in the game.
    /// The class is responsible for updating the game state based on the player's actions.
    /// It isn't responsible for verifying the actions. The actions should be verified by an <see cref="ActionVerifier"/> before being processed.
    /// </summary>
    /// <param name="game">The current game.</param>
    /// <param name="player">The player this processor is for.</param>
    /// <param name="signaler">A <see cref="TurnManager.Signaler"/> for sending signals when processing actions.</param>
    /// <seealso cref="ActionVerifier"/>
    /// <seealso cref="IAction"/>
    /// <seealso cref="IActionProcessor" />
    public class GameActionProcessor(GameCore game, Player player, TurnManager.Signaler signaler) : IActionProcessor
    {
        #region Fields

        private readonly GameState _gameState = game.GameState;

        private readonly PlayerState _playerState = game.PlayerStates[player];

        #endregion

        #region Properties

        /// <summary>
        /// When a <see cref="Puzzle"/> is finished with a <see cref="PlaceTetrominoAction"/>, information about the finished puzzle is added to this queue.
        /// </summary>
        /// <seealso cref="GameCore.TryGetNextPuzzleFinishedBy(Player, out FinishedPuzzleInfo)"/>
        public Queue<FinishedPuzzleInfo> FinishedPuzzlesQueue { get; } = new();

        #endregion

        #region Methods

        /// <summary>
        /// Signals <see cref="TurnManager.Signaler.PlayerEndedFinishingTouches"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessEndFinishingTouchesAction(EndFinishingTouchesAction action)
        {
            signaler.PlayerEndedFinishingTouches();
        }

        /// <summary>
        /// Removes the puzzle from the <see cref="GameCore.GameState"/> and adds it the appropriate <see cref="PlayerState"/>.
        /// Signals <see cref="TurnManager.Signaler.PlayerTookBlackPuzzle"/> if the player took a black puzzle, 
        /// and <see cref="TurnManager.Signaler.BlackDeckIsEmpty"/> if the black deck is empty.
        /// </summary>
        /// <param name="action">The action to be processed.</param>
        /// <exception cref="InvalidOperationException">The specified puzzle was not found.</exception>
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
                throw new InvalidOperationException("The specified puzzle was not found");
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
        /// Removes the puzzles from the <see cref="GameCore.GameState"/> puzzle rows and puts them to the bottom of the deck.
        /// Then refills the puzzle rows.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <exception cref="System.InvalidOperationException">One of the puzzles specified in the <see cref="RecycleAction.Order"/> of <paramref name="action"/> was not found.</exception>
        public void ProcessRecycleAction(RecycleAction action)
        {
            foreach (var id in action.Order) {
                Puzzle? puzzle = _gameState.GetPuzzleWithId(id);
                if (puzzle is null) {
                    throw new InvalidOperationException($"Puzzle with id={id} not found");
                }
                _gameState.RemovePuzzleWithId(id);
                _gameState.PutPuzzleToTheBottomOfDeck(puzzle);
            }
            _gameState.RefillPuzzles();
        }

        /// <summary>
        /// Removes a <see cref="TetrominoShape.O1"/> tetromino from the <see cref="GameCore.GameState"/> and adds it to the appropirate <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessTakeBasicTetrominoAction(TakeBasicTetrominoAction action)
        {
            _gameState.RemoveTetromino(TetrominoShape.O1);
            _playerState.AddTetromino(TetrominoShape.O1);
        }

        /// <summary>
        /// Removes the old tetromino from the appropriate <see cref="PlayerState"/> and returns it to the <see cref="GameCore.GameState"/>.
        /// Then removes the new tetromino from the <see cref="GameCore.GameState"/> and adds it to the <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
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
        /// Adds the tetromino to the puzzle. If this action completes the puzzle and the <see cref="GameCore.CurrentGamePhase"/> is not <see cref="GamePhase.FinishingTouches"/>, 
        /// the player gets a reward and the tetrominos he used to complete the puzzle are returned to him.
        /// This doesn't happen during <see cref="GamePhase.FinishingTouches"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <remarks>
        ///  If this action completes the puzzle, information about it is added to the <see cref="FinishedPuzzlesQueue"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The player doesn't have the puzzle specified by the action</exception>
        public void ProcessPlaceTetrominoAction(PlaceTetrominoAction action)
        {
            // add the tetromino to the puzzle
            Puzzle? puzzle = _playerState.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("The player doesn't have the puzzle specified by the action");
            }
            puzzle.AddTetromino(action.Shape, action.Position);

            // remove the tetromino from the player's state
            _playerState.RemoveTetromino(action.Shape);

            // handle FinishingTouches separately
            if (game.CurrentGamePhase == GamePhase.FinishingTouches) {
                _playerState.Score -= 1;
                if (puzzle.IsFinished) {
                    _playerState.FinishPuzzleWithId(puzzle.Id);
                }
                AddFinishedPuzzleInfoToQueue(puzzle, null, null);
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
        /// Places all the tetrominos specified by <see cref="MasterAction.TetrominoPlacements"/>.
        /// Each of these placements is treaded like a <see cref="PlaceTetrominoAction"/>.
        /// Also signals <see cref="TurnManager.Signaler.PlayerUsedMasterAction()"/>
        /// </summary>
        /// <param name="action">The action to process.</param>
        public void ProcessMasterAction(MasterAction action)
        {
            signaler.PlayerUsedMasterAction();

            foreach (var placement in action.TetrominoPlacements) {
                ProcessPlaceTetrominoAction(placement);
            }
        }

        /// <summary>
        /// Gets the reward for completing a puzzle. If there are multiple options, the player gets to choose.
        /// If the player fails to choose a valid reward, the first available one is picked.
        /// Also adds information about the puzzle to the <see cref="FinishedPuzzlesQueue"/>.
        /// </summary>
        /// <param name="puzzle">The puzzle the reward is for.</param>
        /// <returns>The reward the player chose or <see langword="null"/> if there are no reward options.</returns>
        private TetrominoShape? GetPuzzleReward(Puzzle puzzle)
        {
            var rewardOptions = RewardManager.GetRewardOptions(_gameState.NumTetrominosLeft, puzzle.RewardTetromino);

            // if there are no reward options, the player doesn't get anything
            if (rewardOptions.Count == 0) {
                AddFinishedPuzzleInfoToQueue(puzzle, rewardOptions, null);
                return null;
            }
            // get reward from player
            TetrominoShape? reward;
            try {
                reward = player.GetRewardAsync(rewardOptions, puzzle.Clone()).Result;
            }
            catch (Exception) {
                reward = null;
            }

            // if the chosen reward isn't valid, pick the first one
            if (reward is null || !rewardOptions.Contains(reward.Value)) {
                reward = rewardOptions[0];
            }

            AddFinishedPuzzleInfoToQueue(puzzle, rewardOptions, reward);
            return reward;
        }

        private void AddFinishedPuzzleInfoToQueue(Puzzle puzzle, List<TetrominoShape>? rewardOptions, TetrominoShape? selectedReward)
        {
            FinishedPuzzlesQueue.Enqueue(new FinishedPuzzleInfo(player.Id, puzzle, rewardOptions, selectedReward));
        }

        #endregion
    }
}
