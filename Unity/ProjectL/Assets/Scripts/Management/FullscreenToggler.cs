#nullable enable

namespace ProjectL
{
    using UnityEngine;

    public class FullscreenToggler : StaticInstance<FullscreenToggler>
    {

#if !UNITY_WEBGL

        void Update()
        {
            // toggle fullscreen mode when F11 is pressed
            if (Input.GetKeyDown(KeyCode.F11)) {
                Screen.fullScreen = !Screen.fullScreen;
            }
        }
#endif

    }
}
