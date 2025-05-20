#nullable enable

namespace ProjectL.PlayerSelectionScene
{
    using ProjectL.Data;
    using ProjectL.Sound;
    using ProjectLCore.Players;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the <c>PlayerSelectionRow</c> prefab. Represents one row in the <c>Player Selection</c> scene.
    /// </summary>
    public class PlayerSettingsRow : MonoBehaviour
    {
        #region Constants

        public const int NameCharacterLimit = 18;

        private const string _namePlaceholder = "Enter name...";

        private const string _typePlaceholder = "Select type";

        #endregion

        #region Fields

        /// <summary>
        /// List of all available player types in the game. It is initialized with the <see cref="HumanPlayer"/> and AI player types are added in <see cref="Start"/>.
        /// </summary>
        private readonly List<PlayerTypeInfo> _availablePlayerTypes = new() { new PlayerTypeInfo(typeof(HumanPlayer), "Human", null) };

        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown? playerTypeDropdown;

        [SerializeField] private TMP_InputField? playerNameInput;

        [SerializeField] private Button? resetButton;

        private bool _isDropdownListOpen = false;

        private bool _isInputFieldSelected = false;

        private bool _didTrimInputFieldContent = false;

        private bool _didInitialize = false;

        #endregion

        #region Properties

        /// <summary>
        /// Information about the selected player type.
        /// </summary>
        public PlayerTypeInfo? PlayerType { get; private set; }

        /// <summary>
        /// Trimmed name of the player.
        /// </summary>
        public string PlayerName => playerNameInput!.text.Trim();

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the current selection is empty.
        /// </summary>
        /// <returns><see langword="true"/> if the selection is empty; otherwise, <see langword="false"/>.</returns>
        public bool IsEmpty() => PlayerType == null && string.IsNullOrEmpty(PlayerName);

