#nullable enable

using UnityEngine;

public class TetrominoSizeManager : MonoBehaviour
{
    #region Fields

    private static TetrominoSizeManager? _instance = null;

    [Header("Tetromino scale settings")]
    [Tooltip("Sample of a puzzle card to get it's scale")]
    [SerializeField] private Transform puzzleSample;

    [Tooltip("Sample of a tetromino spawner to get it's scale")]
    [SerializeField] private Transform tetrominoSpawnerSample;

    [Tooltip("Edge of the puzzle zone, to use as a border for scaling")]
    [SerializeField] private Transform puzzleZoneEdgeMarker;

    #endregion

    #region Methods

    public static Vector3 GetScaleFor(Transform tetromino) => _instance?.GetScale(tetromino) ?? Vector3.one;

    internal void Awake()
    {
        // singleton pattern
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // check that all components are assigned
        if (puzzleSample == null || tetrominoSpawnerSample == null || puzzleZoneEdgeMarker == null) {
            Debug.LogError("One or more components are not assigned!", this);
            return;
        }
    }

    private float DistanceToPuzzleZone(float x)
    {
        if (puzzleZoneEdgeMarker == null) {
            return 0f;  // safety check
        }

        // puzzle zone - marker - tetromino spawner
        // if x right of marker
        if (x > puzzleZoneEdgeMarker.position.x) {
            return x - puzzleZoneEdgeMarker.position.x;
        }
        return 0f; // if x left of marker
    }

    private Vector3 GetScale(Transform tetromino)
    {
        float distance = DistanceToPuzzleZone(tetromino.position.x);
        float maxScaleDistance = DistanceToPuzzleZone(tetrominoSpawnerSample.position.x);
        float maxScale = tetrominoSpawnerSample.localScale.x;
        float minScale = puzzleSample.localScale.x;
        float t = Mathf.Clamp01(Mathf.InverseLerp(maxScaleDistance, 0f, distance));
        float scale = Mathf.Lerp(maxScale, minScale, t);
        //Debug.Log($"Scale: {scale} for distance: {distance} from puzzle zone edge marker (x={tetromino.position.x}), minScale={minScale}, maxScale={maxScale}");
        var scaleVector = Vector3.one * scale;
        // preserve original flip
        if (tetromino.localScale.x < 0) {
            scaleVector.x *= -1;
        }
        return scaleVector;
    }

    #endregion
}
