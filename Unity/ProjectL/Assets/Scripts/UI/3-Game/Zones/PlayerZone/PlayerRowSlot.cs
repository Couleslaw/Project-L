#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;

    public class PlayerRowSlot : MonoBehaviour
    {
        [SerializeField] private InteractivePuzzle? puzzleCard;
        [SerializeField] private GameObject? emptySlot;

        public uint? CurrentPuzzleId => puzzleCard != null ? puzzleCard.CurrentPuzzleId : null;

        private void Awake()
        {
            if (puzzleCard == null || emptySlot == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }

            emptySlot.SetActive(true);
        }

        public void FinishPuzzle()
        {
            if (puzzleCard == null || emptySlot == null) {
                return;
            }
            puzzleCard.gameObject.SetActive(false);
            emptySlot.SetActive(true);
        }

        public void PlacePuzzle(PuzzleWithGraphics puzzle)
        {
            if (puzzleCard == null || emptySlot == null) {
                return;
            }
            puzzleCard.gameObject.SetActive(true);
            emptySlot.SetActive(false);
            puzzleCard.SetNewPuzzle(puzzle);
        }

        public void SetAsCurrentPlayer(bool current)
        {
            if (puzzleCard == null || emptySlot == null) {
                return;
            }
            puzzleCard.MakeInteractive(current);
        }

        public Vector2 GetPlacementPositionFor(BinaryImage placement)
        {
            if (puzzleCard != null) {
                return puzzleCard.GetPlacementCenter(placement);
            }
            return default;
        }
    }
}
