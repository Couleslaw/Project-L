#nullable enable

namespace ProjectL.UI.FinalResults
{
    using System.Threading;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using ProjectL.GameData;

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
        public async Task AnimateAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) {
                return;
            }

            ShowNumPuzzles();
            await FinalAnimationManager.WaitForAnimationDelayAndPlaySound(cancellationToken);

            ShowNumTetrominos();
            await FinalAnimationManager.WaitForAnimationDelayAndPlaySound(cancellationToken);
        }

        internal void Awake()
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
