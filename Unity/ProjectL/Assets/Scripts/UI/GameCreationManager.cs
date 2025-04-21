using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectLCore.GameLogic;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectLCore.Players;
using System.Runtime.CompilerServices;
using System.Collections;
using EasyUI;

#nullable enable

public class GameCreationManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider? numPiecesSlider;
    [SerializeField] private TextMeshProUGUI? numPiecesText;
    [SerializeField] private Button? startGameButton;
    [SerializeField] private Toggle? shuffleCheckbox;
    [SerializeField] private TextMeshProUGUI? errorTextBox;
    [SerializeField] private GameObject? loggerPrefab;

    [Header("Fade Settings")]
    [SerializeField] private float errorVisibleDuration = 1.0f;
    [SerializeField] private float errorFadeOutDuration = 1.0f;

    [Header("Player Selection")]
    [SerializeField] private List<PlayerSelectionRowManager>? playerSelectionRows;

    private Coroutine? _activeErrorCoroutine = null;
    private SoundManager? _soundManager;

    private void Awake()
    {
        if (EasyUI.Logger.Instance == null)
            Instantiate(loggerPrefab);
        else
            EasyUI.Logger.Instance.gameObject.SetActive(true);
    }

    void Start()
    {
        if (numPiecesSlider == null || numPiecesText == null || startGameButton == null || shuffleCheckbox == null || errorTextBox == null) {
            Debug.LogError("One or more UI components are not assigned in the inspector.");
            return;
        }

        // try to find the sound manager
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();

        GameStartParams.Reset();
        // Set the initial state of the shuffle players checkbox
        shuffleCheckbox.isOn = GameStartParams.ShufflePlayersDefault;

        // Set the initial state of num pieces slider
        numPiecesSlider.minValue = GameState.MinNumInitialTetrominos;
        numPiecesSlider.value = GameStartParams.NumInitialTetrominosDefault;
        numPiecesText.text = numPiecesSlider.value.ToString();

        // make error text box invisible
        errorTextBox.alpha = 0f;
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
        // check if player selection is valid
        string? errorMessage = null;
        if (!IsPlayerSelectionNonEmpty())
            errorMessage = "No players selected";
        else if (!IsPlayerSelectionValid())
            errorMessage = "Invalid player selection";
        else if (!ArePlayerNamesUnique())
            errorMessage = "Player names must be unique";

        // handle error message
        if (errorMessage != null) {
            // play error sound
            _soundManager?.PlayErrorSound();
            Debug.LogWarning(errorMessage);
            ShowError(errorMessage); // Show the error message on screen
            return;
        }

        // play button sound
        _soundManager?.PlayButtonClickSound();

        // populate GameStartParams with selected players and load GameScene
        GameStartParams.Players.Clear();
        foreach (var row in playerSelectionRows!) {
            if (!row.IsEmpty()) {
                // last check if player reference is valid, should not happen
                if (row.SelectedPlayerType == null || string.IsNullOrEmpty(row.SelectedPlayerName)) {
                    Debug.LogError($"Error while selecting player: Player {row.SelectedPlayerName}, Type {row.SelectedPlayerType?.DisplayName}");
                    ShowError($"Internal error");
                    return;
                }
                GameStartParams.Players.Add(new(row.SelectedPlayerName, row.SelectedPlayerType!.Value));
            }
        }

        GameObject.FindAnyObjectByType<SceneTransitions>()?.LoadGame();
    }

    /// <summary>
    /// Displays an error message and starts the fade-out coroutine.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    private void ShowError(string message)
    {
        if (errorTextBox == null)
            return; // Safety check

        // Stop any previous fade coroutine if it's running
        if (_activeErrorCoroutine != null) {
            StopCoroutine(_activeErrorCoroutine);
        }

        // Start the new fade coroutine
        _activeErrorCoroutine = StartCoroutine(ShowAndFadeErrorCoroutine(message));
    }

    /// <summary>
    /// Coroutine to display a message, wait, and then fade it out.
    /// </summary>
    /// <param name="message">The message to display.</param>
    private IEnumerator ShowAndFadeErrorCoroutine(string message)
    {
        if (errorTextBox == null)
            yield break; // Should not happen

        // 1. Set text and make fully visible
        errorTextBox.text = message;
        errorTextBox.alpha = 1f;

        // 2. Wait for the visible duration
        yield return new WaitForSeconds(errorVisibleDuration);

        // 3. Fade out
        float elapsedTime = 0f;
        while (elapsedTime < errorFadeOutDuration) {
            elapsedTime += Time.deltaTime;
            errorTextBox.alpha = Mathf.Lerp(1f, 0f, elapsedTime / errorFadeOutDuration);
            yield return null; // Wait for the next frame
        }

        _activeErrorCoroutine = null; // Mark coroutine as finished
    }
}
