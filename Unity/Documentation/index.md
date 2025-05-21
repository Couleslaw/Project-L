---
_disableToc: false
_disableAffix: false
---

# Project L Unity

> [!WARNING]
> Make sure to have read the Project L Core library [documentation](https://couleslaw.github.io/Project-L/TechnicalDocs/core/index) first. This documentation is only for the Unity project.

### Player Selection

When the player enters the player selection screen fot the first time, the game will try to load available AI player types specified in the `StreamingAssets/aiplayers.ini` file. This is the job of the [AIPlayerTypesLoader](xref:ProjectL.Data.AIPlayerTypesLoader). After the user has creating the game session, data about the selected players is stored in the [GameSettings](xref:ProjectL.Data.GameSettings) static class.

### Playing the Game

The most high-level class is the `GameSessionManager`. It is responsible for

- loading the puzzles from the Resources folder
- instantiating players based on how they were selected by the user
- initializing the the AI players
- initializing the [GameCore](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameCore.htm) instance which manages the internal game logic
- simulating the game loop
- storing information about how the players played in the [GameSummary](xref:ProjectL.Data.GameSummary) static class.

### Challenges

There are three interesting problems which need to be solved:

- [Making the graphics respond to actions taken by players](./docs/game-flow.md)
- [Animating actions provided by an AI player](./docs/ai-players.md)
- [Getting actions from a human player](./docs/human-players.md)

The solution heavily relies on the listener pattern, where different components listen to events about changes in the game and update their state accordingly.
