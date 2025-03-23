namespace AIPlayerExample
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a tetromino in the shared reserve.
    /// </summary>
    /// <param name="shape">The type of the tetromino.</param>
    /// <param name="numTetrominosLeft">The number of tetrominos left in the shared reserve for each <see cref="TetrominoShape"/>.</param>
    internal class ShapeNode(TetrominoShape shape, IReadOnlyList<int> numTetrominosLeft) : INode<ShapeNode>
    {
        #region Properties

        /// <summary>
        /// The ID of the node. Unique for each <see cref="TetrominoShape"/>.
        /// </summary>
        public int Id => (int)shape;

        #endregion

        #region Methods

        /// <summary>
        /// Heuristic function to estimate distances between two <see cref="ShapeNode"/> nodes.
        /// </summary>
        /// <param name="start">The start node.</param>
        /// <param name="goal">The goal node.</param>
        /// <returns>
        /// An optimistic estimate of the distance between the nodes.
        /// </returns>
        public static int Heuristic(ShapeNode start, ShapeNode goal) => start.Id == goal.Id ? 0 : 1;

        /// <summary>
        /// Returns the possible <see cref="ChangeTetrominoAction"/> actions that can be taken from this node.
        /// </summary>
        /// <returns>An enumerable collection of the incident edges.</returns>
        public IEnumerable<IEdge<ShapeNode>> GetEdges()
        {
            foreach (TetrominoShape newShape in RewardManager.GetUpgradeOptions(numTetrominosLeft, shape)) {
                var newNumTetrominosLeft = numTetrominosLeft.ToArray();
                newNumTetrominosLeft[(int)newShape]--;
                newNumTetrominosLeft[(int)shape]++;
                var newShapeNode = new ShapeNode(newShape, newNumTetrominosLeft);
                yield return new ActionEdge<ShapeNode>(this, newShapeNode, [new ChangeTetrominoAction(shape, newShape)]);
            }
        }

        #endregion
    }
}
