#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using NUnit.Framework;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GameManagers;
    using ProjectLCore.GamePieces;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class PlayerStatsManager : GraphicsManager<PlayerStatsManager>, ICurrentPlayerListener, ITetrominoSpawnerListener
    {
        [Header("Player Names")]
        [SerializeField] private GameObject? _playerNamesContainer;
        [SerializeField] private GameObject? _playerNameTemplate;

        [Header("Tetromino Collections")]
        [SerializeField] private GameObject? _tetrominoCollectionsContainer;
        [SerializeField] private PieceCountColumn? _pieceCountColumnPrefab;

        private Dictionary<Player, PieceCountColumn> _pieceColumns = new();
        private Dictionary<Player, TextMeshProUGUI> _playerNameLabels = new();

        private Player? _currentPlayer = null;

        public PieceCountColumn? CurrentPlayerColumn => _currentPlayer != null ? _pieceColumns[_currentPlayer] : null;


        public override void Init(GameCore game)
        {
            if (_playerNamesContainer == null || _playerNameTemplate == null ||
                _tetrominoCollectionsContainer == null || _pieceCountColumnPrefab == null) {
                Debug.LogError("One or more UI elements are not assigned in the inspector.");
                return;
            }

            TetrominoButtonsManager.Instance.RegisterListener(this);
            game.AddListener(this);

            foreach (Player player in game.Players) {
                // create count column
                var column = Instantiate(_pieceCountColumnPrefab, _tetrominoCollectionsContainer.transform);
                column.gameObject.SetActive(true);
                column.Init(0, game.PlayerStates[player]);  // players start with no pieces
                _pieceColumns.Add(player, column);

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
                SetPlayerColumnColor(player, GameGraphicsSystem.InactivePlayerColor);
            }
        }

        public void OnCurrentPlayerChanged(Player currentPlayer)
        {
            _currentPlayer = currentPlayer;
            foreach (var player in _pieceColumns.Keys) {
                if (player == currentPlayer) {
                    SetPlayerColumnColor(player, GameGraphicsSystem.ActivePlayerColor);
                }
                else {
                    SetPlayerColumnColor(player, GameGraphicsSystem.InactivePlayerColor);
                }
            }
            TetrominoButtonsManager.Instance.SetCurrentPieceColumn(_pieceColumns[currentPlayer]);
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
            if (_pieceColumns.TryGetValue(player, out PieceCountColumn? column)) {
                column.SetColor(color);
            }
            else {
                Debug.LogError($"Player {player.Name} not found in piece columns.");
            }
        }

        public void OnTetrominoSpawned(TetrominoShape tetromino)
        {
            _pieceColumns[_currentPlayer!].DecrementDisplayCount(tetromino);
        }

        public void OnTetrominoReturned(TetrominoShape tetromino)
        {
            _pieceColumns[_currentPlayer!].IncrementDisplayCount(tetromino);
        }
    }
}
