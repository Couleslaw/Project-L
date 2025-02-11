using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kostra
{
    public interface INode<TSelf> where TSelf : INode<TSelf>
    {
        public int Id { get; }
        public IEnumerable<IEdge<TSelf>> GetEdges();
    }

    public interface IEdge<T> where T : INode<T>
    {
        public T From { get; }
        public T To { get; }
        public int Cost { get; }
    }

    public class IDAStar
    {
        // returns the path from start to goal or null if no path found
        // also returns the cost of the path (or the bound if no path found)
        // maxDepth <= 0 means no limit
        public static Tuple<List<IEdge<T>>?, int> IterativeDeepeningAStar<T>(T start, T goal, Func<T, T, int>? heuristic, int maxDepth = -1) where T : INode<T>
        {
            heuristic ??= static (a, b) => 0;

            int bound = heuristic(start, goal);
            var path = new List<IEdge<T>>();
            while (true)
            {
                var result = Search(start, goal, heuristic, 0, bound, path, maxDepth);
                if (result == -1)
                {
                    return new(path, path.Count);
                }
                if (result == bound)
                {
                    return new(null, bound); // No path found
                }

                bound = result;
                if (maxDepth >= 0 && bound > maxDepth)
                {
                    break; // Stop if the bound exceeds maxDepth
                }
            }
            return new(null, bound); // No path found within maxDepth
        }
        private static int Search<T>(T node, T goal, Func<T, T, int> heuristic, int g, int bound, List<IEdge<T>> path, int maxDepth) where T : INode<T>
        {
            int f = g + heuristic(node, goal);
            if (f > bound)
            {
                return f; // Cut off; return the new bound
            }
            if (node.Id == goal.Id)
            {
                return -1; // Goal found
            }
            int min = int.MaxValue;
            foreach (var edge in node.GetEdges())
            {
                path.Add(edge);
                var result = Search(edge.To, goal, heuristic, g + edge.Cost, bound, path, maxDepth);
                if (result == -1)
                {
                    return -1; // Goal found
                }
                if (result < min)
                {
                    min = result;
                }
                path.RemoveAt(path.Count - 1);
            }
            return min;
        }
    }
}
