#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using UnityEngine;
    using ProjectLCore.GameLogic;
    using ProjectL.UI.GameScene.Actions;

    public class SharedReserveManager : GraphicsManager<SharedReserveManager>, ITetrominoActionListener
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


        void IHumanPlayerActionListener.OnActionCanceled() => _sharedReserveStats!.ResetColumn();

        void IHumanPlayerActionListener.OnActionConfirmed() => _sharedReserveStats!.ResetColumn();

        void IHumanPlayerActionListener.OnActionRequested() { }
    }
}
