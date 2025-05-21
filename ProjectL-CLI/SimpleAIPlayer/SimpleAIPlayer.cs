namespace SimpleAIPlayer
{
    using ProjectLCore;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameActions.Verification;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A Simple AI player that chooses the best puzzle to solve and then solves it using IDA*.
    /// </summary>
    public class SimpleAIPlayer : AIPlayerBase
    {
        #region Constants

        private const int MinTotalTetrominoLevelForBlackPuzzle = 16;

        #endregion

        #region Fields

        private readonly Random _rng = new();

        private List<PuzzleSolutionInfo> _puzzleStrategies = new();

        private Queue<GameAction>? _finishingTouchesStrategy = null;

        private int[] _numTetrominosOwned = new int[TetrominoManager.NumShapes];

        private Stage _currentStage = Stage.Ealy;

        #endregion

        private enum Stage
        {
            Ealy, Mid, Late
        }

        #region Properties

        private bool IsSolvingBlackPuzzle => _puzzleStrategies.Any(p => p.Puzzle.IsBlack);

        #endregion

        #region Methods

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="numPlayers">The number of players in the game.</param>
        /// <param name="allPuzzles">All the puzzles in the game.</param>
        /// <param name="filePath">The path to a file where the player might be storing some information.</param>
        protected override void Init(int numPlayers, List<Puzzle> allPuzzles, string? filePath)
        {
        }

        /// <summary>
        /// Chooses a random reward.
        /// </summary>
        /// <param name="rewardOptions">The reward options.</param>
        /// <param name="puzzle">The puzzle that was completed.</param>
        /// <returns>
        /// A random element of <paramref name="rewardOptions"/>.
        /// </returns>
        protected override TetrominoShape GetReward(List<TetrominoShape> rewardOptions, Puzzle puzzle)
        {
            return rewardOptions.GetRandomElement();
        }

        /// <summary>
        /// Uses the IDA* algorithm to find the best solution for the given puzzle. The strategy goes as follows:
        /// <para>
        /// During the <see cref="GamePhase.Normal"/> phase:
        /// <list type="bullet">
        ///   <item>
        ///     <description>If the player has no puzzles, take a new puzzle.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the player has more than one puzzle and more than one puzzle has a Place action at the end of its queue, use the Master action.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the player already has a puzzle, he will take a new one if after solving the puzzles he already has, he will still have at least one tetromino left.
        ///     If he will take a puzzle and which one it will be is determined by the following criteria:
        ///     <list type="bullet">
        /// <item><description>how many pieces of which level he has in total</description></item>    
        /// <item><description>how many pieces of which level he has in his collection right now</description></item>    
        /// <item><description>if he is solving a black puzzle right now</description></item>    
        /// <item><description>how many puzzles are left in the black deck</description></item>    
        /// </list>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>There is a 3% random chance to use a recycle action.</description>
        ///   </item>
        ///   <item>
        ///     <description>Otherwise, pick the puzzle with the shortest solution and use its next action.</description>
        ///   </item>
        ///   <item>
        ///     <description>Near the end of the game, limit the number of puzzles based on the number of puzzles left in the black deck. Always try to have: number of unfinished puzzles &lt;= number of puzzles left in the black deck.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// 
        /// <para>
        /// During the <see cref="GamePhase.EndOfTheGame"/> phase:
        ///   <list type="bullet">
        ///   <item>
        ///     <description>If the player has no puzzles, attempt to use tetromino actions to gain more tetrominos, as leftover tetrominos may be used for tiebreakers.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the player has unfinished puzzles, attempt to solve them, prioritizing the closest one to being solved first.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// 
        /// <para>
        /// During the <see cref="GamePhase.FinishingTouches"/> phase:
        ///  <list type="bullet">
        ///   <item>
        ///     <description>If there are no unfinished puzzles, simply returns <see cref="EndFinishingTouchesAction"/>.</description>
        ///   </item>
        ///   <item>
        ///     <description>For each unfinished puzzle, the method attempts to solve it using only the tetrominos the player currently owns.</description>
        ///   </item>
        ///   <item>
        ///     <description>that provide a net positive score are included in the strategy.</description>
        ///   </item>
        ///   <item>
        ///     <description>The resulting list of actions is ordered to maximize the player's final score, ending with an <see cref="EndFinishingTouchesAction"/>.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gameInfo">Information about the shared resources.</param>
        /// <param name="myInfo">Information about the resources of THIS player</param>
        /// <param name="enemyInfos">Information about the resources of the OTHER players.</param>
        /// <param name="turnInfo">Information about the current turn.</param>
        /// <param name="verifier">Verifier for verifying the validity of actions in the current game context.</param>
        /// <returns>
        /// The action the player wants to take.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Invalid game phase</exception>
        protected override GameAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            // if we finished some puzzles last time --> remove them from the list and use the returned tetrominos
            if (TryRemovingFinishedPuzzles()) {
                UpdateTetrominosOwned(myInfo);
                RecalculateSolutionsForPuzzles(gameInfo, myInfo);
            }

            UpdateStage(gameInfo, myInfo, turnInfo);

            // try to get an action
            GameAction action = GetAction();

            if (verifier.Verify(action) is VerificationFailure) {
                UpdateTetrominosOwned(myInfo);
                RecalculateSolutionsForPuzzles(gameInfo, myInfo);
                action = GetAction();
            }

            return action;

            GameAction GetAction()
            {
                return turnInfo.GamePhase switch {
                    GamePhase.Normal => GetActionDuringNormalPhase(gameInfo, myInfo, turnInfo),
                    GamePhase.EndOfTheGame => GetActionDuringEndOfTheGame(gameInfo, myInfo, turnInfo),
                    GamePhase.FinishingTouches => GetActionDuringFinishingTouchesPhase(gameInfo, myInfo, turnInfo),
                    _ => new DoNothingAction()
                };
            }
        }

        /// <summary>
        /// Solves the given puzzle using IDA*. It minimizes the number of actions needed to solve the puzzle.
        /// </summary>
        /// <param name="puzzle">The puzzle to solve.</param>
        /// <param name="numTetrominosLeft">Information about the tetrominos left in the shared reserve.</param>
        /// <param name="numTetrominosOwned">Information about the tetrominos owned by THIS player.</param>
        /// <param name="finishingTouches">If set to <see langword="true"/> the algorithm will use only the tetrominos owned by the player.</param>
        /// <returns>
        /// Information about the solution to the puzzle. If the puzzle can't be solved, it returns <see langword="null"/>.
        /// </returns>
        private static PuzzleSolutionInfo? SolvePuzzleWithIDAStar(Puzzle puzzle, IReadOnlyList<int> numTetrominosLeft, IReadOnlyList<int> numTetrominosOwned, bool finishingTouches = false)
        {
            var solution = IDAStar.IterativeDeepeningAStar(
                new PuzzleNode(puzzle.Image, puzzle.Id, numTetrominosLeft, numTetrominosOwned, finishingTouches),
                PuzzleNode.FinishedPuzzle
            );

            if (solution.Item1 is null) {
                return null;
            }

            var path = new List<GameAction>();
            foreach (ActionEdge<PuzzleNode> edge in solution.Item1.Cast<ActionEdge<PuzzleNode>>()) {
                path.AddRange(edge.Action);
            }

            ActionEdge<PuzzleNode> lastEdge = (ActionEdge<PuzzleNode>)solution.Item1[^1];
            int[] numTetrominosLeftAfter = lastEdge.To.NumTetrominosLeft;
            int[] numTetrominosOwnedAfter = lastEdge.To.NumTetrominosOwned;

            return new PuzzleSolutionInfo(puzzle, path, numTetrominosLeftAfter, numTetrominosOwnedAfter);
        }

        private int CalculateTotalTetrominoLevel(int[] numTetrominosOwned)
        {
            int level = 0;
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                level += numTetrominosOwned[i] * TetrominoManager.GetLevelOf((TetrominoShape)i);
            }
            return level;
        }

        private void UpdateStage(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, TurnInfo turnInfo)
        {
            if (turnInfo.GamePhase == GamePhase.EndOfTheGame || gameInfo.NumBlackPuzzlesLeft <= 3) {
                _currentStage = Stage.Late;
                return;
            }

            int num = GetTotalNumberOfTetrominosOwned();

            if (num < MinTotalTetrominoLevelForBlackPuzzle) {
                _currentStage = Stage.Ealy;
            }
            else if (num < 2 * MinTotalTetrominoLevelForBlackPuzzle) {
                _currentStage = Stage.Mid;
            }
            else {
                _currentStage = Stage.Late;
            }

            int GetTotalNumberOfTetrominosOwned()
            {
                int n = CalculateTotalTetrominoLevel(myInfo.NumTetrominosOwned);
                foreach (Puzzle puzzle in myInfo.UnfinishedPuzzles) {
                    foreach (TetrominoShape tetromino in puzzle.GetUsedTetrominos()) {
                        n += TetrominoManager.GetLevelOf(tetromino);
                    }
                }
                return n;
            }
        }

        /// <summary>
        /// Determines the action to take during the <see cref="GamePhase.Normal"/> phase of the game.
        /// <para>
        /// Strategy:
        /// <list type="bullet">
        ///   <item>
        ///     <description>If the player has no puzzles, take a new puzzle.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the player has more than one puzzle and more than one puzzle has a Place action at the end of its queue, use the Master action.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the player already has a puzzle, he will take a new one if after solving the puzzles he already has, he will still have at least one tetromino left.
        ///     If he will take a puzzle and which one it will be is determined by the following criteria:
        ///     <list type="bullet">
        /// <item><description>how many pieces of which level he has in total</description></item>    
        /// <item><description>how many pieces of which level he has in his collection right now</description></item>    
        /// <item><description>if he is solving a black puzzle right now</description></item>    
        /// <item><description>how many puzzles are left in the black deck</description></item>    
        /// </list>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>There is a 3% random chance to use a recycle action.</description>
        ///   </item>
        ///   <item>
        ///     <description>Otherwise, pick the puzzle with the shortest solution and use its next action.</description>
        ///   </item>
        ///   <item>
        ///     <description>Near the end of the game, limit the number of puzzles based on the number of puzzles left in the black deck. Always try to have: number of unfinished puzzles &lt;= number of puzzles left in the black deck.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gameInfo">The game information.</param>
        /// <param name="myInfo">My information.</param>
        /// <param name="turnInfo">The turn information.</param>
        /// <returns>The action to take during the <see cref="GamePhase.Normal"/>.</returns>
        private GameAction GetActionDuringNormalPhase(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, TurnInfo turnInfo)
        {
            int numPuzzles = _puzzleStrategies.Count;

            if (numPuzzles == 0) {
                UpdateTetrominosOwned(myInfo);
            }
            else {
                UpdateTetrominosOwnedBasedOnLastPuzzle();
            }

            // 0 or 1 puzzles --> take a new one before using master
            if (numPuzzles <= Math.Min(1, gameInfo.NumBlackPuzzlesLeft)) {
                if (TryTakingNewPuzzle(gameInfo, myInfo, out var newPuzzleAction)) {
                    return newPuzzleAction!;
                }
            }

            // if can use master --> try it
            if (!turnInfo.UsedMasterAction) {
                if (TryToUseMasterAction(out var masterAction)) {
                    return masterAction!;
                }
            }

            // try to take a new puzzle if the player has more than one puzzle and can still take one
            if (numPuzzles > 1 && numPuzzles <= Math.Min(3, gameInfo.NumBlackPuzzlesLeft)) {
                if (TryTakingNewPuzzle(gameInfo, myInfo, out var newPuzzleAction)) {
                    return newPuzzleAction!;
                }
            }

            // random chance to recycle
            if (_currentStage != Stage.Ealy && _rng.Next(0, 100) < 3 && TryCreatingRecycleAction(gameInfo, out var recycleAction)) {
                return recycleAction!;
            }

            // pick next action from the puzzle with the shortest solution
            if (TryToGetNextSolutionAction(out var action)) {
                return action!;
            }

            // nothing seems to work --> at least try taking a tetromino
            if (TryToGetValidTetrominoAction(gameInfo, myInfo, out var tetrominoAction)) {
                UpdateTetrominosOwned(tetrominoAction!);
                return tetrominoAction!;
            }

            return new DoNothingAction();
        }

        /// <summary>
        /// Determines the action to take during the <see cref="GamePhase.EndOfTheGame"/> phase.
        /// <para>
        /// Strategy:
        /// <list type="bullet">
        ///   <item>
        ///     <description>If the player has no puzzles, attempt to use tetromino actions to gain more tetrominos, as leftover tetrominos may be used for tiebreakers.</description>
        ///   </item>
        ///   <item>
        ///     <description>If the player has unfinished puzzles, attempt to solve them, prioritizing the closest one to being solved first.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gameInfo">The game information.</param>
        /// <param name="myInfo">My information.</param>
        /// <param name="turnInfo">The turn information.</param>
        /// <returns>The action to take during the <see cref="GamePhase.EndOfTheGame"/>.</returns>
        private GameAction GetActionDuringEndOfTheGame(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, TurnInfo turnInfo)
        {
            if (TryToGetNextSolutionAction(out var action)) {
                return action!;
            }

            if (TryToGetValidTetrominoAction(gameInfo, myInfo, out var tetrominoAction)) {
                UpdateTetrominosOwned(tetrominoAction!);
                return tetrominoAction!;
            }

            return new DoNothingAction();
        }

        /// <summary>
        /// Determines the action to take during the <see cref="GamePhase.EndOfTheGame"/> phase.
        /// </summary>
        /// <param name="gameInfo">The game information.</param>
        /// <param name="myInfo">My information.</param>
        /// <param name="turnInfo">The turn information.</param>
        /// <returns>The action to take during the <see cref="GamePhase.FinishingTouches"/>.</returns>
        private GameAction GetActionDuringFinishingTouchesPhase(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, TurnInfo turnInfo)
        {
            if (_finishingTouchesStrategy is null) {
                _finishingTouchesStrategy = new(GetFinishingTouchesStrategy(gameInfo, myInfo, turnInfo));
                _finishingTouchesStrategy.Enqueue(new EndFinishingTouchesAction());
            }

            return _finishingTouchesStrategy.Dequeue();
        }

        /// <summary>
        /// Creates a strategy for the FinishingTouches phase, where the player can use their remaining tetrominos to complete unfinished puzzles to prevent negative points.
        /// </summary>
        /// <param name="gameInfo">The current game information, including available puzzles and shared resources.</param>
        /// <param name="myInfo">Information about the player's current state, such as owned tetrominos and unfinished puzzles.</param>
        /// <param name="turnInfo">Information about the current turn, including the game phase and actions left.</param>
        /// <returns>
        /// A list of actions representing the optimal sequence of actions to maximize the player's score during the FinishingTouches phase.
        /// If there are no unfinished puzzles or it is not beneficial to use tetrominos, the list will be empty.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>Strategy:</b>
        /// <list type="bullet">
        ///   <item>
        ///     <description>If there are no unfinished puzzles, the strategy ends immediately.</description>
        ///   </item>
        ///   <item>
        ///     <description>For each unfinished puzzle, the method attempts to solve it using only the tetrominos the player currently owns.</description>
        ///   </item>
        ///   <item>
        ///     <description>The method evaluates whether completing a puzzle is worthwhile by considering the points lost for unfinished puzzles and the cost of using each tetromino.</description>
        ///   </item>
        ///   <item>
        ///     <description>Only puzzles that can be completed with the available tetrominos and that provide a net positive score are included in the strategy.</description>
        ///   </item>
        ///   <item>
        ///     <description>The resulting list of actions is ordered to maximize the player's final score, ending with an <see cref="EndFinishingTouchesAction"/>.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        private List<PlaceTetrominoAction> GetFinishingTouchesStrategy(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, TurnInfo turnInfo)
        {
            UpdateTetrominosOwned(myInfo);

            List<PlaceTetrominoAction> strategy = new();
            List<Puzzle> puzzlesToConsider = myInfo.UnfinishedPuzzles.ToList();

            while (puzzlesToConsider.Count > 0) {

                // solve each puzzle and add it to the strategy if it is worth it
                List<PuzzleSolutionInfo> solutionInfos = new();
                object lockObject = new();

                Parallel.ForEach(puzzlesToConsider, puzzle => {
                    var solution = SolvePuzzleWithIDAStar(puzzle, gameInfo.NumTetrominosLeft, _numTetrominosOwned, finishingTouches: true);
                    // if puzzle has a solution --> add it to the list
                    if (solution is not null) {
                        lock (lockObject) {
                            solutionInfos.Add(solution);
                        }
                    }
                });

                // if there are no puzzles with a solution --> we are done
                if (solutionInfos.Count == 0) {
                    return strategy;
                }

                // get the puzzle where the net score is the highest
                PuzzleSolutionInfo bestSolution = solutionInfos.OrderByDescending(p => GetNetScore(p)).FirstOrDefault();

                // if the best solution is not worth it --> we are done
                if (bestSolution is null || GetNetScore(bestSolution) <= 0) {
                    return strategy;
                }

                // add the solution to the strategy
                strategy.AddRange(bestSolution.Solution.OfType<PlaceTetrominoAction>());

                UpdateTetrominosOwned(bestSolution);
                puzzlesToConsider.Remove(bestSolution.Puzzle);
            }

            return strategy;

            static int GetNetScore(PuzzleSolutionInfo puzzleInfo)
            {
                return puzzleInfo.Puzzle.RewardScore - puzzleInfo.NumSteps;
            }
        }

        /// <summary>
        /// Removes all finished puzzles from the current puzzle strategies and updates the tetromino count accordingly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if any finished puzzles were removed; otherwise, <see langword="false"/>.
        /// </returns>
        private bool TryRemovingFinishedPuzzles()
        {
            if (_puzzleStrategies.Count == 0) {
                return false;
            }

            List<PuzzleSolutionInfo> finishedPuzzles = _puzzleStrategies.Where(p => p.NumSteps == 0).ToList();
            if (finishedPuzzles.Count == 0) {
                return false;
            }

            foreach (PuzzleSolutionInfo puzzle in finishedPuzzles) {
                _puzzleStrategies.Remove(puzzle);
            }

            return true;
        }

        /// <summary>
        /// Recalculates the solutions for all unfinished puzzles owned by the player and updates the puzzle strategies list.
        /// </summary>
        /// <param name="gameInfo">The current game information, including available tetrominos and puzzles.</param>
        /// <param name="myInfo">Information about THIS player.</param>
        private void RecalculateSolutionsForPuzzles(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo)
        {
            List<Puzzle> whitePuzzlesToSolve = myInfo.UnfinishedPuzzles.Where(p => !p.IsBlack).ToList();
            List<Puzzle> blackPuzzlesToSolve = myInfo.UnfinishedPuzzles.Where(p => p.IsBlack).ToList();

            // sort puzzles by the number of empty cells in the puzzle
            whitePuzzlesToSolve.Sort((p1, p2) => p1.Image.CountEmptyCells().CompareTo(p2.Image.CountEmptyCells()));
            blackPuzzlesToSolve.Sort((p1, p2) => p1.Image.CountEmptyCells().CompareTo(p2.Image.CountEmptyCells()));

            // put black puzzles in front of white ones
            var puzzlesToSolve = blackPuzzlesToSolve.Concat(whitePuzzlesToSolve).ToList();

            // solve each puzzle and add it to the list
            _puzzleStrategies.Clear();

            foreach (Puzzle puzzle in puzzlesToSolve) {
                var solution = SolvePuzzleWithIDAStar(puzzle, gameInfo.NumTetrominosLeft, _numTetrominosOwned);
                if (solution is null) {
                    continue;
                }
                _puzzleStrategies.Add(solution);
                UpdateTetrominosOwnedBasedOnLastPuzzle();
            }
        }

        /// <summary>
        /// Gets the valid action involving getting a tetromino.
        /// </summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <param name="myInfo">Information about THIS player.</param>
        /// <param name="action">A <see cref="TakeBasicTetrominoAction"/> if possible, else <see cref="ChangeTetrominoAction"/>. <see langword="null"/> if no such action is possible.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="null"/>.</returns>
        private bool TryToGetValidTetrominoAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, out TetrominoAction? action)
        {
            action = null;

            if (gameInfo.NumTetrominosLeft[(int)TetrominoShape.O1] > 0) {
                action = new TakeBasicTetrominoAction();
                return true;
            }
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                if (myInfo.NumTetrominosOwned[i] > 0) {
                    var options = RewardManager.GetUpgradeOptions(gameInfo.NumTetrominosLeft, (TetrominoShape)i);
                    if (options.Count > 0) {
                        action = new ChangeTetrominoAction((TetrominoShape)i, options.GetRandomElement());
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a valid recycle action.
        /// </summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <param name="action">A <see cref="RecycleAction"/> or <see langword="null"/> if no such action is possible.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="null"/>.</returns>
        private bool TryCreatingRecycleAction(GameState.GameInfo gameInfo, out RecycleAction? action)
        {
            action = null;

            List<RecycleAction> actions = new();

            if (gameInfo.AvailableWhitePuzzles.Length > 0) {
                List<uint> order = gameInfo.AvailableWhitePuzzles.Select(p => p.Id).ToList();
                order.Shuffle();
                actions.Add(new RecycleAction(order, RecycleAction.Options.White));
            }
            if (gameInfo.AvailableBlackPuzzles.Length > 0) {
                List<uint> order = gameInfo.AvailableBlackPuzzles.Select(p => p.Id).ToList();
                order.Shuffle();
                actions.Add(new RecycleAction(order, RecycleAction.Options.Black));
            }

            if (actions.Count == 0) {
                return false;
            }

            action = actions.GetRandomElement();
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the next action from the puzzle with the shortest remaining solution.
        /// </summary>
        /// <param name="action">
        /// When this method returns, contains the next <see cref="GameAction"/> to perform, 
        /// or <see langword="null"/> if no valid action is available.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a next action was successfully retrieved; otherwise, <see langword="false"/>.
        /// </returns>
        private bool TryToGetNextSolutionAction(out GameAction? action)
        {
            action = null;

            // return action from the puzzle with the shortest solution
            if (_puzzleStrategies.Count == 0) {
                return false;
            }

            List<PuzzleSolutionInfo> sortedStrategies = _puzzleStrategies.Where(p => p.NumSteps > 0).OrderBy(p => p.NumSteps).ToList();

            if (IsSolvingBlackPuzzle) {
                // find first black puzzle with a solution
                sortedStrategies = sortedStrategies.Where(p => p.Puzzle.IsBlack).ToList();
            }

            var best = sortedStrategies.FirstOrDefault();

            if (best is null) {
                return false;
            }

            action = best.Solution.Dequeue();
            return true;
        }

        /// <summary>
        /// Tries to use master action. If there are no puzzles or only one puzzle with a place action at the end of the queue, it returns <see langword="null"/>.
        /// </summary>
        /// <param name="action">A <see cref="MasterAction"/> if it is advantageous to use it; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if successful; otherwise <see langword="null"/>.</returns>
        private bool TryToUseMasterAction(out MasterAction? action)
        {
            action = null;

            if (_puzzleStrategies.Count == 0) {
                return false;
            }

            int numPlaceActions = 0;

            // get all the place actions from the puzzles
            foreach (PuzzleSolutionInfo info in _puzzleStrategies) {
                GameAction nextAction = info.Solution.Peek();
                if (nextAction is PlaceTetrominoAction placeAction) {
                    numPlaceActions++;
                }
            }

            // check if worth using master action
            if (numPlaceActions < 2) {
                return false;
            }

            // create the master action for real
            List<PlaceTetrominoAction> placeActions = new();
            foreach (PuzzleSolutionInfo info in _puzzleStrategies) {
                GameAction nextAction = info.Solution.Peek();
                if (nextAction is PlaceTetrominoAction placeAction) {
                    info.Solution.Dequeue();
                    placeActions.Add(placeAction);
                }
            }

            action = new MasterAction(placeActions);
            return true;
        }

        /// <summary>
        /// Chooses the next puzzle to solve. If the player doesn't have a lot of tetrominos yet, it considers only white puzzles.
        /// It first solves all of the considers puzzles and than picks the most advantageous one using the <see cref="PuzzleComparer"/>.
        /// </summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <param name="action">If successful, contains a <see cref="TakePuzzleAction"/></param>
        /// <param name="myInfo">My information.</param>
        /// <returns>
        /// <see langword="true"/> if the player can take a new puzzle and solve it; otherwise <see langword="null"/>.
        /// </returns>
        private bool TryTakingNewPuzzle(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, out TakePuzzleAction? action)
        {
            action = null;

            List<Puzzle> possiblePuzzles = new();

            int numPuzzles = _puzzleStrategies.Count;
            int currentNumPieces = _numTetrominosOwned.Sum();
            int currentTotalLevel = CalculateTotalTetrominoLevel(_numTetrominosOwned);
            int actualTotalLevel = CalculateTotalTetrominoLevel(myInfo.NumTetrominosOwned);

            bool anyBlackPuzzlesLeft = gameInfo.NumBlackPuzzlesLeft > 0;

            bool recalculateAfter = false;
            bool onlyTakeShortSolution = false;
            int shortSolutionMaxLength = 3;

            switch (_currentStage) {
                case Stage.Ealy:
                    // solve only white
                    if (_numTetrominosOwned.Sum() > 0) {
                        possiblePuzzles.AddRange(gameInfo.AvailableWhitePuzzles);
                        onlyTakeShortSolution = numPuzzles >= 1 && currentNumPieces <= 2;
                    }
                    break;

                case Stage.Mid:
                    // solve 1 black and rest white
                    if (IsSolvingBlackPuzzle) {
                        if (numPuzzles <= 2) {
                            possiblePuzzles.AddRange(gameInfo.AvailableWhitePuzzles);
                            onlyTakeShortSolution = numPuzzles > 1 && currentNumPieces <= 2;
                        }
                    }
                    else if (currentTotalLevel >= MinTotalTetrominoLevelForBlackPuzzle) {
                        possiblePuzzles.AddRange(gameInfo.AvailableBlackPuzzles);
                    }
                    else if (actualTotalLevel >= MinTotalTetrominoLevelForBlackPuzzle) {
                        possiblePuzzles.AddRange(gameInfo.AvailableBlackPuzzles);
                        recalculateAfter = true;
                    }
                    break;

                case Stage.Late:
                    // solve only black
                    if (currentTotalLevel >= MinTotalTetrominoLevelForBlackPuzzle) {
                        possiblePuzzles.AddRange(gameInfo.AvailableBlackPuzzles);
                    }
                    else if (!IsSolvingBlackPuzzle && actualTotalLevel >= MinTotalTetrominoLevelForBlackPuzzle) {
                        possiblePuzzles.AddRange(gameInfo.AvailableBlackPuzzles);
                        recalculateAfter = true;
                    }
                    break;
            }

            // if there are no puzzles to choose from --> return null
            if (possiblePuzzles.Count == 0 && !anyBlackPuzzlesLeft) {
                possiblePuzzles.AddRange(gameInfo.AvailableWhitePuzzles);
                if (possiblePuzzles.Count == 0) {
                    return false;
                }
            }

            if (recalculateAfter) {
                UpdateTetrominosOwned(myInfo);
            }

            // choose the best puzzle
            List<PuzzleSolutionInfo> solutionInfos = new();
            object lockObject = new();

            Parallel.ForEach(possiblePuzzles, puzzle => {
                var solution = SolvePuzzleWithIDAStar(puzzle, gameInfo.NumTetrominosLeft, _numTetrominosOwned);
                // if puzzle has a solution --> add it to the list
                if (solution is not null) {
                    lock (lockObject) {
                        solutionInfos.Add(solution);
                    }
                }
            });

            if (onlyTakeShortSolution) {
                solutionInfos = solutionInfos.Where(p => p.NumSteps <= shortSolutionMaxLength || p.Solution.Peek() is PlaceTetrominoAction).ToList();
            }

            // if there are no puzzles with a solution --> return null
            if (solutionInfos.Count == 0) {
                return false;
            }
            // sort the solution infos using the PuzzleComparer and chose best
            var best = solutionInfos.OrderByDescending(p => p, new PuzzleComparer(_currentStage)).FirstOrDefault();

            // add it to the list of my puzzles
            if (recalculateAfter) {
                _numTetrominosOwned = best.TetrominosOwnedAfter;
                RecalculateSolutionsForPuzzles(gameInfo, myInfo);
                _puzzleStrategies.Insert(0, best);
            }
            else {
                _puzzleStrategies.Add(best);
            }

            // create the action to take the puzzle
            action = new TakePuzzleAction(TakePuzzleAction.Options.Normal, best.Puzzle.Id);
            return true;
        }

        private void UpdateTetrominosOwnedBasedOnLastPuzzle()
        {
            if (_puzzleStrategies.Count == 0) {
                return;
            }
            _numTetrominosOwned = _puzzleStrategies[^1].TetrominosOwnedAfter;
        }

        private void UpdateTetrominosOwned(PuzzleSolutionInfo puzzleSolutionInfo)
        {
            _numTetrominosOwned = puzzleSolutionInfo.TetrominosOwnedAfter;
        }

        private void UpdateTetrominosOwned(TetrominoAction action)
        {
            if (action is TakeBasicTetrominoAction) {
                _numTetrominosOwned[(int)TetrominoShape.O1]++;
            }
            else if (action is ChangeTetrominoAction changeAction) {
                _numTetrominosOwned[(int)changeAction.NewTetromino]++;
                _numTetrominosOwned[(int)changeAction.OldTetromino]--;
            }
        }

        private void UpdateTetrominosOwned(PlayerState.PlayerInfo myCurrentInfo)
        {
            _numTetrominosOwned = myCurrentInfo.NumTetrominosOwned;
        }

        #endregion

        /// <summary>
        /// Represents the information about a puzzle needed to determine how advantageous would be to take and solve it.
        /// </summary>
        private class PuzzleSolutionInfo
        {
            #region Constructors

            public PuzzleSolutionInfo(Puzzle puzzle, List<GameAction> solution, int[] tetrominosLeftAfter, int[] tetrominosOwnedAfter)
            {
                Puzzle = puzzle;
                Solution = new(solution);
                TetrominosLeftAfter = tetrominosLeftAfter;
                TetrominosOwnedAfter = tetrominosOwnedAfter;
            }

            #endregion

            #region Properties

            public Puzzle Puzzle { get; }

            public Queue<GameAction> Solution { get; }

            public int NumSteps => Solution.Count;

            public int[] TetrominosLeftAfter { get; }

            public int[] TetrominosOwnedAfter { get; }

            #endregion
        }

        /// <summary>
        /// Defines a method for comparing <see cref="PuzzleSolutionInfo"/> objects.
        /// </summary>
        /// <seealso cref="IComparer{T}"/>
        /// <seealso cref="PuzzleSolutionInfo"/>
        private class PuzzleComparer : IComparer<PuzzleSolutionInfo>
        {
            #region Fields

            private Stage _stage;

            #endregion

            #region Constructors

            public PuzzleComparer(Stage gameStage)
            {
                _stage = gameStage;
            }

            #endregion

            #region Methods

            public int Compare(PuzzleSolutionInfo x, PuzzleSolutionInfo y)
            {
                int xLevel = TetrominoManager.GetLevelOf(x.Puzzle.RewardTetromino);
                int yLevel = TetrominoManager.GetLevelOf(y.Puzzle.RewardTetromino);

                int xScore = 0;
                int yScore = 0;

                if (_stage == Stage.Ealy) {
                    xScore = (x.Puzzle.RewardScore + xLevel) / x.NumSteps;
                    yScore = (y.Puzzle.RewardScore + yLevel) / y.NumSteps;
                }

                else {
                    xScore = (2 * x.Puzzle.RewardScore + xLevel) / x.NumSteps;
                    yScore = (2 * y.Puzzle.RewardScore + yLevel) / y.NumSteps;
                }

                return xScore.CompareTo(yScore);
            }

            #endregion
        }
    }
}
