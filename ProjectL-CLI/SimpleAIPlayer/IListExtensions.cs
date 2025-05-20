namespace AIPlayerExample
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for <see cref="IList{T}"/>.
    /// </summary>
    public static class IListExtensions
    {
        #region Fields

        private static Random _rnd = new Random();

        #endregion

        #region Methods

        /// <summary>
        /// Chooses a random element from the given list.
        /// </summary>
        /// <param name="list">The list to choose from.</param>
        /// <returns>A random element from <paramref name="list"/>.</returns>
        public static T GetRandomElement<T>(this IList<T> list)
        {
            return list[_rnd.Next(list.Count)];
        }

        #endregion
    }
}
