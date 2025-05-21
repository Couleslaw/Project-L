---
_disableToc: false
_disableAffix: false
---

# Project L Unity

> [!WARNING]
> Make sure to have read the Project L Core library [documentation](https://couleslaw.github.io/Project-L/TechnicalDocs/core/index) first. This documentation is only for the Unity project.

There are three interesting problems in this project:

- animating actions provided by an AI player
- getting actions from a human player
- making the game respond to these actions being processed

The solution heavily relies on the listener pattern, where different components listen to events about changes in the game and update their state accordingly.

## Project L Core Interfaces

Some classes use the game flow interfaces provided by the Project L Core library, the documentation for them is [here](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/N_ProjectLCore_GameLogic.htm). For example, the [TetrominoCountsColumn](xref:ProjectL.GameScene.PieceZone.TetrominoCountsColumn), which represents one column in the piece zone, is a `ITetrominoCollectionListener` because it listens to the changes in the tetromino collection of one player. It is however also a `ITetrominoCollectionNotifier` because the [PieceZoneManager](xref:ProjectL.GameScene.PieceZone.PieceZoneManager) (who is also a `ITetrominoCollectionListener`) needs to be notified when a tetromino count reaches zero, so that it can gray out the corresponding [TetrominoButton](xref:ProjectL.GameScene.PieceZone.TetrominoButton).
