#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using System;
    using Unity.VisualScripting;
    using UnityEngine;

    public class ActionZonesManager : GraphicsManager<ActionZonesManager>, ICurrentTurnListener
    {
        private GameCore? _game;

        [SerializeField] private PuzzleActionZone? _puzzleActionZone;
        [SerializeField] private PieceActionZone? _pieceActionZone;

        public override void Init(GameCore game)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                Debug.LogError("One or more action zones are not assigned in the inspector", this);
                return;
            }

            _game = game;
            _game.AddListener(this);
        }

        public void ConnectToButtons(ActionCreationManager acm)
        {
            if (_puzzleActionZone == null || _pieceActionZone == null) {
                return;
            }

            // action canceling
            ActionButton.CancelAction += acm.OnActionCanceled;

            // action confirming
            _puzzleActionZone.OnConfirmButtonClick += acm.OnActionConfirmed;
            _pieceActionZone.OnConfirmButtonClick += acm.OnActionConfirmed;

            // action requestion
            _puzzleActionZone.OnTakePuzzleButtonClick += acm.OnTakePuzzleActionRequested;
            _puzzleActionZone.OnRecycleButtonClick += acm.OnRecycleActionRequested;

            _pieceActionZone.OnTakeBasicTetrominoButtonClick += acm.OnTakeBasicTetrominoActionRequested;
            _pieceActionZone.OnChangeTetrominoButtonClick += acm.OnChangeTetrominoActionRequested;
            _pieceActionZone.OnMasterActionButtonClick += acm.OnMasterActionRequested;

            // TODO: finishing touches
        }

        public void SetAIPlayerMode()
        {
            throw new NotImplementedException();
        }

        public void SetHumanPlayerMode()
        {
            throw new NotImplementedException();
        }

        public void EnableConfirmButtons()
        {
            throw new NotImplementedException();
        }

        public void DisableConfirmButtons()
        {
            throw new NotImplementedException();
        }

        void ICurrentTurnListener.OnCurrentTurnChanged(TurnInfo currentTurnInfo)
        {
            var gameInfo = _game!.GameState.GetGameInfo();
            var playerInfo = _game.PlayerStates[_game.CurrentPlayer].GetPlayerInfo();

            // TODO: update UI
            throw new NotImplementedException();
        }
    }
}
