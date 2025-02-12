using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Kostra
{
    /// <summary>
    /// A Simple AI player that chooses the best puzzle to solve and then solves it using IDA*.
    /// </summary>
    internal class SimpleAIPlayer : AIPlayerBase
    {
        /// <summary>A random number generator.</summary>
        private static readonly Random _rnd = new();

        /// <summary>Returns a random element from the given list.</summary>
        private static T RandomElementFrom<T>(List<T> list) => list[_rnd.Next(list.Count)];

        public override void Init(string? filePath) { }
        public override TetrominoShape GetReward(List<TetrominoShape> rewardOptions)
        {
            return RandomElementFrom(rewardOptions);
        }

        /// <summary>The puzzle we are currently solving.</summary>
        private Puzzle? _currentPuzzle;

        /// <summary>The strategy to solve <see cref="_currentPuzzle"/>.</summary>
        private Queue<VerifiableAction> _currentStrategy = new();

        /// <summary>The strategy for the <see cref="GamePhase.FinishingTouches"/> game phase.</summary>
        private Queue<VerifiableAction> _finishingTouchesStrategy = null;

        public override VerifiableAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            // get an unfinished puzzle if there is one
            _currentPuzzle = myInfo.UnfinishedPuzzles.Length == 0 ? null : myInfo.UnfinishedPuzzles[0];

            switch (turnInfo.GamePhase)
            {
                case GamePhase.Normal:
                    // if no strategy --> create one
                    if (_currentStrategy.Count == 0)
                    {
                        _currentStrategy = GetStrategy(gameInfo, myInfo);
                    }

                    // if next action is valid --> submit it
                    var nextAction = _currentStrategy.Dequeue();
                    if (nextAction.GetVerifiedBy(verifier) is VerificationSuccess)
                    {
                        return nextAction;
                    }

                    // if not --> create a new strategy
                    _currentStrategy = GetStrategy(gameInfo, myInfo);
                    return _currentStrategy.Dequeue();

                case GamePhase.EndOfTheGame:
                    // if we have a puzzle --> continue solving it
                    if (_currentPuzzle is not null)
                    {
                        goto case GamePhase.Normal; 
                    }

                    // if we don't have a puzzle --> don't take a new one (negative points)
                    // try to get more tetrominos (in case tie the player with more tetrominos leftover wins)
                    VerifiableAction? action = GetValidTetrominoAction(gameInfo, myInfo);
                    if (action != null) return action;

                    // try to recycle, any action is more interesting than DoNothingAction()
                    action = GetValidRecycleAction(gameInfo);
                    if (action != null) return action;

                    // last resort
                    return new DoNothingAction();

                case GamePhase.FinishingTouches:
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
                        _finishingTouchesStrategy = new Queue<VerifiableAction>(solution);
                    }

                    // proceed with the strategy
                    return _finishingTouchesStrategy.Dequeue();

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
        /// <returns>A <see cref="TakeBasicTetrominoAction"/> if possible, else <see cref="ChangeTetrominoAction"/>. <c>null</c> if no such action is possible.</returns>
        private static TetrominoAction? GetValidTetrominoAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo)
        {
            if (gameInfo.NumTetrominosLeft[(int)TetrominoShape.O1] > 0)
            {
                return new TakeBasicTetrominoAction();
            }
            for (int i = 0; i < TetrominoManager.NumShapes; i++)
            {
                if (myInfo.NumTetrominosOwned[i] > 0)
                {
                    var options = RewardManager.GetUpgradeOptions(gameInfo.NumTetrominosLeft, (TetrominoShape)i);
                    if (options.Count > 0)
                    {
                        return new ChangeTetrominoAction((TetrominoShape)i, RandomElementFrom(options));
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a valid recycle action.
        /// </summary>
        /// <param name="gameInfo">Information about the game state.</param>
        /// <returns>A <see cref="RecycleAction"/> or <c>null</c> if no such action is possible.</returns>
        private static RecycleAction? GetValidRecycleAction(GameState.GameInfo gameInfo)
        {
            if (gameInfo.AvailableWhitePuzzles.Length > 0)
            {
                List<uint> order = gameInfo.AvailableWhitePuzzles.Select(p => p.Id).ToList();
                return new RecycleAction(order, RecycleAction.Options.White);
            }
            if (gameInfo.AvailableBlackPuzzles.Length > 0)
            {
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
        /// <param name="finishingTouches">If set to <c>true</c> the algorithm will use only the tetrominos owned by the player.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><c>(shortest solution, length)</c> if the solution was found.</item>
        ///     <item><c>(null, bound)</c> where bound is the estimated length of the shortest solution, if the goal wasn't reached within the given <c>maxDepth</c>.</item>
        ///     <item><c>(null, -1)</c> if the puzzle can't be solved using the available resources.</item>
        ///   </list>
        /// </returns>
        private static Tuple<List<VerifiableAction>?, int> SolvePuzzleWithIDAStar(Puzzle puzzle, IReadOnlyList<int> numTetrominosLeft, IReadOnlyList<int> numTetrominosOwned, int maxDepth=-1, bool finishingTouches = false)
        {
            var solution = IDAStar.IterativeDeepeningAStar(
                new PuzzleNode(puzzle.Image, puzzle.Id, numTetrominosLeft, numTetrominosOwned, finishingTouches),
                PuzzleNode.FinishedPuzzle,
                maxDepth
            );

            if (solution.Item1 is null) return new(null, solution.Item2);

            var path = new List<VerifiableAction>();
            foreach (ActionEdge<PuzzleNode> edge in solution.Item1.Cast<ActionEdge<PuzzleNode>>())
            {
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
        /// <returns>The puzzle to solve and a solution, or <c>null</c> if no puzzle can be solved.</returns>
        private static Tuple<Puzzle, List<VerifiableAction>>? ChoosePuzzle(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, int maxDepth = -1, int levelSumToConsiderBlackPuzzles = 20)
        {
            List<Puzzle> possiblePuzzles = new();

            bool ShouldChooseWhitePuzzle(IReadOnlyList<int> numTetrominosOwned)
            {
                int totalTetrominoLevel = 0;
                for (int i = 0; i < TetrominoManager.NumShapes; i++)
                {
                    int level = TetrominoManager.GetLevelOf((TetrominoShape)i);
                    totalTetrominoLevel += level * numTetrominosOwned[i];
                }

                return totalTetrominoLevel < levelSumToConsiderBlackPuzzles;
            }

            if (ShouldChooseWhitePuzzle(myInfo.NumTetrominosOwned))
            {
                possiblePuzzles.AddRange(gameInfo.AvailableWhitePuzzles);
            }
            if (possiblePuzzles.Count == 0)
            {
                possiblePuzzles.AddRange(gameInfo.AvailableBlackPuzzles);
            }
            // if there are no puzzles to choose from --> return null
            if (possiblePuzzles.Count == 0)
            {
                return null;
            }

            // choose the best puzzle
            List<PuzzleSolutionInfo> solutionInfos = new();
            foreach (Puzzle puzzle in possiblePuzzles)
            {
                var solution = SolvePuzzleWithIDAStar(puzzle, gameInfo.NumTetrominosLeft, myInfo.NumTetrominosOwned, maxDepth);
                // if puzzle has a solution --> add it to the list
                if (solution.Item1 is not null)
                {
                    solutionInfos.Add(new(puzzle, solution.Item1, solution.Item1.Count));
                }
            }

            // if there are no puzzles with a solution --> return null
            if (solutionInfos.Count == 0)
            {
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
        ///     <item>The <see cref="DoNothingAction" /> if there are no puzzles the player can solve in the current game context.
        /// </item>
        ///     <item>A strategy to (take) and solve the next puzzle otherwise.
        /// </item>
        ///   </list>
        /// </returns>
        private Queue<VerifiableAction> GetStrategy(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, int maxDepth = -1)
        {

            if (_currentPuzzle is null)
            {
                // choose puzzle
                var res = ChoosePuzzle(gameInfo, myInfo, maxDepth);
                // if there are no puzzles left --> do nothing
                if (res is null)
                {
                    return new([new DoNothingAction()]);
                }
                // else: take the puzzle and solve it
                var strategy = new Queue<VerifiableAction>();
                TakePuzzleAction takePuzzleAction = new(TakePuzzleAction.Options.Normal, res.Item1.Id);
                strategy.Enqueue(takePuzzleAction);
                foreach (var action in res.Item2)
                {
                    strategy.Enqueue(action);
                }
                return strategy;
            }
            
            // find a solution to current puzzle
            var solution = SolvePuzzleWithIDAStar(_currentPuzzle, gameInfo.NumTetrominosLeft, myInfo.NumTetrominosOwned, maxDepth).Item1;
            if (solution is null)
            {
                return new([new DoNothingAction()]);
            }

            return new(solution);
        }


        /// <summary>
        /// Represents the information about a puzzle needed to determine how advantageous would be to take and solve it.
        /// </summary>
        private record struct PuzzleSolutionInfo(Puzzle Puzzle, List<VerifiableAction> Solution, int NumSteps);

        /// <summary>
        /// Defines a method for comparing <see cref="PuzzleSolutionInfo"/> objects.
        /// </summary>
        /// <seealso cref="IComparer{T}"/>
        /// <seealso cref="PuzzleSolutionInfo"/>
        private class PuzzleComparer : IComparer<PuzzleSolutionInfo>
        {
            public int Compare(PuzzleSolutionInfo x, PuzzleSolutionInfo y)
            {
                int xLevel = TetrominoManager.GetLevelOf(x.Puzzle.RewardTetromino);
                int yLevel = TetrominoManager.GetLevelOf(y.Puzzle.RewardTetromino);
                int xScore = (x.Puzzle.RewardScore + xLevel) / x.NumSteps;
                int yScore = (y.Puzzle.RewardScore + yLevel) / y.NumSteps;

                return xScore.CompareTo(yScore);
            }
        }
    }

    /// <summary>
    /// Represents transition between two game states using an action.
    /// </summary>
    class ActionEdge<T>(T from, T to, IReadOnlyList<VerifiableAction> actions) : IEdge<T> where T : INode<T>
    {
        /// <summary>
        /// The original game state.
        /// </summary>
        public T From => from;

        /// <summary>
        /// The new game state.
        /// </summary>
        public T To => to;

        /// <summary>
        /// The number of actions needed to get from <see cref="From"/> to <see cref="To"/>.
        /// </summary>
        public int Cost => actions.Count;

        /// <summary>
        /// The actions needed to get from <see cref="From"/> to <see cref="To"/>.
        /// </summary>
        public IReadOnlyList<VerifiableAction> Action => actions;
    }

    /// <summary>
    /// Represents a puzzle being solved by a player.
    /// </summary>
    class PuzzleNode(BinaryImage puzzle, uint puzzleId, IReadOnlyList<int> numTetrominosLeft, IReadOnlyList<int> numTetrominosOwned, bool finishingTouches) : INode<PuzzleNode>
    {
        public int Id => _puzzle.GetHashCode();
        public uint PuzzleId => puzzleId;

        /// <summary>
        /// Represents a puzzle that has been completed.
        /// </summary>
        public static PuzzleNode FinishedPuzzle => new(BinaryImage.FullImage, 0, null, null, false);

        // capture puzzle and numTetrominosOwned for heuristic (its static)
        private readonly BinaryImage _puzzle = puzzle; 
        private readonly IReadOnlyList<int> _numTetrominosOwned = numTetrominosOwned; 

        public static int Heuristic(PuzzleNode node, PuzzleNode goal)
        {
            // how many tetrominos we need to place to finish the puzzle
            // we also might need to take some tetrominos from the bank
            // simplify the problem --> we just need to fill in X cells and tetromino of level L can fill in L cells

            int numCellsToFillIn = goal._puzzle.CountFilledCells() - node._puzzle.CountFilledCells();
            int[] numShapesOfLevelOwned = new int[TetrominoManager.MaxLevel + 1];
            for (int i = 0; i < TetrominoManager.NumShapes; i++)
            {
                numShapesOfLevelOwned[TetrominoManager.GetLevelOf((TetrominoShape)i)] += node._numTetrominosOwned[i];
            }
            int[] numShapesOfLevelUsed = new int[TetrominoManager.MaxLevel + 1];
            // put in the largest shapes we can
            for (int level = TetrominoManager.MaxLevel; level >= TetrominoManager.MinLevel; level--)
            {
                int numUsed = Math.Min(numShapesOfLevelOwned[level], numCellsToFillIn / level);
                numCellsToFillIn -= level * numUsed;
                numShapesOfLevelUsed[level] = numUsed;
                numShapesOfLevelOwned[level] -= numUsed;
            }

            return numShapesOfLevelUsed.Sum() + GetStepsToFixDiff(numCellsToFillIn);

            // fix the difference
            int GetStepsToFixDiff(int diff)
            {
                if (diff == 0) return 0;
                // if we have used everything --> assume we could have upgraded the tetrominos used to make up the difference
                if (numShapesOfLevelOwned.Sum() == 0) return diff;

                // we have not used everything --> the difference can be 1, 2 or 3

                // if diff == 1, we can upgrade a used tetromino (+1) or perhaps there could a change we could do 
                // e.g. use 2 instead of 3 and than use 2 again --> still result +1
                if (diff == 1) return 1;

                // if diff == 2, we dont have any level 1 or 2 tetrominos
                // if we have used a level 4 and still have two level 3, we can do: (4, diff=2, price=n+2) -> (3,3, diff=0, price=n+1)
                if (diff == 2)
                {
                    if (numShapesOfLevelUsed[4] > 0 && numShapesOfLevelOwned[3] >= 2) return 1;
                    return 2;
                }

                if (diff != 3)
                {
                    throw new InvalidOperationException("Invalid diff - should never happen");
                }

                // if diff == 3, one of the following must be true
                // 1. I have used everything (already taken care of)
                // 2. I had only level 4 tetrominos
                //    a) original sum == 3 and I had no tetrominos at all --> need to upgrade to a level 3 from nothing

                if (numShapesOfLevelUsed[4] == 0 && numShapesOfLevelOwned[4] == 0) return 4;

                //    b) original sum == 3 and I had a level 4 tetromino --> downgrade to level 3 and use it
                //    c) original sum == 4*k + 3 and I still have a level 4 tetromino --> downgrade to level 3 and use it

                return 2;
            }
        }

        // cache the edges to avoid recalculating them
        private List<ActionEdge<PuzzleNode>>? _getEdgesCache = null;

        /// <summary>
        /// Returns the possible transitions from this node. To get to a neighbor, the use of a <see cref="PlaceTetrominoAction"/> is required. 
        /// This ensures that there are no loops in the graph. (<see cref="PuzzleNode.Id"/> is based on how filled in the puzzle is).
        /// </summary>
        public IEnumerable<IEdge<PuzzleNode>> GetEdges()
        {
            _getEdgesCache ??= GetEdgesEnumerable().ToList();
            return _getEdgesCache;
        }
        private IEnumerable<ActionEdge<PuzzleNode>> GetEdgesEnumerable()
        {
            // foreach tetromino shape
            for (int i = 0; i < TetrominoManager.NumShapes; i++)
            {
                var newNumTetrominosLeft = numTetrominosLeft.ToArray();
                var newNumTetrominosOwned = numTetrominosOwned.ToArray();

                // if we have the shape --> try placing it in all possible positions
                if (numTetrominosOwned[i] > 0)
                {
                    newNumTetrominosOwned[i]--;

                    foreach (var placement in GetAllValidPlacements(puzzle, (TetrominoShape)i))
                    {
                        var newPuzzleNode = new PuzzleNode(puzzle | placement.Position, puzzleId, numTetrominosLeft, newNumTetrominosOwned, finishingTouches);
                        yield return new ActionEdge<PuzzleNode>(this, newPuzzleNode, [placement]);
                    }
                    continue;
                }

                // if FinishingTouches --> can only used the shapes we own --> continue
                if (finishingTouches) continue;

                // if there are no tetrominos of this shape left --> continue
                if (numTetrominosLeft[i] == 0) continue;

                // if we don't have the basic shape
                if (i == (int)TetrominoShape.O1)
                {
                    newNumTetrominosLeft[i]--;

                    foreach (var placement in GetAllValidPlacements(puzzle, (TetrominoShape)i))
                    {
                        var newPuzzleNode = new PuzzleNode(puzzle | placement.Position, puzzleId, newNumTetrominosLeft, numTetrominosOwned, finishingTouches);
                        yield return new ActionEdge<PuzzleNode>(this, newPuzzleNode, [new TakeBasicTetrominoAction(), placement]);
                    }
                    continue; 
                }

                // if we don't have a more complex shape --> try to upgrade to it
                var upgradePath = GetUpgradePathTo((TetrominoShape)i);
                if (upgradePath is null) continue;

                // adjust the game state
                TetrominoAction firstAction = upgradePath[0];
                if (firstAction is TakeBasicTetrominoAction)
                {
                    newNumTetrominosLeft[(int)TetrominoShape.O1]--;
                }
                else if (firstAction is ChangeTetrominoAction changeAction)
                {
                    int firstShapeTraded = (int)changeAction.OldTetromino;
                    newNumTetrominosOwned[firstShapeTraded]--;
                    newNumTetrominosLeft[firstShapeTraded]++;
                    newNumTetrominosLeft[i]--;
                }

                foreach (var placement in GetAllValidPlacements(puzzle, (TetrominoShape)i))
                {
                    var newPuzzleNode = new PuzzleNode(puzzle | placement.Position, puzzleId, newNumTetrominosLeft, numTetrominosOwned, finishingTouches);
                    yield return new ActionEdge<PuzzleNode>(this, newPuzzleNode, new List<VerifiableAction>(upgradePath) { placement });
                }
            }
        }

        /// <summary>
        /// Gets all valid placements of the given tetromino shape to the given puzzle.
        /// </summary>
        /// <param name="puzzle">The puzzle.</param>
        /// <param name="shape">The shape.</param>
        /// <returns>A list of placements.</returns>
        private List<PlaceTetrominoAction> GetAllValidPlacements(BinaryImage puzzle, TetrominoShape shape)
        {
            return TetrominoManager.GetAllUniqueConfigurationsOf(shape) // get all possible placements
                .FindAll(tetromino => (puzzle & tetromino) == BinaryImage.EmptyImage) // find valid ones
                .Select(tetromino => new PlaceTetrominoAction(PuzzleId, shape, tetromino)) // create actions
                .ToList();
        }

        /// <summary>
        /// Gets a strategy on how to get the given shape based on the shapes the player already owns..
        /// </summary>
        /// <param name="shape">The goal shape.</param>
        /// <returns>The strategy on how to get the shape or <c>null</c> if it isn't possible.</returns>
        private List<TetrominoAction>? GetUpgradePathTo(TetrominoShape shape)
        {
            // get a shape of the closest level
            int level = TetrominoManager.GetLevelOf(shape);
            TetrominoShape? closestShape = null;

            for (int i = 0; i < TetrominoManager.NumShapes; i++)
            {
                if (numTetrominosOwned[i] == 0) continue;
                if (closestShape is null)
                {
                    closestShape = (TetrominoShape)i;
                    continue;
                }
                if (Math.Abs(TetrominoManager.GetLevelOf((TetrominoShape)i) - level) < Math.Abs(TetrominoManager.GetLevelOf(closestShape.Value) - level))
                {
                    closestShape = (TetrominoShape)i;
                }
            }

            List<TetrominoAction> strategy;

            // if I have no tetrominos
            if (closestShape == null)
            {
                // if there are no level1 tetrominos left --> we cant do anything
                if (numTetrominosLeft[(int)TetrominoShape.O1] == 0) return null;

                // otherwise take a level1 tetromino and then upgrade it
                closestShape = TetrominoShape.O1;
                strategy = [new TakeBasicTetrominoAction()];
            }
            else
            {
                strategy = [];
            }

            // use IDA* to find the path
            var start = new ShapeNode(closestShape.Value, numTetrominosLeft);
            var goal = new ShapeNode(shape, null);
            var path = IDAStar.IterativeDeepeningAStar(start, goal).Item1;

            // if there is no path --> return null
            if (path is null) return null;

            // add the path to the strategy
            strategy.AddRange(path.Cast<ActionEdge<ShapeNode>>().Select(edge => (ChangeTetrominoAction)edge.Action[0]));

            return strategy;
        }
    }

    /// <summary>
    /// Represents a tetromino in the shared reserve.
    /// </summary>
    class ShapeNode(TetrominoShape shape, IReadOnlyList<int> numTetrominosLeft) : INode<ShapeNode>
    {
        public int Id => (int)shape;

        public static int Heuristic(ShapeNode node, ShapeNode goal) => node.Id == goal.Id ? 0 : 1;

        /// <summary>
        /// Returns the possible <see cref="ChangeTetrominoAction"/> actions that can be taken from this node.
        /// </summary>
        public IEnumerable<IEdge<ShapeNode>> GetEdges()
        {
            foreach (TetrominoShape newShape in RewardManager.GetUpgradeOptions(numTetrominosLeft, shape))
            {
                var newNumTetrominosLeft = numTetrominosLeft.ToArray();
                newNumTetrominosLeft[(int)newShape]--;
                newNumTetrominosLeft[(int)shape]++;
                var newShapeNode = new ShapeNode(newShape, newNumTetrominosLeft);
                yield return new ActionEdge<ShapeNode>(this, newShapeNode, [new ChangeTetrominoAction(shape, newShape)]);
            }
        }
    }
}
