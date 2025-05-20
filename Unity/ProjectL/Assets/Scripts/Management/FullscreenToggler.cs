#nullable enable

namespace ProjectL.Management
{
    using UnityEngine;

    /// <summary>
    /// A class that toggles full-screen mode in Unity when <c>F11</c> is pressed.
    /// </summary>
    /// <seealso cref="ProjectL.StaticInstance&lt;ProjectL.FullscreenToggler&gt;" />
    public class FullscreenToggler : StaticInstance<FullscreenToggler>
    {

#if !UNITY_WEBGL

        private void Update()
        {
            // toggle full-screen mode when F11 is pressed
            if (Input.GetKeyDown(KeyCode.F11)) {
                Screen.fullScreen = !Screen.fullScreen;
            }
        }
#endif

    }
}
