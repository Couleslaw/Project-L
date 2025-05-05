#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class ActionZone : MonoBehaviour
    {
        [SerializeField] private GameObject? actionButtonsPanel;
        [SerializeField] private Button? finishingTouchesButton;

        public Action? OnFinishingTouchesButtonClick { get; set; }

        private void Awake()
        {
            if (actionButtonsPanel == null || finishingTouchesButton == null) {
                Debug.LogError("One or more UI components is not assigned in the inspector", this);
                return;
            }

            actionButtonsPanel.SetActive(true);
            finishingTouchesButton.gameObject.SetActive(false);

            finishingTouchesButton.onClick.AddListener(
                () => OnFinishingTouchesButtonClick?.Invoke()
                );
        }

        public void SetFinishingTouchesMode()
        {
            if (actionButtonsPanel == null || finishingTouchesButton == null) {
                return;
            }
            actionButtonsPanel.SetActive(false);
            finishingTouchesButton.gameObject.SetActive(true);
        }
    }
}