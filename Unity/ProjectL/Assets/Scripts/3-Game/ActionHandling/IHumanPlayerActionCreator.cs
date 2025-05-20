#nullable enable

namespace ProjectL.GameScene.ActionHandling
{
    using ProjectLCore.GameActions;
    using System;

    public interface IHumanPlayerActionCreator<out T> where T : GameAction
    {
        #region Events

        event Action<IActionModification<T>>? ActionModifiedEventHandler;

        #endregion

        #region Methods

        void OnActionRequested();

        void OnActionCanceled();

        void OnActionConfirmed();

        #endregion
    }
}
