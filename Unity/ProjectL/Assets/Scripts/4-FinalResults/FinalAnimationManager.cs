#nullable enable

namespace ProjectL.FinalResultsScene
{
    using ProjectL.Animation;
    using ProjectL.Data;
    using ProjectL.Management;
    using ProjectL.Sound;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the animation of the final results screen.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class FinalAnimationManager : MonoBehaviour
    {
        #region Fields

        private readonly List<PlayerStatsColumn> _playerStatsColumns = new();

        private readonly List<ScoreDetailsColumn> _scoreDetailsColumns = new();

        private readonly List<FinalResultsTableRow> _finalResultsRows = new();

        [Header("Final Results Panel")]
        [SerializeField] private CanvasGroup? finalResultsPanel;
        [SerializeField] private GameObject? finalResultsTableContainer;
        [SerializeField] private FinalResultsTableRow? finalResultsRowPrefab;
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

        #endregion

        #region Methods

        /// <summary>
        /// Handles the click event of the "Home" button. Loads the main menu scene.
        /// </summary>
        private void OnHomeButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            SceneLoader.Instance?.LoadMainMenuAsync();
        }

        private void Awake()
        {
            // check that required components are assigned
            if (finalResultsPanel == null || finalResultsRowPrefab == null || homeButton == null ||
                finalResultsTableContainer == null || playerColumnsParent == null || playerStatsColumnPrefab == null ||
                detailsColumnsParent == null || detailsColumnPrefab == null || dividerLine == null || detailsPanel == null) {
                Debug.LogError("One or more required components are not assigned in the inspector.");
                return;
            }

            // instantiate player stats columns
            foreach (var item in GameSummary.PlayerStats) {
                PlayerStatsColumn playerColumn = Instantiate(playerStatsColumnPrefab, playerColumnsParent);
                playerColumn.Setup(item.Key.Name, item.Value);
                _playerStatsColumns.Add(playerColumn);
            }
            // instantiate detail columns 
            foreach (var item in GameSummary.PlayerStats) {
                ScoreDetailsColumn detailsColumn = Instantiate(detailsColumnPrefab, detailsColumnsParent);
                detailsColumn.Setup(item.Value);
                _scoreDetailsColumns.Add(detailsColumn);
            }

            homeButton.onClick.AddListener(OnHomeButtonClick);
        }

        private async void Start()
        {
            // hide everything
            HideDividerLine();
            HideDetailsPanel();
            HidePlayerStatsPanel();
            SetupFinalResultsPanel();

            try {
                await Animate(destroyCancellationToken);
            }
            catch (OperationCanceledException) {
                Debug.Log("Final animation cancelled.");
                return;
            }
        }

        private async Task Animate(CancellationToken cancellationToken)
        {
            // show player stats labels
            await AnimationManager.WaitForScaledDelay(1f, cancellationToken);
            ShowPlayerStatsPanel();
            await AnimationManager.PlayTapSoundAndWaitForScaledDelay(1f, cancellationToken);

            // animate player stats
            var tasks = new List<Task>();
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var playerColumn in _playerStatsColumns) {
                tasks.Add(playerColumn.AnimateStartAsync(cancellationToken));
            }
            await Task.WhenAll(tasks);

            // animate completed puzzles
            tasks.Clear();
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var playerColumn in _playerStatsColumns) {
                tasks.Add(playerColumn.AnimateCompletedAsync(cancellationToken));
            }
            await Task.WhenAll(tasks);

            // animate finishing touches
            tasks.Clear();
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var playerColumn in _playerStatsColumns) {
                tasks.Add(playerColumn.AnimateFinishingTouchesAsync(cancellationToken));
            }
            await Task.WhenAll(tasks);

            // animate incomplete puzzles
            tasks.Clear();
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var playerColumn in _playerStatsColumns) {
                tasks.Add(playerColumn.AnimateIncompleteAsync(cancellationToken));
            }
            await Task.WhenAll(tasks);

            // show divider
            ShowDividerLine();
            await AnimationManager.PlayTapSoundAndWaitForScaledDelay(1f, cancellationToken);

            // if two players have the same score, show the detail columns
            if (!AreAllScoresDifferent()) {
                ShowDetailsPanel();
                await AnimationManager.PlayTapSoundAndWaitForScaledDelay(1f, cancellationToken);

                // animate details panel
                tasks.Clear();
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var detailsColumn in _scoreDetailsColumns) {
                    tasks.Add(detailsColumn.AnimateAsync(cancellationToken));
                }
                await Task.WhenAll(tasks);
            }

            // show final results panel
            cancellationToken.ThrowIfCancellationRequested();
            await AnimateFinalResultsPanelAsync(cancellationToken);
        }

        private async Task AnimateFinalResultsPanelAsync(CancellationToken cancellationToken)
        {
            if (finalResultsPanel == null || homeButton == null) {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            // show final results panel
            finalResultsPanel.alpha = 1;
            SoundManager.Instance?.PlayTapSoundEffect();

            // show final results table rows
            foreach (var row in _finalResultsRows) {
                await AnimationManager.WaitForScaledDelayAndPlayTapSound(1f, cancellationToken);
                row.Show();
            }

            // show home button
            await AnimationManager.WaitForScaledDelayAndPlayTapSound(1f, cancellationToken);
            homeButton.interactable = true;
        }

        private bool AreAllScoresDifferent()
        {
            var scores = new HashSet<int>();
            foreach (var playerColumn in _playerStatsColumns) {
                scores.Add(playerColumn.Score);
            }
            return scores.Count == _playerStatsColumns.Count;
        }

        private void HidePlayerStatsPanel()
        {
            if (playerStatsPanel == null) {
                return;
            }
            playerStatsPanel.alpha = 0;
        }

        private void ShowPlayerStatsPanel()
        {
            if (playerStatsPanel == null) {
                return;
            }
            playerStatsPanel.alpha = 1;
        }

        private void SetupFinalResultsPanel()
        {
            if (homeButton == null || finalResultsRowPrefab == null || finalResultsTableContainer == null || finalResultsPanel == null) {
                return;
            }

            // disable home button at start
            homeButton.interactable = false;

            // set final results text
            foreach (var item in GameSummary.FinalResults) {
                var row = Instantiate(finalResultsRowPrefab, finalResultsTableContainer.transform);
                row.gameObject.SetActive(true);
                row.Init(item.Key.Name, item.Value);
                row.Hide();
                _finalResultsRows.Add(row);
            }

            // hide the final results panel
            finalResultsPanel.alpha = 0;
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

        #endregion
    }
}
