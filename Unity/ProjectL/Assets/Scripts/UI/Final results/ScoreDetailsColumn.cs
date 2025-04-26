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

    void Awake()
    {
        if (numCompletedPuzzlesLabel == null || numLeftoverTetrominosLabel == null) {
            Debug.LogError("UI elements are not assigned in the inspector.");
            return;
        }
        HideColumn();
    }

    private void HideColumn()
    {
        numCompletedPuzzlesLabel!.alpha = 0;
        numLeftoverTetrominosLabel!.alpha = 0;
    }
    private void ShowNumPuzzles() => numCompletedPuzzlesLabel!.alpha = 1;
    private void ShowNumTetrominos() => numLeftoverTetrominosLabel!.alpha = 1;

    public void Setup(GameEndStats.GameEndInfo gameEndInfo)
    {
        numCompletedPuzzlesLabel!.text = gameEndInfo.FinishedPuzzles.Count.ToString();
        numLeftoverTetrominosLabel!.text = gameEndInfo.NumLeftoverTetrominos.ToString();
    }

    public async Task AnimateAsync()
    {
        await Task.Delay(500);
        ShowNumPuzzles();
        await Task.Delay(1000);
        ShowNumTetrominos();
    }
}
