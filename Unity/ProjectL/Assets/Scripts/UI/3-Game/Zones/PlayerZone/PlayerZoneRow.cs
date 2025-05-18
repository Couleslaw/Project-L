#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using System;
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
            SetAsCurrentPlayer(false);
        }

        void IPlayerStatePuzzleListener.OnPuzzleFinished(FinishedPuzzleInfo info)
        {
            if (TryGetPuzzleWithId(info.Puzzle.Id, out var puzzleSlot)) {
                puzzleSlot!.FinishPuzzle();
            }
            else {
                Debug.LogError($"Puzzle with ID {info.Puzzle.Id} not found in player row.");
            }
        }

        void IPlayerStatePuzzleListener.OnPuzzleAdded(Puzzle puzzle)
        {
            if (puzzle is not PuzzleWithGraphics) {
                Debug.LogError($"Puzzle {puzzle} is not a {nameof(PuzzleWithGraphics)}.");
                return;
            }

            foreach (var puzzleSlot in puzzlesContainer!) {
                if (puzzleSlot.PuzzleId == null) {
                    puzzleSlot.PlacePuzzle((PuzzleWithGraphics)puzzle);
                    return;
                }
            }
        }

        public void SetAsCurrentPlayer(bool current)
        {
            // make name white / gray
            if (playerNameLabel != null) {
                playerNameLabel.color = current ? GameGraphicsSystem.ActiveColor : GameGraphicsSystem.InactiveColor;
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

        public bool TryGetPuzzleWithId(uint puzzleId, out PlayerRowSlot? result)
        {
            result = null;
            foreach (var puzzle in puzzlesContainer!) {
                if (puzzle.PuzzleId == puzzleId) {
                    result = puzzle;
                    return true;
                }
            }
            return false;
        }
    }
}
