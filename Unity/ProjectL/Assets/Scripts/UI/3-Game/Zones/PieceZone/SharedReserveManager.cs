#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using UnityEngine;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameActions;

    public class SharedReserveManager : GraphicsManager<SharedReserveManager>, ITetrominoActionCanceledListener
    {
        [SerializeField] private PieceCountColumn? _sharedReserveStats;
        
        public override void Init(GameCore game)
        {
            if (_sharedReserveStats == null) {
                Debug.LogError("Shared Reserve column is not assigned!", this);
                return;
            }

            _sharedReserveStats.Init(game.GameState.NumInitialTetrominos, game.GameState);
            ActionCreationManager.Instance.AddListener(this);
        }

        public void OnActionCanceled()
        {
            _sharedReserveStats!.ResetColumn();
        }
    }
}
