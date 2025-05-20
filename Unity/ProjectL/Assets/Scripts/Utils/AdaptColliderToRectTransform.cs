#nullable enable

namespace ProjectL.Utils
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    [ExecuteAlways] // Makes the script run in Edit mode as well as Play mode
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class AdaptColliderToRectTransform : UIBehaviour // UIBehaviour provides OnRectTransformDimensionsChange
    {
        #region Fields

        private RectTransform? _rectTransform;

        private BoxCollider2D? _boxCollider;

        #endregion

        /// <summary>
        /// Updates the BoxCollider2D size and offset to match the RectTransform's world bounds.
        /// </summary>
        public void UpdateCollider()
        {
            if (_rectTransform == null || _boxCollider == null) {
                Debug.LogError("AdaptColliderToRectTransform: Missing RectTransform or BoxCollider2D.", this);
                return;
            }

            _boxCollider.size = new Vector2(_rectTransform.sizeDelta.x, _rectTransform.sizeDelta.y);
            _boxCollider.offset = Vector2.zero;
        }

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCollider();
        }

        // Called when the RectTransform dimensions change (e.g., by layout group)
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            if (isActiveAndEnabled && _rectTransform != null && _boxCollider != null) {
                UpdateCollider();
            }
        }

#if UNITY_EDITOR
        // Ensure update happens if values change in Inspector during Edit mode
        protected override void OnValidate()
        {
            base.OnValidate();
            // Refresh references in case they were changed/removed
            _rectTransform = GetComponent<RectTransform>();
            _boxCollider = GetComponent<BoxCollider2D>();
            // Update immediately
            if (isActiveAndEnabled) {
                UpdateCollider();
            }
        }
#endif
    }
}
