using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectLCore.GameLogic;

#nullable enable

public class GameCreationManager : MonoBehaviour
{
    [SerializeField]
    private Slider? numPiecesSlider;

    [SerializeField]
    private TextMeshProUGUI? numPiecesText;

    [SerializeField]
    private Button? startGameButton;

    [SerializeField]
    private Toggle? shuffleCheckbox;

    void Start()
    {
        if (numPiecesSlider == null || numPiecesText == null || startGameButton == null || shuffleCheckbox == null) {
            Debug.LogError("One or more UI components are not assigned in the inspector.");
            return;
        }

        GameStartParams.Reset();
        // Set the initial state of the shuffle players checkbox
        shuffleCheckbox.isOn = GameStartParams.ShufflePlayersDefault;

        // Set the initial state of num pieces slider
        numPiecesSlider.minValue = GameState.MinNumInitialTetrominos;
        numPiecesSlider.value = GameStartParams.NumInitialTetrominosDefault;
        numPiecesText.text = numPiecesSlider.value.ToString();
    }

    /// <summary>
    /// Changes the number of initial tetrominos.
    /// </summary>
    public void OnNumPiecesChanged()
    {
        if (numPiecesSlider == null || numPiecesText == null) {
            Debug.LogError("Slider or text component is not assigned.");
            return;
        }
        int num = (int)numPiecesSlider.value;
        GameStartParams.NumInitialTetrominos = num;
        numPiecesText.text = num.ToString();
    }

    /// <summary>
    /// Toggles the shuffle players setting.
    /// </summary>
    public void OnShuffleToggled()
    {
        if (shuffleCheckbox == null) {
            Debug.LogError("Shuffle checkbox is not assigned.");
            return;
        }
        GameStartParams.ShufflePlayers = shuffleCheckbox.isOn;
    }
}
