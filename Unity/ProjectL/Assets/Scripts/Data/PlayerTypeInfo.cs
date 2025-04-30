#nullable enable

namespace ProjectL.Data
{
    using System;

    /// <summary>
    /// Represents information about a loaded player type.
    /// </summary>
    public readonly struct PlayerTypeInfo
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerTypeInfo"/> struct.
        /// </summary>
        /// <param name="type">The type of the player (Human or some kind of AI).</param>
        /// <param name="name">The display name of the player type.</param>
        /// <param name="initPath">The initialization path for the player, if any.</param>
        public PlayerTypeInfo(Type type, string name, string? initPath)
        {
            PlayerType = type;
            DisplayName = name;
            InitPath = initPath;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The type of the player.
        /// </summary>
        public Type PlayerType { get; }

        /// <summary>
        /// The display name of the player type.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The initialization path for the player, if any.
        /// </summary>
        public string? InitPath { get; }

        #endregion
    }
}