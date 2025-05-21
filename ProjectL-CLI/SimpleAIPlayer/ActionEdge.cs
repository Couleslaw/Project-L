namespace SimpleAIPlayer
{
    using ProjectLCore.GameActions;
    using System.Collections.Generic;

    /// <summary>
    /// Represents transition between two states using a sequence of <see cref="GameAction"/> actions.
    /// </summary>
    internal class ActionEdge<T> : IEdge<T> where T : INode<T>
    {

        /// <param name="from">The original state.</param>
        /// <param name="to">The new state.</param>
        /// <param name="actions">The actions needed to get from <paramref name="from"/> to <paramref name="to"/>.</param>
        public ActionEdge(T from, T to, IReadOnlyList<GameAction> actions)
        {
            From = from;
            To = to;
            Action = actions;
        }
        #region Properties

        /// <summary>
        /// The original state.
        /// </summary>
        public T From { get; }

        /// <summary>
        /// The new state.
        /// </summary>
        public T To { get; }

        /// <summary>
        /// The number of actions needed to get from <see cref="From"/> to <see cref="To"/>.
        /// </summary>
        public int Cost => Action.Count;

        /// <summary>
        /// The actions needed to get from <see cref="From"/> to <see cref="To"/>.
        /// </summary>
        public IReadOnlyList<GameAction> Action { get; }

        #endregion
    }
}
