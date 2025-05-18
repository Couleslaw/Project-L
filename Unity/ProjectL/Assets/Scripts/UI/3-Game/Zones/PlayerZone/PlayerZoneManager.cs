#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using System.Collections.Generic;
    using UnityEngine;
    using ProjectLCore.Players;
    using ProjectLCore.GameLogic;
    using System.Threading.Tasks;
    using ProjectLCore.GameActions;
    using System.Threading;
    using System;

    public class PlayerZoneManager : GraphicsManager<PlayerZoneManager>, ICurrentPlayerListener
    {
        [SerializeField] private GameObject? _playerZoneRowPrefab;

        private Dictionary<Player, PlayerZoneRow> _playerZoneRows = new();

        private Player? _currentPlayer;

        public bool CanConfirmTakePuzzleAction {
            set {
                var currentPlayerRow = _playerZoneRows[_currentPlayer!];
                foreach (var row in _playerZoneRows.Values) {
                    row.CanConfirmTakePuzzleAction = value && row == currentPlayerRow;
                }
            }
        }

        public override void Init(GameCore game)
        {
            if (_playerZoneRowPrefab == null) {
                Debug.LogError("PlayerZoneRow prefab is not assigned!", this);
                return;
            }

            game.AddListener((ICurrentPlayerListener)this);

            foreach (var player in game.Players) {
                GameObject rowParent = Instantiate(_playerZoneRowPrefab, transform);
                rowParent.SetActive(true);

                var row = rowParent.GetComponentInChildren<PlayerZoneRow>();
                row.Init(player.Name, game.PlayerStates[player]);
                _playerZoneRows.Add(player, row);
            }
        }

        void ICurrentPlayerListener.OnCurrentPlayerChanged(Player currentPlayer)
        {
            _currentPlayer = currentPlayer;
            foreach (var kvp in _playerZoneRows) {
                var playerZoneRow = kvp.Value;
                playerZoneRow.SetAsCurrentPlayer(kvp.Key == currentPlayer);
            }
        }

        public Vector2 GetPlacementPositionFor(PlaceTetrominoAction action)
        {
            if (_currentPlayer == null) {
                Debug.LogError("Current player is not set!", this);
                return default;
            }

            var playerZoneRow = _playerZoneRows[_currentPlayer];
            return playerZoneRow.GetPlacementPositionFor(action);
        }


        public PlayerRowSlot? GetPuzzleWithId(uint puzzleId)
        {
            PlayerRowSlot? puzzle = null;
            foreach (PlayerZoneRow row in _playerZoneRows.Values) {
                if (row.TryGetPuzzleWithId(puzzleId, out puzzle)) {
                    return puzzle;
                }
            }
            return null;
        }
    }
}
