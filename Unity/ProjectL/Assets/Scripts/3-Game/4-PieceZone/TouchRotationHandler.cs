#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using ProjectL.Management;
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
    using InputTouchPhase = UnityEngine.InputSystem.TouchPhase; // Use alias to resolve ambiguity

    public class TouchRotationHandler : MonoBehaviour
    {
        #region Fields

        // To store the previous angle between two touches
        private Vector2 lastFingerPos;

        private bool trackingTwoTouches = false;

        private Action<float>? _onRotate;

        private Camera? _camera;

        #endregion

        #region Methods

        public void Init(Action<float> OnRotate)
        {
            _onRotate = OnRotate;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (GameManager.IsGamePaused) {
                trackingTwoTouches = false;
                return;
            }

            // Check for two touches on the screen
            if (Touchscreen.current == null || Touchscreen.current.touches.Count < 2) {
                // Less than two touches, reset tracking
                trackingTwoTouches = false;
                return;
            }

            // check that app is in focus
            if (!Application.isFocused) {
                trackingTwoTouches = false;
                return;
            }

            InputControl firstTouch = Touchscreen.current.touches[0];
            InputControl secondTouch = Touchscreen.current.touches[1];

            // Ensure both touches are valid (e.g., not ended, not cancelled)
            if (firstTouch is not TouchControl touch0 || secondTouch is not TouchControl touch1 ||
                touch0.phase.ReadValue() != InputTouchPhase.Moved || touch1.phase.ReadValue() != InputTouchPhase.Moved) {
                // One or both touches are no longer in progress, reset tracking
                trackingTwoTouches = false;
                return;
            }

            // Get current positions of the two touches
            Vector2 touch0Pos = touch0.position.ReadValue();
            Vector2 touch1Pos = touch1.position.ReadValue();

            // Calculate the vector between the two touches
            Vector2 currentFingerPos = _camera!.ScreenToWorldPoint((touch0Pos + touch1Pos) / 2f);

            if (!trackingTwoTouches) {
                // First frame with two touches, initialize the angle
                lastFingerPos = currentFingerPos;
                trackingTwoTouches = true;
                return;
            }

            // Calculate the difference in angle
            Vector2 delta = currentFingerPos - lastFingerPos;
            _onRotate?.Invoke(Math.Sign(delta.y) * delta.magnitude);

            lastFingerPos = currentFingerPos;
        }

        #endregion
    }
}
