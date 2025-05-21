# Animating AI Player Actions

Animating AI player actions is handled by the [AIPlayerActionAnimationManager](xref:ProjectL.GameScene.Management.AIPlayerActionAnimationManager), referred to as `AIPAAM`. This manager implements the [IAsyncActionProcessor](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameActions_IAsyncActionProcessor.htm) interface.

When the `GameCore` processes an action, it checks if the current player is an AI player. If so, it passes the action to `AIPAAM`, which animates the action and waits until the animation is complete.

To animate actions, various classes implement the [IAIPlayerActionAnimator](xref:ProjectL.GameScene.ActionHandling.IAIPlayerActionAnimator`1) generic interface, which defines the `AnimateAsync` method. The `AIPAAM` uses these animators to perform the animations. Additionally, several `IDisposable` helper classes are used to keep the code organized and safe.

### Example: Animating a `ChangeTetrominoAction`

First, the player needs to click on the **Change Tetromino** button in the action zone.
The `AIPAAM` uses a [ActionZonesManager.DisposableButtonSelector](xref:ProjectL.GameScene.ActionZones.ActionZonesManager.DisposableButtonSelector) to do so.

Then it redirects the job to the [PieceZoneManager](xref:ProjectL.GameScene.PieceZone.PieceZoneManager), which implements the `IAIPlayerActionAnimator<ChangeTetrominoAction>` interface. It uses two disposables to animate the action.

- The [TetrominoButton.DisposableButtonSelector](xref:ProjectL.GameScene.PieceZone.TetrominoButton.DisposableButtonSelector) to visually select the piece to give away and the piece to take.
- The (private) `PieceZoneManager.DisposableButtonHighlighter` to highlight the possible trade options.

### Animating Reward Selection

Reward selection is a bit different. With regular actions, the `GameSessionManager` calls `GameAction.AcceptAsync(AIPAAM)` before `GameCore.ProcessActionAsync(GameAction)`, so the `AIPAAM` can animate the action before it is processed and the game graphics respond to it.

However, reward selection takes place inside the `GameCore.ProcessActionAsync` method. When a player completes a puzzle, the [GameActionProcessor](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameActions_GameActionProcessor.htm) processing the action calls the player's `GetRewardAsync` method. This method returns the piece the player wants to take as a reward. We need to animate this selection before the `GameCore.ProcessActionAsync` method returns — otherwise, the graphics would already reflect the updated game state after the reward is chosen, making the animation ineffective.

Thankfully, the **Project L Core** library provides a bunch of game listener interfaces, one of which is the [IPlayerStatePuzzleFinishedAsyncListener](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_IPlayerStatePuzzleFinishedAsyncListener.htm). When a puzzle is finished by a player, the `OnPuzzleFinishedAsync(FinishedPuzzleInfo)` method of all listeners subscribed to his `PlayerState` is called and awaited. This happens before any changes are made to his [PlayerState](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PlayerState.htm) and the [GameState](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_GameState.htm).

The `AIPAAM` implements this interface to animate the reward selection using the `PieceZoneManager` in a very similar way as with the `ChangeTetrominoAction`.
