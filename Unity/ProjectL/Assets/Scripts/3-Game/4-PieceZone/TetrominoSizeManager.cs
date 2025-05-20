#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using UnityEngine;

    public class TetrominoSizeManager : StaticInstance<TetrominoSizeManager>
    {
        #region Fields

        [Header("Tetromino scale settings")]
        [Tooltip("Sample of a puzzle card to get it's scale")]
        [SerializeField] private Transform? puzzleSample;

        [Tooltip("Edge of the puzzle zone, to use as a border for scaling")]
        [SerializeField] private Transform? puzzleZoneEdgeMarker;

        #endregion

        #region Properties

        public float PuzzleZoneScale => puzzleSample?.localScale.x ?? 1f;

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
            if (puzzleSample == null || puzzleZoneEdgeMarker == null) {
                Debug.LogError("One or more components are not assigned!", this);
                return;
            }
        }

        #endregion
    }
}
