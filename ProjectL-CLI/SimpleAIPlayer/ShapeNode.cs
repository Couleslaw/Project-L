namespace SimpleAIPlayer
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a tetromino in the shared reserve.
    /// </summary>
    internal class ShapeNode : INode<ShapeNode>
    {
        #region Fields

        private readonly TetrominoShape _shape;

        private readonly int[] _numTetrominosLeft;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeNode"/> class.
        /// </summary>
        /// <param name="shape">The type of the tetromino.</param>
        /// <param name="numTetrominosLeft">The number of tetrominos left in the shared reserve for each <see cref="TetrominoShape"/>.</param>
        public ShapeNode(TetrominoShape shape, IReadOnlyList<int> numTetrominosLeft)
        {
            _shape = shape;
            Id = (int)shape;
            _numTetrominosLeft = numTetrominosLeft.ToArray();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the node. Unique for each <see cref="TetrominoShape"/>.
        /// </summary>
        public int Id { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Heuristic function to estimate distances between this node an the <paramref name="other"/> node.
        /// Returns 0 if the nodes are equal, otherwise returns 1.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// The estimated distance between the nodes.
        /// </returns>
        public int Heuristic(ShapeNode other) => Id == other.Id ? 0 : 1;

        /// <summary>
        /// Returns the possible <see cref="ChangeTetrominoAction"/> actions that can be taken from this node.
        /// </summary>
        /// <returns>An enumerable collection of the incident edges.</returns>
        public IEnumerable<IEdge<ShapeNode>> GetEdges()
        {
            foreach (TetrominoShape newShape in RewardManager.GetUpgradeOptions(_numTetrominosLeft, _shape)) {
                var newNumTetrominosLeft = _numTetrominosLeft.ToArray();
                newNumTetrominosLeft[(int)newShape]--;
                newNumTetrominosLeft[(int)_shape]++;
                var newShapeNode = new ShapeNode(newShape, newNumTetrominosLeft);
                var action = new List<GameAction>() { new ChangeTetrominoAction(_shape, newShape) };
                yield return new ActionEdge<ShapeNode>(this, newShapeNode, action);
            }
        }

        #endregion
    }
}
