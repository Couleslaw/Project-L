#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System.Threading;
    using System.Threading.Tasks;
    using TMPro;
    using UnityEngine;

    public class PlayerZoneRow : MonoBehaviour, IPlayerStatePuzzleListener
    {
        [SerializeField] private TextMeshProUGUI? playerNameLabel;
        [SerializeField] private PlayerPuzzlesPanel? puzzlesContainer;

        private void Awake()
        {
            if (playerNameLabel == null || puzzlesContainer == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
        }

        public void Init(string playerName, PlayerState playerState)
        {
            if (playerNameLabel != null) {
                playerNameLabel.text = playerName;
            }
            playerState.AddListener(this);
        }

        public void OnPuzzleFinished(int index, FinishedPuzzleInfo info)
        {
            puzzlesContainer![index].FinishPuzzle();
        }

        public void OnPuzzleAdded(int index, Puzzle puzzle)
        {
            if (puzzle is PuzzleWithGraphics grPuzzle) {
                puzzlesContainer![index].PlacePuzzle(grPuzzle);
            }
            else {
                Debug.LogError($"Puzzle {puzzle} is not a {nameof(PuzzleWithGraphics)}.");
            }
        }

        public void SetAsCurrentPlayer(bool current)
        {
            // make name white / gray
            if (playerNameLabel != null) {
                playerNameLabel.color = current ? GameGraphicsSystem.ActivePlayerColor : GameGraphicsSystem.InactivePlayerColor;
            }

            // enable / disable puzzles container
            if (puzzlesContainer != null) {
                puzzlesContainer.SetAsCurrentPlayer(current);
            }
        }

        public Vector2 GetPlacementPositionFor(PlaceTetrominoAction action)
        {
            return puzzlesContainer!.GetPlacementPositionFor(action);
        }
    }
}
