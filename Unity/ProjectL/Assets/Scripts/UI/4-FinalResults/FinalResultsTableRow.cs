#nullable enable

namespace ProjectL.UI.FinalResults
{
    using TMPro;
    using UnityEngine;

    public class FinalResultsTableRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI? _playerNameText;
        [SerializeField] private TextMeshProUGUI? _playerRankText;

        public void Init(string playerName, int rank)         {
            if (_playerNameText == null || _playerRankText == null) {
                Debug.LogError("Player name or rank text is not assigned in the inspector.");
                return;
            }

            _playerNameText.text = playerName;
            _playerRankText.text = rank.ToString() + ".";
        }

        public void Hide()
        {
            _playerNameText!.color = Color.clear;
            _playerRankText!.color = Color.clear;
        }

        public void Show()
        {
            _playerNameText!.color = Color.white;
            _playerRankText!.color = Color.white;
        }
    }
}
