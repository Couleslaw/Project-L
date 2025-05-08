#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using System;
    using UnityEngine;

    public class PuzzleActionZone : ActionZoneBase
    {
        public event Action? OnClearBoardButtonClick;

        private new void Awake()
        {
            base.Awake();
      
            _finishingTouchesButton!.onClick.AddListener(
                () => OnClearBoardButtonClick?.Invoke()
            );
        }
    }
}
