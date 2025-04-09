namespace ProjectLCore
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for <see cref="IList{T}"/>.
    /// </summary>
    public static class IListExtensions
    {
        #region Methods

        /// <summary>
        /// Shuffles the given list in place using the Fisher-Yates algorithm.
        /// </summary>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        #endregion
    }
}
