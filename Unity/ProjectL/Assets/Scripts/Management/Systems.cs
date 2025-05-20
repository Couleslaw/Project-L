#nullable enable

namespace ProjectL.Management
{
    /// <summary>
    /// The only persistent singleton in the game. This class is used to prevent the <c>Systems</c> prefab from being destroyed when loading a new scene.
    /// </summary>
    /// <seealso cref="ProjectL.PersistentSingleton&lt;ProjectL.Management.Systems&gt;" />
    public class Systems : PersistentSingleton<Systems>
    {

    }
}
