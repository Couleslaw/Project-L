#nullable enable

namespace ProjectL.GameScene.ActionZones
{
    using ProjectL.Animation;
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.Sound;
    using ProjectLCore.GameLogic;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public abstract class ActionZoneBase : MonoBehaviour, IActionCreationController
    {
        #region Fields

        [SerializeField] protected GameObject? _actionButtonsPanel;

        [SerializeField] protected Button? _finishingTouchesButton;

        [SerializeField] protected Button? _selectRewardButton;

        [SerializeField] protected Button? _confirmButton;

        private PlayerMode _playerMode = PlayerMode.NonInteractive;

        private ActionMode _actionMode;

        #endregion

        #region Properties

        public bool CanConfirmAction {
            get {
                if (_confirmButton == null) {
                    return false;
                }
                return _confirmButton.interactable && _playerMode == PlayerMode.Interactive;
            }
            set {
                if (_confirmButton != null) {
                    _confirmButton.interactable = value && _playerMode == PlayerMode.Interactive;
                }
            }
        }

        public bool CanSelectReward {
            get {
                if (_selectRewardButton == null) {
                    return false;
                }
                return _selectRewardButton.interactable && _actionMode == ActionMode.RewardSelection;
            }
            set {
                if (_selectRewardButton != null) {
                    _selectRewardButton.interactable = value && _actionMode == ActionMode.RewardSelection;
                }
            }
        }

        protected bool CanUseFinishingTouchesButton {
            get {
                if (_finishingTouchesButton == null) {
                    return false;
                }
                return _finishingTouchesButton.interactable && _actionMode == ActionMode.FinishingTouches;
            }
            set {
                if (_finishingTouchesButton != null) {
                    _finishingTouchesButton.interactable = value && _actionMode == ActionMode.FinishingTouches;
                }
            }
        }

        #endregion

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
            SetDefaultButtonsIntractability();

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
            SetDefaultButtonsIntractability();
        }

        public abstract void EnabledButtonsBasedOnGameState(GameState.GameInfo gameInfo, PlayerState.PlayerInfo playerInfo, TurnInfo turnInfo);

        public virtual void AddListener(HumanPlayerActionCreationManager acm)
        {
            _confirmButton!.onClick.AddListener(acm.OnActionConfirmed);
        }

        public virtual void RemoveListener(HumanPlayerActionCreationManager acm)
        {
            _confirmButton!.onClick.RemoveListener(acm.OnActionConfirmed);
        }

        public void AddSelectRewardListener(HumanPlayerActionCreationManager acm)
        {
            _selectRewardButton!.onClick.AddListener(acm.OnRewardSelected);
        }

        public void RemoveSelectRewardListener(HumanPlayerActionCreationManager acm)
        {
            _selectRewardButton!.onClick.RemoveListener(acm.OnRewardSelected);
        }

        public void SimulateConfirmActionClick()
        {
            if (CanConfirmAction) {
                StartCoroutine(SimulateClickCoroutine(_confirmButton!));
            }
            else if (CanSelectReward) {
                StartCoroutine(SimulateClickCoroutine(_selectRewardButton!));
            }
            else if (CanUseFinishingTouchesButton) {
                StartCoroutine(SimulateClickCoroutine(_finishingTouchesButton!));
            }
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

            HumanPlayerActionCreationManager.RegisterController(this);
        }

        private void SetDefaultButtonsIntractability()
        {
            CanConfirmAction = false;
            CanSelectReward = false;
            CanUseFinishingTouchesButton = _playerMode == PlayerMode.Interactive && _actionMode == ActionMode.FinishingTouches;
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
