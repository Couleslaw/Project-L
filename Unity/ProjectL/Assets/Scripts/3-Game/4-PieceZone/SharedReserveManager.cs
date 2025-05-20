#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;

    public class SharedReserveManager : GraphicsManager<SharedReserveManager>
    {
        #region Fields

        [SerializeField] private PieceCountColumn? _sharedReserveStats;

        #endregion

        #region Properties

        public PieceCountColumn PieceColumn => _sharedReserveStats!;

        #endregion

        #region Methods

        public override void Init(GameCore game)
        {
            if (_sharedReserveStats == null) {
                Debug.LogError("Shared Reserve column is not assigned!", this);
                return;
            }

            _sharedReserveStats.Init(game.GameState.NumInitialTetrominos, game.GameState);
        }

        public int[] GetNumTetrominosLeft()
        {
            int[] tetrominosLeft = new int[TetrominoManager.NumShapes];
            foreach (TetrominoShape shape in Enum.GetValues(typeof(TetrominoShape))) {
                tetrominosLeft[(int)shape] = _sharedReserveStats!.GetDisplayCount(shape);
            }
            return tetrominosLeft;
        }

        #endregion
    }
}
