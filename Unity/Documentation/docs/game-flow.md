# Graphics Reacting to Player Actions

When an action is processed by the `GameCore`, for example when a player takes a new puzzle, the game graphics need to respond to it.

The [GraphicsManager](xref:ProjectL.GameScene.GraphicsManager`1) abstract class provides a way for different types to access the `GameCore`. The [GameGraphicsSystem](xref:ProjectL.GameScene.Management.GameGraphicsSystem) calls the `Init(GameCore)` method of all registered `GraphicsManagers` at the start of the game. They can use it to connect to the game listener interfaces provided by the **Project L Core** library, which are documented [here](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/N_ProjectLCore_GameLogic.htm).

For example, the [TetrominoCountsColumn](xref:ProjectL.GameScene.PieceZone.TetrominoCountsColumn), which represents one column in the piece zone, is a `ITetrominoCollectionListener` because it listens to the changes in the tetromino collection of a player. It is however also a `ITetrominoCollectionNotifier` because the [PieceZoneManager](xref:ProjectL.GameScene.PieceZone.PieceZoneManager) (who is also a `ITetrominoCollectionListener`) needs to be notified when a tetromino count reaches zero, so that it can gray out the corresponding [TetrominoButton](xref:ProjectL.GameScene.PieceZone.TetrominoButton).
