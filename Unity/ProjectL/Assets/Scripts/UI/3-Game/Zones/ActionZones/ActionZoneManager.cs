#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using UnityEngine;

    public class ActionZoneManager : StaticInstance<ActionZoneManager>, IGameZoneManager, ICurrentPlayerListener, ICurrentTurnListener
    {
        [SerializeField] private PuzzleActionZone? puzzleActionZone;
        [SerializeField] private PieceActionZone? pieceActionZone;

        public void Init(GameCore game)
        {
            throw new System.NotImplementedException();
        }

        public void OnCurrentPlayerChanged(Player currentPlayer)
        {
            throw new System.NotImplementedException();
        }

        public void OnCurrentTurnChanged(TurnInfo currentTurnInfo)
        {
            throw new System.NotImplementedException();
        }

        protected override void Awake()
        {
            base.Awake();
            if (puzzleActionZone == null || pieceActionZone == null) {
                Debug.LogError("One or more Action Zones are not assigned in the inspector", this);
                return;
            }
        }
    }
}
