using ProjectLCore.GameActions.Verification;
using ProjectLCore.GameActions;
using ProjectLCore.GameLogic;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProjectLCore.GameManagers;
using ProjectLCore.GamePieces;
using ProjectLCore.Players;
using static ProjectLCore.GameLogic.GameState;
using static ProjectLCore.GameLogic.PlayerState;
using System.Text;
using TMPro;
using UnityEngine.UI;
using System.Net;

#nullable enable

public class TextBasedGame : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI? gameStateBox;
    [SerializeField] private TextMeshProUGUI? playerStatesBox;
    [SerializeField] private TextMeshProUGUI? actionsBox;
    [SerializeField] private Button? continueButton;

    private bool _shouldContinue = false;

    private void Start()
    {
        if (gameStateBox == null || playerStatesBox == null || actionsBox == null) {
            Debug.LogError("One or more UI elements are not assigned");
            return;
        }
        if (continueButton == null) {
            Debug.LogError("Continue button is not assigned");
            return;
        }

        GameTextView.GameStateTextBox = gameStateBox;
        GameTextView.PlayerStatesTextBox = playerStatesBox;
        GameTextView.ActionsTextBox = actionsBox;
        continueButton.onClick.AddListener(OnContinueButtonClick);
    }

    public void OnContinueButtonClick()
    {
        _shouldContinue = true;
    }

    public async Task GameLoopAsync(GameCore game)
    {
        if (game == null) {
            Debug.LogError("GameCore is null. Cannot start game loop.");
            return; // safety check
        }

        Debug.Log("Starting game loop.");
        GameTextView.Clear();   

        while (!destroyCancellationToken.IsCancellationRequested) {
            TurnInfo turnInfo = game.GetNextTurnInfo();

            // check if game ended
            if (game.CurrentGamePhase == GamePhase.Finished) {
                Debug.Log("Game ended!");
                game.GameEnded();
                break;
            }

            GameTextView.PrintTurnInfo(game.CurrentPlayer, turnInfo);

            // create verifier for the current player
            var gameInfo = game.GameState.GetGameInfo();
            var playerInfos = game.GetPlayerInfos();
            var currentPlayerInfo = game.PlayerStates[game.CurrentPlayer].GetPlayerInfo();
            var verifier = new ActionVerifier(gameInfo, currentPlayerInfo, turnInfo);

            GameTextView.PrintGameScreen(gameInfo, playerInfos, game);

            // get action from player
            IAction? action;
            try {
                action = await game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier, destroyCancellationToken);
            }
            catch (Exception e) {
                action = null;
                GameTextView.PrintPlayerProvidedNoAction(game.CurrentPlayer, e.Message);
            }

            // verify if got some action
            if (action != null) {
                var result = verifier.Verify(action);
                if (result is VerificationFailure fail) {
                    GameTextView.PrintPlayerProvidedInvalidAction(action, fail, game.CurrentPlayer);
                    action = null;
                }
            }

            // assign default action if null
            if (action == null) {
                action = GetDefaultAction(game.CurrentGamePhase);
            }
            else {
                GameTextView.PrintPlayerProvidedValidAction(action, game.CurrentPlayer);
            }

            // process valid action
            game.ProcessAction(action);

            // if not Finishing touches --> log finished puzzles
            if (game.CurrentGamePhase != GamePhase.FinishingTouches) {
                while (game.TryGetNextPuzzleFinishedBy(game.CurrentPlayer, out var finishedPuzzleInfo)) {
                    GameEndStats.AddFinishedPuzzle(game.CurrentPlayer, finishedPuzzleInfo.Puzzle);
                    GameTextView.PrintFinishedPuzzleInfo(finishedPuzzleInfo, game.CurrentPlayer);
                }
            }

            // if finishing touches --> log used tetrominos
            if (game.CurrentGamePhase == GamePhase.FinishingTouches && action is PlaceTetrominoAction a) {
                GameEndStats.AddFinishingTouchTetromino(game.CurrentPlayer, a.Shape);
            }

            // await for continue button click
            while (!_shouldContinue) {
                await Awaitable.WaitForSecondsAsync(0.1f, destroyCancellationToken);
            }
            _shouldContinue = false;
            GameTextView.Clear();
        }
    }

    private IAction GetDefaultAction(GamePhase gamePhase)
    {
        return (gamePhase == GamePhase.FinishingTouches)
                    ? new EndFinishingTouchesAction()
                    : new DoNothingAction();
    }


    private static class GameTextView
    {
        private const uint FirstPlayerId = 0;

        private readonly static string LargeSeparator = new String('X', 90);

        private readonly static string SmallSeparator = new String('-', 90);
        private readonly static string PlayerSeparator = new String('-', 60);

        private static int RoundCount = 0;

        public static TextMeshProUGUI? GameStateTextBox { get; set; }
        public static TextMeshProUGUI? PlayerStatesTextBox { get; set; }
        public static TextMeshProUGUI? ActionsTextBox { get; set; }


        private static void WriteLine(TextMeshProUGUI textBox, object? text = null)
        {
            textBox.text += text?.ToString() + "\n";
        }
        private static void Write(TextMeshProUGUI textBox,object text)
        {
            textBox.text += text.ToString();
        }

        public static void Clear()
        {
            if (GameStateTextBox == null || PlayerStatesTextBox == null || ActionsTextBox == null) {
                Debug.LogError("One or more UI elements are not assigned");
                return;
            }
            GameStateTextBox.text = "";
            PlayerStatesTextBox.text = "";
            ActionsTextBox.text = "";
        }

        public static void PrintTurnInfo(Player currentPlayer, TurnInfo turnInfo)
        {
            // check if new round
            if (currentPlayer.Id == FirstPlayerId && turnInfo.NumActionsLeft == TurnManager.NumActionsInTurn - 1) {
                RoundCount++;
            }
            // print turn info
            WriteLine(GameStateTextBox!, $"Round: {RoundCount}, Current player: {currentPlayer.Name} ({currentPlayer.GetType().Name}), Action: {3 - turnInfo.NumActionsLeft}");
            WriteLine(GameStateTextBox!, $"TurnInfo: GamePhase={turnInfo.GamePhase}, LastRound={turnInfo.LastRound}, TookBlackPuzzle={turnInfo.TookBlackPuzzle}, UsedMaster={turnInfo.UsedMasterAction}");
        }

        public static void PrintFinishedPuzzleInfo(FinishedPuzzleInfo puzzleInfo, Player currentPlayer)
        {
            WriteLine(ActionsTextBox!, $"{currentPlayer.Name} completed puzzle with ID={puzzleInfo.Puzzle.Id}");
            WriteLine(ActionsTextBox!, $"   Returned pieces: {GetUsedTetrominos()}");
            WriteLine(ActionsTextBox!, $"   Reward: {puzzleInfo.SelectedReward}");
            WriteLine(ActionsTextBox!, $"   Points: {puzzleInfo.Puzzle.RewardScore}");

            string GetUsedTetrominos()
            {
                StringBuilder sb = new StringBuilder();
                int[] tetrominos = new int[TetrominoManager.NumShapes];
                foreach (var tetromino in puzzleInfo.Puzzle.GetUsedTetrominos()) {
                    tetrominos[(int)tetromino]++;
                }

                bool first = true;
                for (int i = 0; i < tetrominos.Length; i++) {
                    if (tetrominos[i] == 0) {
                        continue;
                    }
                    if (first) {
                        first = false;
                    }
                    else {
                        sb.Append(", ");
                    }
                    sb.Append($"{(TetrominoShape)i}: {tetrominos[i]}");
                }
                return sb.ToString();
            }
        }

        public static void PrintGameScreen(GameInfo gameInfo, PlayerInfo[] playerInfos, GameCore game)
        {
            WriteLine(GameStateTextBox!, SmallSeparator);
            Write(GameStateTextBox!, gameInfo);
            foreach (var playerInfo in playerInfos) {
                Player player = game.GetPlayerWithId(playerInfo.PlayerId);
                WriteLine(PlayerStatesTextBox!, PlayerSeparator);
                Write(PlayerStatesTextBox!, $"{player.Name}. ");
                Write(PlayerStatesTextBox!, playerInfo);
            }
            WriteLine(ActionsTextBox!, LargeSeparator);
            WriteLine(ActionsTextBox!);
        }

        public static void PrintPlayerProvidedNoAction(Player player, string message)
        {
            WriteLine(ActionsTextBox!, $"{player.Name} failed to provide an action with error: {message}.\nSkipping action...");
        }

        public static void PrintPlayerProvidedInvalidAction(IAction action, VerificationFailure fail, Player player)
        {
            WriteLine(ActionsTextBox!, $"{player.Name} provided an invalid {action.GetType()}. Verification result:\n{fail.GetType()}: {fail.Message}\n");

        }

        public static void PrintPlayerProvidedValidAction(IAction action, Player player)
        {
            WriteLine(ActionsTextBox!, $"{player.Name} used a {action}\n");
        }

    }
}
