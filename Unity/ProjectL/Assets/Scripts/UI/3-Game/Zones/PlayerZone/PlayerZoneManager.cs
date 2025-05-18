#nullable enable

namespace ProjectL.UI.GameScene.Zones.PlayerZone
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerZoneManager : GraphicsManager<PlayerZoneManager>, ICurrentPlayerListener
    {
        #region Fields

        [SerializeField] private GameObject? _playerZoneRowPrefab;

        private Dictionary<Player, PlayerZoneRow> _playerZoneRows = new();

        private Player? _currentPlayer;

        #endregion

        #region Properties

        public bool CanConfirmTakePuzzleAction {
            set {
                var currentPlayerRow = _playerZoneRows[_currentPlayer!];
                foreach (var row in _playerZoneRows.Values) {
                    row.CanConfirmTakePuzzleAction = value && row == currentPlayerRow;
                }
            }
        }

        public bool IsMouseOverCurrentPlayersRow => _playerZoneRows[_currentPlayer!].IsMouseOverRow;

        #endregion

        #region Methods

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

        public Vector2 GetPlacementPositionFor(PlaceTetrominoAction action)
        {
            if (_currentPlayer == null) {
                Debug.LogError("Current player is not set!", this);
                return default;
            }

            var playerZoneRow = _playerZoneRows[_currentPlayer];
            return playerZoneRow.GetPlacementPositionFor(action);
        }

        public PuzzleSlot? GetPuzzleWithId(uint puzzleId)
        {
            PuzzleSlot? puzzle = null;
            foreach (PlayerZoneRow row in _playerZoneRows.Values) {
                if (row.TryGetPuzzleWithId(puzzleId, out puzzle)) {
                    return puzzle;
                }
            }
            return null;
        }

        void ICurrentPlayerListener.OnCurrentPlayerChanged(Player currentPlayer)
        {
            _currentPlayer = currentPlayer;
            foreach (var kvp in _playerZoneRows) {
                var playerZoneRow = kvp.Value;
                playerZoneRow.SetAsCurrentPlayer(kvp.Key == currentPlayer);
            }
        }

        #endregion
    }
}
