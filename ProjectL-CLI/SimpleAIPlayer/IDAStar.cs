namespace SimpleAIPlayer
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a node of a graph.
    /// </summary>
    /// <typeparam name="TSelf">The type of the node.</typeparam>
    public interface INode<TSelf> where TSelf : INode<TSelf>
    {
        #region Properties

        /// <summary>
        /// The ID of the node. Two nodes with the same ID are considered equal.
        /// </summary>
        public int Id { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Heuristic function to estimate distances between this node an the <paramref name="other"/> node. For IDA* to work properly, it needs to be admissible (optimistic), meaning <c>heuristic(a,b) &lt;= distance(a,b)</c>.
        /// </summary>
        /// <param name="other">The node to estimate the distance to.</param>
        /// <returns>The estimated distance between the nodes.</returns>
        public int Heuristic(TSelf other);

        /// <summary>
        /// Gets the edges incident with this node. The entire graph doesn't need to be stored in memory but it can be dynamically generated instead.
        /// </summary>
        /// <returns>An enumerable collection of the incident edges.</returns>
        public IEnumerable<IEdge<TSelf>> GetEdges();

        #endregion
    }

    /// <summary>
    /// Represents an edge in a graph.
    /// </summary>
    /// <typeparam name="T">Type of the nodes of the graph</typeparam>
    public interface IEdge<T> where T : INode<T>
    {
        #region Properties

        /// <summary>
        /// The start node of the edge.
        /// </summary>
        public T From { get; }

        /// <summary>
        /// The end node of the edge.
        /// </summary>
        public T To { get; }

        /// <summary>
        /// The cost (length) of the edge. For IDA* to work properly, it needs to be non-negative.
        /// </summary>
        public int Cost { get; }

        #endregion
    }

    /// <summary>
    /// Implementation of the Iteratives the deepening A* algorithm.
    /// </summary>
    public class IDAStar
    {
        #region Methods

        /// <summary>Iteratives the deepening A*.</summary>
        /// <typeparam name="T">The type of the nodes of the graph</typeparam>
        /// <param name="start">The starting node.</param>
        /// <param name="goal">The goal node.</param>
        /// <param name="maxDepth">The maximum depth the algorithm should go to. Negative values indicate infinite depth.</param>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><c>(shortest path, length)</c> if the path was found</item>
        ///     <item><c>(null, bound)</c> where bound is the estimated length of shortest path, if the goal wasn't reached within the given <c>maxDepth</c>.</item>
        ///     <item><c>(null, -1)</c> if there doesn't exist a path between <c>>start</c> and <c>goal</c>.</item>
        ///   </list>
        /// </returns>
        public static Tuple<List<IEdge<T>>?, int> IterativeDeepeningAStar<T>(T start, T goal, int maxDepth = -1) where T : INode<T>
        {
            int bound = start.Heuristic(goal);
            var path = new List<IEdge<T>>();
            while (true) {
                var result = Search(start, goal, 0, bound, path);
                if (result == -1) {
                    return new(path, path.Count);
                }

                // if the bound hasn't increased, the entire graph has been searched --> no path exists
                if (result == bound) {
                    return new(null, -1);
                }

                bound = result;
                if (maxDepth >= 0 && bound > maxDepth) {
                    break; // Stop if the bound exceeds maxDepth
                }
            }
            return new(null, bound); // No path found within maxDepth
        }

        /// <summary>
        /// Searches from the given node to the goal node.
        /// </summary>
        /// <typeparam name="T">Type of the node.</typeparam>
        /// <param name="node">The current node.</param>
        /// <param name="goal">The goal node.</param>
        /// <param name="g">The distance from the start node to the current node.</param>
        /// <param name="bound">The maximum search depth.</param>
        /// <param name="path">The path from the start node to the current node.</param>
        /// <returns>
        ///  <list type="bullet">
        ///     <item><c>-1</c> if the goal node was found.</item>
        ///     <item><c>newBound</c> if the goal wasn't found within the current bound.</item>
        ///   </list>
        /// </returns>
        private static int Search<T>(T node, T goal, int g, int bound, List<IEdge<T>> path) where T : INode<T>
        {
            int f = g + node.Heuristic(goal);
            if (f > bound) {
                return f; // Cut off; return the new bound
            }
            if (node.Id == goal.Id) {
                return -1; // Goal found
            }
            int min = int.MaxValue;
            foreach (var edge in node.GetEdges()) {
                path.Add(edge);
                var result = Search(edge.To, goal, g + edge.Cost, bound, path);
                if (result == -1) {
                    return -1; // Goal found
                }
                if (result < min) {
                    min = result;
                }
                path.RemoveAt(path.Count - 1);
            }
            return min;
        }

        #endregion
    }
}
