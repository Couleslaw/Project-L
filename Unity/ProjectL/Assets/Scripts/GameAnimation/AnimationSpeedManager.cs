using System.Globalization;
using System;
using UnityEngine;

public class AnimationSpeedManager : MonoBehaviour
{
    public const string AnimationSpeedPlayerPrefKey = "AnimationSpeed";
    private const float _animationSpeedDefault = 1f;
    public const float AnimationSpeedStep = 0.1f;

    private void Awake()
    {
        // check if player preference for animation speed exists
        if (!PlayerPrefs.HasKey(AnimationSpeedPlayerPrefKey)) {
            PlayerPrefs.SetFloat(AnimationSpeedPlayerPrefKey, _animationSpeedDefault);
        }
    }

    public static float AnimationSpeed => PlayerPrefs.GetFloat(AnimationSpeedPlayerPrefKey);

    /// <summary>
    /// Calculates the closest (integer) multiple of <see cref="AnimationSpeedStep"/>.
    /// </summary>
    /// <param name="value">The value to adjust.</param>
    /// <returns>The adjusted value.</returns>
    public static float CalculateAdjustedAnimationSpeed(Single value)
    {
        return Mathf.Round(value / AnimationSpeedStep) * AnimationSpeedStep;
    }
}
