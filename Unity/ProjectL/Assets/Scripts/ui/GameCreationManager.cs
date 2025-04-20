using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectLCore.GameLogic;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectLCore.Players;
using System.Runtime.CompilerServices;

#nullable enable

public class GameCreationManager : MonoBehaviour
{
    [SerializeField]
    private Slider? numPiecesSlider;

    [SerializeField]
    private TextMeshProUGUI? numPiecesText;

    [SerializeField]
    private Button? startGameButton;

    [SerializeField]
    private Toggle? shuffleCheckbox;

    [SerializeField]
    private List<PlayerSelectionRowManager>? playerSelectionRows;

    void Start()
    {
        if (numPiecesSlider == null || numPiecesText == null || startGameButton == null || shuffleCheckbox == null) {
            Debug.LogError("One or more UI components are not assigned in the inspector.");
            return;
        }

        GameStartParams.Reset();
        // Set the initial state of the shuffle players checkbox
        shuffleCheckbox.isOn = GameStartParams.ShufflePlayersDefault;

        // Set the initial state of num pieces slider
        numPiecesSlider.minValue = GameState.MinNumInitialTetrominos;
        numPiecesSlider.value = GameStartParams.NumInitialTetrominosDefault;
        numPiecesText.text = numPiecesSlider.value.ToString();
    }

    /// <summary>
    /// Changes the number of initial tetrominos.
    /// </summary>
    public void OnNumPiecesChanged()
    {
        if (numPiecesSlider == null || numPiecesText == null) {
            Debug.LogError("Slider or text component is not assigned.");
            return;
        }
        int num = (int)numPiecesSlider.value;
        GameStartParams.NumInitialTetrominos = num;
        numPiecesText.text = num.ToString();
    }

    /// <summary>
    /// Toggles the shuffle players setting.
    /// </summary>
    public void OnShuffleToggled()
    {
        if (shuffleCheckbox == null) {
            Debug.LogError("Shuffle checkbox is not assigned.");
            return;
        }
        GameStartParams.ShufflePlayers = shuffleCheckbox.isOn;
    }

    private bool IsPlayerSelectionNonEmpty()
    {
        // at least one player must be selected
        foreach (var row in playerSelectionRows!) {
            if (!row.IsEmpty()) {
                return true;
            }
        }
        return false;
    }

    private bool IsPlayerSelectionValid()
    {
        // check if all players are valid
        foreach (var row in playerSelectionRows!) {
            if (!row.IsValid()) {
                return false;
            }
        }
        return true;
    }
    
    private bool ArePlayerNamesUnique()
    {
        // check if all player names are unique
        HashSet<string> playerNames = new HashSet<string>();
        foreach (var row in playerSelectionRows!) {
            if (!row.IsEmpty()) {
                string playerName = row.SelectedPlayerName;
                if (playerNames.Contains(playerName)) {
                    return false;
                }
                playerNames.Add(playerName);
            }
        }

        return true;
    }

    public void OnClickStartGame()
    {
        if (!IsPlayerSelectionNonEmpty()) {
            Debug.LogWarning("No players selected.");
            return;
        }

        if (!IsPlayerSelectionValid()) {
            Debug.LogWarning("Invalid player selection.");
            return;
        }

        if (!ArePlayerNamesUnique()) {
            Debug.LogWarning("Player names must be unique.");
            return;
        }

        // populate GameStartParams with selected players and load GameScene
        foreach (var row in playerSelectionRows!) {
            if (!row.IsEmpty()) {
                GameStartParams.Players.Add(new(row.SelectedPlayerName, row.SelectedPlayerType!.Value));
            }
        }

        GameObject.FindAnyObjectByType<SceneTransitions>()?.LoadGame();
    }
}
