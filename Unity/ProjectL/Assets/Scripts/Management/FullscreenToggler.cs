#nullable enable

namespace ProjectL.Management
{
    using System;
    using UnityEngine;

    public interface IFullscreenListener
    {
        void OnFullscreenToggled(bool isFullscreen);
    }

    /// <summary>
    /// A class that toggles full-screen mode in Unity when <c>F11</c> is pressed.
    /// </summary>
    /// <seealso cref="ProjectL.StaticInstance&lt;ProjectL.FullscreenToggler&gt;" />
    public class FullscreenToggler : Singleton<FullscreenToggler>
    {
        const string WindowWidthKey = "WindowWidth";
        const string WindowHeightKey = "WindowHeight";

        private int PreferredWidth {
            get {
                if (!PlayerPrefs.HasKey(WindowWidthKey)) {
                    PlayerPrefs.SetInt(WindowWidthKey, Screen.currentResolution.width);
                }
                return PlayerPrefs.GetInt(WindowWidthKey);
            }
            set {
                PlayerPrefs.SetInt(WindowWidthKey, value);
            }
        }

        private int PreferredHeight {
            get {
                if (!PlayerPrefs.HasKey(WindowHeightKey)) {
                    PlayerPrefs.SetInt(WindowHeightKey, Screen.currentResolution.height);
                }
                return PlayerPrefs.GetInt(WindowHeightKey);
            }
            set {
                PlayerPrefs.SetInt(WindowHeightKey, value);
            }
        }

        static private event Action<bool>? FullscreenToggledEventHandler;

        public static void AddListener(IFullscreenListener listener)
        {
            if (listener == null)
                return;
            FullscreenToggledEventHandler += listener.OnFullscreenToggled;
        }

        public static void RemoveListener(IFullscreenListener listener)
        {
            if (listener == null)
                return;
            FullscreenToggledEventHandler -= listener.OnFullscreenToggled;
        }

        private void NotifyListeners(bool fullscreen)
        {
            FullscreenToggledEventHandler?.Invoke(fullscreen);
        }

        public void ToggleFullscreen()
        {
            NotifyListeners(!Screen.fullScreen);

#if UNITY_WEBGL

            Screen.fullScreen = !Screen.fullScreen;

#else

            if (Screen.fullScreen) {
                Screen.SetResolution(PreferredWidth, PreferredHeight, FullScreenMode.Windowed);
            }
            else {
                PreferredWidth = Screen.width;
                PreferredHeight = Screen.height;
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            }

#endif
        }

        protected override void Awake()
        {
            base.Awake();
            NotifyListeners(Screen.fullScreen);
        }

#if !UNITY_WEBGL

        private void Update()
        {
            // toggle full-screen mode when F11 is pressed
            if (Input.GetKeyDown(KeyCode.F11)) {
                ToggleFullscreen();
            }
        }
#endif

    }
}
