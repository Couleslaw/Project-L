using ProjectLCore.Players;
using ProjectLCore.GamePieces;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable
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

    private GameEndStats.GameEndInfo? _gameEndInfo;

    private SoundManager? _soundManager;

    #endregion

    #region Properties

    public int Score { get; private set; } = 0;

    #endregion

    #region Methods

    public void Setup(Player player, GameEndStats.GameEndInfo gameEndInfo)
    {
        if (playerNameLabel == null || playerScoreLabel == null) {
            return;
        }
        playerNameLabel.text = player.Name;
        playerScoreLabel.text = "0";
        _gameEndInfo = gameEndInfo;
    }

    public async Task AnimateStartAsync()
    {
        if (_gameEndInfo == null) {
            return;
        }

        ShowColumn();
        _soundManager!.PlayTapSoundEffect();
        await Awaitable.WaitForSecondsAsync(FinalAnimationManager.AnimationDelay * AnimationSpeedManager.AnimationSpeed);
        ShowScoreLabel();
        _soundManager!.PlayTapSoundEffect();
        await Awaitable.WaitForSecondsAsync(FinalAnimationManager.AnimationDelay * AnimationSpeedManager.AnimationSpeed);
    }

    public async Task AnimateCompletedAsync()
    {
        if (_gameEndInfo == null) {
            return;
        }

        foreach (var puzzle in _gameEndInfo.FinishedPuzzles) {
            ShowCompletedPuzzle();
            SetCompletedPuzzleSprite(puzzle);
            _soundManager!.PlayTapSoundEffect();
            UpdateScore(puzzle.RewardScore);
            await Awaitable.WaitForSecondsAsync(FinalAnimationManager.AnimationDelay * AnimationSpeedManager.AnimationSpeed);
        }
    }

    public async Task AnimateTetrominosAsync()
    {
        if (_gameEndInfo == null) {
            return;
        }

        foreach (var tetromino in _gameEndInfo.FinishingTouchesTetrominos) {
            ShowFinishingTouches();
            SetTetrominoSprite(tetromino);
            _soundManager!.PlayTapSoundEffect();
            UpdateScore(-1);
            await Awaitable.WaitForSecondsAsync(FinalAnimationManager.AnimationDelay * AnimationSpeedManager.AnimationSpeed);
        }
    }

    public async Task AnimateIncompleteAsync()
    {
        if (_gameEndInfo == null) {
            return;
        }
        
        foreach (var puzzle in _gameEndInfo.UnfinishedPuzzles) {
            ShowIncompletePuzzles();
            SetIncompletePuzzleSprite(puzzle);
            _soundManager!.PlayTapSoundEffect();
            UpdateScore(puzzle.RewardScore);
            await Awaitable.WaitForSecondsAsync(FinalAnimationManager.AnimationDelay * AnimationSpeedManager.AnimationSpeed);
        }
    }

    private void Awake()
    {
        if (completedPuzzleImage == null || incompletePuzzleImage == null || tetrominoImage == null
            || playerNameLabel == null || playerScoreLabel == null) {
            Debug.LogError("One or more required components are not assigned in the inspector.");
            return;
        }
        _canvasGroup = GetComponent<CanvasGroup>();
        _soundManager = FindAnyObjectByType<SoundManager>();

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
