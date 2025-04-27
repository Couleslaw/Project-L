#nullable enable

namespace ProjectL.UI.FinalResults
{
    using ProjectLCore.GamePieces;
    using ProjectL.DataManagement;
    using System.Threading;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the "PlayerStatsColumn" prefab.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class PlayerStatsColumn : MonoBehaviour
    {
        #region Fields

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI? playerNameLabel;
        [SerializeField] private Image? completedPuzzleImage;
        [SerializeField] private Image? incompletePuzzleImage;
        [SerializeField] private Image? tetrominoImage;
        [SerializeField] private TextMeshProUGUI? playerScoreLabel;

        private CanvasGroup? _canvasGroup;

        private GameSummary.Stats? _gameEndInfo;

        #endregion

        #region Properties

        /// <summary>
        /// The score of the player evaluated so far.
        /// </summary>
        public int Score { get; private set; } = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Prepares this instance to animate information about the specified player's end game statistics.
        /// </summary>
        /// <param name="player">The name of the player.</param>
        /// <param name="gameEndInfo">The game end information about the player.</param>
        public void Setup(string playerName, GameSummary.Stats gameEndInfo)
        {
            if (playerNameLabel == null || playerScoreLabel == null) {
                return;
            }
            playerNameLabel.text = playerName;
            playerScoreLabel.text = "0";
            _gameEndInfo = gameEndInfo;
        }

        /// <summary>
        /// Shows the player's name and score.
        /// </summary>
        public async Task AnimateStartAsync(CancellationToken cancellationToken)
        {
            if (_gameEndInfo == null || cancellationToken.IsCancellationRequested) {
                return;
            }

            ShowColumn();
            await FinalAnimationManager.WaitForAnimationDelayAndPlaySound(cancellationToken);
            ShowScoreLabel();
            await FinalAnimationManager.WaitForAnimationDelayAndPlaySound(cancellationToken);
        }

        /// <summary>
        /// Animates player's completed puzzles.
        /// </summary>
        public async Task AnimateCompletedAsync(CancellationToken cancellationToken)
        {
            if (_gameEndInfo == null || cancellationToken.IsCancellationRequested) {
                return;
            }

            foreach (var puzzle in _gameEndInfo.FinishedPuzzles) {
                ShowCompletedPuzzle();
                SetCompletedPuzzleSprite(puzzle);
                UpdateScore(puzzle.RewardScore);
                await FinalAnimationManager.WaitForAnimationDelayAndPlaySound(cancellationToken);

            }
        }

        /// <summary>
        /// Animates the player's finishing touches tetrominos.
        /// </summary>
        public async Task AnimateTetrominosAsync(CancellationToken cancellationToken)
        {
            if (_gameEndInfo == null || cancellationToken.IsCancellationRequested) {
                return;
            }

            foreach (var tetromino in _gameEndInfo.FinishingTouchesTetrominos) {
                ShowFinishingTouches();
                SetTetrominoSprite(tetromino);
                UpdateScore(-1);
                await FinalAnimationManager.WaitForAnimationDelayAndPlaySound(cancellationToken);
            }
        }

        /// <summary>
        /// Animates the player's incomplete puzzles.
        /// </summary>
        public async Task AnimateIncompleteAsync(CancellationToken cancellationToken)
        {
            if (_gameEndInfo == null || cancellationToken.IsCancellationRequested) {
                return;
            }

            foreach (var puzzle in _gameEndInfo.UnfinishedPuzzles) {
                ShowIncompletePuzzles();
                SetIncompletePuzzleSprite(puzzle);
                UpdateScore(puzzle.RewardScore);
                await FinalAnimationManager.WaitForAnimationDelayAndPlaySound(cancellationToken);
            }
        }

        private void Awake()
        {
            // check that all components are assigned
            if (completedPuzzleImage == null || incompletePuzzleImage == null || tetrominoImage == null
                || playerNameLabel == null || playerScoreLabel == null) {
                Debug.LogError("One or more required components are not assigned in the inspector.");
                return;
            }
            _canvasGroup = GetComponent<CanvasGroup>();

            // hide everything
            HideColumn();
            HideCompletedPuzzle();
            HideIncompletePuzzles();
            HideScoreLabel();
            HideFinishingTouches();
        }

        private void UpdateScore(int delta)
        {
            if (playerScoreLabel == null) {
                return;
            }
            Score += delta;
            playerScoreLabel.text = Score.ToString();
        }

        private void SetTetrominoSprite(TetrominoShape tetromino)
        {
            if (tetrominoImage == null) {
                return;
            }
            if (ResourcesLoader.TryGetTetrominoSprite(tetromino, out Sprite? sprite)) {
                tetrominoImage.sprite = sprite!;
            }
        }

        private void SetPuzzleSprite(Image? puzzleImage, Puzzle puzzle)
        {
            if (puzzleImage == null) {
                return;
            }
            if (ResourcesLoader.TryGetPuzzleSprite(puzzle, PuzzleSpriteType.BorderBright, out Sprite? sprite)) {
                puzzleImage.sprite = sprite!;
            }
        }

        private void SetCompletedPuzzleSprite(Puzzle puzzle) => SetPuzzleSprite(completedPuzzleImage, puzzle);

        private void SetIncompletePuzzleSprite(Puzzle puzzle) => SetPuzzleSprite(incompletePuzzleImage, puzzle);

        private void HideColumn()
        {
            if (_canvasGroup == null) {
                return;
            }
            _canvasGroup.alpha = 0;
        }

        private void ShowColumn()
        {
            if (_canvasGroup == null) {
                return;
            }
            _canvasGroup.alpha = 1;
        }

        private void HideCompletedPuzzle()
        {
            if (completedPuzzleImage == null) {
                return;
            }
            completedPuzzleImage.color = Color.black;
        }

        private void HideIncompletePuzzles()
        {
            if (incompletePuzzleImage == null) {
                return;
            }
            incompletePuzzleImage.color = Color.black;
        }

        private void HideFinishingTouches()
        {
            if (tetrominoImage == null) {
                return;
            }
            tetrominoImage.color = Color.black;
        }

        private void ShowCompletedPuzzle()
        {
            if (completedPuzzleImage == null) {
                return;
            }
            completedPuzzleImage.color = Color.white;
        }

        private void ShowIncompletePuzzles()
        {
            if (incompletePuzzleImage == null) {
                return;
            }
            incompletePuzzleImage.color = Color.white;
        }

        private void ShowFinishingTouches()
        {
            if (tetrominoImage == null) {
                return;
            }
            tetrominoImage.color = Color.white;
        }

        private void HideScoreLabel()
        {
            if (playerScoreLabel == null) {
                return;
            }
            playerScoreLabel.color = Color.black;
        }

        private void ShowScoreLabel()
        {
            if (playerScoreLabel == null) {
                return;
            }
            playerScoreLabel.color = Color.white;
        }

        #endregion
    }
}
