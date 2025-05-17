#nullable enable

namespace ProjectL.Data
{
    using ProjectLCore.GameLogic;
    using System.Collections.Generic;

    public static class RuntimeGameInfo
    {
        #region Fields

        private static GameCore? _game;

        #endregion

        #region Properties

        public static bool IsGameInProgress => _game != null;

        #endregion

        #region Methods

        /// <summary>
        /// Registers the game instance as the current game.
        /// </summary>
        /// <param name="game">The game.</param>
        public static void RegisterGame(GameCore game) => _game = game;

        /// <summary>
        /// Unregisters the current game instance.
        /// </summary>
        public static void UnregisterGame() => _game = null;

        /// <summary>
        /// Retrieves information about the current game state.
        /// </summary>
        /// <param name="result">When this methods succeeds, contains a <see cref="Info"/> instance representing the current game state; or unspecified value on failure.</param>
        /// <returns>
        /// <see langword="true"/> if a <see cref="GameCore"/> instance is registered; otherwise, <see langword="false"/>.
        ///</returns>
        public static bool TryGetCurrentInfo(out Info result)
        {
            if (_game == null) {
                result = default;
                return false;
            }
            result = new Info {
                PlayerName = _game.CurrentPlayer.Name,
                CurrentTurnInfo = _game.CurrentTurn,
                PlayerScores = GetPlayerScores()
            };
            return true;

            static Dictionary<string, int> GetPlayerScores()
            {
                var scores = new Dictionary<string, int>();
                foreach (var player in _game!.Players) {
                    scores[player.Name] = _game.PlayerStates[player].Score;
                }
                return scores;
            }
        }

        #endregion

        /// <summary>
        /// Represents information about the current game state.
        /// </summary>
        public struct Info
        {
            #region Properties

            /// <summary> The name of the current player. </summary>
            public string PlayerName { get; set; }

            /// <summary> The current phase of the game. </summary>
            public TurnInfo CurrentTurnInfo { get; set; }

            /// <summary> Dictionary where the key is the player's name and the value is their score.</summary>
            public Dictionary<string, int> PlayerScores { get; set; }

            #endregion
        }
    }
}
