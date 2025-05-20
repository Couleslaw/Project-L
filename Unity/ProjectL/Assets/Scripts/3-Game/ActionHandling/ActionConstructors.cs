#nullable enable

namespace ProjectL.GameScene.ActionHandling
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public interface IActionConstructor
    {
        #region Methods

        void Reset();

        void ApplyActionModification<T>(IActionModification<T> change) where T : GameAction;

        T? GetAction<T>() where T : GameAction;

        #endregion
    }

    public abstract class ActionConstructor<T> : IActionConstructor where T : GameAction
    {
        #region Methods

        public T1? GetAction<T1>() where T1 : GameAction
        {
            if (GetAction() is null) {
                return null;
            }
            return GetAction() as T1 ?? throw new InvalidCastException($"Cannot cast {typeof(T)} to {typeof(T1)}");
        }

        public void ApplyActionModification<T1>(IActionModification<T1> change) where T1 : GameAction
        {
            ApplyActionModification(change as IActionModification<T> ?? throw new InvalidCastException($"Cannot cast {typeof(T1)} to {typeof(T)}"));
        }

        public abstract void Reset();

        protected abstract T? GetAction();

        protected abstract void ApplyActionModification(IActionModification<T> change);

        #endregion
    }

    public class TakePuzzleConstructor : ActionConstructor<TakePuzzleAction>
    {
        #region Fields

        private TakePuzzleAction? _action;

        #endregion

        #region Methods

        public override void Reset() => _action = null;

        protected override TakePuzzleAction? GetAction() => _action;

        protected override void ApplyActionModification(IActionModification<TakePuzzleAction> change)
        {
            if (change is not TakePuzzleActionModification ch) {
                Debug.LogError($"Unknown action change type: {change.GetType().Name}");
                return;
            }

            _action = ch.Action;
        }

        #endregion
    }

    public class RecycleConstructor : ActionConstructor<RecycleAction>
    {
        #region Fields

        private readonly List<(uint, RecycleAction.Options)> _puzzleOrder = new();

        #endregion

        #region Methods

        public override void Reset() => _puzzleOrder.Clear();

        protected override RecycleAction? GetAction()
        {
            if (_puzzleOrder.Count == 0) {
                return null;
            }
            List<uint> order = _puzzleOrder.Select(x => x.Item1).ToList();
            RecycleAction.Options color = _puzzleOrder[0].Item2;
            return new RecycleAction(order, color);
        }

        protected override void ApplyActionModification(IActionModification<RecycleAction> change)
        {
            if (change is not RecycleActionModification ch) {
                Debug.LogError($"Unknown action change type: {change.GetType().Name}");
                return;
            }

            if (ch.IsSelected) {
                _puzzleOrder.Add((ch.PuzzleId, ch.Color));
            }
            else {
                _puzzleOrder.Remove((ch.PuzzleId, ch.Color));
            }
        }

        #endregion
    }

    public class TakeBasicConstructor : ActionConstructor<TakeBasicTetrominoAction>
    {
        #region Fields

        private TakeBasicTetrominoAction? _action;

        #endregion

        #region Methods

        public override void Reset() => _action = null;

        protected override TakeBasicTetrominoAction? GetAction() => _action;

        protected override void ApplyActionModification(IActionModification<TakeBasicTetrominoAction> change)
        {
            if (change is not TakeBasicTetrominoActionModification takeBasicChanged) {
                Debug.LogError($"Unknown action change type: {change.GetType().Name}");
                return;
            }

            _action = takeBasicChanged.IsSelected ? new TakeBasicTetrominoAction() : null;
        }

        #endregion
    }

    public class ChangeTetrominoConstructor : ActionConstructor<ChangeTetrominoAction>
    {
        #region Fields

        private TetrominoShape? _oldTetromino;

        private TetrominoShape? _newTetromino;

        #endregion

        #region Methods

        public override void Reset()
        {
            _oldTetromino = null;
            _newTetromino = null;
        }

        protected override ChangeTetrominoAction? GetAction()
        {
            if (_oldTetromino == null || _newTetromino == null) {
                return null;
            }
            return new ChangeTetrominoAction(_oldTetromino.Value, _newTetromino.Value);
        }

        protected override void ApplyActionModification(IActionModification<ChangeTetrominoAction> change)
        {
            if (change is not ChangeTetrominoActionModification ch) {
                Debug.LogError($"Unknown action change type: {change.GetType().Name}");
                return;
            }

            _oldTetromino = ch.OldTetromino;
            _newTetromino = ch.NewTetromino;
        }

        #endregion
    }

    public class PlaceTetrominoConstructor : ActionConstructor<PlaceTetrominoAction>
    {
        #region Fields

        private readonly List<PlaceTetrominoAction> _placements = new();

        #endregion

        #region Methods

        public override void Reset() => _placements.Clear();

        public MasterAction? GetMasterAction()
        {
            if (_placements.Count == 0) {
                return null;
            }
            return new MasterAction(new(_placements));
        }

        public Queue<PlaceTetrominoAction> GetPlacementsQueue() => new(_placements);

        protected override PlaceTetrominoAction? GetAction()
        {
            return _placements.Count == 1 ? _placements[0] : null;
        }

        protected override void ApplyActionModification(IActionModification<PlaceTetrominoAction> change)
        {
            if (change is not PlaceTetrominoActionModification ch) {
                Debug.LogError($"Unknown action change type: {change.GetType().Name}");
                return;
            }

            if (ch.Option == PlaceTetrominoActionModification.Options.Placed) {
                _placements.Add(ch.Placement);
            }
            else {
                _placements.Remove(ch.Placement);
            }
        }

        #endregion
    }

    public class SelectRewardConstructor : ActionConstructor<SelectRewardAction>
    {
        #region Fields

        private TetrominoShape? _selectedShape;

        #endregion

        #region Methods

        public override void Reset() => _selectedShape = null;

        protected override SelectRewardAction? GetAction()
        {
            return _selectedShape == null ? null : new SelectRewardAction(null, _selectedShape.Value);
        }

        protected override void ApplyActionModification(IActionModification<SelectRewardAction> change)
        {
            if (change is not SelectRewardActionModification ch) {
                Debug.LogError($"Unknown action change type: {change.GetType().Name}");
                return;
            }
            _selectedShape = ch.SelectedReward;
        }

        #endregion
    }
}
