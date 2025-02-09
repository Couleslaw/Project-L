namespace Kostra {
    enum GamePhase { Normal, EndOfTheGame, FinishingTouches, Finished }

    // TODO kdyz je konec hry tak muze vzit jen jedno cerne puzzle
    record struct TurnInfo(int ActionsLeft, GamePhase GamePhase, bool UsedMasterAction, bool TookBlackPuzzle);

    class TurnManager(int numPlayers) {
        private readonly int _numPlayers = numPlayers;

        public const int NumActionsInTurn = 3;
        public int CurrentPlayer { get; private set; } = 0;
        private bool IsEndOfRound => CurrentPlayer == _numPlayers - 1;

        private TurnInfo _turnInfo = new(ActionsLeft: NumActionsInTurn, GamePhase.Normal, UsedMasterAction: false, TookBlackPuzzle: false);

        private void SetNextPlayer() {
            CurrentPlayer = (CurrentPlayer + 1) % _numPlayers;
            _turnInfo.ActionsLeft = NumActionsInTurn;
            _turnInfo.UsedMasterAction = false;
            _turnInfo.TookBlackPuzzle = false;
        }

        private bool _lastRound = false;
        private void ChangeGamePhaseIfNeeded()
        {
            if (_turnInfo.GamePhase == GamePhase.EndOfTheGame)
            {
                if (_lastRound == false)
                {
                    _lastRound = true;
                }
                else
                {
                    _turnInfo.GamePhase = GamePhase.FinishingTouches;
                }
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

            // turn of a new player
            // after EndOfTheGame is triggered, finish current round and then play 1 last round

            if (IsEndOfRound) {
                ChangeGamePhaseIfNeeded();
            }
            SetNextPlayer();

            return _turnInfo;
        }

        public class Signals(TurnManager turnManager) {
            private TurnManager _turnManager = turnManager;

            public void PlayerTookBlackPuzzle()
            {
                _turnManager._turnInfo.TookBlackPuzzle = true;
            }
            public void NoCardsLeftInBlackDeck() {
                _turnManager._turnInfo.GamePhase = GamePhase.EndOfTheGame;
            }
            public void PlayerUsedMasterAction() {
                _turnManager._turnInfo.UsedMasterAction = true;
            }
            public void PlayerEndedFinishingTouches() {
                _turnManager.SetNextPlayer();
                if (_turnManager.CurrentPlayer == 0) {
                    _turnManager._turnInfo.GamePhase = GamePhase.Finished;
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

        public GameCore(GameState gameState, IList<Player> players, bool shufflePlayers) {
            if (players.Count > MaxPlayers) {
                throw new ArgumentException("Too many players");
            }
            GameState = gameState;

            Players = new Player[players.Count];
            players.CopyTo(Players, 0);
            if (shufflePlayers)
            {
                Players.Shuffle();
            }

            PlayerStates = new PlayerState[NumPlayers];
            for (int i = 0; i < NumPlayers; i++) {
                PlayerStates[i] = new PlayerState(playerId: Players[i].Id);
            }
        }
    }
}