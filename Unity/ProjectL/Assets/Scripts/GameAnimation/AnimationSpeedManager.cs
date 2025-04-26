using System;
using UnityEngine;

public class AnimationSpeedManager : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// The name of the <see cref="PlayerPrefs"/> key for animation speed.
    /// </summary>
    public const string AnimationSpeedPlayerPrefKey = "AnimationSpeed";

    /// <summary>
    /// The animation speed slider's display value will be adjusted to the nearest multiple of this value.
    /// </summary>
    private const float _animationSpeedDefault = 1f;

    #endregion

    #region Properties

    /// <summary>
    /// Multiplier for the animation speed.
    /// </summary>
    public static float AnimationSpeed => PlayerPrefs.GetFloat(AnimationSpeedPlayerPrefKey);

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
