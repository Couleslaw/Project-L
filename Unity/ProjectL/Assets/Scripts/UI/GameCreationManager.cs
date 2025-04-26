using ProjectLCore.GameLogic;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

/// <summary>
/// Manages the game creation UI, including player selection and game start parameters.
/// </summary>
public class GameCreationManager : MonoBehaviour
{
    #region Fields

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
    private SceneTransitions? _sceneTransitions;

    private const int _sliderMultiplier = 5;
    private const int _maxNumInitialTetrominos = 30;

    #endregion

    #region Methods

    /// <summary>
    /// Changes the number of initial tetrominos.
    /// </summary>
    public void OnNumPiecesChanged()
    {
        if (numPiecesSlider == null || numPiecesText == null) {
            Debug.LogError("Slider or text component is not assigned.");
            return;
        }
        int num = (int)numPiecesSlider.value * _sliderMultiplier;
        GameStartParams.NumInitialTetrominos = num;
        numPiecesText.text = num.ToString();
        _soundManager?.PlaySliderSound();
    }

    /// <summary>
    /// Toggles the shuffle players setting.
    /// </summary>
    /// <param name="shuffle"> <see langword="true"/> to shuffle players; otherwise, <see langword="false"/>.</param>
    public void OnShuffleToggled(bool shuffle)
    {
        _soundManager?.PlayButtonClickSound();
        GameStartParams.ShufflePlayers = shuffle;
    }

    /// <summary>
    /// Handles the back button click event. Loads the main menu scene.
    /// </summary>
    public void OnBackButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        _sceneTransitions?.LoadMainMenuAsync();
    }


    /// <summary>
    /// Handles the start game button click event.
    /// Starts the game if the player selection is valid. Otherwise shows an error message.
    /// </summary>
    public void OnStartGameButtonClick()
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

        // play button click sound
        _soundManager?.PlayButtonClickSound();

        // populate GameStartParams with selected players
        GameStartParams.Players.Clear();
        foreach (var row in playerSelectionRows!) {
            if (!row.IsEmpty()) {
                // last check if player reference is valid, should not happen
                if (row.PlayerType == null || string.IsNullOrEmpty(row.PlayerName)) {
                    Debug.LogError($"Error while selecting player: Player {row.PlayerName}, Type {row.PlayerType?.DisplayName}");
                    ShowError($"Internal error");
                    return;
                }
                GameStartParams.Players.Add(row.PlayerName, row.PlayerType!.Value);
            }
        }

        // load the game scene
        _sceneTransitions?.LoadGameAsync();
    }

    private void Awake()
    {
        // create a logger instance if it doesn't exist
        if (EasyUI.Logger.Instance == null)
            Instantiate(loggerPrefab);
        else
            EasyUI.Logger.Instance.gameObject.SetActive(true);
    }

    private void Start()
    {
        // check if all UI components are assigned
        if (numPiecesSlider == null || numPiecesText == null || startGameButton == null || shuffleCheckbox == null || errorTextBox == null) {
            Debug.LogError("One or more UI components are not assigned in the inspector.");
            return;
        }

        // get transitions
        _sceneTransitions = gameObject.transform.GetComponent<SceneTransitions>();

        // reset the start parameters
        GameStartParams.Reset();

        // Set the initial state of the shuffle players checkbox
        shuffleCheckbox.isOn = GameStartParams.ShufflePlayersDefault;

        // Set the initial state of num pieces slider
        SetUpSlider();

        // make error text box invisible
        errorTextBox.alpha = 0f;

        // try to find the sound manager
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();
    }

    private void SetUpSlider()
    {
        if (numPiecesSlider == null || numPiecesText == null) {
            Debug.LogError("Slider or text component is not assigned.");
            return;
        }

        numPiecesSlider.minValue = GameState.MinNumInitialTetrominos / _sliderMultiplier;
        numPiecesSlider.maxValue = _maxNumInitialTetrominos / _sliderMultiplier;
        numPiecesSlider.value = GameStartParams.NumInitialTetrominosDefault / _sliderMultiplier;
    }

    /// <summary>
    /// Checks if at least one player is selected.
    /// </summary>
    /// <returns><see langword="true"/> if at least one player is selected; otherwise, <see langword="false"/>.</returns>
    private bool IsPlayerSelectionNonEmpty()
    {
        foreach (var row in playerSelectionRows!) {
            if (!row.IsEmpty()) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Validates the player selection.
    /// </summary>
    /// <returns><see langword="true"/> if all selected players are valid; otherwise, <see langword="false"/>.</returns>
    private bool IsPlayerSelectionValid()
    {
        foreach (var row in playerSelectionRows!) {
            if (!row.IsValid()) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if all player names are unique.
    /// </summary>
    /// <returns><see langword="true"/> if all player names are unique; otherwise, <see langword="false"/>.</returns>
    private bool ArePlayerNamesUnique()
    {
        HashSet<string> playerNames = new HashSet<string>();
        foreach (var row in playerSelectionRows!) {
            if (!row.IsEmpty()) {
                string playerName = row.PlayerName;
                if (playerNames.Contains(playerName)) {
                    return false;
                }
                playerNames.Add(playerName);
            }
        }

        return true;
    }

    /// <summary>
    /// Displays an error message and starts the fade-out coroutine to hide it after a delay.
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
    /// Displays a message, waits for <see cref="errorVisibleDuration"/> seconds and then fade it out over <see cref="errorFadeOutDuration"/> seconds.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator ShowAndFadeErrorCoroutine(string message)
    {
        if (errorTextBox == null)
            yield break; // Should not happen

        // Set text and make fully visible
        errorTextBox.text = message;
        errorTextBox.alpha = 1f;

        // Wait for the visible duration
        yield return new WaitForSeconds(errorVisibleDuration);

        // Fade out
        float elapsedTime = 0f;
        while (elapsedTime < errorFadeOutDuration) {
            elapsedTime += Time.deltaTime;
            errorTextBox.alpha = Mathf.Lerp(1f, 0f, elapsedTime / errorFadeOutDuration);
            yield return null; // Wait for the next frame
        }

        // Mark coroutine as finished
        _activeErrorCoroutine = null;
    }

    #endregion
}