        /// <summary>
        /// Validates the current selection.
        /// </summary>
        /// <returns><see langword="true"/> if the selection is valid; otherwise, <see langword="false"/>.</returns>
        public bool IsValid()
        {
            // empty selection is valid
            if (IsEmpty()) {
                return true;
            }
            // fully initialized selection (type AND name) is valid
            if (PlayerType != null && !string.IsNullOrEmpty(PlayerName)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the dropdown click event.
        /// </summary>
        public void OnDropdownClick()
        {
            _isDropdownListOpen = true;
            SetPlayerDropdownPlaceholder();
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Handles the dropdown item click event.
        /// </summary>
        public void OnDropdownItemClick()
        {
            _isDropdownListOpen = false;
            UpdatePlayerDropdownPlaceholder();
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Handles the dropdown cancel event.
        /// </summary>
        public void OnDropdownCancel()
        {
            _isDropdownListOpen = false;
            UpdatePlayerDropdownPlaceholder();
        }

        /// <summary>
        /// Handles the dropdown enter event.
        /// </summary>
        public void OnDropdownEnter()
        {
            SetPlayerDropdownPlaceholder();
        }

        /// <summary>
        /// Handles the dropdown exit event.
        /// </summary>
        public void OnDropdownExit()
        {
            if (!_isDropdownListOpen)
                UpdatePlayerDropdownPlaceholder();
        }

        /// <summary>
        /// Handles the dropdown deselect event.
        /// </summary>
        public void OnDropdownDeselect()
        {
            StartCoroutine(CallMethodIfDropdownCloses(waitTime: 2f, method: UpdatePlayerDropdownPlaceholder));
        }

        /// <summary>
        /// Handles the dropdown value change event.
        /// </summary>
        /// <param name="index">The index of the selected dropdown option.</param>
        public void OnDropdownValueChanged(int index)
        {
            if (index < 0 || index >= _availablePlayerTypes.Count) {
                Debug.LogWarning($"Invalid dropdown index mapping. Index: {index}.");
                PlayerType = null;
            }
            else {
                PlayerType = _availablePlayerTypes[index];
            }

            UpdateUI();
        }

        /// <summary>
        /// Handles the input field select event.
        /// </summary>
        /// <param name="value">The current value of the input field.</param>
        public void OnInputFieldSelect(string value)
        {
            _isInputFieldSelected = true;
            SetInputFieldPlaceholder();
        }

        /// <summary>
        /// Handles the input field deselect event.
        /// </summary>
        /// <param name="value">The current value of the input field.</param>
        public void OnInputFieldDeselect(string value)
        {
            _isInputFieldSelected = false;
            UpdateInputFieldPlaceholder();
        }

        /// <summary>
        /// Handles the input field enter event.
        /// </summary>
        public void OnInputFieldEnter()
        {
            SetInputFieldPlaceholder();
        }

        /// <summary>
        /// Handles the input field exit event.
        /// </summary>
        public void OnInputFieldExit()
        {
            if (!_isInputFieldSelected)
                UpdateInputFieldPlaceholder();
        }

        /// <summary>
        /// Handles the input field value change event.
        /// </summary>
        /// <param name="value">The new value of the input field.</param>
        public void OnInputFieldValueChanged(string value)
        {
            UpdateUI();

            // don't play sound if the change was caused by trimming the value
            if (!_didTrimInputFieldContent) {
                if (_didInitialize)
                    SoundManager.Instance?.PlayInputLineSound();
            }
            else {
                _didTrimInputFieldContent = false;
            }
        }

        /// <summary>
        /// Handles the input field end edit event.
        /// </summary>
        /// <param name="value">The final value of the input field.</param>
        public void OnInputFieldEndEdit(string value)
        {
            if (playerNameInput!.text != value.Trim()) {
                playerNameInput.text = value.Trim();
                _didTrimInputFieldContent = true;
            }
            UpdateUI();
        }

        /// <summary>
        /// Handles the reset button click event.
        /// </summary>
        public void OnResetButtonClick()
        {
            ResetToBlankSelection();
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();
        }

        public void Init(string? playerName, PlayerTypeInfo? playerType)
        {
            if (playerName is null || playerType is null) {
                _didInitialize = true;
                return;
            }

            // set player name
            playerNameInput!.text = playerName.Trim();

            // set player type in dropdown
            int index = _availablePlayerTypes.FindIndex(info => info.PlayerType == playerType.Value.PlayerType);
            if (index >= 0) {
                playerTypeDropdown!.SetValueWithoutNotify(index);
                PlayerType = playerType;
            }
            else {
                Debug.LogWarning($"Player type {playerType} not found in available player types.");
                PlayerType = null;
            }

            _didInitialize = true;
        }

        private void Awake()
        {
            if (playerTypeDropdown == null || playerNameInput == null || resetButton == null) {
                Debug.LogError("Dropdown, input field or reset button is not assigned in the inspector.");
                return;
            }

            // this needs to be in start, so that the Logger is activated - that is done in PlayerSelectionManager.Awake()
            InitializePlayerTypeDropdownOptions();
            ResetToBlankSelection();
            playerNameInput.characterLimit = NameCharacterLimit;
        }

        /// <summary>
        /// Makes the reset button visible if the selection is not empty.
        /// </summary>
        private void UpdateResetButtonVisibility()
        {
            resetButton!.interactable = !IsEmpty(); // enable reset button if selection is not empty
        }

        /// <summary>
        /// Sets the player type dropdown placeholder to "Select type".
        /// </summary>
        private void SetPlayerDropdownPlaceholder()
        {
            playerTypeDropdown!.placeholder.GetComponent<TextMeshProUGUI>().text = _typePlaceholder;
        }

        /// <summary>
        /// Updates the player dropdown placeholder. If player name is not empty, it sets the placeholder to "Select type".
        /// </summary>
        private void UpdatePlayerDropdownPlaceholder()
        {
            string placeholderText = string.IsNullOrEmpty(playerNameInput!.text) ? String.Empty : _typePlaceholder;
            playerTypeDropdown!.placeholder.GetComponent<TextMeshProUGUI>().text = placeholderText;
        }

        /// <summary>
        /// Sets the player name input field placeholder to "Enter name...".
        /// </summary>
        private void SetInputFieldPlaceholder()
        {
            playerNameInput!.placeholder.GetComponent<TextMeshProUGUI>().text = _namePlaceholder;
        }

        /// <summary>
        /// Updates the input field placeholder. If player type is selected, it sets the placeholder to "Enter name...".
        /// </summary>
        private void UpdateInputFieldPlaceholder()
        {
            string placeholderText = (PlayerType != null) ? _namePlaceholder : String.Empty;
            playerNameInput!.placeholder.GetComponent<TextMeshProUGUI>().text = placeholderText;
        }

        private void UpdateUI()
        {
            UpdatePlayerDropdownPlaceholder();
            UpdateInputFieldPlaceholder();
            UpdateResetButtonVisibility();
        }

        private void ResetToBlankSelection()
        {
            // reset player type
            playerTypeDropdown!.SetValueWithoutNotify(-1);
            _isDropdownListOpen = false;
            PlayerType = null;

            // reset player name
            if (playerNameInput!.text != String.Empty) {
                _didTrimInputFieldContent = true;
                playerNameInput.text = String.Empty;
            }

            UpdateUI();
        }

        /// <summary>
        /// Adds available player types to the dropdown.
        /// </summary>
        private void InitializePlayerTypeDropdownOptions()
        {
            // clear existing options
            playerTypeDropdown!.ClearOptions();

            // add possible player options
            _availablePlayerTypes.AddRange(AIPlayerTypesLoader.AvailableAIPlayerTypes);
            playerTypeDropdown.AddOptions(_availablePlayerTypes.Select(type => type.DisplayName).ToList());
        }

        /// <summary>
        /// Waits for a maximum of <paramref name="waitTime"/> seconds for the dropdown to close. If it closes, it calls the specified <paramref name="method"/>.
        /// </summary>
        /// <param name="waitTime">The maximum wait time.</param>
        /// <param name="method">The method to call upon close.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator CallMethodIfDropdownCloses(float waitTime, Action method)
        {
            float elapsedTime = 0f;
            while (elapsedTime < waitTime) {
                // search for the expanded dropdown list gameobject
                GameObject? dropdownList = playerTypeDropdown!.gameObject.transform.Find("Dropdown List")?.gameObject;

                // if dropdown list is closed
                if (dropdownList == null || dropdownList.activeSelf == false) {
                    _isDropdownListOpen = false;
                    method.Invoke();
                    break;
                }

                // wait until next frame
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        #endregion
    }
}
