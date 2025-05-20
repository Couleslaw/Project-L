#nullable enable

namespace ProjectL.GameScene.ActionZones
{
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.Sound;
    using ProjectL.Utils;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class ActionButton : MonoBehaviour
    {
        #region Fields

        private Button? _button;

        private SpriteState _interactiveSprites;

        private Sprite? _originalSprite;

        private bool _canActionBeCreated = true;

        private PlayerMode _mode = PlayerMode.NonInteractive;

        #endregion

        #region Events

        public static event Action? CancelActionEventHandler;

        public event Action? SelectActionEventHandler;

        #endregion

        #region Properties

        public PlayerMode Mode {
            get => _mode;
            set {
                _mode = value;
                UpdateUI();
            }
        }

        public bool CanActionBeCreated {
            get => _canActionBeCreated;
            set {
                _canActionBeCreated = value;
                if (_button != null) {
                    UpdateUI();
                }
            }
        }

        #endregion

        #region Methods

        public static void DeselectCurrentButton()
        {
            if (!RadioButtonsGroup.TryGetSelectedButton(nameof(ActionButton), out var button)) {
                return;
            }
            if (!button!.gameObject.TryGetComponent<ActionButton>(out var actionButton)) {
                Debug.LogError("ActionButton component is missing!", button);
                return;
            }

            var newSpriteState = actionButton.GetCurrentSpriteState(selected: false);
            RadioButtonsGroup.UpdateSpritesForButton(button, newSpriteState);
            button.spriteState = newSpriteState;
            RadioButtonsGroup.ForceDeselectButtonInGroup(nameof(ActionButton));
        }

        public void ManuallySelectButton()
        {
            if (_button == null) {
                return;
            }
            if (CanActionBeCreated == false) {
                return;
            }

            var newSpriteState = GetCurrentSpriteState(selected: true);
            RadioButtonsGroup.UpdateSpritesForButton(_button, newSpriteState);
            _button.spriteState = newSpriteState;
            _button.onClick.Invoke();
        }

        private SpriteState GetCurrentSpriteState(bool selected)
        {
            if (Mode == PlayerMode.Interactive) {
                return _interactiveSprites;
            }

            // non interactive state
            var newState = _interactiveSprites;
            if (CanActionBeCreated) {
                Sprite displaySprite = selected ? _interactiveSprites.selectedSprite : _originalSprite!;
                newState.disabledSprite = displaySprite;
            }
            return newState;
        }

        private void UpdateUI()
        {
            if (_button == null) {
                return;
            }

            // update intractability
            _button.interactable = CanActionBeCreated && Mode == PlayerMode.Interactive;
            if (_button.interactable == false) {
                RadioButtonsGroup.ForceDeselectButton(_button);
            }

            bool isSelected = RadioButtonsGroup.IsButtonSelected(_button);
            RadioButtonsGroup.UpdateSpritesForButton(_button, GetCurrentSpriteState(isSelected));
            _button.spriteState = GetCurrentSpriteState(isSelected);
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button == null) {
                Debug.LogError("Button component is missing!", this);
                return;
            }

            if (_button.transition != Selectable.Transition.SpriteSwap) {
                Debug.LogError("Button transition is not set to SpriteSwap!", this);
                return;
            }

            _interactiveSprites = _button.spriteState;
            _originalSprite = _button.image.sprite;
            if (_originalSprite == null) {
                Debug.LogError("The buttons image has no default sprite!", this);
                return;
            }

            Action onSelect = () => SelectActionEventHandler?.Invoke();
            Action onCancel = () => CancelActionEventHandler?.Invoke();
            RadioButtonsGroup.RegisterButton(_button, nameof(ActionButton), onSelect, onCancel);
        }

        private void Start()
        {
            if (_button != null) {
                _button.onClick.AddListener(SoundManager.Instance!.PlayButtonClickSound);
            }
        }

        private void OnDestroy()
        {
            if (_button != null) {
                RadioButtonsGroup.UnregisterButton(_button);
            }
        }

        #endregion
    }
}
