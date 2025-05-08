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

            // add listener
            UnityAction listener = () => OnButtonClick(button, groupName);
            button.onClick.AddListener(listener);

            // store the button info
            var buttonInfo = new ButtonInfo(button, listener, onSelect, onCancel);
            _buttonGroups[groupName].Add(button, buttonInfo);
        }

        public static void UnregisterButton(Button button, string groupName)
        {
            if (!_buttonGroups.ContainsKey(groupName)) {
                Debug.LogError($"Group {groupName} not found.");
                return;
            }
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
                    UnregisterButton(button, groupName);
                }
            }
        }

        public static void ForceDeselectButton(Button button, string groupName)
        {
            TryUnselectingButton(button, groupName, deselect: true);
        }

        public static void ForceDeselectButton(string groupName)
        {
            var selectedButton = _selectedButtons[groupName];
            if (selectedButton != null) {
                ForceDeselectButton(selectedButton, groupName);
            }
        }

        public static Button? GetSelectedButton(string groupName)
        {
            if (_selectedButtons.ContainsKey(groupName)) {
                return _selectedButtons[groupName];
            }
            return null;
        }

        private static void OnButtonClick(Button button, string groupName)
        {
            var selectedButton = _selectedButtons[groupName];

            // this button was already selected --> cancel
            if (selectedButton == button) {
                TryUnselectingButton(button, groupName, deselect: true);
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
            button.image.sprite = buttonInfo.SelectedSpriteState.selectedSprite;

            // invoke connected methods
            buttonInfo.OnSelect?.Invoke();
        }

        private static void TryUnselectingButton(Button? button, string groupName, bool deselect = false)
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

            // restore original sprite setting
            _selectedButtons[groupName] = null;
            var buttonInfo = _buttonGroups[groupName][selectedButton];
            selectedButton.spriteState = buttonInfo.UnselectedSpriteState;
            selectedButton.image.sprite = buttonInfo.OriginalSprite;

            // deselect the button in the event system
            if (deselect && EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(null!);
            }

            // invoke connected methods
            buttonInfo.OnCancel?.Invoke();
        }

        #endregion

        private struct ButtonInfo
        {
            #region Constructors

            public ButtonInfo(Button button, UnityAction onClickListener, Action? onSelect, Action? onCancel)
            {
                OnClickListener = onClickListener;
                OnSelect = onSelect;
                OnCancel = onCancel;

                OriginalSprite = button.image.sprite;
                UnselectedSpriteState = button.spriteState;
                SelectedSpriteState = new SpriteState {
                    highlightedSprite = button.spriteState.selectedSprite,
                    pressedSprite = button.spriteState.pressedSprite,
                    selectedSprite = button.spriteState.selectedSprite,
                    disabledSprite = button.spriteState.disabledSprite
                };
            }

            #endregion

            #region Properties

            public SpriteState SelectedSpriteState { get; set; }

            public SpriteState UnselectedSpriteState { get; set; }

            public Sprite OriginalSprite { get; set; }

            public UnityAction OnClickListener { get; set; }

            public Action? OnSelect { get; set; }

            public Action? OnCancel { get; set; }

            #endregion
        }
    }
}
