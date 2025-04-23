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
- [ ] finalize docstrings

### Start creating main game screen

- [ ] initialize the game from data from Game Creation
  - [x] load puzzles and initialize players
  - [ ] create GameCore object
  - [ ] add error message box for invalid initialization
    - [ ] compile fail init AI player to netstandard2.1 and test
- [ ] crete basic game loop with AI players for testing
  - [ ] decide how to agregate data for final results screen

### Create final screen

- [ ] create final animation
  - [ ] animate finished puzzles
  - [ ] animate finishing touches
  - [ ] animate unfinished puzzles
  - [ ] show NumPuzzles and NumPieces if results are ambiguous
  - [ ] show final results
  - [ ] show home button
- [ ] add final screen pause menu
- [ ] finalize docstrings

### Create main game screen
