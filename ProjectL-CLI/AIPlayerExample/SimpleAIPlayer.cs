namespace AIPlayerExample
{
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
        #region Fields

        /// <summary>The puzzle we are currently solving.</summary>
        private Puzzle? _currentPuzzle;

        /// <summary>The strategy to solve <see cref="_currentPuzzle"/>.</summary>
        private Queue<IAction> _currentStrategy = new();

        /// <summary>The strategy for the <see cref="GamePhase.FinishingTouches"/> game phase.</summary>
        private Queue<IAction>? _finishingTouchesStrategy = null;

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
            // do nothing
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
        /// Uses the IDA* algorithm to solve one puzzle at a time.
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
        protected override IAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            // get an unfinished puzzle if there is one
            _currentPuzzle = myInfo.UnfinishedPuzzles.Length == 0 ? null : myInfo.UnfinishedPuzzles[0];

            switch (turnInfo.GamePhase) {
                case GamePhase.Normal: {
                    // if no strategy --> create one
                    if (_currentStrategy.Count == 0) {
                        _currentStrategy = GetStrategy(gameInfo, myInfo);
                    }

                    // if next action is valid --> submit it
                    var nextAction = _currentStrategy.Dequeue();
                    if (verifier.Verify(nextAction) is VerificationSuccess) {
                        return nextAction;
                    }

                    // if not --> create a new strategy
                    _currentStrategy = GetStrategy(gameInfo, myInfo);
                    return _currentStrategy.Dequeue();
                }

                case GamePhase.EndOfTheGame: {
                    // if we have a puzzle --> continue solving it
                    if (_currentPuzzle is not null) {
                        goto case GamePhase.Normal;
                    }

                    // if we don't have a puzzle --> don't take a new one (negative points)
                    // try to get more tetrominos (in case tie the player with more tetrominos leftover wins)
                    IAction? action = GetValidTetrominoAction(gameInfo, myInfo);
                    if (action != null)
                        return action;

                    // try to recycle, any action is more interesting than DoNothingAction()
                    action = GetValidRecycleAction(gameInfo);
                    if (action != null)
                        return action;

                    // last resort
                    return new DoNothingAction();
                }

                case GamePhase.FinishingTouches: {
                    // if no puzzle to complete --> end the game
                    if (_currentPuzzle is null) {
                        return new EndFinishingTouchesAction();
                    }

                    // if I don't have a strategy --> create one
                    if (_finishingTouchesStrategy is null) {
                        var solution = SolvePuzzleWithIDAStar(_currentPuzzle, gameInfo.NumTetrominosLeft, myInfo.NumTetrominosOwned, finishingTouches: true).Item1;
                        if (solution is null) {
                            return new EndFinishingTouchesAction();
                        }
                        _finishingTouchesStrategy = new Queue<IAction>(solution);
                    }

                    // proceed with the strategy
                    return _finishingTouchesStrategy.Dequeue();
                }

                case GamePhase.Finished:
                    break;
                default:
                    break;
            }
            throw new InvalidOperationException("Invalid game phase");
        }

        /// <summary>
        /// Gets the valid action involving getting a tetromino.
        /// </summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <param name="myInfo">Information about THIS player.</param>
        /// <returns>A <see cref="TakeBasicTetrominoAction"/> if possible, else <see cref="ChangeTetrominoAction"/>. <see langword="null"/> if no such action is possible.</returns>
        private static TetrominoAction? GetValidTetrominoAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo)
        {
            if (gameInfo.NumTetrominosLeft[(int)TetrominoShape.O1] > 0) {
                return new TakeBasicTetrominoAction();
            }
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                if (myInfo.NumTetrominosOwned[i] > 0) {
                    var options = RewardManager.GetUpgradeOptions(gameInfo.NumTetrominosLeft, (TetrominoShape)i);
                    if (options.Count > 0) {
                        return new ChangeTetrominoAction((TetrominoShape)i, options.GetRandomElement());
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a valid recycle action.
        /// </summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <returns>A <see cref="RecycleAction"/> or <see langword="null"/> if no such action is possible.</returns>
        private static RecycleAction? GetValidRecycleAction(GameState.GameInfo gameInfo)
        {
            if (gameInfo.AvailableWhitePuzzles.Length > 0) {
                List<uint> order = gameInfo.AvailableWhitePuzzles.Select(p => p.Id).ToList();
                return new RecycleAction(order, RecycleAction.Options.White);
            }
            if (gameInfo.AvailableBlackPuzzles.Length > 0) {
                List<uint> order = gameInfo.AvailableBlackPuzzles.Select(p => p.Id).ToList();
                return new RecycleAction(order, RecycleAction.Options.Black);
            }
            return null;
        }

        /// <summary>
        /// Solves the given puzzle using IDA*. It minimizes the number of actions needed to solve the puzzle.
        /// </summary>
        /// <param name="puzzle">The puzzle to solve.</param>
        /// <param name="numTetrominosLeft">Information about the tetrominos left in the shared reserve.</param>
        /// <param name="numTetrominosOwned">Information about the tetrominos owned by THIS player.</param>
        /// <param name="maxDepth">The maximum depth for IDA*.</param>
        /// <param name="finishingTouches">If set to <see langword="true"/> the algorithm will use only the tetrominos owned by the player.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><c>(shortest solution, length)</c> if the solution was found.</item>
        ///     <item><c>(null, bound)</c> where bound is the estimated length of the shortest solution, if the goal wasn't reached within the given <c>maxDepth</c>.</item>
        ///     <item><c>(null, -1)</c> if the puzzle can't be solved using the available resources.</item>
        ///   </list>
        /// </returns>
        private static Tuple<List<IAction>?, int> SolvePuzzleWithIDAStar(Puzzle puzzle, IReadOnlyList<int> numTetrominosLeft, IReadOnlyList<int> numTetrominosOwned, int maxDepth = -1, bool finishingTouches = false)
        {
            var solution = IDAStar.IterativeDeepeningAStar(
                new PuzzleNode(puzzle.Image, puzzle.Id, numTetrominosLeft, numTetrominosOwned, finishingTouches),
                PuzzleNode.FinishedPuzzle,
                maxDepth
            );

            if (solution.Item1 is null) {
                return new(null, solution.Item2);
            }

            var path = new List<IAction>();
            foreach (ActionEdge<PuzzleNode> edge in solution.Item1.Cast<ActionEdge<PuzzleNode>>()) {
                path.AddRange(edge.Action);
            }
            return new(path, solution.Item2);
        }

        /// <summary>
        /// Chooses the next puzzle to solve. If the player doesn't have a lot of tetrominos yet, it considers only white puzzles.
        /// It first solves all of the considers puzzles and than picks the most advantageous one using the <see cref="PuzzleComparer"/>.
        /// </summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <param name="myInfo">Information about THIS player.</param>
        /// <param name="maxDepth">The maximum depth for IDA*.</param>
        /// <param name="levelSumToConsiderBlackPuzzles">If the sum of the levels of the tetrominos owned by the player is less than this number then only white puzzles are considered.</param>
        /// <returns>The puzzle to solve and a solution, or <see langword="null"/> if no puzzle can be solved.</returns>
        private static Tuple<Puzzle, List<IAction>>? ChoosePuzzle(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, int maxDepth = -1, int levelSumToConsiderBlackPuzzles = 15)
        {
            List<Puzzle> possiblePuzzles = new();

            bool ShouldChooseWhitePuzzle(IReadOnlyList<int> numTetrominosOwned)
            {
                int totalTetrominoLevel = 0;
                for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                    int level = TetrominoManager.GetLevelOf((TetrominoShape)i);
                    totalTetrominoLevel += level * numTetrominosOwned[i];
                }

                return totalTetrominoLevel < levelSumToConsiderBlackPuzzles;
            }

            if (ShouldChooseWhitePuzzle(myInfo.NumTetrominosOwned)) {
                possiblePuzzles.AddRange(gameInfo.AvailableWhitePuzzles);
            }
            if (possiblePuzzles.Count == 0) {
                possiblePuzzles.AddRange(gameInfo.AvailableBlackPuzzles);
            }
            // if there are no puzzles to choose from --> return null
            if (possiblePuzzles.Count == 0) {
                return null;
            }

            // choose the best puzzle
            List<PuzzleSolutionInfo> solutionInfos = new();
            object lockObject = new();

            Parallel.ForEach(possiblePuzzles, puzzle => {
                var solution = SolvePuzzleWithIDAStar(puzzle, gameInfo.NumTetrominosLeft, myInfo.NumTetrominosOwned, maxDepth);
                // if puzzle has a solution --> add it to the list
                if (solution.Item1 is not null) {
                    var solutionInfo = new PuzzleSolutionInfo(puzzle, solution.Item1, solution.Item1.Count);
                    lock (lockObject) {
                        solutionInfos.Add(solutionInfo);
                    }
                }
            });

            // if there are no puzzles with a solution --> return null
            if (solutionInfos.Count == 0) {
                return null;
            }

            // sort the solution infos using the PuzzleComparer
            solutionInfos.Sort(new PuzzleComparer());
            // choose the best puzzle = the one with the highest reward
            var best = solutionInfos[^1];
            return new(best.Puzzle, best.Solution);
        }

        /// <summary>Gets the strategy on what to do next given the current game state.</summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <param name="myInfo">Information about THIS player.</param>
        /// <param name="maxDepth">The maximum depth for IDA*.</param>
        /// <returns>
        ///   <para>
        /// A queue containing:
        /// </para>
        ///   <list type="bullet">
        ///     <item>The <see cref="DoNothingAction" /> if there are no puzzles the player can solve in the current game context.</item>
        ///     <item>A strategy to (take) and solve the next puzzle otherwise.</item>
        ///   </list>
        /// </returns>
        private Queue<IAction> GetStrategy(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, int maxDepth = -1)
        {
            var strategy = new Queue<IAction>();

            // if we already have a puzzle to solve
            if (_currentPuzzle is not null) {
                // else find a solution to current puzzle
                var solution = SolvePuzzleWithIDAStar(_currentPuzzle, gameInfo.NumTetrominosLeft, myInfo.NumTetrominosOwned, maxDepth).Item1;
                if (solution is null) {
                    strategy.Enqueue(new DoNothingAction());
                    return strategy;
                }

                return new(solution);
            }

            // choose puzzle
            var res = ChoosePuzzle(gameInfo, myInfo, maxDepth);
            // if there are no puzzles left --> do nothing
            if (res is null) {
                strategy.Enqueue(new DoNothingAction());
                return strategy;
            }
            // else: take the puzzle and solve it
            TakePuzzleAction takePuzzleAction = new(TakePuzzleAction.Options.Normal, res.Item1.Id);
            strategy.Enqueue(takePuzzleAction);
            foreach (var action in res.Item2) {
                strategy.Enqueue(action);
            }
            return strategy;
        }

        #endregion

        /// <summary>
        /// Represents the information about a puzzle needed to determine how advantageous would be to take and solve it.
        /// </summary>
        private readonly struct PuzzleSolutionInfo
        {
            #region Constructors
            public PuzzleSolutionInfo(Puzzle puzzle, List<IAction> solution, int numSteps)
            {
                Puzzle = puzzle;
                Solution = solution;
                NumSteps = numSteps;
            }

            #endregion

            #region Properties

            public Puzzle Puzzle { get; }

            public List<IAction> Solution { get; }

            public int NumSteps { get; }

            #endregion
        }

        /// <summary>
        /// Defines a method for comparing <see cref="PuzzleSolutionInfo"/> objects.
        /// </summary>
        /// <seealso cref="IComparer{T}"/>
        /// <seealso cref="PuzzleSolutionInfo"/>
        private class PuzzleComparer : IComparer<PuzzleSolutionInfo>
        {
            #region Methods

            public int Compare(PuzzleSolutionInfo x, PuzzleSolutionInfo y)
            {
                int xLevel = TetrominoManager.GetLevelOf(x.Puzzle.RewardTetromino);
                int yLevel = TetrominoManager.GetLevelOf(y.Puzzle.RewardTetromino);
                int xScore = (x.Puzzle.RewardScore + xLevel) / x.NumSteps;
                int yScore = (y.Puzzle.RewardScore + yLevel) / y.NumSteps;

                return xScore.CompareTo(yScore);
            }

            #endregion
        }
    }
}
