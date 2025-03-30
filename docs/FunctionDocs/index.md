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

The game consists of a couple of different screens which Alice and Charlie will encounter when playing the game. First they will see the [<u>Start Screen</u>](#start-screen), where they can create a new game or view the user guide. After that, they will be taken to the [<u>New Game</u>](#new-game) screen, where they can add players and set up the game. Once they are ready, they will be taken to the [<u>Main Game</u>](#main-game) screen, where they will play the game. Finally, when the game is over, they will be taken to the [<u>Final Results</u>](#final-results) screen, where they can see the results of the game and buy the winner a nice trophy.

Screens are referred to by their canonical names, always underlined in this document, e.g. <u>Start Screen</u>.

### Table of Contents

- [Start Screen](#start-screen)
- [Credits](#credits)
- [New Game](#new-game)
- [Main Game](#main-game)
  - [Main Game Zones](#main-game-zones)
  - [Creating Actions](#creating-actions)
  - [Completing Puzzles](#completing-puzzles)
- [End Screen](#end-screen)
- [Final Results](#final-results)
- [Pause Menu](#pause-menu)

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

### Main Game Zones

{% include important.html content="
The following text assumes that you have read the rules of the **Project L BASE GAME**. If you haven't, go read them [here](../UserDocs/rulebook.pdf).
"%}

The <u>Main Game</u> screen is subdivided into multiple subsections as seen in the image below.

![Main Game Zones](images/main-game-zones.png)

#### <u>Player Zone</u>

Contains the unfinished puzzles of each player.

- There is a row of 4 puzzles / blanks on the right of each player's name.
- E.g. Karel has 2 unfinished puzzles and 2 empty slots.
- The first player has the first row, the second player has the second row, etc.
- The name of the current player (Karel) is **highlighted**.

#### <u>Control Zone</u>

Has two purposes:

- displaying critical information about the game,
- confirming and cancelling actions.

#### <u>Piece Zone</u>

Contains information about the number of pieces owned by each player and the number of pieces left in the shared reserve. It consists of several columns which from left to right are:

- A column of all piece types in the game (`O1`, `I2`, `L2`, `O2`, `I3`, `Z`, `T`, `L3`, `I4` in order from top to bottom).
- One column for each player (NEAT, Karel and Alice), which says how many pieces of each type they have.
  - The first letter of each player's name (N, K and A) is displayed above their respective column.
  - The first player has the first column, the second player has the second column, etc.
  - The column of the current player (Karel) is **highlighted**.
- Lastly, there is a column with the number of pieces left in the shared reserve.

For example, NEAT has 3 `L2` pieces, Karel has 1, Alice has 0 and there are 8 left in the shared reserve.

{% include tip.html content="
The name and piece column of the current player are **highlighted**.
"%}

#### <u>Score Zone</u>

Displays the current score of each player and is located bellow the <u>Piece Zone</u>. In this case, Karel has 7 points, NEAT has 9 and Alice has 14.

#### <u>Puzzle Zone</u>

Consists of the following:

- Column of white puzzles which can be taken by the current player.
  - The <u>White Deck Card</u> located below this column indicates the number of puzzles left in the white deck - 25 in this case.
- Column of black puzzles and the <u>Black Deck Card</u> indicating the number of puzzles left in the back deck (12).

{% include tip.html content="
The number on the <u>White Deck Card</u> is white and the number on the <u>Black Deck Card</u> is black."%}

#### <u>Action Zone</u>

Located above the <u>Puzzle Zone</u> and used for creating actions. It contains:

- The <u>Action Number</u> (in orange), indicating how many actions the current player has left in this turn - 2 in this case.
- Buttons for using the different actions. From left to right they are:
  - <u>Take Puzzle</u> - used for taking a puzzle from the <u>PuzzleZone</u>.
  - <u>Recycle</u> - recycles the black or white column in the <u>Puzzle Zone</u>.
  - <u>Take Basic Piece</u> - gives the player the `O1` piece from the shared reserve.
  - <u>Upgrade</u> - changes one piece for another.
  - <u>Master</u> - parallel place action.

When the game comes to the Finishing Touches phase, all of these buttons will disappear and be replaced by the <u>End Finishing Touches</u> button.

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

Drag a piece from the <u>Piece Zone</u> to the <u>Player Zone</u> and place it on a puzzle. The mouse button doesn't need to be pressed while dragging. The piece can be rotated using the mouse wheel and flipped by using the right mouse button. Once you aee happy with the placement, you can left click to lock it in place. This can be done only if the placement is valid (the piece doesn't overlap with other pieces and is inside the puzzle). This is visually indicated in the following way. When the piece is dragged, the cells in the puzzle closest to the position of the piece are **highlighted**. It basically shows where would the piece be placed if you locked its position right now. If it would be a valid placement, the highlight is in the color of the piece.

Once the position of the piece has been locked, it will be **highlighted** to indicate its position. It can then be dragged again and moved to a new place. This can be repeated until the action is _confirmed_.

#### Take Puzzle

To take a puzzle from the <u>Puzzle zone</u> simply click on it. To take the top puzzle from one of the puzzle decks, just click on the corresponding <u>Deck Card</u>.
The selected puzzle/card will be highlighter and you can change your mind until you confirm the action.

After you confirm the action, the puzzle will be placed on the first empty slop in your row in the <u>Player Zone</u>. If you puzzle row is full (you have 4 unfinished puzzles), the <u>Take Puzzle</u> button will be disabled.

{% include note.html content="
If the black deck is empty and the game is coming to an end, you can only take one black puzzle per turn. This means that you will not be able to select a black puzzle if you already took one in the same turn.
"%}

#### Recycle

Chose a row to recycle and then click on the puzzles in the row in the order you want to recycle them. The first puzzle will go to the bottom of the deck first etc. After you have clicked on all the puzzles in the row, you can confirm the action. The puzzles you have clicked on will be **highlighted**. CLicking on a **highlighted** puzzle again will remove it from the selection and shift the recycle order. Once you have clicked on a puzzle of a certain color, you cannot add a puzzle of the other color to the selection.

{% include tip.html content="
If you want to recycle the white row, but accidentally click on a black puzzle, you can either unselect it by clicking on it again, or you can cancel the action and start over.
"%}

#### Take Basic Piece

Simply confirm the action to get the `O1` piece. The button will be disabled if there are no `O1` pieces left in the shared reserve.

#### Upgrade

The process of creating an upgrade action happens in the <u>Piece Zone</u>, is very visual and goes like this:

1. The pieces which you don't own are **grayed out**.
2. To select a piece to upgrade, click on it. The piece will be **highlighted** and all the pieces which you _cannot_ change it to will be **grayed out**.
3. If you now click on the selected piece again, it will unselect and you go back to 1.
4. If you instead choose to click on one of the pieces you can change the selected piece to, it will be **highlighted** as well, and all the leftover pieces will also be **grayed out**.
5. If you are happy with the selection, you can confirm the action now.
6. Otherwise, you can click on the second piece again to unselect it, which will take you back to 2.

{% include tip.html content="
Once you select a piece to upgrade, it's numer in you column will decrease by 1 and appear in red, while the number in the shared reserve will increase by 1 and appear in green. After you have selected a piece to upgrade to, the number in the shared reserve will decrease by 1 and appear in red and the number in your column will increase by 1 and appear in green.
"%}

{% include note.html content="
If you don't have any pieces in your collection, the <u>Upgrade</u> button will be disabled.
"%}

#### Master

You can drag and lock the pieces in the same manner as with the [Place Piece](#place-piece) action. The only difference is that you can put up to one piece into each of your unfinished puzzles.

{% include note.html content="
The *Master* action can be used only once per turn, so the button will be disabled if you have already used it.
"%}

#### End Finishing Touches

During _Finishing Touches_ you can only place pieces into your unfinished puzzles to reduce negative points. If you are satisfied and want to end your _Finishing Touches_ turn, click on the <u>End Finishing Touches</u> button and confirm the action.

{% include note.html content="
The <u>End Finishing Touches</u> button will appear only after the game has reached the *Finishing Touches* phase.
"%}

### Completing Puzzles

After a puzzle is completed (all empty cells are filled in), it will be **highlighted** and the text "Select reward" will appear in the <u>Control Zone</u>. The pieces in the <u>Piece Zone</u> which you _cannot_ take as a reward will be **grayed out**.

{% include note.html content="
You can usually only take the piece specified on the puzzle card as the reward. However, if there are no pieces of this type left in the shared reserve, you get to choose from a collection of pieces as specified in the rules.
"%}

To choose a reward, click on the piece you want to take and it will be **highlighted**. If there are multiple options and you want to change your selection, simply click on a different piece. Once you are happy with your choice, click on the **Confirm** button in the <u>Control Zone</u>.

{% include tip.html content="
When you select a reward, its number in the shared reserve will decrease by 1 and appear in red, while the number in your column will increase by 1 and appear in green.
"%}

After you choose a reward, you will get the points for the puzzle and the pieces used to complete it. It is also removed from your row in the <u>Player Zone</u>.

{% include note.html content="
If you have used the *Master* action and completed more than one puzzle, you will be prompted to choose a reward for each of them, one at a time.
"%}

If an AI player finishes a puzzle, the process of it choosing a reward will be visually indicated in the same way as described above.

In the very unlikely event that there are no pieces left in the shared reserve (and therefore there is no possible reward), you will _not_ be prompted to choose a reward. Instead, the puzzle will be highlighted, you will be returned the used pieces and then it will be automatically removed from your row in the <u>Player Zone</u>.

### Control Zone in Detail

## End Screen

## Final Results

## Pause Menu
