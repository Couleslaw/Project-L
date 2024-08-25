namespace Kostra {
    enum GamePhase { Normal, LastRound, FinishingTouches, Ended }
    record struct TurnInfo(int ActionsLeft, bool UsedMasterAction, GamePhase GamePhase);

    class TurnManager(int numPlayers) {
        private int _numPlayers = numPlayers;

        public const int NumActionsInTurn = 3;
        public int CurrentPlayer { get; private set; } = 0;
        private bool IsEndOfRound => CurrentPlayer == _numPlayers - 1;

        private TurnInfo _turnInfo = new(ActionsLeft: NumActionsInTurn, UsedMasterAction: false, GamePhase.Normal);

        private void SetNextPlayer() {
            CurrentPlayer = (CurrentPlayer + 1) % _numPlayers;
            _turnInfo.ActionsLeft = NumActionsInTurn;
            _turnInfo.UsedMasterAction = false;
        }

        private bool _nextRoundWillBeLast = false;
        private void ChangeGamePhaseIfNeeded() {
            if (_nextRoundWillBeLast && _turnInfo.GamePhase == GamePhase.Normal) {
                _turnInfo.GamePhase = GamePhase.LastRound;
                _nextRoundWillBeLast = false;
                return;
            }

            if (_turnInfo.GamePhase == GamePhase.LastRound) {
                _turnInfo.GamePhase = GamePhase.FinishingTouches;
            }
        }

        public TurnInfo NextTurn() {
            if (_turnInfo.GamePhase == GamePhase.FinishingTouches) {
                return _turnInfo;
            }

            if (_turnInfo.ActionsLeft > 0) {
                _turnInfo.ActionsLeft--;
                return _turnInfo;
            }

            SetNextPlayer();
            if (IsEndOfRound) {
                ChangeGamePhaseIfNeeded();
            }
            return _turnInfo;
        }

        public class Signals(TurnManager turnManager) {
            private TurnManager _turnManager = turnManager;

            public void NoCardsLeftInBlackDeck() {
                _turnManager._nextRoundWillBeLast = true;
            }
            public void PlayerUsedMasterAction() {
                _turnManager._turnInfo.UsedMasterAction = true;
            }
            public void PlayerEndedFinishingTouches() {
                _turnManager.SetNextPlayer();
                if (_turnManager.CurrentPlayer == 0) {
                    _turnManager._turnInfo.GamePhase = GamePhase.Ended;
                }
            }
        }
    }

    class ProcessorManager {
        public GameStateActionProcessor GameStateActionProcessor { get; }
        public GameStateGraphicsProcessor GameStateGraphicsProcessor { get; }
        public PlayerStateActionProcessor[] PlayerStateActionProcessors { get; }
        public PlayerStateGraphicsProcessor[] PlayerStateGraphicsProcessors { get; }

        public ProcessorManager(GameState gameState, PlayerState[] playerStates, TurnManager turnManager) {
            var gameEventSignaller = new TurnManager.Signals(turnManager);

            GameStateActionProcessor = new GameStateActionProcessor(gameState, gameEventSignaller);
            GameStateGraphicsProcessor = new GameStateGraphicsProcessor(gameState);

            PlayerStateActionProcessors = new PlayerStateActionProcessor[playerStates.Length];
            PlayerStateGraphicsProcessors = new PlayerStateGraphicsProcessor[playerStates.Length];
            for (int i = 0; i < playerStates.Length; i++) {
                PlayerStateActionProcessors[i] = new PlayerStateActionProcessor(playerStates[i], gameState, gameEventSignaller);
                PlayerStateGraphicsProcessors[i] = new PlayerStateGraphicsProcessor(playerStates[i]);
            }
        }
    }

    class GameCore {
        public const int MaxPlayers = 4;
        public int NumPlayers => Players.Length;

        public GameState GameState { get; }
        public Player[] Players { get; }
        public PlayerState[] PlayerStates { get; }

        public GameCore(GameState gameState, Player[] players) {
            if (players.Length > MaxPlayers) {
                throw new ArgumentException("Too many players");
            }
            GameState = gameState;
            Players = players;

            PlayerStates = new PlayerState[NumPlayers];
            for (int i = 0; i < NumPlayers; i++) {
                PlayerStates[i] = new PlayerState(playerId: Players[i].Id);
            }
        }
    }
}