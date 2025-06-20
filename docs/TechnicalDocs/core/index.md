<link rel='stylesheet' href='../../css/code-highlight.css'/>
<link rel='stylesheet' href='../../css/markdown-alert.css'/>

# Core Library Documentation

The game is played by both human and AI players, but the human players choose actions and interact with the game using a GUI, which isn't the focus of this document. This document only discusses the inner workings of the [Project L Core](../../ProjectLCoreDocs/index) library. The Unity side of things is documented [here](../unity/index).
As a result we will be mostly focusing on the AI player point of view.

{% include important.html content="
Before reading this document, please make sure that you have read the rules of the game. The [rulebook](../../UserDocs/rulebook.pdf) can be found on the official [Boardcubator](https://www.boardcubator.com/games/project-l/) website. You should know the rules of the **Project L BASE GAME**.
"%}

### Outline

We will first analyze which aspects of _Project L_ might be challenging from the object oriented programming perspective.

- [Humans vs AI Players](#humans-vs-ai-players)
- [Shared Resources](#shared-resources)
- [Individual Resources](#individual-resources)
- [Game Phases](#game-phases)
- [Actions](#actions)
- [Rewards](#rewards)
- [Pieces](#pieces)
- [Puzzles](#puzzles)

With these considerations in mind, we will then take a look at a _Project L_ game loop written with the final game engine.

- [Showcase of the Game Engine](#showcase-of-the-game-engine)

And after that, we will discuss in detail the inner workings of it all, different design options and motivation behind the decisions made.

- [Puzzles & Pieces](#puzzles--pieces-solution)
- [Shared Resources](#shared-resources-solution)
- [Individual Resource](#individual-resource-solution)
- [Game Phases](#game-phases-solution)
- [Rewards](#rewards-solution)
- [Actions](#actions-solution)
- [Humans vs AI Players](#humans-vs-ai-players-solution)
- [Tying It All Together](#tying-it-all-together)

## Humans vs AI Players

The game needs to be played by both human and AI players, so we should design an interface which can be used by both. As a result the game engine will not distinguish between them. But it will need to be designed in such a way that the AI players can easily interact with it and have the necessary information to make decisions.

#### Challenges:

- How do we represent the players?
- What information do the AI players need to make decisions?
- What interface should a player implement to interact with the game engine?

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

## Individual Resources

Each player has their own resources which can be viewed by everyone, but only modified by the player who owns them:

- their own pieces,
- their unfinished puzzles,
- their finished puzzles.

We will call the individual resources of a single player his _player state_.

#### Challenges:

- The same as for shared resources.

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

## Rewards

Players are rewarded for finishing puzzles by getting points and new pieces. This usually isn't very interesting, because each puzzle has only one piece reward. But if this piece isn't available in the shared reserve, the player can choose from a collection of pieces as described in the rules. This means, that when we are processing a _place piece_ action, and it finishes a puzzle, we need a way to prompt the player to choose a reward.

## Pieces

There are nine different pieces of pieces in the game. They can be rotated and flipped, so each piece has a bunch of different configurations, but they all have the same shape. To verify the actions we will need a way to generate all possible configurations of a piece. The AI players should also have access to this information.

#### Challenges:

- How do we represent the pieces?
- How do we represent the configurations of a piece?

## Puzzles

The puzzles are 5x5 grids with filled and empty cells and the players are trying to fill them with pieces. We need a way to check if a given configuration of a piece can be placed into a puzzle. We also need to remember which pieces have been placed into a puzzle, because they are returned to the player when the puzzle is finished.

Note that there is quite a large number of puzzles in the board game (32 white and 20 black), so it doesn't make sense to hard code them into the game engine. Instead we will provide a way to load them from a file. This approach also has the following added benefit. The GUI needs a graphic for each puzzle, but the game engine only needs the simple binary representation of the puzzle cells. This means that its easy to create additional puzzles to train AI players if needed.

#### Challenges:

- How do we represent the puzzles?
- How do we check if a piece can be placed into a puzzle?
- How do we remember which pieces have been placed into a puzzle?
- In what format do we store the puzzles?

## Showcase of the Game Engine

With all these considerations in mind, we can take a look at the game engine. Every game loop will contain the following steps:

1. Load puzzles from a file
2. Create the players
3. Initialize AI players
4. Create the game core and initialize it
5. Game loop
   - Get the next turn and check if the game ended
   - Create read-only views of the game state and player states
   - Get action from the player (passing the read-only info)
   - Verify the action and process it if it is valid
6. Get final results

```c#
// load puzzles from file
var gameState = GameState.CreateFromFile("puzzles.txt");

// create players
Player[] players = { new HumanPlayer(), new MinMaxAIPlayer(), new RandomAIPlayer() };

// initialize AI players
foreach (var player in players) {
    if (player is AIPlayerBase aiPlayer) {
        aiPlayer.InitAsync(
            players.Length,
            gameState.GetAllPuzzlesInGame(),
            "path/to/ai/player/config"
        );
    }
}

// create game core and initialize it
var game = new GameCore(gameState, players);
game.InitializeGame();

// game loop
while (true) {
    // get next turn and if game ended, break
    var turnInfo = game.GetNextTurnInfo();
    if (game.CurrentGamePhase == GamePhase.Finished) {
        break;
    }

    // create read only views of the game state and player states
    var gameInfo = gameState.GetGameInfo();
    var playerInfos = game.GetPlayerInfos();

    // create action verifier
    var currentPlayerInfo = game.PlayerStates[game.CurrentPlayer].GetPlayerInfo();
    var verifier = new ActionVerifier(gameInfo, currentPlayerInfo, turnInfo);

    // get action from the player
    var action = game.CurrentPlayer.GetActionAsync(gameInfo, playerInfos, turnInfo, verifier).Result;

    // verify the action and process it if it is valid
    if (verifier.Verify(action) is VerificationSuccess) {
        game.ProcessAction(action);
    }
}

// get final results
game.FinalizeGame();
var results = game.GetFinalResults();
```

Lets go through what is going on behind the scenes.

## Puzzles & Pieces (solution)

Since every puzzle is a 5x5 grid, we can represent it using a 32 bit integer and manipulate it using simple bitwise operations. Different piece positions on the grid (tetromino configurations) can also be represented this way. The [BinaryImage](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_BinaryImage.htm) struct implements this.

The pieces are represented by the [TetrominoShape](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_TetrominoShape.htm) enum. The class [TetrominoManager](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_TetrominoManager.htm) serves as a proxy between the `TetrominoShape` abstraction and `BinaryImage` configurations.

{% include tip.html content="
Especially the methods [CompareShapeToImage](../../ProjectLCoreDocs/html/M_ProjectLCore_GameManagers_TetrominoManager_CompareShapeToImage.htm) and [GetAllConfigurationsOf](../../ProjectLCoreDocs/html/M_ProjectLCore_GameManagers_TetrominoManager_GetAllConfigurationsOf.htm)(`shape`) might come in handy when implementing your own AI player.
"%}

The puzzles are represented by the [Puzzle](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_Puzzle.htm) which contains the `BinaryImage` of the puzzle and a list of pieces which have been placed into the puzzle. The class also has methods to check if a piece can be placed into the puzzle and to place it.

{% include note.html content="
The `Puzzle` class has two other noteworthy methods. The [GetUsedTetrominos](../../ProjectLCoreDocs/html/M_ProjectLCore_GamePieces_Puzzle_GetUsedTetrominos.htm) method is called when the puzzle is finished and the pieces are returned to the player. The [Clone](../../ProjectLCoreDocs/html/M_ProjectLCore_GamePieces_Puzzle_Clone.htm) method returns a deep copy of the puzzle. This is used when creating a representation the game, which can safely be passed to an AI player, without the risk of it modifying the actual game state.
"%}

The puzzles are loaded from a file using the [PuzzleParser](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PuzzleParser_1.htm) class. The puzzles are stored in a simple text format which is easy to read and write. For more details on the format, please refer to the [documentations](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PuzzleParser_1.htm).

## Shared Resources (solution)

The game state is represented by the [GameState](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameState.htm) class. It remembers the puzzle rows, puzzle decks and the shared tetromino reserve. It has a simple API for viewing and modifying these resources, which is used when validating and processing actions.

{% include tip.html content="
The `GameState` can be easily initialized from a puzzle file with the [CreateFromFile](../../ProjectLCoreDocs/html/M_ProjectLCore_GameLogic_GameState_CreateFromFile__1.htm) method, which uses a [PuzzleParser](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PuzzleParser_1.htm) and a [GameStateBuilder](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameStateBuilder.htm).
" %}

As we have mentioned previously, we need a means to represent the game state in a way that the AI players can interact with it, without a risk of modifying the underlying data. We can get such a representation using the [GetGameInfo](../../ProjectLCoreDocs/html/M_ProjectLCore_GameLogic_GameState_GetGameInfo.htm) method, which returns a [GameInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameState_GameInfo.htm) object. It provides all necessary information an AI player might need and nothing more, while preventing any modifications.

{% include note.html content="
This is done by a combination of copies, deep copies and read-only views of the underlying `GameState` data.
"%}

## Individual Resource (solution)

The player state is represented by the [PlayerState](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PlayerState.htm) class. It remembers the player's score, the tetrominos he has, the puzzles he is working on, and the puzzles he has finished. Similarly to `GameState`, it also has a simple API to modify these resources and a method to create a [PlayerInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PlayerState_PlayerInfo.htm) object, which is similar in nature to `GameInfo`.

{% include note.html content="
The `PlayerState` class also implements the `IComparable` interface to allow sorting the players by their score, finished puzzles and left-over pieces. This is used when calculating the final results.
"%}

## Game Phases (solution)

The game phase is represented by a simple [GamePhase](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GamePhase.htm) enum. Managing the current game phase and transitioning between phases is the job of the [TurnManager](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_TurnManager.htm) class. The other job it has is keeping track of who the current player is. It contains the [GetNextTurn](../../ProjectLCoreDocs/html/M_ProjectLCore_GameManagers_TurnManager_GetNextTurn.htm) method which adjusts the internal turn state and return a [TurnInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_TurnInfo.htm) object, which contains information about the current turn, such as the number of actions the current player has left in their turn or the current game phase.

{% include note.html content="
The `TurnInfo` object also contains some additional information needed to determine the validity of actions in some cases. For example, it remembers if the current player has already used the Master action in this turn. Recall that the Master action can be used only once per turn.
"%}

You might wander, how does the `TurnManager` get the information that the master action has been used? The answer lies in the [TurnManager.Signaler](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_TurnManager_Signaler.htm) class. When an action processor is created, it is passed a `Signaler` for the `TurnManager` and when a Master action is processed, the action processor will inform the `TurnManager` by calling the appropriate function on the `Signaler`.

## Rewards (solution)

Reward information is provided by the [RewardManager](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_RewardManager.htm) static class, which provides two useful methods. The first one is [GetRewardOptions](../../ProjectLCoreDocs/html/M_ProjectLCore_GameManagers_RewardManager_GetRewardOptions.htm) which returns a list of all possible rewards for a given puzzle in the current game context. This is used for rewarding players after completing a puzzle.

The second one is [GetUpgradeOptions](../../ProjectLCoreDocs/html/M_ProjectLCore_GameManagers_RewardManager_GetUpgradeOptions.htm) which for a given `TetrominoShape` returns a list of all shapes the player can get in exchange for the given shape by using a `ChangeTetrominoAction`. This is used for action verification, but it can also be very useful for AI players.

## Actions (solution)

The actions are represented by the [GameAction](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_GameAction.htm) abstract class. Together with the [IActionProcessor](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_IActionProcessor.htm) interface, it implements the visitor pattern for processing actions. The idea is that in this way it is easy to add new action processors, e.g. for modifying the graphics in the Unity game.

{% include tip.html content="
For details about the individual actions, see the game action
[namespace](../../ProjectLCoreDocs/html/N_ProjectLCore_GameActions.htm).
"%}

The validity of an action in the current game context can be checked using an [ActionVerifier](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_Verification_ActionVerifier.htm) and its `Verify` method. The verifier is constructed with regard to the current game context, meaning that it is given the current `GameInfo`, `PlayerInfo` of each player and `TurnInfo` of the current turn. The [Verify](../../ProjectLCoreDocs/html/M_ProjectLCore_GameActions_Verification_ActionVerifier_Verify.htm)(`action`) method returns a [VerificationResult](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_Verification_VerificationResult.htm), which can either be a success or a failure.

{% include tip.html content="
For more details about verification results see the [VerificationFailure](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_Verification_VerificationFailure.htm) abstract class and its derived classes, which can be found in the action verification [namespace](../../ProjectLCoreDocs/html/N_ProjectLCore_GameActions_Verification.htm). The derived classes contain the information about the reason why the action is invalid. This can be used by the AI player (and its programmer) to debug its decision making process.
"%}

After a successful verification, the action can be processed using the [GameActionProcessor](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_GameActionProcessor.htm), which implements the `IActionProcessor` interface and modifies the `GameState` and `PlayerState` objects accordingly. It also has a queue for storing information about completed puzzles.

{% include important.html content="
The `GameActionProcessor` doesn't check action validity, so it should only be called after a successful verification. It is the responsibility of the caller to ensure that the action is valid.
"%}

A `GameActionProcessor` needs to be created for every player.

{% include note.html content="
If an action processor would be universal (remember all player states), then it would need a way to identify the correct player from the given action. So either the action would need to contain the ID of the player, or the action processor would need to have a reference to some other object which could tell it who the current player is. <br/>Neither of these solutions are elegant in my opinion, so I chose to create a separate action processor for each player.
"%}

If a `PlaceTetrominoAction` completes a puzzle, the player needs to be rewarded. The `GameActionProcessor` will get a list of all possible rewards (usually only one) from the [RewardManager](../../ProjectLCoreDocs/html/T_ProjectLCore_GameManagers_RewardManager.htm), and the player will be prompted to choose one by calling the `Player.GetRewardAsync` method. Subsequently, a [FinishedPuzzleInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GamePieces_FinishedPuzzleInfo.htm) object containing information about the possible rewards and the selected reward will be added to the [FinishedPuzzlesQueue](../../ProjectLCoreDocs/html/P_ProjectLCore_GameActions_GameActionProcessor_FinishedPuzzlesQueue.htm).

{% include note.html content="
The `GameActionProcessor.FinishedPuzzlesQueue` of a player contains information about puzzles the player finished. Users of the library can process them how they please or ignore them entirely. They aren't needed for the logic of the game but might be useful for debugging or other similar purposes.
"%}

## Humans vs AI Players (solution)

We represent players with the abstract class [Player](../../ProjectLCoreDocs/html/T_ProjectLCore_Players_Player.htm). It contains the [GetActionAsync](../../ProjectLCoreDocs/html/M_ProjectLCore_Players_Player_GetActionAsync.htm) method which is called by the game engine to get the action from the player. The method takes in a safe-to-modify wrapper of the current game context (using [GameInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameState_GameInfo.htm) and [PlayerInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PlayerState_PlayerInfo.htm) objects), the current [TurnInfo](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_TurnInfo.htm) and an [ActionVerifier](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_Verification_ActionVerifier.htm) for AI players to debug their decision making process.

{% include note.html content="
The [Player](../../ProjectLCoreDocs/html/T_ProjectLCore_Players_Player.htm) also has a [GetRewardAsync](../../ProjectLCoreDocs/html/M_ProjectLCore_Players_Player_GetRewardAsync.htm) method, which is called by a [GameActionProcessor](../../ProjectLCoreDocs/html/T_ProjectLCore_GameActions_GameActionProcessor.htm) to get the reward for the player when he finishes a puzzle and there is more then one reward option.
"%}

These methods are asynchronous, so that they don't block the main thread and therefore make the game unresponsive. Every AI player inherits from the abstract class [AIPlayerBase](../../ProjectLCoreDocs/html/T_ProjectLCore_Players_AIPlayerBase.htm) and must implement the `GetAction` and `GetReward` methods. The asynchronous methods are implemented in the base class and asynchronously call the synchronous methods.

AI players also have to implement the [Init](../../ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_Init.htm) method which is called by the game engine at the start of the game. The AI player gets information about the number of players in the game and a list of all puzzles in the game. There is also an option to pass a path to a configuration file. This is useful for AI players which need to load some data from a file.

{% include tip.html content="
The puzzle information might also prove useful. Among the information the player gets when `GetAction` is called, there is a list of finished puzzles for every player. This can be used to deduce which puzzles are still left in the puzzle decks.
"%}

{% include note.html content="
The `AIPlayerBase` abstract class also implements the `InitAsync` method, which asynchronously calls the user implemented `Init` method. All of the non-async methods (`Init`, `GetAction`, `GetReward`) are protected, meaning that only their async counterparts can be called from outside.
"%}

## Tying It All Together

It all comes together in the [GameCore](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameCore.htm) class which provides high-level abstraction of the entire game. It remembers the `GameState`, `Players` and their `PlayerStates`. It also has a `TurnManager` and `ActionProcessors` for each player. The API of the `GameCore` class is simple and easy to use, as seen in the [game loop example](#showcase-of-the-game-engine).

{% include note.html content="
For more details about the `GameCore` API, see the [GameCore](../../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameCore.htm) class documentation.
"%}

## Where to Go From Here?

If you are interested in implementing your own AI player, I suggest you take a look at the [guide](../../AIPlayerGuide/index) for doing so.

If you are interested in the graphical side of things, you can take a look at the [Unity part](../unity/index) of the technical documentation.
