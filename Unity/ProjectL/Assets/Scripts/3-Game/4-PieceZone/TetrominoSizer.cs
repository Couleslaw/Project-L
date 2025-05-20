#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using UnityEngine;

    [RequireComponent(typeof(DraggableTetromino))]
    [RequireComponent(typeof(RectTransform))]
    public class TetrominoSizer : MonoBehaviour
    {
        #region Fields

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
