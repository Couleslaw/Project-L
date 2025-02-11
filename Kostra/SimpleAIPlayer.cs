using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Kostra
{
    internal class SimpleAIPlayer : AIPlayerBase
    {
        private static readonly Random _rnd = new();
        private static T RandomElementFrom<T>(IList<T> list)
        {
            return list[_rnd.Next(list.Count)];
        }
        public override void Init(string? filePath) { }
        public override TetrominoShape GetReward(List<TetrominoShape> rewardOptions)
        {
            return RandomElementFrom(rewardOptions);
        }

        private Puzzle? _currentPuzzle;
        private Queue<VerifiableAction> _currentStrategy = new();
        private Queue<VerifiableAction> _finishingTouchesStrategy = null;
        public override VerifiableAction GetAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, List<PlayerState.PlayerInfo> enemyInfos, TurnInfo turnInfo, ActionVerifier verifier)
        {
            // get unfinished puzzle
            _currentPuzzle = myInfo.UnfinishedPuzzles.Length == 0 ? null : myInfo.UnfinishedPuzzles[0];

            switch (turnInfo.GamePhase)
            {
                case GamePhase.Normal:
                    // if no stratefy --> create one
                    if (_currentStrategy.Count == 0)
                    {
                        _currentStrategy = GetStrategy(gameInfo, myInfo);
                    }

                    // if next action is valid --> submit it
                    var nextAction = _currentStrategy.Dequeue();
                    if (verifier.Verify(nextAction) is VerificationSuccess)
                    {
                        return nextAction;
                    }

                    // if not --> create a new strategy
                    _currentStrategy = GetStrategy(gameInfo, myInfo);
                    return _currentStrategy.Dequeue();

                case GamePhase.EndOfTheGame:
                    // if we have puzzle --> continue solving it
                    if (_currentPuzzle is not null)
                    {
                        goto case GamePhase.Normal; 
                    }

                    // if we dont have a puzzle --> dont take a new one (negative points)
                    // try to get more tetrominos (in case tie the player with more tetrominos leftover wins)
                    var action = GetValidTetrominoAction(gameInfo, myInfo);
                    if (action != null) return action;
                    // try to recycle, any action is more interesting than DoNothingAction()
                    action = GetValidRecycleAction(gameInfo);
                    if (action != null) return action;
                    // last resort
                    return new DoNothingAction();

                case GamePhase.FinishingTouches:
                    // if no puzzles left --> end the game
                    if (_currentPuzzle is null) {
                        return new EndFinishingTouchesAction();
                    }

                    // if I dont have a strategy --> create one
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

        private static VerifiableAction? GetValidTetrominoAction(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo)
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

        private Queue<VerifiableAction> GetStrategy(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, int maxDepth = -1)
        {

            if (_currentPuzzle is null)
            {
                // choose puzzle
                var res = ChoosePuzzle(gameInfo, myInfo, maxDepth);
                // if there are no puzzles left --> do nothing
                if (res.Item1 is null)
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

        private int IDAStarHeuristic(PuzzleNode node, PuzzleNode goal)
        {
            return 0;
        }

        private Tuple<List<VerifiableAction>?, int> SolvePuzzleWithIDAStar(Puzzle puzzle, IReadOnlyList<int> numTetrominosLeft, IReadOnlyList<int> numTetrominosOwned, int maxDepth=-1, bool finishingTouches = false)
        {
            var solution = IDAStar.IterativeDeepeningAStar(
                new PuzzleNode(puzzle.Image, puzzle.Id, numTetrominosLeft, numTetrominosOwned, finishingTouches),
                PuzzleNode.FinishedPuzzle,
                IDAStarHeuristic,
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

        private Tuple<Puzzle?, List<VerifiableAction>> ChoosePuzzle(GameState.GameInfo gameInfo, PlayerState.PlayerInfo myInfo, int maxDepth = -1)
        {
            List<Puzzle> possiblePuzzles = new();

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
                return new(null, []);
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
                return new(null, []);
            }

            // sort the solution infos using the PuzzleComparor
            solutionInfos.Sort(new PuzzleComparor());
            // choose the best puzzle = the one with the highest reward
            var best = solutionInfos[^1];
            return new(best.Puzzle, best.Solution);

        }
        private static bool ShouldChooseWhitePuzzle(IReadOnlyList<int> numTetrominosOwned)
        {
            int totalTetrominoLevel = 0;
            for (int i = 0; i < TetrominoManager.NumShapes; i++)
            {
                int level = TetrominoManager.GetLevelOf((TetrominoShape)i);
                totalTetrominoLevel += level * numTetrominosOwned[i];
            }

            return totalTetrominoLevel <= 20;
        }

        private record struct PuzzleSolutionInfo(Puzzle Puzzle, List<VerifiableAction> Solution, int NumSteps);

        private class PuzzleComparor : IComparer<PuzzleSolutionInfo>
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

    class PuzzleNode(BinaryImage puzzle, uint puzzleId, IReadOnlyList<int> numTetrominosLeft, IReadOnlyList<int> numTetrominosOwned, bool finishingTouches) : INode<PuzzleNode>
    {
        public uint PuzzleId => puzzleId;
        public static PuzzleNode FinishedPuzzle => new(BinaryImage.FullImage, 0, null, null, false);

        public int Id => puzzle.GetHashCode();

        private List<ActionEdge<PuzzleNode>>? _getEdgesCache = null;
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

                // if we dont have the basic shape
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

                // if we dont have a more complex shape --> try to upgrade to it
                var upgradePath = GetUpgradePathTo((TetrominoShape)i);
                if (upgradePath is null) continue;

                int firstShapeTrated = (int)upgradePath[0].OldTetromino;
                newNumTetrominosOwned[firstShapeTrated]--;
                newNumTetrominosLeft[firstShapeTrated]++;
                newNumTetrominosLeft[i]--;

                foreach (var placement in GetAllValidPlacements(puzzle, (TetrominoShape)i))
                {
                    var newPuzzleNode = new PuzzleNode(puzzle | placement.Position, puzzleId, newNumTetrominosLeft, numTetrominosOwned, finishingTouches);
                    yield return new ActionEdge<PuzzleNode>(this, newPuzzleNode, new List<VerifiableAction>(upgradePath) { placement });
                }
            }
        }

        private List<PlaceTetrominoAction> GetAllValidPlacements(BinaryImage puzzle, TetrominoShape shape)
        {
            return TetrominoManager.GetAllUniqueConfigurationsOf(shape) // get all possible placements
                .FindAll(tetromino => (puzzle & tetromino) == BinaryImage.EmptyImage) // find valid ones
                .Select(tetromino => new PlaceTetrominoAction(PuzzleId, shape, tetromino)) // create actions
                .ToList();
        }

        private List<ChangeTetrominoAction>? GetUpgradePathTo(TetrominoShape shape)
        {
            // if I have nothing --> return null
            if (numTetrominosOwned.Sum() == 0) return null;

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

            // use IDA* to find the path
            var start = new ShapeNode(closestShape!.Value, numTetrominosLeft);
            var goal = new ShapeNode(shape, null);
            var path = IDAStar.IterativeDeepeningAStar(start, goal, null).Item1;

            // if there is no path --> return null
            if (path is null) return null;

            // convert the path to an array of actions
            return path.Cast<ActionEdge<ShapeNode>>().Select(edge => (ChangeTetrominoAction)edge.Action[0]).ToList();
        }
    }

    class ShapeNode(TetrominoShape shape, IReadOnlyList<int> numTetrominosLeft) : INode<ShapeNode>
    {
        public int Id => (int)shape;
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

    class ActionEdge<T>(T from, T to, IReadOnlyList<VerifiableAction> actions) : IEdge<T> where T : INode<T>
    {
        public T From => from;
        public T To => to;
        public int Cost => actions.Count;
        public IReadOnlyList<VerifiableAction> Action => actions;
    }
}
