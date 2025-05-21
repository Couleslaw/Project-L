# Creating Human Player Actions

Creating human player actions is the responsibility of the [HumanPlayerActionCreationManager](xref:ProjectL.GameScene.ActionHandling.HumanPlayerActionCreationManager), which we will shorten to `HPACM`. The `GameSessionManager` registers [HumanPlayers](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_Players_HumanPlayer.htm) to it at the start of the game through the `RegisterPlayer(HumanPlayer)` method. The `HPACM` then listens to the [ActionRequested](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/E_ProjectLCore_Players_HumanPlayer_ActionRequested.htm) and [RewardChoiceRequested](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/E_ProjectLCore_Players_HumanPlayer_RewardChoiceRequested.htm) events of the player.

Two enums, [PlayerMode](xref:ProjectL.GameScene.ActionHandling.PlayerMode) and [ActionMode](xref:ProjectL.GameScene.ActionHandling.ActionMode), control the action creation logic. Any type that implements the [IActionCreationController](xref:ProjectL.GameScene.ActionHandling.IActionCreationController) interface and registers with the `HPACM` will be notified whenever either mode changes, allowing it to respond appropriately.

### Action Requested by the `GameSessionManager`

During the game loop, the `GameSessionManager` calls the `Player.GetActionAsync` method of a `HumanPlayer`. This triggers the `HumanPlayer.ActionRequested` event. As a result, the `HPACM` is notified and receives an [ActionVerifier](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameActions_Verification_ActionVerifier.htm) object. In response, it

- changes the modes to `PlayerMode.Interactive` and `ActionMode.ActionCreation`
- and connects to the UI buttons using the `ActionZonesManager.ConnectToActionButtons(HPACM)` method

When the user clicks on a button in the action zone, the `HPACM` will now be notified.

### User Wants to Take a Certain Action

When a user wants to perform an action, certain types need to be notified about the action's creation, cancellation, or confirmation. To receive these notifications, these types implement the generic [IHumanPlayerActionCreator](xref:ProjectL.GameScene.ActionHandling.IHumanPlayerActionCreator`1) interface and register themselves with the `HPACM`. The `HPACM` then listens for their `ActionModifiedEventHandler` events, ensuring it is informed whenever an action changes.

Let's say that the user clicked on the **Take Puzzle** button. The `HPACM` then calls the `OnActionRequested` method of all subscribed `IHumanPlayerActionCreator<TakePuzzleAction>` instances (for example the [PuzzleZoneManager](xref:ProjectL.GameScene.PuzzleZone.PuzzleZoneManager)) and they can react to it.

### Modifying the Action

When the user somehow changes the action, in our case by clicking on a puzzle button to select it, the `PuzzleZoneManager` fires the `ActionModifiedEventHandler` event and the `HPACM` is notified and receives a [TakePuzzleActionModification](xref:ProjectL.GameScene.ActionHandling.TakePuzzleActionModification) object.

The `HPACM` internally uses a [TakePuzzleActionConstructor](xref:ProjectL.GameScene.ActionHandling.TakePuzzleActionConstructor) to keep track of the current action. After it applies the modification, it

- calls the constructors `TakePuzzleActionConstructor.GetAction` method
- checks if the action is valid using its `ActionVerifier`
- tells the `ActionZonesManager` to enable or disable the **Confirm** buttons depending on the action validity

### Confirming the Action

If the action was valid, then the **Confirm** buttons are enabled and if the player clicks on them, the `OnActionConfirmed` method of the `HPACM` is called. It in turn

- calls the `OnActionConfirmed` method of all subscribed `IHumanPlayerActionCreator<TakePuzzleAction>` instances
- sets the player mode back to `PlayerMode.NonInteractive`
- disconnects from the action buttons
- calls the `HumanPlayer.SetReward` method of the player who requested the action

### Summary

- The `HPACM` acts as a bridge between the game core, UI, and action creation logic.
- It uses the listener pattern to notify various classes about game state changes.
- The `IActionCreationController` interface:
  - Allows classes to control action creation logic.
  - Notifies them when player or action modes change.
- The `IHumanPlayerActionCreator<T>` interface:
  - Notifies classes when an action is requested, modified, or confirmed.
- The `HPACM` maintains the current action using an `ActionConstructor<T>`.
- When an action is modified:
  - `IHumanPlayerActionCreator<T>` sends an `IActionModification<T>` to the `HPACM`.
  - The `HPACM` checks action validity and enables/disables **Confirm** buttons.
- When an action is confirmed:
  - The `HPACM` calls `OnActionConfirmed` on all subscribed `IHumanPlayerActionCreator<T>` instances.
  - Sets player mode back to `PlayerMode.NonInteractive`.
- When an action is canceled:
  - The `HPACM` calls `OnActionCanceled` on all subscribed `IHumanPlayerActionCreator<T>` instances.

## Requesting a Reward

When a player completes a puzzle, the [GameActionProcessor](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_GameActions_GameActionProcessor.htm) processing the action calls the player's `GetRewardAsync` method. This will trigger the `HumanPlayer.RewardChoiceRequested` event, the `HPACM` will be notified and the whole process starts again in a very similar fashion.
