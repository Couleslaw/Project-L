#nullable enable

namespace ProjectL.UI.GameScene
{
    using UnityEngine;
    using ProjectLCore.GameActions;
    using ProjectLCore.GameLogic;
    using System;

    public interface IPlaceActionCanceledListener
    {
        void OnActionCanceled();
    }

    public interface IPlaceActionConfirmedListener
    {
        void OnActionConfirmed();
    }

    public interface ITetrominoActionCanceledListener
    {
        void OnActionCanceled();
    }

    public interface ITetrominoActionConfirmedListener
    {
        void OnActionConfirmed();
    }


    public class ActionCreationManager : GraphicsManager<ActionCreationManager>
    {
        private IAction? _action;

        private event Action? OnPlaceActionCanceled;
        private event Action? OnPlaceActionConfirmed;
        private event Action? OnTetrominoActionCanceled;
        private event Action? OnTetrominoActionConfirmed;

        public void AddListener(IPlaceActionCanceledListener listener) => OnPlaceActionCanceled += listener.OnActionCanceled;
        public void RemoveListener(IPlaceActionCanceledListener listener) => OnPlaceActionCanceled -= listener.OnActionCanceled;
        public void AddListener(IPlaceActionConfirmedListener listener) => OnPlaceActionConfirmed += listener.OnActionConfirmed;
        public void RemoveListener(IPlaceActionConfirmedListener listener) => OnPlaceActionConfirmed -= listener.OnActionConfirmed;
        public void AddListener(ITetrominoActionCanceledListener listener) => OnTetrominoActionCanceled += listener.OnActionCanceled;
        public void RemoveListener(ITetrominoActionCanceledListener listener) => OnTetrominoActionCanceled -= listener.OnActionCanceled;
        public void AddListener(ITetrominoActionConfirmedListener listener) => OnTetrominoActionConfirmed += listener.OnActionConfirmed;
        public void RemoveListener(ITetrominoActionConfirmedListener listener) => OnTetrominoActionConfirmed -= listener.OnActionConfirmed;

        private void CancelAction()
        {
            switch (_action) {
                case PlaceTetrominoAction a:
                    OnPlaceActionCanceled?.Invoke();
                    break;
                case MasterAction a:
                    OnPlaceActionCanceled?.Invoke();
                    break;
                case TetrominoAction a:
                    OnTetrominoActionCanceled?.Invoke();
                    break;
                default:
                    break;
            }
        }

        private void ConfirmAction()
        {
            switch (_action) {
                case PlaceTetrominoAction a:
                    OnPlaceActionConfirmed?.Invoke();
                    break;
                case MasterAction a:
                    OnPlaceActionConfirmed?.Invoke();
                    break;
                case TetrominoAction a:
                    OnTetrominoActionConfirmed?.Invoke();
                    break;
                default:
                    break;
            }
        }

        public override void Init(GameCore game)
        {
        }
    }
}
