using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Globalization;
using ProjectLCore.GameLogic;

#nullable enable

public class PauseMenuManager : MonoBehaviour
{
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

    public void HidePauseMenu()
    {
        if (_canvasGroup == null) {
            return; // safety check
        }
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnHomeButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        // call the function is separate thread
        _sceneTransitions?.LoadMainMenuAsync();
        _pauseLogic?.Resume();
    }

    public void OnBackButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        _pauseLogic?.Resume();
    }

    public void OnScoreToggleClick(bool showScore)
    {
        if (scoreNamesLabel == null || scoreValuesLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required ScoreInfo UI elements are not assigned.");
            return;
        }

        if (showScore) {
            // set alpha to 1
            scoreNamesLabel.alpha = 1f;
            scoreValuesLabel.alpha = 1f;
        }
        else {
            scoreNamesLabel.alpha = 0;
            scoreValuesLabel.alpha = 0;
        }
    }

    public void OnAnimationSpeedSliderValueChanged(Single value)
    {
        // save value to PlayerPrefs
        PlayerPrefs.SetFloat(AnimationSpeedManager.AnimationSpeedPlayerPrefKey, value);

        // update label
        if (animationSpeedSliderValueLabel == null) {
            return;  // safety check
        }

        value = AnimationSpeedManager.CalculateAdjustedAnimationSpeed(value);
        animationSpeedSliderValueLabel.text = value.ToString(CultureInfo.InvariantCulture) + "×";
    }

    private void UpdateUI()
    {
        // safety check
        if (currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null || scoreNamesLabel == null || scoreValuesLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required TurnInfo UI elements are not assigned.");
            return;
        }

        // GameManager will be null in final results scene
        if (_gameManager == null) {
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

    private void Awake()
    {
        // check if all required components are assigned
        if (currentPlayerLabel == null || actionsLeftLabel == null || gamePhaseLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required TurnInfo UI elements are not assigned.");
            return;
        }
        if (scoreToggle == null || scoreNamesLabel == null || scoreValuesLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required ScoreInfo UI elements are not assigned.");
            return;
        }
        if (animationSpeedSliderValueLabel == null) {
            Debug.LogError("PauseMenuManager: AnimationSpeedSliderValueLabel is not assigned.");
            return;
        }


        _gameManager = GameObject.FindAnyObjectByType<GameManager>();

        _sceneTransitions = transform.GetComponent<SceneTransitions>();
        if (_sceneTransitions == null) {
            Debug.Log("PauseMenuManager: SceneTransitions not found");
        }

        _pauseLogic = GameObject.FindAnyObjectByType<PauseLogic>();
        if (_pauseLogic == null) {
            Debug.LogError("PauseMenuManager: PauseLogic not found");
        }
        _canvasGroup = GetComponent<CanvasGroup>();

        // try to find SoundManager
        _soundManager = GameObject.FindAnyObjectByType<SoundManager>();
    }


    private void AdjustPanelSize()
    {
        if (scoreValuesLabel == null || scoreNamesLabel == null) {
            Debug.LogError("PauseMenuManager: One or more required ScoreInfo UI elements are not assigned.");
            return;
        }
        if (outerScoreRectTransform == null || innerScoreRectTransform == null || panelRectTransform == null) {
            Debug.LogError("PauseMenuManager: One or more required RectTransforms are not assigned.");
            return;
        }

        int numPlayers = GameStartParams.Players.Count;

        string text = "";
        for (int i = 0; i < numPlayers; i++) {
            text += "0\n";
        }
        scoreValuesLabel.text = text;
        scoreNamesLabel.text = text;

        // set the size of the panel to fit the content
        LayoutRebuilder.ForceRebuildLayoutImmediate(innerScoreRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(outerScoreRectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRectTransform);
    }

    private void Start()
    {
        // hide score by default
        if (scoreToggle == null) {
            return;
        }
        scoreToggle.isOn = false;

        // load animation speed from player prefs
        if (animationSpeedSlider == null) {
            return;
        }

        // set animation speed
        animationSpeedSlider.value = AnimationSpeedManager.AnimationSpeed;

        AdjustPanelSize();

        HidePauseMenu();
    }
}
