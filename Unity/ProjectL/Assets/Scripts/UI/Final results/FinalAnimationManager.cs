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

    [Header("Detail Columns")]
    [SerializeField] private Transform? detailsColumnsParent;
    [SerializeField] private ScoreDetailsColumn? detailsColumnPrefab;
    [SerializeField] private Image? dividerLine;
    [SerializeField] private CanvasGroup? detailsPanel;

    private SoundManager? _soundManager;

    private List<PlayerStatsColumn> playerStatsColumns = new();
    private List<ScoreDetailsColumn> detailsColumns = new();

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
        HideDividerLine();
        HideDetailsPanel();
        SetupFinalResultsPanel();
        

        // animate each player column
        foreach (var playerColumn in playerStatsColumns) {
            await playerColumn.AnimateAsync();
        }

        // if two players have the same score, show the detail columns
        if (!AreAllScoresDifferent()) {
            ShowDividerLine();
            ShowDetailsPanel();

            // animate all columns at once
            var tasks = new List<Task>();
            foreach (var detailsColumn in detailsColumns) {
                tasks.Add(detailsColumn.AnimateAsync());
            }
            await Task.WhenAll(tasks);
        }

        ShowFinalResultsPanel();
    }

    private bool AreAllScoresDifferent()
    {
        var scores = new HashSet<int>();
        foreach (var playerColumn in playerStatsColumns) {
            scores.Add(playerColumn.Score);
        }
        return scores.Count == playerStatsColumns.Count;
    }

    private void SetupFinalResultsPanel()
    {
        if (homeButton == null || finalResultsText == null || finalResultsPanel == null) {
            Debug.LogError("One or more Final Results Panel UI elements are not assigned in the inspector.");
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

    private void ShowFinalResultsPanel()
    {
        homeButton!.interactable = true;
        finalResultsPanel!.alpha = 1;
    }


    private void HideDividerLine() => dividerLine!.color = new Color(dividerLine.color.r, dividerLine.color.g, dividerLine.color.b, 0);

    private void ShowDividerLine() => dividerLine!.color = new Color(dividerLine.color.r, dividerLine.color.g, dividerLine.color.b, 1);

    private void HideDetailsPanel() => detailsPanel!.alpha = 0;

    private void ShowDetailsPanel() => detailsPanel!.alpha = 1;



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
