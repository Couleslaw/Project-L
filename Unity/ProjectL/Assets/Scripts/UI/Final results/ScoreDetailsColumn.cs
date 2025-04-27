using ProjectLCore.Players;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

#nullable enable

public class ScoreDetailsColumn : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI? numCompletedPuzzlesLabel;
    [SerializeField] private TextMeshProUGUI? numLeftoverTetrominosLabel;

    private SoundManager? _soundManager;

    void Awake()
    {
        if (numCompletedPuzzlesLabel == null || numLeftoverTetrominosLabel == null) {
            Debug.LogError("UI elements are not assigned in the inspector.");
            return;
        }

        _soundManager = FindAnyObjectByType<SoundManager>();

        HideColumn();
    }

    private void HideColumn()
    {
        numCompletedPuzzlesLabel!.alpha = 0;
        numLeftoverTetrominosLabel!.alpha = 0;
    }
    private void ShowNumPuzzles()
    {
        if (numCompletedPuzzlesLabel == null) {
            return;
        }
        numCompletedPuzzlesLabel!.alpha = 1;
    }
    private void ShowNumTetrominos()
    {
        if (numLeftoverTetrominosLabel == null) {
            return;
        }
        numLeftoverTetrominosLabel!.alpha = 1;
    }

    public void Setup(GameEndStats.GameEndInfo gameEndInfo)
    {
        if (numCompletedPuzzlesLabel == null || numLeftoverTetrominosLabel == null) {
            return;
        }
        numCompletedPuzzlesLabel!.text = gameEndInfo.FinishedPuzzles.Count.ToString();
        numLeftoverTetrominosLabel!.text = gameEndInfo.NumLeftoverTetrominos.ToString();
    }

    public async Task AnimateAsync()
    {
        ShowNumPuzzles();
        _soundManager!.PlayTapSoundEffect();
        await Awaitable.WaitForSecondsAsync(FinalAnimationManager.AnimationDelay * AnimationSpeedManager.AnimationSpeed);
        ShowNumTetrominos();
        _soundManager!.PlayTapSoundEffect();
        await Awaitable.WaitForSecondsAsync(FinalAnimationManager.AnimationDelay * AnimationSpeedManager.AnimationSpeed);
    }
}
