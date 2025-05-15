namespace ProjectLCore.GameActions
{
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;


    /// <summary>
    /// Processes actions of one player in the game.
    /// The class is responsible for updating the game state based on the player's actions.
    /// It isn't responsible for verifying the actions. The actions should be verified by an <see cref="ActionVerifier"/> before being processed.
    /// </summary>
    /// <seealso cref="ActionVerifier"/>
    /// <seealso cref="GameAction"/>
    /// <seealso cref="ActionProcessorBase" />
    public class GameActionProcessor : ActionProcessorBase, IAsyncActionProcessor
    {
        #region Fields

        private readonly GameCore _game;

        private readonly GameState _gameState;

        private readonly Player _player;

        private readonly PlayerState _playerState;

        private readonly TurnManager.Signaler _signaler;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GameActionProcessor"/> class.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="player">The player this processor is for.</param>
        /// <param name="signaler">A <see cref="TurnManager.Signaler"/> for sending signals when processing actions.</param>
        public GameActionProcessor(GameCore game, Player player, TurnManager.Signaler signaler)
        {
            _game = game;
            _gameState = game.GameState;
            _player = player;
            _playerState = game.PlayerStates[player];
            _signaler = signaler;
        }

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
        /// Processes the given <see cref="GameAction"/> asynchronously.
        /// </summary>
        /// <param name="action">The game action to process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ProcessActionAsync(GameAction action, CancellationToken cancellationToken = default)
        {
            switch (action) {
                case PlaceTetrominoAction a:
                    await ProcessPlaceActionAsync(a, cancellationToken);
                    break;
                case MasterAction a:
                    await ProcessMasterActionAsync(a, cancellationToken);
                    break;
                case TakePuzzleAction a:
                    await ProcessTakePuzzleActionAsync(a, cancellationToken);
                    break;
                case RecycleAction a:
                    await ProcessRecycleActionAsync(a, cancellationToken);
                    break;
                default:
                    ProcessAction(action);
                    break;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected override void ProcessAction(DoNothingAction action)
        {
        }

        /// <summary>
        /// Signals <see cref="TurnManager.Signaler.PlayerEndedFinishingTouches"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected override void ProcessAction(EndFinishingTouchesAction action)
        {
            _signaler.PlayerEndedFinishingTouches();
        }

        /// <summary>
        /// Removes the puzzle from the <see cref="GameCore.GameState"/> and adds it the appropriate <see cref="PlayerState"/>.
        /// Signals <see cref="TurnManager.Signaler.PlayerTookBlackPuzzle"/> if the player took a black puzzle, 
        /// and <see cref="TurnManager.Signaler.BlackDeckIsEmpty"/> if the black deck is empty.
        /// </summary>
        /// <param name="action">The action to be processed.</param>
        /// <exception cref="InvalidOperationException">The specified puzzle was not found.</exception>
        protected override void ProcessAction(TakePuzzleAction action)
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
                    break;
                }
            }

            // check if the puzzle was found
            if (puzzle is null) {
                throw new InvalidOperationException("The specified puzzle was not found");
            }

            // signal if the player took a black puzzle
            if (puzzle.IsBlack) {
                _signaler.PlayerTookBlackPuzzle();
            }

            // signal if the black deck is empty
            if (_gameState.NumBlackPuzzlesLeft == 0) {
                _signaler.BlackDeckIsEmpty();
            }

            // add the puzzle to the player's state
            _playerState.PlaceNewPuzzle(puzzle!);

            // refill the missing puzzle
            _gameState.RefillPuzzles();
        }

        private async Task ProcessTakePuzzleActionAsync(TakePuzzleAction action, CancellationToken cancellationToken)
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
                    break;
                }
            }

            // check if the puzzle was found
            if (puzzle is null) {
                throw new InvalidOperationException("The specified puzzle was not found");
            }

            // signal if the player took a black puzzle
            if (puzzle.IsBlack) {
                _signaler.PlayerTookBlackPuzzle();
            }

            // signal if the black deck is empty
            if (_gameState.NumBlackPuzzlesLeft == 0) {
                _signaler.BlackDeckIsEmpty();
            }

            // add the puzzle to the player's state
            _playerState.PlaceNewPuzzle(puzzle!);

            // refill the missing puzzle
            await _gameState.RefillPuzzlesAsync(cancellationToken);
        }


        /// <summary>
        /// Removes the puzzles from the <see cref="GameCore.GameState"/> puzzle rows and puts them to the bottom of the deck.
        /// Then refills the puzzle rows.
        /// </summary>
        /// <param name="action">The action to process.</param>
        /// <exception cref="System.InvalidOperationException">One of the puzzles specified in the <see cref="RecycleAction.Order"/> of <paramref name="action"/> was not found.</exception>
        protected override void ProcessAction(RecycleAction action)
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

        private async Task ProcessRecycleActionAsync(RecycleAction action, CancellationToken cancellationToken)
        {
            foreach (var id in action.Order) {
                Puzzle? puzzle = _gameState.GetPuzzleWithId(id);
                if (puzzle is null) {
                    throw new InvalidOperationException($"Puzzle with id={id} not found");
                }
                _gameState.RemovePuzzleWithId(id);
                _gameState.PutPuzzleToTheBottomOfDeck(puzzle);
            }
            await _gameState.RefillPuzzlesAsync(cancellationToken);
        }


        /// <summary>
        /// Removes a <see cref="TetrominoShape.O1"/> tetromino from the <see cref="GameCore.GameState"/> and adds it to the appropirate <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected override void ProcessAction(TakeBasicTetrominoAction action)
        {
            _gameState.RemoveTetromino(TetrominoShape.O1);
            _playerState.AddTetromino(TetrominoShape.O1);
        }

        /// <summary>
        /// Removes the old tetromino from the appropriate <see cref="PlayerState"/> and returns it to the <see cref="GameCore.GameState"/>.
        /// Then removes the new tetromino from the <see cref="GameCore.GameState"/> and adds it to the <see cref="PlayerState"/>.
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected override void ProcessAction(ChangeTetrominoAction action)
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
        protected override void ProcessAction(PlaceTetrominoAction action)
        {
            // add the tetromino to the puzzle
            Puzzle? puzzle = _playerState.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("The player doesn't have the puzzle specified by the action");
            }
            _playerState.RemoveTetromino(action.Shape);
            puzzle.AddTetromino(action.Shape, action.Position);

            // if puzzle not finished --> return
            if (!puzzle.IsFinished) {
                // place costs 1 point in FinishingTouches
                if (_game.CurrentGamePhase == GamePhase.FinishingTouches) {
                    _playerState.Score -= 1;
                }
                return;
            }

            FinishedPuzzleInfo info;
            if (_game.CurrentGamePhase == GamePhase.FinishingTouches)
                info = new FinishedPuzzleInfo(_player.Id, puzzle, null, null);
            else
                info = GetPuzzleRewardFromPlayer(puzzle);

            // call finishPuzzle before modifying the game state
            FinishedPuzzlesQueue.Enqueue(info);
            _playerState.FinishPuzzle(info);

            // no reward in FinishingTouches
            if (_game.CurrentGamePhase == GamePhase.FinishingTouches) {
                return;
            }

            // if not finishing touches --> reward the player
            _playerState.Score += puzzle.RewardScore;
            TetrominoShape? reward = info.SelectedReward;

            if (reward is not null) {
                _playerState.AddTetromino(reward.Value);
                _gameState.RemoveTetromino(reward.Value);
            }

            // return the used pieces to the player
            foreach (var tetromino in puzzle.GetUsedTetrominos()) {
                _playerState.AddTetromino(tetromino);
            }
        }

        private async Task ProcessPlaceActionAsync(PlaceTetrominoAction action, CancellationToken cancellationToken)
        {
            // add the tetromino to the puzzle
            Puzzle? puzzle = _playerState.GetPuzzleWithId(action.PuzzleId);
            if (puzzle is null) {
                throw new InvalidOperationException("The player doesn't have the puzzle specified by the action");
            }
            _playerState.RemoveTetromino(action.Shape);
            puzzle.AddTetromino(action.Shape, action.Position);

            // if puzzle not finished --> return
            if (!puzzle.IsFinished) {
                // place costs 1 point in FinishingTouches
                if (_game.CurrentGamePhase == GamePhase.FinishingTouches) {
                    _playerState.Score -= 1;
                }
                return;
            }

            FinishedPuzzleInfo info;
            if (_game.CurrentGamePhase == GamePhase.FinishingTouches)
                info = new FinishedPuzzleInfo(_player.Id, puzzle, null, null);
            else
                info = await GetPuzzleRewardFromPlayerAsync(puzzle, cancellationToken);

            // call finishPuzzle before modifying the game state
            FinishedPuzzlesQueue.Enqueue(info);
            await _playerState.FinishPuzzleAsync(info, cancellationToken);

            // no reward in FinishingTouches
            if (_game.CurrentGamePhase == GamePhase.FinishingTouches) {
                return;
            }

            // if not finishing touches --> reward the player
            _playerState.Score += puzzle.RewardScore;
            TetrominoShape? reward = info.SelectedReward;

            if (reward is not null) {
                _playerState.AddTetromino(reward.Value);
                _gameState.RemoveTetromino(reward.Value);
            }

            // return the used pieces to the player
            foreach (var tetromino in puzzle.GetUsedTetrominos()) {
                _playerState.AddTetromino(tetromino);
            }
        }

        /// <summary>
        /// Places all the tetrominos specified by <see cref="MasterAction.TetrominoPlacements"/>.
        /// Each of these placements is treaded like a <see cref="PlaceTetrominoAction"/>.
        /// Also signals <see cref="TurnManager.Signaler.PlayerUsedMasterAction()"/>
        /// </summary>
        /// <param name="action">The action to process.</param>
        protected override void ProcessAction(MasterAction action)
        {
            _signaler.PlayerUsedMasterAction();

            foreach (var placement in action.TetrominoPlacements) {
                ProcessAction(placement);
            }
        }

        private async Task ProcessMasterActionAsync(MasterAction action, CancellationToken cancellationToken)
        {
            _signaler.PlayerUsedMasterAction();
            foreach (var placement in action.TetrominoPlacements) {
                await ProcessPlaceActionAsync(placement, cancellationToken);
            }
        }

        /// <summary>
        /// Gets the reward for completing a puzzle and other information about the reward selection process. If there are multiple options, the player gets to choose.
        /// If the player fails to choose a valid reward, the first available one is picked.
        /// Also adds information about the puzzle to the <see cref="FinishedPuzzlesQueue"/>.
        /// </summary>
        /// <param name="puzzle">The puzzle the reward is for.</param>
        /// <returns>Information about se reward selection process. The selected reward is in <see cref="FinishedPuzzleInfo.SelectedReward"/>.</returns>
        private FinishedPuzzleInfo GetPuzzleRewardFromPlayer(Puzzle puzzle)
        {
            List<TetrominoShape> rewardOptions = RewardManager.GetRewardOptions(_gameState.GetNumTetrominosLeft(), puzzle.RewardTetromino);

            // if there are no reward options, the player doesn't get anything
            if (rewardOptions.Count == 0) {
                return new FinishedPuzzleInfo(_player.Id, puzzle, rewardOptions, null);
            }
            // get reward from player
            TetrominoShape? reward;
            try {
                reward = _player.GetRewardAsync(rewardOptions, puzzle.Clone()).GetAwaiter().GetResult();
            }
            catch (Exception) {
                reward = null;
            }

            // if the chosen reward isn't valid, pick the first one
            if (reward is null || !rewardOptions.Contains(reward.Value)) {
                reward = rewardOptions[0];
            }

            return new FinishedPuzzleInfo(_player.Id, puzzle, rewardOptions, reward);
        }

        private async Task<FinishedPuzzleInfo> GetPuzzleRewardFromPlayerAsync(Puzzle puzzle, CancellationToken cancellationToken)
        {
            List<TetrominoShape> rewardOptions = RewardManager.GetRewardOptions(_gameState.GetNumTetrominosLeft(), puzzle.RewardTetromino);

            // if there are no reward options, the player doesn't get anything
            if (rewardOptions.Count == 0) {
                return new FinishedPuzzleInfo(_player.Id, puzzle, rewardOptions, null);
            }
            // get reward from player
            TetrominoShape? reward;
            try {
                reward = await _player.GetRewardAsync(rewardOptions, puzzle.Clone(), cancellationToken);
            }
            catch (OperationCanceledException) {
                throw;
            }
            catch (Exception) {
                reward = null;
            }

            // if the chosen reward isn't valid, pick the first one
            if (reward is null || !rewardOptions.Contains(reward.Value)) {
                reward = rewardOptions[0];
            }

            return new FinishedPuzzleInfo(_player.Id, puzzle, rewardOptions, reward);
        }

        #endregion
    }
}
