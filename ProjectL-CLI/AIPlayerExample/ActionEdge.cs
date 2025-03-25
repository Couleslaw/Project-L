namespace AIPlayerExample
{
    using ProjectLCore.GameActions;
    using System.Collections.Generic;

    /// <summary>
    /// Represents transition between two states using a sequence of <see cref="IAction"/> actions.
    /// </summary>
    /// <param name="from">The original state.</param>
    /// <param name="to">The new state.</param>
    /// <param name="actions">The actions needed to get from <paramref name="from"/> to <paramref name="to"/>.</param>
    internal class ActionEdge<T>(T from, T to, IReadOnlyList<IAction> actions) : IEdge<T> where T : INode<T>
    {
        #region Properties

        /// <summary>
        /// The original state.
        /// </summary>
        public T From => from;

        /// <summary>
        /// The new state.
        /// </summary>
        public T To => to;

        /// <summary>
        /// The number of actions needed to get from <see cref="From"/> to <see cref="To"/>.
        /// </summary>
        public int Cost => actions.Count;

        /// <summary>
        /// The actions needed to get from <see cref="From"/> to <see cref="To"/>.
        /// </summary>
        public IReadOnlyList<IAction> Action => actions;

        #endregion
    }
}
