## TODO

- [x] create a new Unity project
- [x] add start screen and credits

### Create Game Creation screen

- [x] add nuget package for parsing INI files
- [x] define where the aiplayers.ini file is
- [x] add AI player example to it
- [x] create the screen
  - [x] player selection
  - [x] num pieces slider
  - [x] shuffle players checkbox
- [x] add UI sounds

### Start creating main game screen

- [x] initialize the game from data from Game Creation
  - [x] load puzzles and initialize players
  - [x] create GameCore object
  - [x] add error message box for invalid initialization
    - [x] compile fail init AI player to netstandard2.1 and test - rename the old one to TargetErrorExample
- [x] crete basic game loop with AI players for testing
  - [x] decide how to agregate data for final results screen
- [x] add game screen pause menu

### Create final screen

- [x] create final animation
  - [x] animate finished puzzles
  - [x] animate finishing touches
  - [x] animate unfinished puzzles
  - [x] show NumPuzzles and NumPieces if results are ambiguous
  - [x] show final results
  - [x] show home button
- [x] add final screen pause menu

### Create main game screen

- [x] create game layout
- [x] create game board from PlayerSelection
  - [x] Action zone
    - [x] action buttons UI logic
    - [x] end finishes touches button
  - [x] Puzzle zone
    - [x] populate columns with puzzles
    - [x] logic for empty slot
  - [x] Player zone
    - [x] player row setup
      - [x] name
      - [x] show puzzles + empty slots
      - [x] player row collider visualization
    - [x] populate with player rows
      - [x] enable only row of selected player
  - [x] Piece zone
    - [x] populate names
    - [x] collection column setup
      - [x] add field for each shape
    - [x] add player collection columns
    - [x] add shared reserve column
- [x] add action buttons logic
- [x] add logic for human actions
  - [x] standard actions
    - [x] connect TetrominoSpawners to action manager
    - [x] take puzzle
    - [x] recycle
    - [x] take basic piece
    - [x] change piece
    - [x] master action
  - [x] finishing touches action
    - [x] clear board
    - [x] end finishing touches
  - [x] reward selection
- [x] add animation for AI actions
  - [x] puzzle finished
    - [x] select reward
  - [x] place piece
  - [x] take puzzle
  - [x] recycle
  - [x] take basic piece
  - [x] change piece
  - [x] master action
  - [x] end finishing touches
- [x] rework tetromino + puzzle interaction
  - [x] fix: place animation doesnt change scale
  - [x] I4 jiny scale
  - [x] space --> place last selected piece

### Polishing

- [x] fix: when creating action - hover over selected action button makes it appear unselected
- [x] make pause menu darker and parse game phase better
- [x] animate number changes in shared reserve
- [x] take puzzle action on puzzle click
- [x] select reward & finishing touches keyboard shortcuts
- [x] master enabled - update confirm button
- [x] allow multiple placements at the same time - consumes actions
- [x] player row border
  - inside not black - make a bit gray
  - at start of game - all visible
- [x] increase confirm button hitbox
- [x] place puzzle on selected slot
- [x] AI place - rotate / flip while moving
- [x] right click should not start dragging
- [x] place dilek - check rotace dilku
- [x] shortcut for cancel action
- [x] FIX: drag tetromino when AI player is selected
- [x] show last finishing tocuhes pice at end of final animation
- [x] make debug log darker
- [ ] hide current player panel when animating finished puzzle

### Finishing touches

- [x] change num initial tetrominos bounds to 5 and 25
- [x] final results align names bellow each other properly
  - [x] 1. is a shorter tan 2,3,4 --> it is more to the left
- [x] recycle - rows act as radio buttons
- [x] disable AI players feature on WebGL
- [x] prefill game creation screen with last used settings
- [x] make num actions left in pause menu colorful
- [x] add icon (512x512) and customize player settings

### Code Cleanup

- [x] cancellation token workflow
  - [x] final scene
  - [x] main game scene
- [x] use singleton base class for all singletons
- [x] game session manager - remove update
- [x] cleanup namespaces and prefab folders

### Testing

- [x] 4 players game screen size resposiveness
- [x] player with init file
- [x] player with recycle and master actions
