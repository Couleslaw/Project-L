#nullable enable

namespace ProjectL.FinalResultsScene
{
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Manages the <c>FinalResultsTableRow</c> prefab. Represents a row in the final results table, displaying the player's name and rank.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class FinalResultsTableRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI? _playerNameText;
        [SerializeField] private TextMeshProUGUI? _playerRankText;

        /// <summary>
        /// Initializes the row with the player's name and rank (order in results table).
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <param name="rank">The rank of the player.</param>
        public void Init(string playerName, int rank)         {
            if (_playerNameText == null || _playerRankText == null) {
                Debug.LogError("Player name or rank text is not assigned in the inspector.");
                return;
            }

            _playerNameText.text = playerName;
            _playerRankText.text = rank.ToString() + ".";
        }

        /// <summary>
        /// Hides the row by setting the text color to clear.
        /// </summary>
        public void Hide()
        {
            _playerNameText!.color = Color.clear;
            _playerRankText!.color = Color.clear;
        }

        /// <summary>
        /// Shows the row by setting the text color to white.
        /// </summary>
        public void Show()
        {
            _playerNameText!.color = Color.white;
            _playerRankText!.color = Color.white;
        }
    }
}
