<link rel='stylesheet' href='../css/markdown-alert.css'/>

# Functional Specification

## Overview

This project is a Unity implementation of the **Project L** board game published by [Boardcubator](https://boardcubator.com/). It is a strategy game designed for 2-4 players, in which the players solve puzzles and collect tetrominos. You can either play against your friends or you can create your own AI players and play against them. Or both!

**This spec is not, by any stretch of the imagination, complete.** All of the wording will need to be revised several times before it is finalized. The graphics and layout of the screens is shown here merely to illustrate the underlying functionality. The actual look will be added once the game is finished.

This spec doesn't discuss the technical side of things (how it works). If you are interested in that, please check the [Technical Documentation](../TechnicalDocs/core/index).

## Scenarios

Alice and her friend Charlie want to try a new strategic game and so Alice decides to fire up her newest discovery - _Project L_. After reading the [user guide](../UserDocs/index), they are ready to play. They simply create a new game, add two players named Alice and Charlie and soon they are solving puzzles and building their tetromino piece collection.

The next day, Alice decides to also invite her friend Karel to play with them. Karel is studying computer science, so when Alice starts creating a new game, he gets immediately interested by the option of adding AI players to the game. During the weekend, he reads the AI player creation [guide](../AIPlayerGuide/index), implements the necessary interface, exports his project as a DLL and now he is ready to play against his own creation.

## Non Goals

This project will _not_ support the following features:

- online play
- saving and loading of unfinished games
- any of the expansions of the original game

## Screen by Screen Specification

The game consists of a couple of different screens which Alice and Charlie will encounter when playing the game. First they will see the [<u>Start Screen</u>](#start-screen), where they can create a new game or view the user guide. After that, they will be taken to the [<u>New Game</u>](#new-game) screen, where they can add players and set up the game. Once they are ready, they will be taken to the [<u>Main Game</u>](#main-game) screen, where they will play the game. Finally, when the game is over, they will be taken to the [<u>End Screen</u>](#end-screen), where they can see the results of the game and buy the winner a nice trophy.

Screens are referred to by their canonical names, always underlined in this document, e.g. <u>Start Screen</u>.

## Start Screen

The first thing Alice and Charlie will see when they start the game is the following:

![Start Screen](images/start-screen.png)

The <u>Start Screen</u> contains the buttons:

- **NEW GAME** - takes you to the [<u>New Game</u>](#new-game) screen
- **USER GUIDE** - opens the [User Guide](../UserDocs/index) in your browser
- **CREDITS** - takes you to the [<u>Credits</u>](#credits) screen

## Credits

After trying out what the **CREDITS** button in the [<u>Start Screen</u>](#start-screen) does, Alice and Charlie ended up here:

![Credits](images/credits.png)

They don't want to read a novel but to play a game and so they click on the arrow in the bottom left corner to get back to the [<u>Start Screen</u>](#start-screen).

## New Game

Karel created his own AI player which uses the NEAT algorithm to decide the next action. After clicking the **NEW GAME** button in the [<u>Start Screen</u>](#start-screen) he was taken here.

![New Game](images/new-game.png)

Karel added his NEAT player to the Json defining available AI players, as specified in the [AI Player Guide](../AIPlayerGuide/index). He wants to test if it works correctly by playing a game against it.

He clicks on the dropdown menu for the type of the first player, selects "Human" and enters his name. From the dropdown menu for the second player he chooses "NEAT" and come ups with a name for it.

He then uses the slider below to set the initial amount of pieces of each type in the shared reserve and decides he wants to shuffle the players to make things more interesting.

After that he just hits "Start game" and is taken to the [<u>Main Game</u>](#main-game) screen.

If Alice had a change of heart and wanted to read the credits, she could click on the arrow in the bottom left corner to go back to the [<u>Start Screen</u>](#start-screen).

### Player selection in detail

The game can can be played by a maximum of 4 players and a minimum of 1 player (it might not be very interesting though). To add a player, Karel needs to select a type from the dropdown menu and add a name.

{% include note.html content="
The AI players are defined in a Json file, for details see the [AI Player Guide](../AIPlayerGuide/index).
"%}

{% include important.html content="
Only the entries containing a valid AI player will be shown in the dropdown menu.
"%}

Once at least one parameter for a player is set, the reset button will appear to the right of their name. The player information can then be cleared by clicking on it. The game cannot be started until at least one player is added. If a player is only partially defined (e.g. only a name is set), the game cannot be started either.

The number of starting pieces in the shared reserve is 15 by default (the value in the original game), but can be adjusted using a slider below the player selection. The minimum number of pieces is 10, the maximum is 30 and the tick interval is 5.

Players are shuffled by default to ensure random order of play. This can be turned off by unchecking the checkbox below the player selection. In that case, the order of play is specified by the numbers left of the players in the [<u>New Game</u>](#new-game) screen.

## Main Game

After Karel is done creating the game for him, Alice and his NEAT algorithm AI player, he clicks on the **Start game** button and is taken to the <u>Main Game</u> screen. After playing for a while, he takes a screenshot of the game to show it to his friends.

![Main Game](images/main-game.png)

{% include important.html content="
The following text assumes that you have read the rules of the **Project L BASE GAME**. If you haven't, go read them [here](../UserDocs/rulebook.pdf).
"%}

The <u>Main Game</u> screen is subdivided into multiple subsections.

![Main Game Zones](images/main-game-zones.png)

- <u>Player Zone</u> - consists of the unfinished puzzles of each player
  - there is a row of 4 puzzles / blanks on the right of each player's name
  - e.g. Karel has 2 unfinished puzzles and 2 blanks
  - the first player has the first row, the player who plays second has the second row, etc.
  - the name of the current player (Karel) is **highlighted**
- <u>Control Zone</u> - the area above the <u>Player Zone</u> where "Cancel", "Last round!" and "Confirm" are on the previous image
  - used for displaying critical information about the game and for confirming or cancelling actions
- <u>Piece Zone</u> - contains information about the number of pieces owned by each player and the number of pieces left in the shared reserve
  - on the left, there is column of all piece types in the game (`O1`, `I2`, `L2`, `O2`, `I3`, `Z`, `T`, `L3`, `I4` in order from top to bottom)
  - then there is one column for each player (NEAT, Karel and Alice), which says how many pieces of each type they have
    - above each column, there is the first letter of the player's name (N, K and A)
    - the first player has the first column, the second player who plays second has the second column, etc.
    - the column of the current player (Karel) is **highlighted**
  - lastly, there is a column with the number of pieces left in the shared reserve
  - e.g. NEAT has 3 `L2` pieces, Karel has 1, Alice has 0 and there are 8 left in the shared reserve
- <u>Score Zone</u> - the area below <u>Piece Zone</u> where the score of each player is displayed
  - in this case, Karel has 7 points, NEAT has 9 and Alice has 14
- <u>Puzzle Zone</u> - consists of the following:
  - column of white puzzles which can be taken by the current player
    - the <u>White Deck Card</u> located below this column indicates the number of puzzles left in the white deck - 25 in this case
  - column of black puzzles and the <u>Black Deck Card</u> indicating the number of puzzles left in the back deck
- <u>Action Zone</u> - the area above the <u>Puzzle Zone</u>, which contains
  - the <u>Action Number</u> (in orange), indicating how many actions the current player has left in this turn - 2 in this case
  - buttons for using the different actions; from left to right they are:
    - <u>Take Puzzle</u> - used for taking a puzzle from the <u>Puzzle Zone</u>
    - <u>Recycle</u> - recycles the black or white column in the <u>Puzzle Zone</u>
    - <u>Take Basic Piece</u> - gives the player the `O1` piece from the shared reserve
    - <u>Upgrade</u> - changes one piece for another
    - <u>Master</u> - parallel place action

{% include tip.html content="
The name and piece column of the current player are **highlighted**.
"%}

{% include tip.html content="
The number on the <u>White Deck Card</u> is white and the number on the <u>Black Deck Card</u> is black."%}

{% include tip.html content="
The <u>Action Number</u> changes color depending on the number of actions left for the current player: green (3), orange (2), red (1)."%}

### Creating Actions

When Alice first enters the <u>Main Game</u> screen, she has no puzzles, so for her first action she decides to take a puzzle from the <u>Puzzle Zone</u>. She clicks on <u>Take Puzzle</u>, selects a puzzle by clicking on it, confirms her selection in the <u>Control Zone</u> and the puzzle appears in her row in the <u>Player Zone</u>.

Now she would like to complete the puzzle to get the reward. Thankfully, every player gets two pieces (`O1` and `I2`) at the start of the game. She decides to place the `I2` piece, so she drags it onto the puzzle using her mouse. Then she rotates the piece with the mouse wheel and once she's happy with the position, she clicks to lock it in place. After contemplating for a bit, she changes her mind about the placement, so she drags the piece to a new position, locks it in place and confirms her action in the <u>Control Zone</u>.

#### Creating Actions in Detail

The general process of creating an action is as follows:

1. Start dragging a piece from the <u>Piece Zone</u> to use the _Place action_ or click the appropriate button in the <u>Action Zone</u> to use any of the other actions.
2. The **Cancel** button will appear in the <u>Control Zone</u>, clicking on it will return the game to its state before the action was started.
3. After a valid action is created, the **Confirm** button will appear in the <u>Control Zone</u>. Clicking on it will confirm the action and the game will be updated accordingly. The number of actions left for the current player decrease by 1. The action can also be confirmed by pressing the **Enter** key on your keyboard.

{% include note.html content="
The **Cancel** button is visible if and only if an action is being created.<br/>The **Confirm** button is visible if and only if the action is valid.
"%}

#### Place Piece

#### Take Puzzle

#### Recycle

#### Take Basic Piece

#### Upgrade

#### Master

#### End Finishing Touches

## End Screen
