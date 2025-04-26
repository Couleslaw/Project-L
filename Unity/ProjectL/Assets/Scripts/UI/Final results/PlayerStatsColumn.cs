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

    #endregion

    #region Properties

    public int Score { get; private set; } = 0;

    #endregion

    #region Methods

    public void Setup(Player player, GameEndStats.GameEndInfo gameEndInfo)
    {
        // set player name
        playerNameLabel!.text = player.Name;
        _gameEndInfo = gameEndInfo;
    }

    public async Task AnimateAsync()
    {
        ShowColumn();

        foreach (var puzzle in _gameEndInfo!.FinishedPuzzles) {
            HideCompletedPuzzle();
            await Task.Delay(100);
            ShowCompletedPuzzle();
            SetCompletedPuzzleSprite(puzzle);
            UpdateScore(puzzle.RewardScore);
            await Task.Delay(1000);
            
        }

        // show finishing touches tetrominos
        foreach (var tetromino in _gameEndInfo.FinishingTouchesTetrominos) {
            HideFinishingTouches();
            await Task.Delay(100);
            ShowFinishingTouches();
            SetTetrominoSprite(tetromino);
            UpdateScore(-1);
            await Task.Delay(1000);
        }

        // show incomplete puzzles
        foreach (var puzzle in _gameEndInfo.UnfinishedPuzzles) {
            HideIncompletePuzzles();
            await Task.Delay(100);
            ShowIncompletePuzzles();
            SetIncompletePuzzleSprite(puzzle);
            UpdateScore(-puzzle.RewardScore);
            await Task.Delay(1000);
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

        HideColumn();
        HideCompletedPuzzle();
        HideIncompletePuzzles();
        HideFinishingTouches();
    }

    private void UpdateScore(int delta)
    {
        Score += delta;
        playerScoreLabel!.text = Score.ToString();
    }

    private void SetTetrominoSprite(TetrominoShape tetromino)
    {
        if (ResourcesLoader.TryGetTetrominoSprite(tetromino, out Sprite? sprite)) {
            tetrominoImage!.sprite = sprite!;
        }
    }

    private void SetPuzzleSprite(Image? puzzleImage, Puzzle puzzle)
    {
        if (ResourcesLoader.TryGetPuzzleSprite(puzzle, PuzzleSpriteType.BorderBright, out Sprite? sprite)) {
            puzzleImage!.sprite = sprite!;
        }
    }

    private void SetCompletedPuzzleSprite(Puzzle puzzle) => SetPuzzleSprite(completedPuzzleImage, puzzle);

    private void SetIncompletePuzzleSprite(Puzzle puzzle) => SetPuzzleSprite(incompletePuzzleImage, puzzle);

    private void HideColumn() => _canvasGroup!.alpha = 0;
    private void ShowColumn() => _canvasGroup!.alpha = 1;

    private void HideCompletedPuzzle() => completedPuzzleImage!.color = Color.black;
    private void HideIncompletePuzzles() => incompletePuzzleImage!.color = Color.black;
    private void HideFinishingTouches() => tetrominoImage!.color = Color.black;

    private void ShowCompletedPuzzle() => completedPuzzleImage!.color = Color.white;
    private void ShowIncompletePuzzles() => incompletePuzzleImage!.color = Color.white;
    private void ShowFinishingTouches() => tetrominoImage!.color = Color.white;


    #endregion
}
