#nullable enable

namespace ProjectL.GameScene.PlayerZone
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerZoneManager : GraphicsManager<PlayerZoneManager>, ICurrentPlayerListener
    {
        #region Fields

        [SerializeField] private GameObject? _playerZoneRowPrefab;

        private Dictionary<Player, PlayerPuzzlesRow> _playerZoneRows = new();

        private Player? _currentPlayer;

        #endregion

        #region Properties

        public bool IsMouseOverCurrentPlayersRow => _playerZoneRows[_currentPlayer!].IsMouseOverRow;

        public PlayerPuzzlesRow? CurrentPlayerRow => _currentPlayer != null ? _playerZoneRows[_currentPlayer] : null;

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

                var row = rowParent.GetComponentInChildren<PlayerPuzzlesRow>();
                row.Init(player.Name, game.PlayerStates[player]);
                _playerZoneRows.Add(player, row);
            }
        }

        public PuzzleSlot? GetPuzzleWithId(uint puzzleId)
        {
            PuzzleSlot? puzzle = null;
            foreach (PlayerPuzzlesRow row in _playerZoneRows.Values) {
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
