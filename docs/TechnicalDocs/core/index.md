<link rel='stylesheet' href='code_highlight.css'/>

# Technical Documentation

The game is played by both human and AI players, but the human players choose actions and interact with the game using a GUI, which isn't the focus of this document. This document only discusses the inner workings of the [Project L Core](../../ProjectLCoreDocs/index) library. The graphical side of things is documented [here](../unity/index).
As a result we will be mostly focusing on the AI players point of view.

### Prerequisites

Before reading this document, please make sure that you have read the rules of the game. The [rulebook](../../UserDocs/rulebook.pdf) can be found on the official boardcubator [website](https://www.boardcubator.com/games/project-l/). You should know the rules of the **Project L BASE GAME**. This project doesn't implement the solo variant or any of the expansions.

### Outline

Lets first analyze which aspects of Project L might be challenging from the object oriented programming perspective. For each topic we will

- shortly introduce the problem and hint some implications
- state the related challenges
- list the objects (classes/structs/enums) which solve these challenges

Then we will take a look at the pseudo code of a simple Project L game engine. We will realize what is wrong with it and how we can fix it, which will lead to a more sophisticated approach. We will then compare this improved pseudo code with the actual code of the game engine.

After that, we will discuss in detail the inner workings of it all, different design options and motivation behind the decisions made.

## Table of Contents

#### Design topics

- [Humans vs AI Players](#humans-vs-ai-players)
- [Shared Resources](#shared-resources)
- [Individual Resources](#individual-resources)
- [Game Phases](#game-phases)
- [Actions](#actions)
- [Rewards](#rewards)
- [Pieces](#pieces)
- [Puzzles](#puzzles)

#### Solution overview

- [Game Engine Pseudocode]
- [Actual Code]

## Humans vs AI Players

The game needs to be played by both human and AI players, so we should design an interface which can be used by both. As a result the game engine will not distinguish between them. But it will need to be designed in such a way that the AI players can easily interact with it and have the necessary information to make decisions.

#### Challenges:

- How do we represent the players?
- What information do the AI players need to make decisions?
- What interface should a player implement to interact with the game engine?

#### Related objects:

- [Player](../../ProjectLCoreDocs/html/T_ProjectLCore_Players_Player.htm), [HumanPlayer](../../ProjectLCoreDocs/html/T_ProjectLCore_Players_HumanPlayer.htm), [AIPlayerBase](../../ProjectLCoreDocs/html/T_ProjectLCore_Players_AIPlayerBase.htm)

## Shared Resources

Each player can view and modify (by using the appropriate action) the following shared resources:

- shared reserve of pieces,
- white puzzles row and black puzzles row,
- white puzzle deck and black puzzle deck.

We will call the shared resources the _game state_.

#### Challenges:

- How do we represent them?
- Where do we remember them?
- How do we modify them?
- Who can modify them?

#### Related objects:

- [GameState](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameState.htm), [GameState.GameInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameState_GameInfo.htm)

## Individual Resources

Each player has their own resources which can be viewed by everyone, but only modified by the player who owns them:

- their own pieces,
- their unfinished puzzles,
- their finished puzzles.

We will call the individual resources of a single player his _player state_.

#### Challenges:

- The same as for shared resources.

#### Related objects:

- [PlayerState](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PlayerState.htm), [PlayerState.PlayerInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PlayerState_PlayerInfo.htm)

## Game Phases

Every game of Project L goes through several phases from the start to the end. The phases are:

- setup - players are given starting pieces and puzzle rows are filled
- normal - players take turns taking actions
- end - triggers when the black puzzle deck is emptied
  - players finish the current round and then play one last round
- finishing touches
- scoring - players calculate their scores

#### Challenges:

- Where do we remember the current phase?
- How do we transition between phases?
- How do we give (AI) players information about the current phase?

#### Related objects:

- [GamePhase](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GamePhase.htm), [TurnInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_TurnInfo.htm), [TurnManager](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_TurnManager.htm), [TurnManager.Signaler](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_TurnManager_Signaler.htm)

## Actions

Players take turns taking actions which modify the _game state_ and the _player state_ of the player who took the action. Since we want the game to be played by both human and AI players, we need to represent the actions in such a way that:

- Human players can easily create the action using the GUI.
- AI players can deduce all valid action from the current _game state_ and _player states_.

This also means that the players cannot have a means of cheating by modifying the _game state_ or _player states_ directly. The player will be prompted to provide an action and he will be provided with a read-only view of the _game state_ and his _player states_. The action will be validated by the game engine and if it is valid, it will be executed. There is a decision to make here:

1. The player needs to submit a valid action and will be prompted to do so until he does.
2. The player can submit an invalid action and the game engine will skip it.

I chose the second option because the AI player might get stuck in a loop where it cannot find a valid action. This is of course an error in the AI player implementation, but it should not stop the game from progressing, especially if it will be played by some humans as well. The fact that the person who implemented the AI player made a mistake should not make the human players unable to progress in the game.

Instead we will provide the AI player with a way to debug its decision making process. He will be provided a validator which he can use to verify if an action is valid or not. If it is invalid, the verifier will provide a reason why it is invalid. If the AI player is still failing to find a valid action, he can submit a _do nothing action_ and the game will progress.

Note that this way the AI player can still get stuck in a loop, but it will not be an error in the game engine, but in the AI player implementation.

#### Challenges:

- How do we represent different actions?
- How do we validate them?
- How do we process them?
- How do we provide the AI player with a way to debug its decision making process?

Also note that different actions can be taken during different phases of the game. They also might have different side effects. For example the _place piece to puzzle_ action.

#### Related objects:

- [IAction](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_IAction.htm), [ActionVerifier](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_Verification_ActionVerifier.htm), [GameActionProcessor](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_GameActionProcessor.htm)

## Rewards

Players are rewarded for finishing puzzles by getting points and new pieces. This usually isn't very interesting, because each puzzle has only one piece reward. But if this piece isn't available in the shared reserve, the player can choose from a collection of pieces as described in the rules. This means, that when we are processing a _place piece_ action, and it finishes a puzzle, we need a way to prompt the player to choose a reward.

#### Related objects:

- [RewardManager](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_RewardManager.htm)

## Pieces

There are nine different pieces of pieces in the game. They can be rotated and flipped, so each piece has a bunch of different configurations, but they all have the same shape. To verify the actions we will need a way to generate all possible configurations of a piece. The AI players should also have access to this information.

#### Challenges:

- How do we represent the pieces?
- How do we represent the configurations of a piece?

#### Related objects:

- [TetrominoShape](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_TetrominoShape.htm), [BinaryImage](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_BinaryImage.htm), [TetrominoManager](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_TetrominoManager.htm)

## Puzzles

The puzzles are 5x5 grids with filled and empty cells and the players are trying to fill them with pieces. We need a way to check if a given configuration of a piece can be placed into a puzzle. We also need to remember which pieces have been placed into a puzzle, because they are returned to the player when the puzzle is finished.

Note that there is quite a large number of puzzles in the board game (32 white and 20 black), so it doesn't make sense to hard code them into the game engine. Instead we will provide a way to load them from a file. This approach also has the following added benefit. The GUI needs a graphic for each puzzle, but the game engine only needs the simple binary representation of the puzzle cells. This means that its easy to create additional puzzles to train AI players if needed.

#### Challenges:

- How do we represent the puzzles?
- How do we check if a piece can be placed into a puzzle?
- How do we remember which pieces have been placed into a puzzle?
- In what format do we store the puzzles?

#### Related objects

- [Puzzle](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_Puzzle.htm), [BinaryImage](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_BinaryImage.htm), [PuzzleParser](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PuzzleParser.htm)

## Simple Game Engine Pseudocode

With the topics discussed above in mind, lets image how could a simple Project L game engine look like in pseudocode.

```
gameState <- CreateGameStateFromFile(puzzles.txt)
players <- CreatePlayers()
playerStates <- CreatePlayerStates()

actionProcessor <- CreateActionProcessor()
actionVerifier <- GetActionVerifier()

InitializeGame(gameState, playerStates)

While (game has not ended) {
    currentPlayer <- GetCurrentPlayer()
    action <- currentPlayer.GetAction(gameState, playerStates, actionVerifier)

    if (action is valid) {
        actionProcessor.ProcessAction(action)
    }
}

CalculateScores(playerStates)
```

```C#
// load puzzles from file
GameState gameState = GameState.CreateFromFile("puzzles.txt");

// create players
Player[] players = { new HumanPlayer(), new MinMaxAIPlayer(), new RandomAIPlayer() };

// initialize AI players
foreach (var player in players) {
    if (player is AIPlayerBase aiPlayer) {
        aiPlayer.Init(players.Length, ["path/to/ai/player/config"]);
    }
}

// create game core and signaller
var game = new GameCore(gameState, players, shufflePlayers: false);
var signaler = game.TurnManager.GetSignaler();

// create an action processor for each player
var actionProcessors = new Dictionary<Player, GameActionProcessor>();
foreach (var player in players) {
    actionProcessors[player] = new GameActionProcessor(game, player.Id, signaler);
}

// initialize game - give players starting pieces and fill the puzzle rows
game.InitializeGame();

// game loop
while (true) {
    // get next turn and if game ended, break
    TurnInfo turnInfo = game.GetNextTurnInfo();
    if (game.CurrentGamePhase == GamePhase.Finished) {
        game.GameEnded();
        break;
    }

    // create read only views of the game state and player states
    var gameInfo = gameState.GetGameInfo();
    var playerInfos = game.PlayerStates.Select(playerState => playerState.GetPlayerInfo()).ToArray();

    // create action verifier
    var currentPlayerInfo = game.GetPlayerStateWithId(game.CurrentPlayer.Id).GetPlayerInfo();
    var verifier = new ActionVerifier(gameInfo, currentPlayerInfo, turnInfo);

    // get action from the player
    IAction action = game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;

    // verify the action and process it if it is valid
    var result = verifier.Verify(action);
    if (result is VerificationSuccess) {
        action.Accept(actionProcessors[game.CurrentPlayer.Id]);
    }
}

// get final results
var results = game.GetFinalResults();
```
