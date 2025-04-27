#nullable enable

namespace ProjectL.UI
{
    using System;
    using System.Globalization;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    using ProjectL.DataManagement;

    /// <summary>
    /// Manages the PauseMenu prefab.
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        #region Constants

        private const int _animationSliderMinValue = 10;

        private const int _animationSliderMaxValue = 40;

        #endregion

        #region Fields

        [Header("Content Size Fitters")]
        [SerializeField] private RectTransform? panelRectTransform;
        [SerializeField] private RectTransform? outerScoreRectTransform;
        [SerializeField] private RectTransform? innerScoreRectTransform;

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

        private CanvasGroup? _canvasGroup;

        private bool _didInitialize = false;

        #endregion

        #region Methods

        /// <summary>
        /// Handles the click event for the "Home" button. Transitions to the main menu scene.
        /// </summary>
        public void OnHomeButtonClick()
        {
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();
            PauseLogic.Instance?.Resume();
            SceneLoader.Instance?.LoadMainMenuAsync();
        }

        /// <summary>
        /// Handles the click event for the "Back" button. Resumes the game.
        /// </summary>
        public void OnResumeButtonClick()
        {
            if (_didInitialize)
                SoundManager.Instance?.PlayButtonClickSound();
            PauseLogic.Instance?.Resume();
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
            if (outerScoreRectTransform == null || innerScoreRectTransform == null || panelRectTransform == null ||
                currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null ||
                scoreToggle == null || scoreNamesLabel == null || scoreValuesLabel == null ||
                animationSpeedSliderValueLabel == null || animationSpeedSlider == null) {
                Debug.LogError("PauseMenuManager: One or more required UI elements are not assigned.");
                return;
            }

            // needed to show / hide the pause menu
            _canvasGroup = GetComponent<CanvasGroup>();

            // hide score info by default
            scoreToggle.isOn = false;

            // setup animation speed slider
            animationSpeedSlider.minValue = _animationSliderMinValue;
            animationSpeedSlider.maxValue = _animationSliderMaxValue;
            animationSpeedSlider.value = Mathf.Round(PlayerPrefs.GetFloat(AnimationSpeed.AnimationSpeedPlayerPrefKey) * 10f);

            HidePauseMenu();

            _didInitialize = true;
        }

        private void Start()
        {
            if (PauseLogic.Instance == null) {
                Debug.LogError("PauseLogic singleton Instance is null.");
                return;
            }
            AdjustPanelSize();
            PauseLogic.Instance.OnPause += ShowPauseMenu;
            PauseLogic.Instance.OnResume += HidePauseMenu;
        }

        private void ShowPauseMenu()
        {
            if (_canvasGroup == null) {
                return; // safety check
            }
            UpdateUI();
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void HidePauseMenu()
        {
            if (_canvasGroup == null) {
                return; // safety check
            }
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void UpdateUI()
        {
            // safety check
            if (currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null || scoreNamesLabel == null || scoreValuesLabel == null) {
                return;
            }

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

        /// <summary>
        /// Adjusts the size of the panel to fit the content (the number of players is variable).
        /// This methods needs to be called when the GameObject is created. Otherwise the panel will flash as it adjust for the first time.
        /// </summary>
        private void AdjustPanelSize()
        {
            // safety check
            if (scoreValuesLabel == null || scoreNamesLabel == null) {
                return;
            }
            if (outerScoreRectTransform == null || innerScoreRectTransform == null || panelRectTransform == null) {
                return;
            }

            // put text with <num players> lines into the score labels
            // this will ensure that their size will not change when the real values are assigned
            int numPlayers = GameSettings.Players.Count;
            string text = "";
            for (int i = 0; i < numPlayers; i++) {
                text += "0\n";
            }
            scoreValuesLabel.text = text;
            scoreNamesLabel.text = text;

            // force adjust size of the panel to fit the content
            LayoutRebuilder.ForceRebuildLayoutImmediate(innerScoreRectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(outerScoreRectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
        }

        #endregion
    }
}