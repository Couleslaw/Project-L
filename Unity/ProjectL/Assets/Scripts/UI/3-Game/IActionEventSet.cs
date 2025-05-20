#nullable enable

namespace ProjectL.UI.GameScene.Actions.Constructing
{
    using ProjectL.UI.GameScene.Actions.Constructing;
    using ProjectLCore.GameActions;
    using System;

    public interface IActionEventSet
    {
        #region Methods

        void Subscribe<T>(IHumanPlayerActionCreator<T> listener) where T : GameAction;

        void Unsubscribe<T>(IHumanPlayerActionCreator<T> listener) where T : GameAction;

        void RaiseRequested();

        void RaiseCanceled();

        void RaiseConfirmed();

        #endregion
    }

    public class ActionEventSet<T> : IActionEventSet where T : GameAction
    {
        #region Events

        public event Action? OnActionRequested;

        public event Action? OnActionCanceled;

        public event Action? OnActionConfirmed;

        #endregion

        #region Methods

        public void Subscribe(IHumanPlayerActionCreator<T> listener)
        {
            OnActionRequested += listener.OnActionRequested;
            OnActionCanceled += listener.OnActionCanceled;
            OnActionConfirmed += listener.OnActionConfirmed;
        }

        public void Unsubscribe(IHumanPlayerActionCreator<T> listener)
        {
            OnActionRequested -= listener.OnActionRequested;
            OnActionCanceled -= listener.OnActionCanceled;
            OnActionConfirmed -= listener.OnActionConfirmed;
        }

        public void RaiseRequested() => OnActionRequested?.Invoke();

        public void RaiseCanceled() => OnActionCanceled?.Invoke();

        public void RaiseConfirmed() => OnActionConfirmed?.Invoke();

        public void Subscribe<T1>(IHumanPlayerActionCreator<T1> listener) where T1 : GameAction
        {
            Subscribe(listener as IHumanPlayerActionCreator<T> ??
                throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T)}")
                );
        }

        public void Unsubscribe<T1>(IHumanPlayerActionCreator<T1> listener) where T1 : GameAction
        {
            Unsubscribe(listener as IHumanPlayerActionCreator<T> ??
                throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T)}")
                );
        }

        #endregion
    }
}
