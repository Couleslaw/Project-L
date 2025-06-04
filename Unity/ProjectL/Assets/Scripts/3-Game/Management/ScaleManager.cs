#nullable enable

namespace ProjectL.GameScene.Management
{
    using UnityEngine;

    public class ScaleManager : StaticInstance<ScaleManager>
    {
        #region Fields

        [Header("Tetromino scale settings")]
        [Tooltip("Sample of a puzzle card to get it's scale")]
        [SerializeField] private Transform? playerZonePuzzleSample;

        [Tooltip("Edge of the puzzle zone, to use as a border for scaling")]
        [SerializeField] private Transform? puzzleZoneEdgeMarker;

        [Header("Puzzle scale settings")]
        [Tooltip("Scale of puzzles in the puzzle zone.")]
        [SerializeField] private Transform? puzzleZonePuzzleSample;

        #endregion

        #region Properties

        public float PlayerZoneScale => playerZonePuzzleSample?.localScale.x ?? 1f;

        public float PuzzleZoneScale => puzzleZonePuzzleSample?.localScale.x ?? 1f;

        #endregion

        #region Methods

        public float GetDistanceToPuzzleZone(Transform tr)
        {
            if (puzzleZoneEdgeMarker == null) {
                return 0f;  // safety check
            }

            // puzzle zone | marker | tetromino spawner

            // if x left of marker
            if (tr.position.x <= puzzleZoneEdgeMarker.position.x) {
                return 0f;
            }

            // if x right of marker
            return tr.position.x - puzzleZoneEdgeMarker.position.x;
        }

        protected override void Awake()
        {
            base.Awake();

            // check that all components are assigned
            if (playerZonePuzzleSample == null || puzzleZoneEdgeMarker == null || puzzleZonePuzzleSample == null) {
                Debug.LogError("One or more components are not assigned!", this);
                return;
            }
        }

        #endregion
    }
}
