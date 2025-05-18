#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectL.Management;
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.Sound;
    using ProjectLCore.GameLogic;
    using System;
    using System.Collections;
    using System.Linq.Expressions;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    public abstract class ActionZoneBase : MonoBehaviour, IGameActionController
    {
        #region Fields

        [SerializeField] protected GameObject? _actionButtonsPanel;

        [SerializeField] protected Button? _finishingTouchesButton;

        [SerializeField] protected Button? _selectRewardButton;

        [SerializeField] protected Button? _confirmButton;

        private PlayerMode _playerMode = PlayerMode.NonInteractive;

        #endregion

        public bool CanConfirmAction {
            set {
                if (_confirmButton != null) {
                    _confirmButton.interactable = value && _playerMode == PlayerMode.Interactive;
                }
            }
        }

        private bool _canSelectReward = false;
        private ActionMode _actionMode;

        public bool CanSelectReward {
            get => _canSelectReward;
            set {
                _canSelectReward = value;
                if (_selectRewardButton != null) {
                    _selectRewardButton.interactable = value && _actionMode == ActionMode.RewardSelection;
                }
            }
        }

        #region Methods

        public void ManuallyClickSelectRewardButton()
        {
            if (_selectRewardButton != null)
                _selectRewardButton!.onClick.Invoke();
        }
        public void ManuallyClickFinishingTouchesButton()
        {
            if (_finishingTouchesButton != null)
                StartCoroutine(SimulateClickCoroutine(_finishingTouchesButton));
        }

        public void SetActionMode(ActionMode mode)
        {
            _actionMode = mode;
            switch (mode) {
                case ActionMode.ActionCreation:
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

        public virtual void SetPlayerMode(PlayerMode mode)
        {
            _playerMode = mode;
            _finishingTouchesButton!.interactable = _playerMode == PlayerMode.Interactive;
            CanSelectReward = false;
            CanConfirmAction = false;
        }

        public abstract void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo);

        public virtual void AddListener(HumanPlayerActionCreator acm)
        {
            _confirmButton!.onClick.AddListener(acm.OnActionConfirmed);
        }
        public virtual void RemoveListener(HumanPlayerActionCreator acm)
        {
            _confirmButton!.onClick.RemoveListener(acm.OnActionConfirmed);
        }

        public void AddSelectRewardListener(HumanPlayerActionCreator acm)
        {
            _selectRewardButton!.onClick.AddListener(acm.OnRewardSelected);
        }

        public void RemoveSelectRewardListener(HumanPlayerActionCreator acm)
        {
            _selectRewardButton!.onClick.RemoveListener(acm.OnRewardSelected);
        }

        protected virtual void Awake()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _confirmButton == null || _selectRewardButton == null) {
                Debug.LogError("One or more UI components is not assigned in the inspector", this);
                return;
            }

            _confirmButton.onClick.AddListener(SoundManager.Instance!.PlayButtonClickSound);
            _selectRewardButton.onClick.AddListener(SoundManager.Instance!.PlayButtonClickSound);
            _finishingTouchesButton.onClick.AddListener(SoundManager.Instance!.PlayButtonClickSound);

            HumanPlayerActionCreator.RegisterController(this);
            if (GameManager.Controls != null) {
                GameManager.Controls.Gameplay.ConfirmAction.performed += SimulateConfirmActionClick;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Controls != null) {
                GameManager.Controls.Gameplay.ConfirmAction.performed -= SimulateConfirmActionClick;
            }
        }

        private void SimulateConfirmActionClick(InputAction.CallbackContext ctx)
        {
            if (_confirmButton != null && _confirmButton.interactable) {
                StartCoroutine(SimulateClickCoroutine(_confirmButton));
            }
            else if (_selectRewardButton != null && _selectRewardButton.interactable) {
                Debug.Log("Simulating click on select reward button");
                StartCoroutine(SimulateClickCoroutine(_selectRewardButton));
            }
            else if (_finishingTouchesButton != null && _finishingTouchesButton.interactable) {
                StartCoroutine(SimulateClickCoroutine(_finishingTouchesButton));
            }
        }

        private void SetFinishingTouchesMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _selectRewardButton == null) {
                return;
            }
            _finishingTouchesButton.gameObject.SetActive(true);
            _selectRewardButton.gameObject.SetActive(false);
            _actionButtonsPanel.SetActive(false);
            _finishingTouchesButton.interactable = _playerMode == PlayerMode.Interactive;
        }

        private void SetNormalMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _selectRewardButton == null) {
                return;
            }
            _finishingTouchesButton.gameObject.SetActive(false);
            _selectRewardButton.gameObject.SetActive(false);
            _actionButtonsPanel.SetActive(true);
            CanConfirmAction = false;
        }

        private void SetSelectRewardMode()
        {
            if (_actionButtonsPanel == null || _finishingTouchesButton == null || _selectRewardButton == null) {
                return;
            }
            _finishingTouchesButton.gameObject.SetActive(false);
            _selectRewardButton.gameObject.SetActive(true);
            _actionButtonsPanel.SetActive(false);
            CanSelectReward = false;
        }

        private IEnumerator SimulateClickCoroutine(Button targetButton)
        {
            if (!targetButton.interactable) {
                yield return new WaitForSeconds(0.1f);
                targetButton.onClick.Invoke();
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current);

            // Simulate PointerDown (Press)
            ExecuteEvents.Execute(targetButton.gameObject, pointerData, ExecuteEvents.pointerDownHandler);

            yield return new WaitForSeconds(0.1f);
            ExecuteEvents.Execute(targetButton.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
            yield return new WaitForSeconds(0.1f);

            // Simulate PointerUp (Release)
            ExecuteEvents.Execute(targetButton.gameObject, pointerData, ExecuteEvents.pointerUpHandler);
        }

        #endregion
    }
}
