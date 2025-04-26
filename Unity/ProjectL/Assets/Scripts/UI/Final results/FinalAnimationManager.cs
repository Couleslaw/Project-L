using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable enable

public class FinalAnimationManager : MonoBehaviour
{
    [Header("Final Results Panel")]
    [SerializeField] private CanvasGroup? finalResultsPanel;
    [SerializeField] private TextMeshProUGUI? finalResultsText;
    [SerializeField] private Button? homeButton;

    private SoundManager? _soundManager;

    private void Awake()
    {
        // check that required components are assigned
        if (homeButton == null) {
            Debug.LogError("Home button is not assigned in the inspector.");
            return;
        }

        // initialize and hide the FINAL RESULTS
        SetupFinalResultsPanel();

        // instantiate all player columns and hide them
        // instantiate all detail columns and hide them

        // try to find the sound manager
        _soundManager = FindAnyObjectByType<SoundManager>();
    }

    private async void Start()
    {

        // for all players:
        //      show the player column
        //      await animation func on column and give it stats

        // if two players have the same order:
        //      show the divider
        //      show the detail columns

        ShowFinalResultsPanel();
    }

    private void SetupFinalResultsPanel()
    {
        if (homeButton == null || finalResultsText == null || finalResultsPanel == null) {
            Debug.LogError("One or more Final Results Panel UI elements are not assigned in the inspector.");
            return;
        }

        // disable home button at start
        homeButton.interactable = false;

        // set final results text
        foreach (var item in GameEndStats.FinalResults) {
            string playerName = item.Key.Name;
            int order = item.Value;
            finalResultsText.text += $"{order}. {playerName}\n";
        }

        // hide the final results panel
        finalResultsPanel.alpha = 0;
    }

    private void ShowFinalResultsPanel()
    {
        homeButton!.interactable = true;
        finalResultsPanel!.alpha = 1;
    }

    public void OnHomeButtonClick()
    {
        _soundManager?.PlayButtonClickSound();
        var _sceneTransitions = GetComponent<SceneTransitions>();
        if (_sceneTransitions != null) {
            _sceneTransitions.LoadMainMenuAsync();
        }
        else {
            Debug.LogError("SceneTransitions component not found.");
        }

    }
}
