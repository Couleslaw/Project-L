#nullable enable

namespace ProjectL.Pause
{
    using ProjectL.Management;
    using ProjectL.Sound;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class ExitGameBox : MonoBehaviour
    {
        [SerializeField] private Button? _confirmButton;
        [SerializeField] private Button? _cancelButton;

        private Action? _onCancelButtonClick;

        public void Init(Action OnCancelButtonClick)
        {
            _onCancelButtonClick = OnCancelButtonClick;
        }

        private void Awake()
        {
            if (_confirmButton == null || _cancelButton == null) {
                Debug.LogError("One or more UI components not assigned");
                return;
            }

            _confirmButton.onClick.AddListener(OnConfirmButtonClick);
            _cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        private void OnConfirmButtonClick()
        {
            SoundManager.Instance.PlayButtonClickSound();
            GameManager.Instance?.ResumeGame();
            SceneLoader.Instance?.LoadMainMenuAsync();
        }

        private void OnCancelButtonClick()
        {
            SoundManager.Instance.PlayButtonClickSound();
            _onCancelButtonClick?.Invoke();
        }
    }
}
