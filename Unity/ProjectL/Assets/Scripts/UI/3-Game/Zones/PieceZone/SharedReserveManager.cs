#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using UnityEngine;
    using ProjectLCore.GameLogic;
    using ProjectL.UI.GameScene.Actions;
    using NUnit.Framework;
    using System.Collections.Generic;
    using ProjectLCore.GameManagers;
    using System;
    using ProjectLCore.GamePieces;

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

        public int[] GetNumTetrominosLeft()
        {
            int[] tetrominosLeft = new int[TetrominoManager.NumShapes];
            foreach (TetrominoShape shape in Enum.GetValues(typeof(TetrominoShape))) {
                tetrominosLeft[(int)shape] = _sharedReserveStats!.GetDisplayCount(shape);
            }
            return tetrominosLeft;
        }


        void IHumanPlayerActionListener.OnActionCanceled() => _sharedReserveStats!.ResetColumn();

        void IHumanPlayerActionListener.OnActionConfirmed() => _sharedReserveStats!.ResetColumn();

        void IHumanPlayerActionListener.OnActionRequested() { }
    }
}
