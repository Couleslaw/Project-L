namespace ProjectL.Data
{
    using UnityEngine;

    public static class AnimationSpeed
    {
        #region Constants

        /// <summary>
        /// The name of the <see cref="PlayerPrefs"/> key for animation speed.
        /// </summary>
        public const string AnimationSpeedPlayerPrefKey = "AnimationSpeed";

        private const float _animationSpeedDefault = 1f;

        #endregion

        #region Constructors

        static AnimationSpeed()
        {
            // check if player preference for animation speed exists
            if (!PlayerPrefs.HasKey(AnimationSpeedPlayerPrefKey)) {
                PlayerPrefs.SetFloat(AnimationSpeedPlayerPrefKey, _animationSpeedDefault);
            }
        }

        #endregion

        #region Properties

        public static float Multiplier => PlayerPrefs.GetFloat(AnimationSpeedPlayerPrefKey);

        public static float DelayMultiplier => 1f / Multiplier;

        #endregion
    }
}
