#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
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

    [RequireComponent(typeof(DraggableTetromino))]
    [RequireComponent(typeof(RectTransform))]
    public class TetrominoSizer : MonoBehaviour
    {
        #region Fields

        private RectTransform? _rt;

        private bool _initialized = false;

        private float _spawnerScale;

        private float _spawnerDistanceToPuzzleZone;

        #endregion

        #region Methods

        public void Init(TetrominoButton spawner)
        {
            _spawnerScale = spawner.transform.localScale.x;
            _spawnerDistanceToPuzzleZone = TetrominoSizeManager.Instance.GetDistanceToPuzzleZone(spawner.transform);
            _initialized = true;
            UpdateScale();
        }

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        private void UpdateScale()
        {
            float distance = TetrominoSizeManager.Instance.GetDistanceToPuzzleZone(transform);

            float t = Mathf.Clamp01(Mathf.InverseLerp(_spawnerDistanceToPuzzleZone, 0f, distance));
            float scale = Mathf.Lerp(_spawnerScale, TetrominoSizeManager.Instance.PuzzleZoneScale, t);

            transform.localScale = SignVector(transform.localScale) * scale;

            static Vector3 SignVector(Vector3 v)
            {
                return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));
            }
        }

        private void FixedUpdate()
        {
            if (!_initialized) {
                return;
            }

            UpdateScale();
        }

        #endregion
    }
}
