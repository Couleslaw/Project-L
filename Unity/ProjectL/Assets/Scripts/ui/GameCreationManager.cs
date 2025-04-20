using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectLCore.GameLogic;

public class GameCreationManager : MonoBehaviour
{
    [SerializeField]
    private Slider numPiecesSlider;

    [SerializeField]
    private TextMeshProUGUI numPiecesText;

    [SerializeField]
    private Button startGameButton;

    private const int _defaultNumPieces = 15;

    void Start()
    {
        // shuffle players by default
        PlayerPrefs.SetInt("ShufflePlayers", 1);

        // set default number of pieces
        PlayerPrefs.SetInt("NumPieces", _defaultNumPieces);
        numPiecesSlider.minValue = GameState.MinNumInitialTetrominos;
        numPiecesSlider.value = _defaultNumPieces;
        numPiecesText.text = _defaultNumPieces.ToString();
    }

    /// <summary>
    /// Changes the number of initial tetrominos.
    /// </summary>
    public void OnNumPiecesChanged()
    {
        int num = (int)numPiecesSlider.value;
        PlayerPrefs.SetInt("NumPieces", num);
        numPiecesText.text = num.ToString();
    }

    /// <summary>
    /// Toggles the shuffle players setting.
    /// </summary>
    public void OnShuffleToggled()
    {
        int shuffle = PlayerPrefs.GetInt("ShufflePlayers");
        PlayerPrefs.SetInt("ShufflePlayers", shuffle == 1 ? 0 : 1);
    }
}
