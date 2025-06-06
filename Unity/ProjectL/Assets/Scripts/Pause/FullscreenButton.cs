#nullable enable

namespace ProjectL.Pause
{
    using ProjectL.Sound;
    using UnityEngine;
    using UnityEngine.UI;
    using ProjectL.Management;

    [RequireComponent(typeof(Button))]
    public class FullscreenButton : MonoBehaviour, IFullscreenListener
    {
        [SerializeField] Sprite? _fullscreenOnIcon;
        [SerializeField] Sprite? _fullscreenOffIcon;
        private Button? _button;

        private void OnFullscreenButtonClick()
        {
            SoundManager.Instance.PlayButtonClickSound();
            FullscreenToggler.Instance.ToggleFullscreen();
        }


        private void Awake()
        {
            if (_fullscreenOnIcon == null || _fullscreenOffIcon == null)
            {
                return;
            }

            _button = GetComponent<Button>();
            FullscreenToggler.AddListener(this);
            _button.onClick.AddListener(OnFullscreenButtonClick);
        }

        private void OnDestroy()
        {
            FullscreenToggler.RemoveListener(this);
        }

        public void OnFullscreenToggled(bool isFullscreen)
        {
            if (_button == null || _fullscreenOnIcon == null || _fullscreenOffIcon == null) {
                return;
            }
            var icon = isFullscreen ? _fullscreenOnIcon : _fullscreenOffIcon;
            _button.image.sprite = icon;
        }
    }
}
