#nullable enable

namespace ProjectL
{
    using UnityEngine;

    /// <summary>
    /// A base class for creating static instances of MonoBehaviour-derived classes.
    /// Ensures that only one instance of the class exists at a time.
    /// </summary>
    /// <typeparam name="T">The type of the MonoBehaviour-derived class.</typeparam>
    public class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// Gets the static instance of the class.
        /// </summary>
        public static T Instance { get; private set; } = null!;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Ensures that the instance is assigned to the current object.
        /// </summary>
        protected virtual void Awake()
        {
            T? t = this as T;
            if (t == null) {
                Debug.LogError($"The object {name} is not of type {typeof(T)}.");
                return;
            }
            Instance = t;
        }

        /// <summary>
        /// Called when the MonoBehaviour will be destroyed.
        /// Resets the static instance if it matches the current object.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (Instance == this) {
                Instance = null!;
            }
        }
    }

    /// <summary>
    /// A class for creating singleton instances of MonoBehaviour-derived classes.
    /// Ensures that only one instance exists and destroys duplicates.
    /// </summary>
    /// <typeparam name="T">The type of the MonoBehaviour-derived class.</typeparam>
    public class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Ensures that only one instance exists and destroys any duplicate objects.
        /// </summary>
        protected override void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            base.Awake();
        }
    }

    /// <summary>
    /// A class for creating persistent singleton instances of MonoBehaviour-derived classes.
    /// Ensures that the instance persists across scene loads.
    /// </summary>
    /// <typeparam name="T">The type of the MonoBehaviour-derived class.</typeparam>
    public class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Called when the script instance is being loaded.
        /// Ensures that the instance persists across scene loads if it is the current instance.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this) {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}