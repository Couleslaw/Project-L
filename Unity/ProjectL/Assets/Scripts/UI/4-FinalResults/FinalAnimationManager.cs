#nullable enable

namespace ProjectL.UI.FinalResults
{
    using ProjectL.Data;
    using ProjectL.Management;
    using ProjectL.UI.Sound;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class FinalAnimationManager : MonoBehaviour
    {
        #region Constants

        private const float _defaultAnimationDelay = 1.5f;

        #endregion

        #region Fields

        private readonly List<PlayerStatsColumn> _playerStatsColumns = new();

        private readonly List<ScoreDetailsColumn> _scoreDetailsColumns = new();

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

        #endregion

        #region Properties

        public static float AnimationDelay => _defaultAnimationDelay * AnimationSpeed.Multiplier;

        #endregion

        #region Methods

        public static async Task WaitForAnimationDelayAndPlaySound(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) {
                return;
            }
            try {
                SoundManager.Instance?.PlayTapSoundEffect();
                await Awaitable.WaitForSecondsAsync(AnimationDelay, cancellationToken);
            }
            catch (OperationCanceledException) {
            }
        }

        /// <summary>
        /// Handles the click event of the "Home" button. Loads the main menu scene.
        /// </summary>
        public void OnHomeButtonClick()
        {
            SoundManager.Instance?.PlayButtonClickSound();
            SceneLoader.Instance?.LoadMainMenuAsync();
        }

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
        }

        private async void Start()
        {
            // hide everything
            HideDividerLine();
            HideDetailsPanel();
            HidePlayerStatsPanel();
            SetupFinalResultsPanel();

            // show player stats labels
            if (this != null) {
                await Awaitable.WaitForSecondsAsync(AnimationDelay);
                ShowPlayerStatsPanel();
                await WaitForAnimationDelayAndPlaySound(destroyCancellationToken);
            }

            // animate player stats
            var tasks = new List<Task>();
            if (this != null) {
                foreach (var playerColumn in _playerStatsColumns) {
                    tasks.Add(playerColumn.AnimateStartAsync(destroyCancellationToken));
                }
                await Task.WhenAll(tasks);
            }
            // animate completed puzzles
            if (this != null) {
                tasks.Clear();
                foreach (var playerColumn in _playerStatsColumns) {
                    tasks.Add(playerColumn.AnimateCompletedAsync(destroyCancellationToken));
                }
                await Task.WhenAll(tasks);
            }
            // animate tetrominos
            if (this != null) {
                tasks.Clear();
                foreach (var playerColumn in _playerStatsColumns) {
                    tasks.Add(playerColumn.AnimateTetrominosAsync(destroyCancellationToken));
                }
                await Task.WhenAll(tasks);
            }
            // animate incomplete puzzles
            if (this != null) {
                tasks.Clear();
                foreach (var playerColumn in _playerStatsColumns) {
                    tasks.Add(playerColumn.AnimateIncompleteAsync(destroyCancellationToken));
                }
                await Task.WhenAll(tasks);
            }

            // show divider
            if (this != null) {
                ShowDividerLine();
                await WaitForAnimationDelayAndPlaySound(destroyCancellationToken);
            }

            // if two players have the same score, show the detail columns
            if (!AreAllScoresDifferent()) {
                if (this != null) {
                    ShowDetailsPanel();
                    await WaitForAnimationDelayAndPlaySound(destroyCancellationToken);
                }

                // animate details panel
                if (this != null) {
                    tasks.Clear();
                    foreach (var detailsColumn in _scoreDetailsColumns) {
                        tasks.Add(detailsColumn.AnimateAsync(destroyCancellationToken));
                    }
                    await Task.WhenAll(tasks);
                }
            }

            // show final results panel
            if (this != null) {
                await AnimateFinalResultsPanelAsync(destroyCancellationToken);
            }
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
            if (homeButton == null || finalResultsText == null || finalResultsPanel == null) {
                return;
            }

            // disable home button at start
            homeButton.interactable = false;

            // set final results text
            finalResultsText.text = string.Empty;
            foreach (var item in GameSummary.FinalResults) {
                string playerName = item.Key.Name;
                int order = item.Value;
                finalResultsText.text += $"{order}. {playerName}\n";
            }

            // hide the final results panel
            finalResultsPanel.alpha = 0;
        }

        private async Task AnimateFinalResultsPanelAsync(CancellationToken cancellationToken)
        {
            if (finalResultsPanel == null || homeButton == null) {
                return;
            }

            // show final results
            if (!cancellationToken.IsCancellationRequested) {
                finalResultsPanel.alpha = 1;
                await WaitForAnimationDelayAndPlaySound(cancellationToken);
            }

            // show home button
            if (!cancellationToken.IsCancellationRequested) {
                homeButton.interactable = true;
                SoundManager.Instance?.PlayTapSoundEffect();
            }
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
