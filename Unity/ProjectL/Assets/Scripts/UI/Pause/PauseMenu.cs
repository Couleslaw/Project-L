#nullable enable

namespace ProjectL.UI.Pause
{
    using System;
    using System.Globalization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using ProjectL.Data;
    using ProjectL.Management;
    using ProjectL.UI.Sound;

    /// <summary>
    /// Manages the PauseMenu prefab.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        #region Constants

        private const int _animationSliderMinValue = 10;

        private const int _animationSliderMaxValue = 40;

        #endregion

        #region Fields

        [Header("Turn Info")]
        [SerializeField] private TextMeshProUGUI? currentPlayerLabel;
        [SerializeField] private TextMeshProUGUI? actionsLeftLabel;
        [SerializeField] private TextMeshProUGUI? gamePhaseLabel;

        [Header("Score Info")]
        [SerializeField] private Toggle? scoreToggle;
        [SerializeField] private TextMeshProUGUI? scoreNamesLabel;
        [SerializeField] private TextMeshProUGUI? scoreValuesLabel;

        [Header("Animation Speed")]
        [SerializeField] private TextMeshProUGUI? animationSpeedSliderValueLabel;
        [SerializeField] private Slider? animationSpeedSlider;

        [Header("Scene switching")]
        [SerializeField] private GameObject? scorePanel;
        [SerializeField] private GameObject? turnInfoPanel;

        private bool _didInitialize = false;

        #endregion

        #region Methods

        public void Hide() => gameObject.SetActive(false);
        public void Show() => gameObject.SetActive(true);

        /// <summary>
        /// Handles the click event for the "Home" button. Transitions to the main menu scene.
        /// </summary>
        public void OnHomeButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            GameManager.Instance?.ResumeGame();
            SceneLoader.Instance?.LoadMainMenuAsync();
        }

        /// <summary>
        /// Handles the click event for the "Back" button. Resumes the game.
        /// </summary>
        public void OnResumeButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            GameManager.Instance?.ResumeGame();
        }

        /// <summary>
        /// Handles the click event for the "Score" checkbox. Toggles the visibility of the score labels.
        /// </summary>
        /// <param name="showScore"> <see langword="true"/> to show the score labels; otherwise, <see langword="false"/>.</param>
        public void OnScoreToggleClick(bool showScore)
        {
            // safety check
            if (scoreNamesLabel == null || scoreValuesLabel == null) {
                return;
            }

            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();

            // use alpha to show / hide the labels
            if (showScore) {
                scoreNamesLabel.alpha = 1f;
                scoreValuesLabel.alpha = 1f;
            }
            else {
                scoreNamesLabel.alpha = 0;
                scoreValuesLabel.alpha = 0;
            }
        }

        /// <summary>
        /// Handles the value change event for the animation speed slider. Stores the new value in PlayerPrefs and updates the displayed value.
        /// Values on the slider range from <see cref="_animationSliderMinValue"/> (10) to <see cref="_animationSliderMaxValue"/> (40), which is then converted to a speed factor of 1.0 to 4.0.
        /// </summary>
        /// <param name="value">The value.</param>
        public void OnAnimationSpeedSliderValueChanged(Single value)
        {
            // play sound
            if (_didInitialize)
                SoundManager.Instance?.PlaySliderSound();

            // convert 37 -> 3.7
            value = value / 10f;

            // save value to PlayerPrefs
            PlayerPrefs.SetFloat(AnimationSpeed.AnimationSpeedPlayerPrefKey, value);

            // update label
            if (animationSpeedSliderValueLabel == null) {
                return;  // safety check
            }

            animationSpeedSliderValueLabel.text = value.ToString(CultureInfo.InvariantCulture) + "×";
        }

        private void Awake()
        {
            // check if all required components are assigned
            if (currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null ||
                scoreToggle == null || scoreNamesLabel == null || scoreValuesLabel == null ||
                animationSpeedSliderValueLabel == null || animationSpeedSlider == null) {
                Debug.LogError("PauseMenuManager: One or more required UI elements are not assigned.");
                return;
            }

            // hide score info by default
            scoreToggle.isOn = false;

            // setup animation speed slider
            animationSpeedSlider.minValue = _animationSliderMinValue;
            animationSpeedSlider.maxValue = _animationSliderMaxValue;
            animationSpeedSlider.value = Mathf.Round(PlayerPrefs.GetFloat(AnimationSpeed.AnimationSpeedPlayerPrefKey) * 10f);

            _didInitialize = true;
        }

        private void OnEnable()
        {
            if (currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null || scoreNamesLabel == null || scoreValuesLabel == null || scorePanel == null || turnInfoPanel == null) {
                return;
            }


            // toggle visibility of game related UI elements
            scorePanel.SetActive(RuntimeGameInfo.IsGameInProgress);
            turnInfoPanel.SetActive(RuntimeGameInfo.IsGameInProgress);

            // if this is final results --> we are done
            if (!RuntimeGameInfo.TryGetCurrentInfo(out var gameInfo)) {
                return;
            }

            // update turn info
            currentPlayerLabel.text = gameInfo.PlayerName;
            actionsLeftLabel.text = gameInfo.ActionsLeft.ToString();
            gamePhaseLabel.text = gameInfo.GamePhase.ToString();

            // update player scores
            scoreNamesLabel.text = string.Empty;
            scoreValuesLabel.text = string.Empty;
            foreach (var item in gameInfo.PlayerScores) {
                scoreNamesLabel.text += item.Key + "\n";
                scoreValuesLabel.text += item.Value.ToString() + "\n";
            }
        }

        #endregion
    }
}