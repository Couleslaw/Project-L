#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class ActionButton : MonoBehaviour
    {
        #region Fields

        private static ActionButton? _selectedButton = null;

        private Image? _image;
        private Button? _button;

        private Sprite? _normalSprite;
        private Sprite? _selectedSprite;
        private Sprite? _highlightSprite;

        #endregion

        #region Properties

        public static Action? OnCancel { get; set; }

        public Action? OnSelect { get; set; }

        #endregion

        #region Methods

        public void DisableButton()
        {
            if (_button == null) {
                return;
            }
            if (_selectedButton == this) {
                UnmarkSelection(deselect: true);
            }
            _button.interactable = false;
        }

        public void EnableButton()
        {
            if (_button == null) {
                return;
            }
            _button.interactable = true;
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
            if (_image == null || _button == null) {
                Debug.LogError("Image or Button component is missing!", this);
                return;
            }

            if (_button.transition != Selectable.Transition.SpriteSwap) {
                Debug.LogError("Button transition is not set to SpriteSwap!", this);
                return;
            }
            _normalSprite = _image.sprite;
            _selectedSprite = _button.spriteState.selectedSprite;
            _highlightSprite = _button.spriteState.highlightedSprite;
            _button!.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            if (_image == null || _button == null) {
                return;  // safety check
            }

            // this button was already selected --> cancel
            if (_selectedButton == this) {
                Debug.Log("Deselecting button", this);
                UnmarkSelection(deselect: true);
                return;
            }

            // if a different button was selected before --> cancel it
            if (_selectedButton != null) {
                Debug.Log("Deselecting previous button", this);
                _selectedButton.UnmarkSelection();
            }

            // select this button
            Debug.Log("Selecting button", this);
            MarkAsSelected();
        }

        private void MarkAsSelected()
        {
            if (_image == null || _button == null || _selectedSprite == null) {
                return;
            }
            _selectedButton = this;
            _image.sprite = _selectedSprite;

            // change highlight sprite
            var spriteState = _button.spriteState;
            spriteState.highlightedSprite = _selectedSprite;
            _button.spriteState = spriteState;

            OnSelect?.Invoke();
        }

        private void UnmarkSelection(bool deselect = false)
        {
            if (_image == null || _button == null || _normalSprite == null || _highlightSprite == null) {
                return;
            }
            _selectedButton = null;
            _image.sprite = _normalSprite;

            // change highlight sprite
            var spriteState = _button.spriteState;
            spriteState.highlightedSprite = _highlightSprite;
            _button.spriteState = spriteState;

            // deselect the button in the event system
            if (deselect && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null);
            }

            OnCancel?.Invoke();
        }

        #endregion
    }
}
