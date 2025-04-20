# Project L

**Project L** is a strategic board game where players collect pieces and use them to complete various puzzles. As a reward, they receive new pieces and points. Each turn, players perform three actions. They can choose from the following:

- Place a piece on a puzzle
- Take a new puzzle from the offer
- Take a level-1 piece from the shared supply
- Exchange one piece for another
- Recycle one of the puzzle rows on offer
- Master action – parallel placement, only allowed once per turn

There are two puzzle decks available – white and black. The end-game phase begins when the black deck is emptied. For detailed game rules, please refer to the _Project L Base Game_ section in the rulebook [here](https://couleslaw.github.io/Project-L/UserDocs/rulebook.pdf).

## Unity Version of the Game

Currently a work in progress. Functional specification can be found [here](https://couleslaw.github.io/Project-L/FunctionDocs/index).

## Project L Library

The library [Project L Core](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/G_ProjectLCore.htm), documented [here](https://couleslaw.github.io/Project-L/TechnicalDocs/core/index), serves two purposes. First, it implements the game logic and allows for easy development of applications that simulate the game in various ways. Second, it provides an API for creating and efficiently training intelligent agents (AI players) that can play the game.

### API for Developing Game Applications

How the library works and what it offers is described in detail in the [documentation](https://couleslaw.github.io/Project-L/TechnicalDocs/core/index). The most important class is [GameCore](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameCore.htm), which is responsible for all the game logic. An example of a simple program that simulates the Project L game loop is available [here](https://couleslaw.github.io/Project-L/TechnicalDocs/core/index#showcase-of-the-game-engine).

### API for Creating AI Players

Library users can implement their own AI player by simply implementing the abstract methods of the class [AIPlayerBase](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_Players_AIPlayerBase.htm). Specifically:

- [GetAction](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetAction.htm) – chooses the next action for the AI player
- [GetReward](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetReward.htm) – selects a reward for a completed puzzle
- [Init](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_Init.htm) – initializes the AI player

More detailed information on creating AI players is available [here](https://couleslaw.github.io/Project-L/AIPlayerGuide/index).
