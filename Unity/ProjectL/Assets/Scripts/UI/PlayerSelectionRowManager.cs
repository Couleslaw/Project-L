using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;
using ProjectLCore.Players;
using UnityEngine.UI;
#nullable enable

public class PlayerSelectionRowManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown? playerTypeDropdown;
    [SerializeField] private TMP_InputField? playerNameInput;
    [SerializeField] private Button? resetButton;

    public LoadedPlayerTypeInfo? SelectedPlayerType { get; private set; }
    public string SelectedPlayerName => playerNameInput!.text.Trim();
    private List<LoadedPlayerTypeInfo> _availablePlayerInfos = new() { new LoadedPlayerTypeInfo(typeof(HumanPlayer), "Human", null) };
    private const string _namePlaceholder = "Enter name...";
    private const string _typePlaceholder = "Select type";

    void Start()
    {
        if (playerTypeDropdown == null || playerNameInput == null || resetButton == null) {
            Debug.LogError("Dropdown, input field or reset button is not assigned in the inspector.");
            return;
        }
        // initialize dropdown selection
        _availablePlayerInfos.AddRange(PlayerTypeLoader.AvailableAIPlayerInfos);
        PopulateDropdown();
        playerTypeDropdown.onValueChanged.AddListener(HandleDropdownSelection);

        // reset player type --> blank state
        ResetToBlankSelection();
        resetButton.onClick.AddListener(ResetToBlankSelection);
        playerNameInput.onValueChanged.AddListener(OnInputFieldChanged);
    }

    void PopulateDropdown()
    {
        // clear existing options
        playerTypeDropdown!.ClearOptions();

        // add possible player options
        playerTypeDropdown.AddOptions(_availablePlayerInfos.Select(info => info.DisplayName).ToList());
    }

    void HandleDropdownSelection(int index)
    {
        // check if index is valid
        if (index < 0 || index >= _availablePlayerInfos.Count) {
            Debug.LogWarning($"Invalid dropdown index mapping. Index: {index}.");
            SelectedPlayerType = null;
        }
        else {
            SelectedPlayerType = _availablePlayerInfos[index];
        }

        // set input field placeholder if a player is selected
        playerNameInput!.placeholder.GetComponent<TextMeshProUGUI>().text = (SelectedPlayerType != null) ? _namePlaceholder : String.Empty;

        ToggleResetButtonVisibility();
    }

    public void ToggleResetButtonVisibility()
    {
        resetButton!.interactable = !IsEmpty(); // enable reset button if selection is not empty
    }

    public void OnInputFieldChanged(string value)
    {
        // if input is not empty, set dropdown selection placeholder to "select type"
        playerTypeDropdown!.placeholder.GetComponent<TextMeshProUGUI>().text = string.IsNullOrEmpty(value) ? String.Empty : _typePlaceholder;
        ToggleResetButtonVisibility();
    }

    /// <summary>
    /// Resets the player selection to a blank state.
    /// </summary>
    public void ResetToBlankSelection()
    {
        // reset dropdown menu
        playerTypeDropdown!.SetValueWithoutNotify(-1);
        SelectedPlayerType = null;

        // set player name to blank
        playerNameInput!.text = String.Empty;
        playerNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = String.Empty;

        // disable reset button
        ToggleResetButtonVisibility();
    }

    public bool IsEmpty() => SelectedPlayerType == null && string.IsNullOrEmpty(SelectedPlayerName);

    public bool IsValid()
    {
        // empty selection
        if (IsEmpty()) {
            return true;
        }
        // fully initialized selection
        if (SelectedPlayerType != null && !string.IsNullOrEmpty(SelectedPlayerName)) {
            return true;
        }
        // invalid selection
        return false;
    }
}
