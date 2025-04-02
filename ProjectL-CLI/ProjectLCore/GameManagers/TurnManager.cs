namespace ProjectLCore.GameManagers
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;

    /// <summary>
    /// Takes care of the order in which the players take turns, the current game phase and information about the current turn.
    /// </summary>
    /// <param name="playerIds">The IDs of the players in the game.</param>
    /// <seealso cref="TurnManager.Signaler"/>
    public class TurnManager(uint[] playerIds)
    {
        #region Constants

        /// <summary>
        /// The number of actions a player has each turn.
        /// </summary>
        public const int NumActionsInTurn = 3;

        #endregion

        #region Fields

        private readonly int _numPlayers = playerIds.Length;

        private readonly uint[] _playersIds = playerIds;

        private int _currentPlayersIndex = 0;

        /// <summary>
        /// Internal representation if the current turn.
        /// </summary>
        private TurnInfo _turnInfo = new(NumActionsLeft: NumActionsInTurn, GamePhase.Normal, UsedMasterAction: false, TookBlackPuzzle: false, LastRound: false);

        #endregion

        #region Properties

        /// <summary>
        /// The ID of the current player.
        /// </summary>
        public uint CurrentPlayerId => _playersIds[_currentPlayersIndex];

        /// <summary>
        /// <see langword="true"/> if this is the turn of the last player; else <see langword="false"/>.
        /// </summary>
        private bool IsEndOfRound => _currentPlayersIndex == _numPlayers - 1;

        #endregion

        #region Methods

        /// <summary>
        /// Adjusts the internal turn state to represent the next turn.
        /// </summary>
        /// <returns>Information about the next turn.</returns>
        public TurnInfo NextTurn()
        {
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches || _turnInfo.GamePhase == GamePhase.Finished) {
                return _turnInfo;
            }

            if (_turnInfo.NumActionsLeft > 0) {
                _turnInfo.NumActionsLeft--;
                return _turnInfo;
            }

            // turn of a new player
            // after EndOfTheGame is triggered, finish current round and then play 1 last round

            if (IsEndOfRound) {
                ChangeGamePhaseIfNeeded();
            }
            SetNextPlayer();

            return _turnInfo;
        }

        /// <summary> Creates a new signaler for this instance. </summary>
        /// <returns> A new signaler for this instance. </returns>
        public Signaler GetSignaler() => new Signaler(this);

        /// <summary>
        /// Resets the internal turn state for the next player.
        /// </summary>
        private void SetNextPlayer()
        {
            _currentPlayersIndex = (_currentPlayersIndex + 1) % _numPlayers;
            _turnInfo.NumActionsLeft = NumActionsInTurn - 1;
            _turnInfo.UsedMasterAction = false;
            _turnInfo.TookBlackPuzzle = false;
        }

        /// <summary>
        /// Changes the game phase from <see cref="GamePhase.EndOfTheGame"/> to <see cref="GamePhase.FinishingTouches"/> if the conditions are met.
        /// After <see cref="GamePhase.EndOfTheGame"/> is triggered, the players finish their current round and then play one last round. 
        /// <see cref="GamePhase.FinishingTouches"/> start after that.
        /// </summary>
        private void ChangeGamePhaseIfNeeded()
        {
            if (_turnInfo.GamePhase == GamePhase.EndOfTheGame) {
                if (_turnInfo.LastRound == false) {
                    _turnInfo.LastRound = true;
                }
                else {
                    _turnInfo.GamePhase = GamePhase.FinishingTouches;
                }
            }
        }

        #endregion

        /// <summary>
        /// Signal a <see cref="TurnManager"/> about the events that happened during the turn.
        /// </summary>
        /// <param name="turnManager">The <see cref="TurnManager"/> to send signals to.</param>
        /// <seealso cref="TurnManager"/>
        public class Signaler(TurnManager turnManager)
        {
            #region Methods

            /// <summary>
            /// Signals that the current player took a black puzzle.
            /// Players can take only up to 1 black puzzle per turn once <see cref="GamePhase.EndOfTheGame"/> is triggered.
            /// </summary>
            public void PlayerTookBlackPuzzle()
            {
                turnManager._turnInfo.TookBlackPuzzle = true;
            }

            /// <summary>
            /// Signals that the black deck is empty.
            /// This triggers the <see cref="GamePhase.EndOfTheGame"/> phase, if it is not already triggered.
            /// </summary>
            public void BlackDeckIsEmpty()
            {
                if (turnManager._turnInfo.GamePhase == GamePhase.Normal) {
                    turnManager._turnInfo.GamePhase = GamePhase.EndOfTheGame;
                }
            }

            /// <summary>
            /// Signals that the current player used <see cref="MasterAction"/>
            /// Players can use <see cref="MasterAction"/> only once per turn.
            /// </summary>
            public void PlayerUsedMasterAction()
            {
                turnManager._turnInfo.UsedMasterAction = true;
            }

            /// <summary>
            /// Signals that the current player use <see cref="EndFinishingTouchesAction"/>.
            /// The game ends once all players do this.
            /// </summary>
            public void PlayerEndedFinishingTouches()
            {
                turnManager.SetNextPlayer();
                if (turnManager._currentPlayersIndex == 0) {
                    turnManager._turnInfo.GamePhase = GamePhase.Finished;
                }
            }

            #endregion
        }
    }
}
