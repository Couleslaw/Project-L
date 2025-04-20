using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;
using ProjectLCore.Players;
#nullable enable

public class PlayerSelectionRowManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown? playerTypeDropdown;
    [SerializeField] private TMP_InputField? playerNameInput;

    public Type? SelectedPlayerType { get; private set; }
    public bool IsValidSelection { get; private set; }
    private IReadOnlyList<LoadedPlayerTypeInfo>? _availablePlayerTypes = null;
    private string _namePlaceholder = "Enter name...";

    void Start()
    {
        if (playerTypeDropdown == null || playerNameInput == null) {
            Debug.LogError("Dropdown or input field is not assigned in the inspector.");
            return;
        }
        // set no placeholder for input field
        playerNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = String.Empty;

        // handle dropdown selection
        _availablePlayerTypes = PlayerTypeLoader.AvailablePlayerTypes;
        PopulateDropdown();
        playerTypeDropdown.onValueChanged.AddListener(HandleDropdownSelection);
    }

    void PopulateDropdown()
    {
        if (playerTypeDropdown == null || _availablePlayerTypes == null) {
            Debug.LogError("Dropdown or available player types are not set.");
            return;
        }

        // clear existing options
        playerTypeDropdown.ClearOptions();

        // first add a blank option (default) and human player
        var options = new List<TMP_Dropdown.OptionData> { new(string.Empty) , new("Human") };
        // add AI players
        options.AddRange(_availablePlayerTypes.Select(info => new TMP_Dropdown.OptionData(info.DisplayName)));
        playerTypeDropdown.AddOptions(options);
    }

    void HandleDropdownSelection(int index)
    {
        if (playerNameInput == null) {
            Debug.LogError("Input field is not set.");
            return;
        }
        if (_availablePlayerTypes == null) {
            Debug.LogError("Available player types list is null");
            return;
        }
        // Index 0 is the placeholder ("nothing selected")
        if (index == 0) {
            SelectedPlayerType = null;
        }
        else if (index == 1) {
            // Human player
            SelectedPlayerType = typeof(HumanPlayer);
        }
        else {
            // Adjust index to match the 'availablePlayerTypes' list (which doesn't have the placeholder)
            int actualPlayerIndex = index - 2;

            if (actualPlayerIndex < 0 || actualPlayerIndex >= _availablePlayerTypes.Count) {
                Debug.LogWarning($"Invalid dropdown index mapping. Index: {index}, Calculated Player Index: {actualPlayerIndex}");
                SelectedPlayerType = null;
            }
            else {
                SelectedPlayerType = _availablePlayerTypes[actualPlayerIndex].PlayerType;
                Debug.Log($"Dropdown selection changed. Name: {_availablePlayerTypes[actualPlayerIndex].DisplayName}");
            }
        }

        // set input field placeholder if a player is selected
        playerNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = (SelectedPlayerType != null) ? _namePlaceholder : String.Empty;
    }

    /// <summary>
    /// Resets the player selection to a blank state.
    /// </summary>
    public void ResetToBlankSelection()
    {
        if (playerTypeDropdown == null || playerNameInput == null) {
            Debug.LogError("Dropdown or input field is not set.");
            return;
        }
        // reset dropdown menu
        playerTypeDropdown.SetValueWithoutNotify(0);
        // playerTypeDropdown.RefreshShownValue();

        // set player name to blank
        playerNameInput.text = String.Empty;
        playerNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = String.Empty;
    }

    // Example: Call this from a Button press or other event
    public void OnResetSelectionPress()
    {
        Debug.Log("Resetting dropdown selection...");
        ResetToBlankSelection();
    }
}
