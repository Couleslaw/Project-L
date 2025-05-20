#nullable enable

namespace ProjectL.Pause
{
    using ProjectL.Data;
    using ProjectL.Animation;
    using ProjectL.Management;
    using ProjectL.Sound;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using System;
    using System.Globalization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the <c>PauseMenu</c> prefab.
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
        [SerializeField] private TextMeshProUGUI? gamePhaseLabel;
        [SerializeField] private TextMeshProUGUI? actionsLeftValueLabel;
        [SerializeField] private TextMeshProUGUI? actionsLeftTitleLabel;

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

        /// <summary>
        /// Hides the pause menu.
        /// </summary>
        public void Hide() => gameObject.SetActive(false);

        /// <summary>
        /// Shows the pause menu.
        /// </summary>
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
            if (currentPlayerLabel == null || actionsLeftValueLabel == null || actionsLeftTitleLabel == null || gamePhaseLabel == null ||
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
            if (currentPlayerLabel == null || actionsLeftValueLabel == null || gamePhaseLabel == null || scoreNamesLabel == null || scoreValuesLabel == null || scorePanel == null || turnInfoPanel == null) {
                return;
            }

            // toggle visibility of game related UI elements
            scorePanel.SetActive(RuntimeGameInfo.IsGameInProgress);
            turnInfoPanel.SetActive(RuntimeGameInfo.IsGameInProgress);

            // if this is final results --> we are done
            if (!RuntimeGameInfo.TryGetCurrentInfo(out var gameInfo)) {
                return;
            }

            // update player name and game phase
            currentPlayerLabel.text = gameInfo.PlayerName;
            gamePhaseLabel.text = GetGamePhaseString(gameInfo.CurrentTurnInfo);

            // update number of actions left
            UpdateNumActionsLabel(gameInfo.CurrentTurnInfo);

            // update player scores
            scoreNamesLabel.text = string.Empty;
            scoreValuesLabel.text = string.Empty;
            foreach (var item in gameInfo.PlayerScores) {
                scoreNamesLabel.text += item.Key + "\n";
                scoreValuesLabel.text += item.Value.ToString() + "\n";
            }
        }

        private string GetGamePhaseString(TurnInfo turnInfo)
        {
            return turnInfo.GamePhase switch {
                GamePhase.Normal => "Normal",
                GamePhase.EndOfTheGame => turnInfo.LastRound ? "Final round" : "Next round is final",
                GamePhase.FinishingTouches => "Finishing touches",
                GamePhase.Finished => "Game ended",
                _ => "Unknown"
            };
        }

        private void UpdateNumActionsLabel(TurnInfo turnInfo)
        {
            if (actionsLeftValueLabel == null || actionsLeftTitleLabel == null) {
                return;  // safety check
            }

            // don't display actions left in finishing touches / when game is finished
            if (turnInfo.GamePhase == GamePhase.FinishingTouches || turnInfo.GamePhase == GamePhase.Finished) {
                actionsLeftValueLabel.gameObject.SetActive(false);
                actionsLeftTitleLabel.gameObject.SetActive(false);
                return;
            }

            // get number of actions left
            // if 0 --> this means that the game has not initialized yet - dealing cards animation is playing
            // change to max (3) - first player didn't take any actions yet
            int numActions = turnInfo.NumActionsLeft;
            if (numActions == 0) {
                numActions = TurnManager.NumActionsInTurn;
            }

            // update text and color
            actionsLeftValueLabel.gameObject.SetActive(true);
            actionsLeftTitleLabel.gameObject.SetActive(true);
            actionsLeftValueLabel.text = GetActionsLeftString(numActions);
            actionsLeftValueLabel.color = GetActionsLeftColor(numActions);
        }

        private string GetActionsLeftString(int numActions)
        {
            if (numActions == 0) {
                return "No actions left";
            }
            if (numActions == 1) {
                return "1 Action left";
            }
            return numActions.ToString() + " Actions left";
        }

        private Color GetActionsLeftColor(int numActions)
        {
            return (numActions) switch {
                1 => ColorManager.red,
                2 => ColorManager.orange,
                3 => ColorManager.green,
                _ => Color.white
            };
        }

        #endregion
    }
}
