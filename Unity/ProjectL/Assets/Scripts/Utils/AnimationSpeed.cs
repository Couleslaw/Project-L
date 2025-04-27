namespace ProjectL.Utils
{
    using UnityEngine;

    public class AnimationSpeed : MonoBehaviour
    {
        #region Constants

        /// <summary>
        /// The name of the <see cref="PlayerPrefs"/> key for animation speed.
        /// </summary>
        public const string AnimationSpeedPlayerPrefKey = "AnimationSpeed";

        private const float _animationSpeedDefault = 1f;

        #endregion

        #region Properties

        /// <summary>
        /// Multiplier for the animation speed.
        /// </summary>
        public static float Multiplier => 1f / PlayerPrefs.GetFloat(AnimationSpeedPlayerPrefKey);

        #endregion

        #region Methods

        private void Awake()
        {
            // check if player preference for animation speed exists
            if (!PlayerPrefs.HasKey(AnimationSpeedPlayerPrefKey)) {
                PlayerPrefs.SetFloat(AnimationSpeedPlayerPrefKey, _animationSpeedDefault);
            }
        }

        #endregion
    }
}
