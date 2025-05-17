#nullable enable

namespace ProjectL.UI.GameScene.Zones
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public static class RadioButtonsGroup
    {
        #region Fields

        private static readonly Dictionary<string, Dictionary<Button, ButtonInfo>> _buttonGroups = new();

        private static readonly Dictionary<Button, string> _buttonToGroupMap = new();

        private static readonly Dictionary<string, Button?> _selectedButtons = new();

        #endregion

        #region Methods

        public static void RegisterButton(Button button, string groupName, Action? onSelect = null, Action? onCancel = null)
        {
            if (button.transition != Selectable.Transition.SpriteSwap) {
                Debug.LogError($"Button {button.name} in group {groupName} must use SpriteSwap transition.");
                return;
            }

            // create group if it doesn't exist
            if (!_buttonGroups.ContainsKey(groupName)) {
                _buttonGroups[groupName] = new();
            }
            if (!_selectedButtons.ContainsKey(groupName)) {
                _selectedButtons[groupName] = null;
            }

            _buttonToGroupMap[button] = groupName;

            // add listener
            UnityAction listener = () => OnButtonClick(button, groupName);
            button.onClick.AddListener(listener);

            // store the button info
            var buttonInfo = new ButtonInfo(button, listener, onSelect, onCancel);
            _buttonGroups[groupName].Add(button, buttonInfo);
        }

        public static void UnregisterButton(Button button)
        {
            if (!_buttonToGroupMap.ContainsKey(button)) {
                return;
            }
            string groupName = _buttonToGroupMap[button];
            _buttonToGroupMap.Remove(button);

            // remove listener
            if (_buttonGroups[groupName].ContainsKey(button)) {
                button.onClick.RemoveListener(_buttonGroups[groupName][button].OnClickListener);
                _buttonGroups[groupName].Remove(button);
            }
            // remove group if empty
            if (_buttonGroups[groupName].Count == 0) {
                _buttonGroups.Remove(groupName);
                _selectedButtons.Remove(groupName);
            }
        }

        public static void DeleteGroup(string groupName)
        {
            if (_buttonGroups.ContainsKey(groupName)) {
                foreach (var button in _buttonGroups[groupName].Keys) {
                    UnregisterButton(button);
                }
            }
        }

        public static void UpdateSpritesForButton(Button button, SpriteState newState)
        {
            if (!_buttonToGroupMap.ContainsKey(button)) {
                Debug.LogError($"Button {button.name} is not registered in any group.");
                return;
            }
            string groupName = _buttonToGroupMap[button];
            var buttonInfo = _buttonGroups[groupName][button];
            buttonInfo.UpdateSpriteState(newState);

            if (button == _selectedButtons[groupName]) {
                button.spriteState = buttonInfo.SelectedSpriteState;
                button.image.sprite = buttonInfo.UnselectedSpriteState.selectedSprite;
            }
            else {
                button.spriteState = buttonInfo.UnselectedSpriteState;
                button.image.sprite = buttonInfo.OriginalSprite;
            }
        }

        public static void ForceDeselectButton(Button button)
        {
            if (!_buttonToGroupMap.ContainsKey(button)) {
                Debug.LogError($"Button {button.name} is not registered in any group.");
                return;
            }
            string groupName = _buttonToGroupMap[button];
            TryUnselectingButton(button, groupName, uiDeselect: true);
        }

        public static void ForceDeselectButtonInGroup(string groupName)
        {
            var selectedButton = _selectedButtons[groupName];
            if (selectedButton != null) {
                ForceDeselectButton(selectedButton);
            }
        }

        public static bool TryGetSelectedButton(string groupName, out Button? result)
        {
            if (_selectedButtons.ContainsKey(groupName)) {
                result = _selectedButtons[groupName];
                return result != null;
            }
            result = null;
            return false;
        }

        public static bool IsButtonSelected(Button button)
        {
            if (!_buttonToGroupMap.ContainsKey(button)) {
                Debug.LogError($"Button {button.name} is not registered in any group.");
                return false;
            }
            string groupName = _buttonToGroupMap[button];
            return _selectedButtons[groupName] == button;
        }

        private static void OnButtonClick(Button button, string groupName)
        {
            var selectedButton = _selectedButtons[groupName];

            // this button was already selected --> cancel
            if (selectedButton == button) {
                TryUnselectingButton(button, groupName, uiDeselect: true);
                return;
            }

            TryUnselectingButton(selectedButton, groupName);
            MarkAsSelected(button, groupName);
        }

        private static void MarkAsSelected(Button button, string groupName)
        {
            if (!_buttonGroups.ContainsKey(groupName)) {
                Debug.LogError($"Group {groupName} not found.");
                return;
            }

            // set selected sprite setting
            _selectedButtons[groupName] = button;
            var buttonInfo = _buttonGroups[groupName][button];
            button.spriteState = buttonInfo.SelectedSpriteState;
            button.image.sprite = buttonInfo.UnselectedSpriteState.selectedSprite;

            // invoke connected methods
            buttonInfo.OnSelect?.Invoke();
        }

        private static void TryUnselectingButton(Button? button, string groupName, bool uiDeselect = false)
        {
            if (!_buttonGroups.ContainsKey(groupName)) {
                Debug.LogError($"Group {groupName} not found.");
                return;
            }

            // get selected button
            var selectedButton = _selectedButtons[groupName];
            if (button == null || selectedButton == null || selectedButton != button) {
                return;
            }

            // deselect the button in the event system
            if (uiDeselect && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null!);
            }

            // restore original sprite setting
            _selectedButtons[groupName] = null;
            var buttonInfo = _buttonGroups[groupName][selectedButton];
            selectedButton.image.sprite = buttonInfo.OriginalSprite;
            selectedButton.spriteState = buttonInfo.UnselectedSpriteState;

            // invoke connected methods
            buttonInfo.OnCancel?.Invoke();
        }

        #endregion

        private class ButtonInfo
        {
            #region Constructors

            public ButtonInfo(Button button, UnityAction onClickListener, Action? onSelect = null, Action? onCancel = null)
            {
                OnClickListener = onClickListener;
                OnSelect = onSelect;
                OnCancel = onCancel;

                OriginalSprite = button.image.sprite;
                UpdateSpriteState(button.spriteState);
            }

            #endregion

            #region Properties

            public void UpdateSpriteState(SpriteState newState)
            {
                UnselectedSpriteState = newState;
                SelectedSpriteState = new SpriteState {
                    highlightedSprite = UnselectedSpriteState.selectedSprite,
                    pressedSprite = UnselectedSpriteState.pressedSprite,
                    selectedSprite = UnselectedSpriteState.selectedSprite,
                    disabledSprite = UnselectedSpriteState.disabledSprite
                };
            }

            public SpriteState UnselectedSpriteState { get; set; }
            public SpriteState SelectedSpriteState { get; set; }

            public Sprite OriginalSprite { get; set; }

            public UnityAction OnClickListener { get; set; }

            public Action? OnSelect { get; set; }

            public Action? OnCancel { get; set; }

            #endregion
        }
    }
}
