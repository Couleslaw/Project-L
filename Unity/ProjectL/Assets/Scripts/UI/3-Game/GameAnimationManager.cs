#nullable enable

namespace ProjectL.UI.GameScene
{
    using ProjectL.Data;
    using ProjectL.UI.Sound;
    using System.Threading;
    using System;
    using UnityEngine;
    using System.Threading.Tasks;

    public class GameAnimationManager
    {
        private const float _defaultAnimationDelay = 1.5f;
        public static float AnimationDelay => _defaultAnimationDelay * AnimationSpeed.DelayMultiplier;

        public static async Task WaitForScaledDelayAsync(float animationDelayFraction, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Awaitable.WaitForSecondsAsync(AnimationDelay * animationDelayFraction, cancellationToken);
        }
    }
}
