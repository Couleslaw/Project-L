using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

#nullable enable

public class FinalAnimationManager : MonoBehaviour
{
    [Header("Final Results Panel")]
    [SerializeField] private CanvasGroup? finalResultsPanel;
    [SerializeField] private TextMeshProUGUI? finalResultsText;
    [SerializeField] private Button? homeButton;

    [Header("Player Columns")]
    [SerializeField] private Transform? playerColumnsParent;
    [SerializeField] private PlayerStatsColumn? playerStatsColumnPrefab;
    [SerializeField] private CanvasGroup? playerStatsPanel;

    [Header("Detail Columns")]
    [SerializeField] private Transform? detailsColumnsParent;
    [SerializeField] private ScoreDetailsColumn? detailsColumnPrefab;
    [SerializeField] private Image? dividerLine;
    [SerializeField] private CanvasGroup? detailsPanel;

    private SoundManager? _soundManager;

    private List<PlayerStatsColumn> playerStatsColumns = new();
    private List<ScoreDetailsColumn> detailsColumns = new();

    public const float AnimationDelay = 2f;


    private void Awake()
    {
        // check that required components are assigned
        if (finalResultsPanel == null || finalResultsText == null || homeButton == null ||
            playerColumnsParent == null || playerStatsColumnPrefab == null ||
            detailsColumnsParent == null || detailsColumnPrefab == null || dividerLine == null || detailsPanel == null) {
            Debug.LogError("One or more required components are not assigned in the inspector.");
            return;
        }

        // instantiate player stats columns
        foreach (var item in GameEndStats.PlayerGameEndStats) {
            PlayerStatsColumn playerColumn = Instantiate(playerStatsColumnPrefab, playerColumnsParent);
            playerColumn.Setup(item.Key, item.Value);
            playerStatsColumns.Add(playerColumn);
        }
        // instantiate detail columns 
        foreach (var item in GameEndStats.PlayerGameEndStats) {
            ScoreDetailsColumn detailsColumn = Instantiate(detailsColumnPrefab, detailsColumnsParent);
            detailsColumn.Setup(item.Value);
            detailsColumns.Add(detailsColumn);
        }

        // try to find the sound manager
        _soundManager = FindAnyObjectByType<SoundManager>();
    }

    private async void Start()
    {
        // hide everything
        HideDividerLine();
        HideDetailsPanel();
        HidePlayerStatsPanel();
        SetupFinalResultsPanel();

        // show player stats labels
        await Awaitable.WaitForSecondsAsync(AnimationDelay * AnimationSpeedManager.AnimationSpeed);
        ShowPlayerStatsPanel();
        _soundManager?.PlayTapSoundEffect();
        await Awaitable.WaitForSecondsAsync(AnimationDelay * AnimationSpeedManager.AnimationSpeed);

        // animate player stats
        var tasks = new List<Task>();
        foreach (var playerColumn in playerStatsColumns) {
            tasks.Add(playerColumn.AnimateStartAsync());
        }
        await Task.WhenAll(tasks);

        // animate completed puzzles
        tasks.Clear();
        foreach (var playerColumn in playerStatsColumns) {
            tasks.Add(playerColumn.AnimateCompletedAsync());
        }
        await Task.WhenAll(tasks);

        // animate tetrominos
        tasks.Clear();
        foreach (var playerColumn in playerStatsColumns) {
            tasks.Add(playerColumn.AnimateTetrominosAsync());
        }
        await Task.WhenAll(tasks);

        // animate incomplete puzzles
        tasks.Clear();
        foreach (var playerColumn in playerStatsColumns) {
            tasks.Add(playerColumn.AnimateIncompleteAsync());
        }
        await Task.WhenAll(tasks);

        // if two players have the same score, show the detail columns
        if (!AreAllScoresDifferent()) {
            ShowDividerLine();
            _soundManager?.PlayTapSoundEffect();
            await Awaitable.WaitForSecondsAsync(AnimationDelay * AnimationSpeedManager.AnimationSpeed);
            ShowDetailsPanel();
            _soundManager?.PlayTapSoundEffect();
            await Awaitable.WaitForSecondsAsync(AnimationDelay * AnimationSpeedManager.AnimationSpeed);

            // animate all columns at once
            tasks.Clear();
            foreach (var detailsColumn in detailsColumns) {
                tasks.Add(detailsColumn.AnimateAsync());
            }
            await Task.WhenAll(tasks);
        }

        await AnimateFinalResultsPanelAsync();
    }

    private bool AreAllScoresDifferent()
    {
        var scores = new HashSet<int>();
        foreach (var playerColumn in playerStatsColumns) {
            scores.Add(playerColumn.Score);
        }
        return scores.Count == playerStatsColumns.Count;
    }

    private void HidePlayerStatsPanel()
    {
        if (playerStatsPanel == null) {
            return;
        }
        playerStatsPanel!.alpha = 0;
    }

    private void ShowPlayerStatsPanel()
    {
        if (playerStatsPanel == null) {
            return;
        }
        playerStatsPanel!.alpha = 1;
    }

    private void SetupFinalResultsPanel()
    {
        if (homeButton == null || finalResultsText == null || finalResultsPanel == null) {
            return;
        }

        // disable home button at start
        homeButton.interactable = false;
        homeButton.onClick.AddListener(OnHomeButtonClick);

        // set final results text
        finalResultsText.text = string.Empty;
        foreach (var item in GameEndStats.FinalResults) {
            string playerName = item.Key.Name;
            int order = item.Value;
            finalResultsText.text += $"{order}. {playerName}\n";
        }

        // hide the final results panel
        finalResultsPanel.alpha = 0;
    }

    private async Task AnimateFinalResultsPanelAsync()
    {
        if (finalResultsPanel == null || homeButton == null) {
            return;
        }

        finalResultsPanel.alpha = 1;
        _soundManager?.PlayTapSoundEffect();
        await Awaitable.WaitForSecondsAsync(AnimationDelay * AnimationSpeedManager.AnimationSpeed);
        homeButton.interactable = true;
        _soundManager?.PlayTapSoundEffect();
    }


    private void HideDividerLine()
    {
        if (dividerLine == null) {
            return;
        }
        dividerLine!.color = new Color(dividerLine.color.r, dividerLine.color.g, dividerLine.color.b, 0);
    }

    private void ShowDividerLine()
    {
        if (dividerLine == null) {
            return;
        }
        dividerLine!.color = new Color(dividerLine.color.r, dividerLine.color.g, dividerLine.color.b, 1);
    }

    private void HideDetailsPanel()
    {
        if (detailsPanel == null) {
            return;
        }
        detailsPanel!.alpha = 0;
    }

    private void ShowDetailsPanel()
    {
        if (detailsPanel == null) {
            return;
        }
        detailsPanel!.alpha = 1;
    }

    public void OnHomeButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        var _sceneTransitions = GetComponent<SceneTransitions>();
        if (_sceneTransitions != null) {
            _sceneTransitions.LoadMainMenuAsync();
        }
        else {
            Debug.LogError("SceneTransitions component not found.");
        }

    }
}
