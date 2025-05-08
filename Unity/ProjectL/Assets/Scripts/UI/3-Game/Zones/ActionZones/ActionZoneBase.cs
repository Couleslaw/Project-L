#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public abstract class ActionZoneBase : MonoBehaviour
    {
        [SerializeField] protected GameObject? _actionButtonsPanel;
        [SerializeField] protected Button? _finishingTouchesButton;
        [SerializeField] protected Button? _confirmButton;

        public event Action? OnConfirmButtonClick;

        public void DisableConfirmButton()
        {
            if (_confirmButton != null) {
                _confirmButton.interactable = false;
            }
        }

        public void EnableConfirmButton()
        {
           if (_confirmButton != null) {
                _confirmButton.interactable = true;
            }
        }

        protected void Awake()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _confirmButton == null) {
                Debug.LogError("One or more UI components is not assigned in the inspector", this);
                return;
            }

            _confirmButton.onClick.AddListener(
                () => OnConfirmButtonClick?.Invoke()
            );

            SetNormalMode();
        }

        public void SetFinishingTouchesMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null) {
                return;
            }
            SetNormalMode();
            _actionButtonsPanel.SetActive(false);
            _finishingTouchesButton.gameObject.SetActive(true);
        }

        public virtual void SetNormalMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null) {
                return;
            }
            _actionButtonsPanel.SetActive(true);
            _finishingTouchesButton.gameObject.SetActive(false);
            DisableConfirmButton();
        }
    }
}
