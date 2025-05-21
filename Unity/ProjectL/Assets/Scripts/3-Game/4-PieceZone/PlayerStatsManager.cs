#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using ProjectL.Animation;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class PlayerStatsManager : GraphicsManager<PlayerStatsManager>, ICurrentPlayerListener, ITetrominoSpawnerListener
    {
        #region Fields

        [Header("Player Names")]
        [SerializeField] private GameObject? _playerNamesContainer;

        [SerializeField] private GameObject? _playerNameTemplate;

        [Header("Tetromino Collections")]
        [SerializeField] private GameObject? _tetrominoCollectionsContainer;

        [SerializeField] private TetrominoCountsColumn? _pieceCountColumnPrefab;

        private Dictionary<Player, TetrominoCountsColumn> _tetrominoColumns = new();

        private Dictionary<Player, TextMeshProUGUI> _playerNameLabels = new();

        private Player? _currentPlayer = null;

        #endregion

        #region Properties

        public TetrominoCountsColumn? CurrentPieceColumn => _currentPlayer != null ? _tetrominoColumns[_currentPlayer] : null;

        #endregion

        #region Methods

        public override void Init(GameCore game)
        {
            if (_playerNamesContainer == null || _playerNameTemplate == null ||
                _tetrominoCollectionsContainer == null || _pieceCountColumnPrefab == null) {
                Debug.LogError("One or more UI elements are not assigned in the inspector.");
                return;
            }

            PieceZoneManager.Instance.RegisterListener((ITetrominoSpawnerListener)this);
            game.AddListener((ICurrentPlayerListener)this);

            foreach (Player player in game.Players) {
                // create count column
                var column = Instantiate(_pieceCountColumnPrefab, _tetrominoCollectionsContainer.transform);
                column.gameObject.SetActive(true);
                column.Init(0, game.PlayerStates[player], shouldColorGains: true);  // players start with no pieces
                _tetrominoColumns.Add(player, column);

                // create player name - the TMPro text object is a child of the player name template
                var playerName = Instantiate(_playerNameTemplate, _playerNamesContainer.transform);
                playerName.gameObject.SetActive(true);
                var playerNameLabel = playerName.GetComponentInChildren<TextMeshProUGUI>();
                if (playerNameLabel == null) {
                    Debug.LogError("Player name label not found in the player name template.");
                    continue;
                }

                // set the first letter of the name in uppercase as the player name
                playerNameLabel.text = player.Name[0].ToString();
                _playerNameLabels.Add(player, playerNameLabel);

                // gray out the name and piece column
                SetPlayerColumnColor(player, ColorManager.gray);
            }
        }

        private void SetPlayerColumnColor(Player player, Color color)
        {
            // set color of name text
            if (_playerNameLabels.TryGetValue(player, out TextMeshProUGUI? label)) {
                label.color = color;
            }
            else {
                Debug.LogError($"Player {player.Name} not found in player name labels.");
            }

            // set color of piece column
            if (_tetrominoColumns.TryGetValue(player, out TetrominoCountsColumn? column)) {
                column.SetColor(color);
            }
            else {
                Debug.LogError($"Player {player.Name} not found in piece columns.");
            }
        }

        void ICurrentPlayerListener.OnCurrentPlayerChanged(Player currentPlayer)
        {
            _currentPlayer = currentPlayer;
            foreach (var player in _tetrominoColumns.Keys) {
                if (player == currentPlayer) {
                    SetPlayerColumnColor(player, Color.white);
                }
                else {
                    SetPlayerColumnColor(player, ColorManager.gray);
                }
            }
            PieceZoneManager.Instance.SetCurrentTetrominoColumn(_tetrominoColumns[currentPlayer]);
        }

        void ITetrominoSpawnerListener.OnTetrominoSpawned(TetrominoShape tetromino)
        {
            _tetrominoColumns[_currentPlayer!].DecrementDisplayCount(tetromino);
        }

        void ITetrominoSpawnerListener.OnTetrominoReturned(TetrominoShape tetromino)
        {
            _tetrominoColumns[_currentPlayer!].IncrementDisplayCount(tetromino);
        }

        #endregion
    }
}
