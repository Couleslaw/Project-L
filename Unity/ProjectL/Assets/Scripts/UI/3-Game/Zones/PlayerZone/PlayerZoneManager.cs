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
        [SerializeField] private PlayerZoneRow? _playerZoneRowPrefab;

        private Dictionary<Player, PlayerZoneRow> _playerZoneRows = new();

        private Player? _currentPlayer;

        public override void Init(GameCore game)
        {
            if (_playerZoneRowPrefab == null) {
                Debug.LogError("PlayerZoneRow prefab is not assigned!", this);
                return;
            }

            game.AddListener(this);

            foreach (var player in game.Players) {
                var playerZoneRow = Instantiate(_playerZoneRowPrefab, transform);
                playerZoneRow.gameObject.SetActive(true);
                playerZoneRow.Init(player.Name, game.PlayerStates[player]);
                _playerZoneRows.Add(player, playerZoneRow);
            }
        }

        public void OnCurrentPlayerChanged(Player currentPlayer)
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

        public PlayerRowSlot GetCurrentPlayerPuzzleOnIndex(int index)
        {
            return _playerZoneRows[_currentPlayer!].GetPuzzleOnIndex(index);
        }
    }
}
