#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class PieceActionZone : ActionZoneBase
    {
        [SerializeField] private Button? _selectRewardButton;

        public event Action? OnSelectRewardButtonClick;
        public event Action? OnEndFinishingTouchesButtonClick;

        private new void Awake()
        {
            base.Awake();
    
            _finishingTouchesButton!.onClick.AddListener(
                () => OnEndFinishingTouchesButtonClick?.Invoke()
            );

            if (_selectRewardButton == null) {
                Debug.LogError("Select Reward Button is not assigned in the inspector", this);
                return;
            }

            _selectRewardButton.onClick.AddListener(
                () => OnSelectRewardButtonClick?.Invoke()
            );
        }

        public override void SetNormalMode()
        {
            base.SetNormalMode();
            if (_selectRewardButton == null) {
                return;
            }
            _selectRewardButton.gameObject.SetActive(false);
        }

        public void SetSelectRewardMode()
        {
            if (_selectRewardButton == null || _actionButtonsPanel == null || _finishingTouchesButton == null) {
                return;
            }

            _selectRewardButton.gameObject.SetActive(true);
            _actionButtonsPanel.SetActive(false);
            _finishingTouchesButton.gameObject.SetActive(false);
        }
    }
}
