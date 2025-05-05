#nullable enable

namespace ProjectL.UI.GameScene
{
    using UnityEngine;
    using ProjectLCore.GameLogic;
    using ProjectL.UI.GameScene.Zones.PuzzleZone;

    public class GameGraphicsManager : MonoBehaviour
    {
        [SerializeField] private PuzzleZoneManager? _puzzleZoneManager;

        private void Awake()
        {
            if (_puzzleZoneManager == null) {
                Debug.LogError("PuzzleZoneManager is not assigned!", this);
                return;
            }
        }

        public void Initialize(GameState gameState)
        {
            if (_puzzleZoneManager == null) {
                return;
            }
            _puzzleZoneManager.ListenTo(gameState);
        }
    }
}
