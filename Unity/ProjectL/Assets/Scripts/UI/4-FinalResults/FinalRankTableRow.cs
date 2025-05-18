#nullable enable

namespace ProjectL.UI.FinalResults
{
    using TMPro;
    using UnityEngine;

    public class FinalRankTableRow : MonoBehaviour
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
    }
}
