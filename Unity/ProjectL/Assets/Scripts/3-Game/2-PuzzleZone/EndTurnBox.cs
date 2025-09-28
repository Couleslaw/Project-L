#nullable enable

namespace ProjectL
{
    using ProjectL.Sound;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class EndTurnBox : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button? _confirmButton;

        [SerializeField] private Button? _cancelButton;

        #endregion

        #region Methods

        public void AddListener(UnityAction call)
        {
            _confirmButton!.onClick.AddListener(call);
        }

        public void RemoveListener(UnityAction call)
        {
            _confirmButton!.onClick.RemoveListener(call);
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
            gameObject.SetActive(false);
        }

        private void OnCancelButtonClick()
        {
            SoundManager.Instance.PlayButtonClickSound();
            gameObject.SetActive(false);
        }

        #endregion
    }
}
