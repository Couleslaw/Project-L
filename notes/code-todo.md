## Unity game roadmap

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
- [ ] update the _AI Player Guide_

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
