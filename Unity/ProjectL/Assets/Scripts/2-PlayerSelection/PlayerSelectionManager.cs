#nullable enable

namespace ProjectL.PlayerSelectionScene
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using ProjectL.Sound;
    using ProjectL.Data;
    using ProjectL.Management;
    using System.Linq;

    /// <summary>
    /// Manages the <c>Player Selection</c> scene.
    /// </summary>
    public class PlayerSelectionManager : MonoBehaviour
    {
        #region Constants

        private const int _sliderMultiplier = 5;

        private const int _minNumInitialTetrominos = 5;

        private const int _maxNumInitialTetrominos = 25;

        #endregion

        #region Fields

        [Header("UI Elements")]
        [SerializeField] private Slider? numPiecesSlider;
        [SerializeField] private TextMeshProUGUI? numPiecesText;
        [SerializeField] private Button? startGameButton;
        [SerializeField] private Toggle? shuffleCheckbox;
        [SerializeField] private TextMeshProUGUI? errorTextBox;

        [Header("Fade Settings")]
        [SerializeField] private float errorVisibleDuration = 1.0f;
        [SerializeField] private float errorFadeOutDuration = 1.0f;

        [Header("Player Selection")]
        [SerializeField] private List<PlayerSettingsRow>? playerSelectionRows;

        private Coroutine? _activeErrorCoroutine = null;

        private bool _didInitialize = false;

        #endregion

        #region Methods

        /// <summary>
        /// Handles the value changed event for the "Number of pieces in reserve" slider.
        /// </summary>
        public void OnNumPiecesSliderValueChanged(Single value)
        {
            if (numPiecesSlider == null || numPiecesText == null) {
                return;
            }
            int num = (int)value * _sliderMultiplier;
            numPiecesText.text = num.ToString();
            if (_didInitialize)
                SoundManager.Instance?.PlaySliderSound();
        }

        /// <summary>
        /// Handles the value changed event for the "Shuffle players" checkbox.
        /// </summary>
        /// <param name="toggled"> <see langword="true"/> to shuffle players; otherwise, <see langword="false"/>.</param>
        public void OnShuffleCheckboxValueChanged(bool toggled)
        {
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();
        }

        /// <summary>
        /// Handles the back button click event. Loads the main menu scene.
        /// </summary>
        public void OnBackButtonClick()
        {
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();
            SceneLoader.Instance?.LoadMainMenuAsync();
        }

        /// <summary>
        /// Handles the "Start Game" button click event.
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
                Debug.LogWarning(errorMessage);
                ShowError(errorMessage); // Show the error message on screen
                return;
            }

            // play button click sound
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();

            // get number of pieces from slider
            GameSettings.NumInitialTetrominos = int.Parse(numPiecesText!.text.Trim());

            // get shuffle players checkbox value
            GameSettings.ShouldShufflePlayers = shuffleCheckbox!.isOn;

            // populate GameSettings with selected players
            GameSettings.Players.Clear();
            foreach (var row in playerSelectionRows!) {
                if (!row.IsEmpty()) {
                    // last check if player reference is valid, should not happen
                    if (row.PlayerType == null || string.IsNullOrEmpty(row.PlayerName)) {
                        Debug.LogError($"Error while selecting player: Player {row.PlayerName}, Type {row.PlayerType?.DisplayName}");
                        ShowError($"Internal error");
                        return;
                    }
                    GameSettings.Players.Add(row.PlayerName, row.PlayerType!.Value);
                }
            }

            // load the game scene
            SceneLoader.Instance?.LoadGameAsync();
        }

        private void Start()
        {
            // check if all UI components are assigned
            if (numPiecesSlider == null || numPiecesText == null || startGameButton == null || shuffleCheckbox == null || errorTextBox == null) {
                Debug.LogError("One or more UI components are not assigned in the inspector.");
                return;
            }

            SetUpSettingDefaults();
            HideErrorMessageBox();

            _didInitialize = true;

#if UNITY_WEBGL
            Debug.LogWarning("AI players are not supported in the WebGL version of the game. To use the AI players feature, download the desktop release of the game.");
#endif
        }

        private void SetUpSettingDefaults()
        {
            if (numPiecesSlider == null || numPiecesText == null || shuffleCheckbox == null) {
                return;
            }

            // shuffle players checkbox
            shuffleCheckbox.isOn = GameSettings.ShouldShufflePlayers;

            // number of pieces slider
            numPiecesSlider.minValue = _minNumInitialTetrominos / _sliderMultiplier;
            numPiecesSlider.maxValue = _maxNumInitialTetrominos / _sliderMultiplier;
            numPiecesSlider.value = GameSettings.NumInitialTetrominos / _sliderMultiplier;

            // if there are already some players from last the last game played --> fill in the player selection rows

            int i = 0;
            List<string> lastPlayerNames = GameSettings.Players.Keys.ToList();
            foreach (var row in playerSelectionRows!) {
                // use i-th player name from last game played
                if (lastPlayerNames.Count > i) {
                    string playerName = lastPlayerNames[i];
                    var playerType = GameSettings.Players[playerName];
                    row.Init(playerName, playerType);
                }
                // if there were <= i players last game --> black selection
                else {
                    row.Init(null, null);
                }
                i++;
            }
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

        private void HideErrorMessageBox() => errorTextBox!.alpha = 0f;

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
            SoundManager.Instance?.PlayErrorSound();
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
}