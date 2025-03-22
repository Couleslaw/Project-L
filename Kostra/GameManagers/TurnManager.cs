using Kostra.GameActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kostra.GameManagers
{
    /// <summary>
    /// Represents the current phase of the game
    /// </summary>
    enum GamePhase
    {
        /// <summary>
        /// Standard phase of the game in which players take actions.
        /// </summary>
        Normal,

        /// <summary> 
        /// The EndOfTheGame phase is triggered when there are no more black puzzles in the black deck.
        /// </summary>
        EndOfTheGame,

        /// <summary>
        /// The FinishingTouches phase is triggered after the last round of the game.
        /// </summary>
        FinishingTouches,

        /// <summary>
        /// The game is finishing after all players use the <see cref="EndFinishingTouchesAction"/>.
        /// </summary>
        Finished
    }


    /// <summary>
    /// Represents the information about the current turn.
    /// </summary>
    record struct TurnInfo(
        int ActionsLeft,        // how many actions has the current player left in this turn
        GamePhase GamePhase,    // what is the current game phase
        bool UsedMasterAction,  // did the player use the Master action this turn?
        bool TookBlackPuzzle,   // did the player take a black puzzle this turn?
        bool LastRound          // is this the last round of the game?
        );


    /// <summary>
    /// Takes care of the order of players, the game phase and the current turn.
    /// </summary>
    class TurnManager(uint[] playerIds)
    {
        private readonly int _numPlayers = playerIds.Length;
        private readonly uint[] _playersIds = playerIds;

        /// <summary>
        /// The number of actions a player has each turn.
        /// </summary>
        public const int NumActionsInTurn = 3;
        private int _currentPlayerOrder = 0;

        /// <summary>
        /// Gets the current player's ID
        /// </summary>
        public uint CurrentPlayerId => _playersIds[_currentPlayerOrder];

        /// <summary>
        /// True if this is the turn of the last player. 
        /// </summary>
        private bool IsEndOfRound => _currentPlayerOrder == _numPlayers - 1;

        /// <summary>
        /// Internal representation if the current turn.
        /// </summary>
        private TurnInfo _turnInfo = new(ActionsLeft: NumActionsInTurn, GamePhase.Normal, UsedMasterAction: false, TookBlackPuzzle: false, LastRound: false);

        /// <summary>
        /// Resets the internal turn state for the next player.
        /// </summary>
        private void SetNextPlayer()
        {
            _currentPlayerOrder = (_currentPlayerOrder + 1) % _numPlayers;
            _turnInfo.ActionsLeft = NumActionsInTurn;
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
            if (_turnInfo.GamePhase == GamePhase.EndOfTheGame)
            {
                if (_turnInfo.LastRound == false)
                {
                    _turnInfo.LastRound = true;
                }
                else
                {
                    _turnInfo.GamePhase = GamePhase.FinishingTouches;
                }
            }
        }

        /// <summary>
        /// Adjusts the internal turn state to represent the next turn.
        /// </summary>
        /// <returns>Information about the next turn.</returns>
        public TurnInfo NextTurn()
        {
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches || _turnInfo.GamePhase == GamePhase.Finished)
            {
                return _turnInfo;
            }

            if (_turnInfo.ActionsLeft > 0)
            {
                _turnInfo.ActionsLeft--;
                return _turnInfo;
            }

            // turn of a new player
            // after EndOfTheGame is triggered, finish current round and then play 1 last round

            if (IsEndOfRound)
            {
                ChangeGamePhaseIfNeeded();
            }
            SetNextPlayer();

            return _turnInfo;
        }

        /// <summary> Creates a new signaler for this instance. </summary>
        public Signals GetSignaler() => new Signals(this);

        /// <summary>
        /// Signal the given <see cref="TurnManager"/> about the events that happened during the turn.
        /// </summary>
        public class Signals(TurnManager turnManager)
        {
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
                if (turnManager._turnInfo.GamePhase == GamePhase.Normal)
                {
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
                if (turnManager._currentPlayerOrder == 0)
                {
                    turnManager._turnInfo.GamePhase = GamePhase.Finished;
                }
            }
        }
    }
}
