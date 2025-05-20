#nullable enable

namespace ProjectL.GameScene.ActionHandling
{
    public enum PlayerMode
    {
        Interactive, NonInteractive
    }

    public enum ActionMode
    {
        ActionCreation, FinishingTouches, RewardSelection
    }

    public interface IActionCreationController
    {
        #region Methods

        void SetPlayerMode(PlayerMode mode);

        void SetActionMode(ActionMode mode);

        #endregion
    }
}
