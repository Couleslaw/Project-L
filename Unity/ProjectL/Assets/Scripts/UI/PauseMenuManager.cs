using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

/// <summary>
/// Manages the PauseMenu prefab.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
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

    private SceneTransitions? _sceneTransitions;
    private PauseLogic? _pauseLogic;
    private GameManager? _gameManager;
    private SoundManager? _soundManager;
    private CanvasGroup? _canvasGroup;

    private const int _animationSliderMinValue = 10;
    private const int _animationSliderMaxValue = 40;

    #endregion

    #region Methods

    /// <summary>
    /// Shows the pause menu.
    /// </summary>
    public void ShowPauseMenu()
    {
        if (_canvasGroup == null) {
            return; // safety check
        }
        UpdateUI();
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Hides the pause menu.
    /// </summary>
    public void HidePauseMenu()
    {
        if (_canvasGroup == null) {
            return; // safety check
        }
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Handles the click event for the "Home" button. Transitions to the main menu scene.
    /// </summary>
    public void OnHomeButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        _pauseLogic?.Resume();
        _sceneTransitions?.LoadMainMenuAsync();
    }

    /// <summary>
    /// Handles the click event for the "Back" button. Resumes the game.
    /// </summary>
    public void OnResumeButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        _pauseLogic?.Resume();
    }

    /// <summary>
    /// Handles the click event for the "Score" checkbox. Toggles the visibility of the score labels.
    /// </summary>
    /// <param name="showScore"> <see langword="true"/> to show the score labels; otherwise, <see langword="false"/>.</param>
    public void OnScoreToggleClick(bool showScore)
    {
        // safety check
        if (scoreNamesLabel == null || scoreValuesLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required ScoreInfo UI elements are not assigned.");
            return;
        }

        _soundManager?.PlayButtonClickSound();

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
        _soundManager?.PlaySliderSound();

        // convert 37 -> 3.7
        value = value / 10f;

        // save value to PlayerPrefs
        PlayerPrefs.SetFloat(AnimationSpeedManager.AnimationSpeedPlayerPrefKey, value);

        // update label
        if (animationSpeedSliderValueLabel == null) {
            return;  // safety check
        }

        animationSpeedSliderValueLabel.text = value.ToString(CultureInfo.InvariantCulture) + "×";
    }

    /// <summary>
    /// Updates information displayed in the pause menu.
    /// </summary>
    private void UpdateUI()
    {
        // GameManager will be null in final results scene
        if (_gameManager == null) {
            return;
        }

        // safety check
        if (currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null || scoreNamesLabel == null || scoreValuesLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required TurnInfo UI elements are not assigned.");
            return;
        }

        // update turn info
        currentPlayerLabel.text = _gameManager?.GetCurrentPlayerName() ?? string.Empty;
        actionsLeftLabel.text = _gameManager?.GetCurrentPlayerActionsLeft().ToString() ?? string.Empty;
        gamePhaseLabel.text = _gameManager?.GetCurrentGamePhase().ToString() ?? string.Empty;

        // update player scores
        scoreNamesLabel.text = string.Empty;
        scoreValuesLabel.text = string.Empty;

        var scores = _gameManager?.GetPlayerScores();
        if (scores == null)
            return;

        foreach (var item in scores) {
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
            Debug.LogError("PauseMenuManager: One or more required ScoreInfo UI elements are not assigned.");
            return;
        }
        if (outerScoreRectTransform == null || innerScoreRectTransform == null || panelRectTransform == null) {
            Debug.LogError("PauseMenuManager: One or more required RectTransforms are not assigned.");
            return;
        }

        // put text with <num players> lines into the score labels
        // this will ensure that their size will not change when the real values are assigned
        int numPlayers = GameStartParams.Players.Count;
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

    private void Awake()
    {
        // check if all required components are assigned
        if (outerScoreRectTransform == null || innerScoreRectTransform == null || panelRectTransform == null) {
            Debug.LogError("PauseMenuManager: One or more required RectTransforms are not assigned.");
            return;
        }
        if (currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required TurnInfo UI elements are not assigned.");
            return;
        }
        if (scoreToggle == null || scoreNamesLabel == null || scoreValuesLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required ScoreInfo UI elements are not assigned.");
            return;
        }
        if (animationSpeedSliderValueLabel == null || animationSpeedSlider == null) {
            Debug.LogError("PauseMenuManager: One or more required Slider UI elements are not assigned.");
            return;
        }

        // try to find GameManager
        _gameManager = GameObject.FindAnyObjectByType<GameManager>();

        // get scene transitions
        _sceneTransitions = transform.GetComponent<SceneTransitions>();
        if (_sceneTransitions == null) {
            Debug.Log("PauseMenuManager: SceneTransitions not found");
        }

        // find pause logic --> needed for the Resume button
        _pauseLogic = GameObject.FindAnyObjectByType<PauseLogic>();
        if (_pauseLogic == null) {
            Debug.LogError("PauseMenuManager: PauseLogic not found");
        }

        // needed to show / hide the pause menu GameObject
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // safety check
        if (scoreToggle == null || animationSpeedSlider == null) {
            return;
        }

        // hide score info by default
        scoreToggle.isOn = false;

        // setup animation speed slider
        animationSpeedSlider.minValue = _animationSliderMinValue;
        animationSpeedSlider.maxValue = _animationSliderMaxValue;
        animationSpeedSlider.value = Mathf.Round(AnimationSpeedManager.AnimationSpeed * 10f);

        // make sure that the panel had correct size
        AdjustPanelSize();

        // hide the pause menu by default
        HidePauseMenu(); 

        // try to find SoundManager - at the end, sounds should not play during initialization
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();
    }

    #endregion
}
