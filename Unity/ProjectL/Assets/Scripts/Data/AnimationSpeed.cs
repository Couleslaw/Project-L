namespace ProjectL.Data
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
        public static float Multiplier => PlayerPrefs.GetFloat(AnimationSpeedPlayerPrefKey);

        public static float DelayMultiplier => 1f / Multiplier;

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
