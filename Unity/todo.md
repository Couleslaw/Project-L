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
- [ ] add logic for human actions
  - [ ] standard actions
    - [x] connect TetrominoSpawners to action manager
    - [x] take puzzle
    - [x] recycle
    - [x] take basic piece
    - [ ] change piece
    - [x] master action
  - [x] finishing touches action
    - [x] clear board
    - [x] end finishing touches
  - [ ] reward selection
    - neco se rozbije kdyz clovek hodne klika a vymenuje
    - AI kdyz snizi na 0, tak to nezmizi, ale zustane cervena nula, tady nekde problem
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
- [ ] async workflow
  - [ ] add option to skip animation
  - [ ] cleanup cancellation tokens usage
- [ ] polishing
  - [ ] when creating action - hover over selected action button makes it appear unselected
  - [ ] I4 jiny scale
  - [ ] make pause menu darker and parse game phase better
  - [ ] scale dilku pro dragging
  - [ ] place animation doesnt change scale
  - [ ] animate number changes in shared reserve
  - [ ] take puzzle action on puzzle click
  - [ ] rework tetromino + puzzle interaction
    - [ ] space --> place last selected piece
  - [ ] select reward & finishing touches keyboard shortcuts
  - [ ] master enabled - update confirm button
  - [ ] allow multiple placements at the same time - consumes actions
  - [ ] player row border
    - inside not black - make a bit gray
    - at start of game - all visible
  - [ ] zvetsit confirm button hitbox

### Finishing touches

- [ ] build game
- [ ] disable AI players feature on WebGL
- [ ] tab navigation in player selection
- [ ] lower min num tetrominos to 5
- [ ] final results align names bellow each other properly
  - [ ] 1. is a shorter tan 2,3,4 --> it is more to the left
