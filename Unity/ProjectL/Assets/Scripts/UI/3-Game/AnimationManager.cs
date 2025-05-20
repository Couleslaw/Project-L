#nullable enable

namespace ProjectL.UI.Animation
{
    using ProjectL.Data;
    using ProjectL.UI.Sound;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public static class AnimationManager
    {
        #region Constants

        private const float _defaultAnimationDelay = 1.5f;

        #endregion

        #region Properties

        private static float AnimationDelay => _defaultAnimationDelay * AnimationSpeed.DelayMultiplier;

        #endregion

        #region Methods

        public static async Task WaitForScaledDelay(float animationDelayFraction, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.WaitForSecondsAsync(AnimationDelay * animationDelayFraction, cancellationToken);
        }

        public static async Task PlayTapSoundAndWaitForScaledDelay(float animationDelayFraction, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SoundManager.Instance?.PlayTapSoundEffect();
            await Awaitable.WaitForSecondsAsync(AnimationDelay * animationDelayFraction, cancellationToken);
        }

        public static async Task WaitForScaledDelayAndPlayTapSound(float animationDelayFraction, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.WaitForSecondsAsync(AnimationDelay * animationDelayFraction, cancellationToken);
            SoundManager.Instance?.PlayTapSoundEffect();
        }

        #endregion
    }
}
