#nullable enable

namespace ProjectL.FinalResultsScene
{
    using ProjectL.Data;
    using ProjectL.Animation;
    using System.Threading;
    using System.Threading.Tasks;
    using System;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Manages the <c>ScoreDetailsColumn</c> prefab. Represents a column in the final results screen that contains their number of completed puzzles and number of leftover tetrominos.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class ScoreDetailsColumn : MonoBehaviour
    {
        #region Fields

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI? numCompletedPuzzlesLabel;
        [SerializeField] private TextMeshProUGUI? numLeftoverTetrominosLabel;

        #endregion

        #region Methods

        /// <summary>
        /// Prepares this instance to animate information about score details.
        /// </summary>
        /// <param name="gameEndInfo">The game end information about the player.</param>
        public void Setup(GameSummary.Stats gameEndInfo)
        {
            if (numCompletedPuzzlesLabel == null || numLeftoverTetrominosLabel == null) {
                return;
            }
            numCompletedPuzzlesLabel.text = gameEndInfo.FinishedPuzzles.Count.ToString();
            numLeftoverTetrominosLabel.text = gameEndInfo.NumLeftoverTetrominos.ToString();
        }

        /// <summary>
        /// Animates the score details column by showing the number of completed puzzles and leftover tetrominos.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">The task was canceled.</exception>
        public async Task AnimateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ShowNumPuzzles();
            await AnimationManager.PlayTapSoundAndWaitForScaledDelay(1f, cancellationToken);

            ShowNumTetrominos();
            await AnimationManager.PlayTapSoundAndWaitForScaledDelay(1f, cancellationToken);
        }

        private void Awake()
        {
            // check that required components are assigned
            if (numCompletedPuzzlesLabel == null || numLeftoverTetrominosLabel == null) {
                Debug.LogError("UI elements are not assigned in the inspector.");
                return;
            }

            HideColumn();
        }

        private void HideColumn()
        {
            if (numCompletedPuzzlesLabel == null || numLeftoverTetrominosLabel == null) {
                return;
            }
            numCompletedPuzzlesLabel.alpha = 0;
            numLeftoverTetrominosLabel.alpha = 0;
        }

        private void ShowNumPuzzles()
        {
            if (numCompletedPuzzlesLabel == null) {
                return;
            }
            numCompletedPuzzlesLabel.alpha = 1;
        }

        private void ShowNumTetrominos()
        {
            if (numLeftoverTetrominosLabel == null) {
                return;
            }
            numLeftoverTetrominosLabel.alpha = 1;
        }

        #endregion
    }
}
