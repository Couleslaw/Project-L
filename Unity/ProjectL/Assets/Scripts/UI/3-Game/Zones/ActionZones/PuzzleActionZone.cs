#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class PuzzleActionZone : ActionZoneBase
    {
        public event Action? OnClearBoardButtonClick;
        public event Action? OnRecycleButtonClick;
        public event Action? OnTakePuzzleButtonClick;

        [SerializeField] private ActionButton? _recycleButton;
        [SerializeField] private ActionButton? _takePuzzleButton;

        private new void Awake()
        {
            base.Awake();

            if (_recycleButton == null || _takePuzzleButton == null) {
                Debug.LogError("One or more buttons are not assigned in the inspector", this);
                return;
            }

            _finishingTouchesButton!.onClick.AddListener(
                () => OnClearBoardButtonClick?.Invoke()
            );
            _recycleButton.SelectAction += OnRecycleButtonClick;
            _takePuzzleButton.SelectAction += OnTakePuzzleButtonClick;
        }
    }
}
