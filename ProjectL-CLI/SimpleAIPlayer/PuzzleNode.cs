namespace SimpleAIPlayer
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a puzzle being solved by a player.
    /// </summary>
    internal class PuzzleNode : INode<PuzzleNode>
    {
        #region Fields

        // capture puzzle and numTetrominosOwned for heuristic (its static)
        private readonly BinaryImage _puzzle;

        private readonly IReadOnlyList<int> _numTetrominosLeft;

        private readonly IReadOnlyList<int> _numTetrominosOwned;

        private readonly bool _finishingTouches;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleNode"/> class.
        /// </summary>
        /// <param name="puzzle">The puzzle being solved.</param>
        /// <param name="puzzleId">The ID of the puzzle being solved.</param>
        /// <param name="numTetrominosLeft">The number of tetrominos left in the shared reserve for each <see cref="TetrominoShape"/>.</param>
        /// <param name="numTetrominosOwned">The number of tetrominos owned by the player for each <see cref="TetrominoShape"/>.</param>
        /// <param name="finishingTouches"><see langword="true"/> if <see cref="GameCore.CurrentGamePhase"/> is <see cref="GamePhase.FinishingTouches"/> else <see langword="false"/>.</param>
        public PuzzleNode(BinaryImage puzzle, uint puzzleId, IReadOnlyList<int> numTetrominosLeft, IReadOnlyList<int> numTetrominosOwned, bool finishingTouches)
        {
            _puzzle = puzzle;
            PuzzleId = puzzleId;
            _numTetrominosOwned = numTetrominosOwned;
            _numTetrominosLeft = numTetrominosLeft;
            _finishingTouches = finishingTouches;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Represents a puzzle that has been completed.
        /// </summary>
        public static PuzzleNode FinishedPuzzle => new(BinaryImage.FullImage, 0, Array.Empty<int>(), Array.Empty<int>(), false);

        /// <summary>
        /// The ID of the node. Unique for each puzzle configuration.
        /// </summary>
        public int Id => _puzzle.GetHashCode();

        /// <summary>
        /// The ID of the puzzle represented by this node.
        /// </summary>
        public uint PuzzleId { get; }

        /// <summary>
        /// The number of tetrominos left in the shared reserve for each <see cref="TetrominoShape"/>.
        /// </summary>
        public int[] NumTetrominosLeft => _numTetrominosLeft.ToArray();

        /// <summary>
        /// The number of tetrominos owned by the player for each <see cref="TetrominoShape"/>.
        /// </summary>
        public int[] NumTetrominosOwned => _numTetrominosOwned.ToArray();

        #endregion

        #region Methods

        /// <summary>
        /// Heuristic function to estimate distances between this node an the <paramref name="other"/> node.
        /// </summary>
        /// <param name="other">The node to estimate the distance to.</param>
        /// <returns>
        /// The estimated distance between the nodes.
        /// </returns>
        public int Heuristic(PuzzleNode other)
        {
            // how many tetrominos we need to place to finish the puzzle
            // we also might need to take some tetrominos from the bank
            // simplify the problem --> we just need to fill in X cells and tetromino of level L can fill in L cells

            int numCellsToFillIn = other._puzzle.CountFilledCells() - _puzzle.CountFilledCells();
            int[] numShapesOfLevelOwned = new int[TetrominoManager.MaxLevel + 1];
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                numShapesOfLevelOwned[TetrominoManager.GetLevelOf((TetrominoShape)i)] += _numTetrominosOwned[i];
            }

            // put in the largest shapes we can
            int[] numShapesOfLevelUsed = new int[TetrominoManager.MaxLevel + 1];
            for (int level = TetrominoManager.MaxLevel; level >= TetrominoManager.MinLevel; level--) {
                int numUsed = Math.Min(numShapesOfLevelOwned[level], numCellsToFillIn / level);
                numCellsToFillIn -= level * numUsed;
                numShapesOfLevelUsed[level] = numUsed;
                numShapesOfLevelOwned[level] -= numUsed;
            }

            return numShapesOfLevelUsed.Sum() + GetStepsToFixDiff(numCellsToFillIn);

            // fix the difference
            int GetStepsToFixDiff(int diff)
            {
                if (diff == 0) {
                    return 0;
                }
                // if we have used everything --> assume we could have upgraded the tetrominos used to make up the difference
                if (numShapesOfLevelOwned.Sum() == 0) {
                    return diff;
                }

                // we have not used everything --> the difference can be 1, 2 or 3

                // if diff == 1, we can upgrade a used tetromino (+1) or perhaps there could a change we could do 
                // e.g. use 2 instead of 3 and than use 2 again --> still result +1
                if (diff == 1) {
                    return 1;
                }

                // if diff == 2, we dont have any level 1 or 2 tetrominos
                // if we have used a level 4 and still have two level 3, we can do: (4, diff=2, price=n+2) -> (3,3, diff=0, price=n+1)
                if (diff == 2) {
                    if (numShapesOfLevelUsed[4] > 0 && numShapesOfLevelOwned[3] >= 2)
                        return 1;
                    return 2;
                }

                if (diff != 3) {
                    throw new InvalidOperationException("Invalid diff - should never happen");
                }

                // if diff == 3, one of the following must be true
                // 1. I have used everything (already taken care of)
                // 2. I had only level 4 tetrominos
                //    a) original sum == 3 and I had no tetrominos at all --> need to upgrade to a level 3 from nothing

                if (numShapesOfLevelUsed[4] == 0 && numShapesOfLevelOwned[4] == 0) {
                    return 4;
                }

                //    b) original sum == 3 and I had a level 4 tetromino --> downgrade to level 3 and use it
                //    c) original sum == 4*k + 3 and I still have a level 4 tetromino --> downgrade to level 3 and use it

                return 2;
            }
        }

        /// <summary>
        /// Generates the possible transitions from this node. To get to a neighbor, the use of a <see cref="PlaceTetrominoAction"/> is required. 
        /// This ensures that there are no loops in the graph. (<see cref="Id"/> is based on how filled in the puzzle is).
        /// </summary>
        /// <returns>An enumerable collection of the incident edges.</returns>
        public IEnumerable<IEdge<PuzzleNode>> GetEdges()
        {
            // foreach tetromino shape
            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                var newNumTetrominosLeft = _numTetrominosLeft.ToArray();
                var newNumTetrominosOwned = _numTetrominosOwned.ToArray();

                // if we have the shape --> try placing it in all possible positions
                if (_numTetrominosOwned[i] > 0) {
                    newNumTetrominosOwned[i]--;

                    foreach (var placement in GetAllValidPlacements(_puzzle, (TetrominoShape)i)) {
                        var newPuzzleNode = new PuzzleNode(_puzzle | placement.Position, PuzzleId, _numTetrominosLeft, newNumTetrominosOwned, _finishingTouches);
                        var action = new List<GameAction>() { placement };
                        yield return new ActionEdge<PuzzleNode>(this, newPuzzleNode, action);
                    }
                    continue;
                }

                // if FinishingTouches --> can only used the shapes we own --> continue
                if (_finishingTouches) {
                    continue;
                }

                // if there are no tetrominos of this shape left --> continue
                if (_numTetrominosLeft[i] == 0) {
                    continue;
                }

                // if we don't have the basic shape
                if (i == (int)TetrominoShape.O1) {
                    newNumTetrominosLeft[i]--;

                    foreach (var placement in GetAllValidPlacements(_puzzle, (TetrominoShape)i)) {
                        var newPuzzleNode = new PuzzleNode(_puzzle | placement.Position, PuzzleId, newNumTetrominosLeft, _numTetrominosOwned, _finishingTouches);
                        var actions = new List<GameAction>() { new TakeBasicTetrominoAction(), placement };
                        yield return new ActionEdge<PuzzleNode>(this, newPuzzleNode, actions);
                    }
                    continue;
                }

                // if we don't have a more complex shape --> try to upgrade to it
                var upgradePath = GetUpgradePathTo((TetrominoShape)i);
                if (upgradePath is null)
                    continue;

                // adjust the game state
                TetrominoAction firstAction = upgradePath[0];
                if (firstAction is TakeBasicTetrominoAction) {
                    newNumTetrominosLeft[i]--;
                }
                else if (firstAction is ChangeTetrominoAction changeAction) {
                    int firstShapeTraded = (int)changeAction.OldTetromino;
                    newNumTetrominosOwned[firstShapeTraded]--;
                    newNumTetrominosLeft[firstShapeTraded]++;
                    newNumTetrominosLeft[i]--;
                }

                foreach (var placement in GetAllValidPlacements(_puzzle, (TetrominoShape)i)) {
                    var newPuzzleNode = new PuzzleNode(_puzzle | placement.Position, PuzzleId, newNumTetrominosLeft, newNumTetrominosOwned, _finishingTouches);
                    yield return new ActionEdge<PuzzleNode>(this, newPuzzleNode, new List<GameAction>(upgradePath) { placement });
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
            return TetrominoManager.GetAllConfigurationsOf(shape) // get all possible placements
                .FindAll(tetromino => (puzzle & tetromino) == BinaryImage.EmptyImage) // find valid ones
                .Select(tetromino => new PlaceTetrominoAction(PuzzleId, shape, tetromino)) // create actions
                .ToList();
        }

        /// <summary>
        /// Gets a strategy on how to get the given shape based on the shapes the player already owns..
        /// </summary>
        /// <param name="shape">The goal shape.</param>
        /// <returns>The strategy on how to get the shape or <see langword="null"/> if it isn't possible.</returns>
        private List<TetrominoAction>? GetUpgradePathTo(TetrominoShape shape)
        {
            // get a shape of the closest level
            int level = TetrominoManager.GetLevelOf(shape);
            TetrominoShape? closestShape = null;

            for (int i = 0; i < TetrominoManager.NumShapes; i++) {
                if (_numTetrominosOwned[i] == 0)
                    continue;
                if (closestShape is null) {
                    closestShape = (TetrominoShape)i;
                    continue;
                }
                if (Math.Abs(TetrominoManager.GetLevelOf((TetrominoShape)i) - level) < Math.Abs(TetrominoManager.GetLevelOf(closestShape.Value) - level)) {
                    closestShape = (TetrominoShape)i;
                }
            }

            List<TetrominoAction> strategy = new();

            // if I have no tetrominos
            if (closestShape == null) {
                // if there are no level1 tetrominos left --> we cant do anything
                if (_numTetrominosLeft[(int)TetrominoShape.O1] == 0)
                    return null;

                // otherwise take a level1 tetromino and then upgrade it
                closestShape = TetrominoShape.O1;
                strategy.Add(new TakeBasicTetrominoAction());
            }

            // use IDA* to find the path
            var start = new ShapeNode(closestShape.Value, _numTetrominosLeft);
            var goal = new ShapeNode(shape, Array.Empty<int>());
            var path = IDAStar.IterativeDeepeningAStar(start, goal).Item1;

            // if there is no path --> return null
            if (path is null) {
                return null;
            }

            // add the path to the strategy
            strategy.AddRange(path.Cast<ActionEdge<ShapeNode>>().Select(edge => (ChangeTetrominoAction)edge.Action[0]));

            return strategy;
        }

        #endregion
    }
}
