#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class PieceActionZone : ActionZoneBase
    {
        [SerializeField] private Button? _selectRewardButton;
        [SerializeField] private ActionButton? _takeBasicTetrominoButton;
        [SerializeField] private ActionButton? _changeTetrominoButton;
        [SerializeField] private ActionButton? _masterActionButton;

        public event Action? OnSelectRewardButtonClick;
        public event Action? OnEndFinishingTouchesButtonClick;
        public event Action? OnTakeBasicTetrominoButtonClick;
        public event Action? OnChangeTetrominoButtonClick;
        public event Action? OnMasterActionButtonClick;

        private new void Awake()
        {
            base.Awake();

            if (_selectRewardButton == null || _takeBasicTetrominoButton == null || _changeTetrominoButton == null || _masterActionButton == null) {
                Debug.LogError("One or more buttons are not assigned in the inspector", this);
                return;
            }

            _finishingTouchesButton!.onClick.AddListener(
                () => OnEndFinishingTouchesButtonClick?.Invoke()
            );

            _selectRewardButton.onClick.AddListener(
                () => OnSelectRewardButtonClick?.Invoke()
            );

            _takeBasicTetrominoButton.SelectAction += OnTakeBasicTetrominoButtonClick;
            _changeTetrominoButton.SelectAction += OnChangeTetrominoButtonClick;
            _masterActionButton.SelectAction += OnMasterActionButtonClick;
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
