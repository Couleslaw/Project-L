#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectLCore.GameLogic;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public abstract class ActionZoneBase : MonoBehaviour
    {
        #region Fields

        [SerializeField] protected GameObject? _actionButtonsPanel;

        [SerializeField] protected Button? _finishingTouchesButton;

        [SerializeField] protected Button? _selectRewardButton;

        [SerializeField] protected Button? _confirmButton;

        #endregion

        #region Events

        public event Action? OnConfirmButtonClick;

        public event Action? OnSelectRewardButtonClick;

        #endregion

        #region Methods

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

        public void SetActionMode(ActionMode mode)
        {
            switch (mode) {
                case ActionMode.Normal:
                    SetNormalMode();
                    break;
                case ActionMode.FinishingTouches:
                    SetFinishingTouchesMode();
                    break;
                case ActionMode.RewardSelection:
                    SetSelectRewardMode();
                    break;
                default:
                    break;
            }
        }

        public abstract void SetPlayerMode(PlayerMode mode);

        public abstract void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo);


        protected void Awake()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _confirmButton == null || _selectRewardButton == null) {
                Debug.LogError("One or more UI components is not assigned in the inspector", this);
                return;
            }

            _confirmButton.onClick.AddListener(
                () => OnConfirmButtonClick?.Invoke()
            );

            _selectRewardButton.onClick.AddListener(
                () => OnSelectRewardButtonClick?.Invoke()
            );

            SetNormalMode();
        }

        private void SetFinishingTouchesMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _selectRewardButton == null) {
                return;
            }
            _finishingTouchesButton.gameObject.SetActive(true);
            _selectRewardButton.gameObject.SetActive(false);
            _actionButtonsPanel.SetActive(false);
        }

        private void SetNormalMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _selectRewardButton == null) {
                return;
            }
            _finishingTouchesButton.gameObject.SetActive(false);
            _selectRewardButton.gameObject.SetActive(false);
            _actionButtonsPanel.SetActive(true);
            DisableConfirmButton();
        }

        private void SetSelectRewardMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _selectRewardButton == null) {
                return;
            }
            _finishingTouchesButton.gameObject.SetActive(false);
            _selectRewardButton.gameObject.SetActive(true);
            _actionButtonsPanel.SetActive(false);
        }

        #endregion
    }
}
