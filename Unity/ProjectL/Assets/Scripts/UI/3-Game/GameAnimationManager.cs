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
        public static float AnimationDelay => _defaultAnimationDelay * AnimationSpeed.Multiplier;

        public static async Task WaitForAnimationDelayFraction(float fraction = 1f, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return;
            }
            try {
                await Awaitable.WaitForSecondsAsync(AnimationDelay * fraction, cancellationToken);
            }
            catch (OperationCanceledException) {
            }
        }
    }
}
