#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using System.Collections;
    using TMPro;
    using UnityEngine;

    public class PieceCounter : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI? _countLabel;
        private Color _colorToSet;
        private bool _colorCoroutineRunning = false;

        public static Color IncrementedDisplayColor { get; } = new Color(115f / 255, 218f / 255, 69f / 255);
        public static Color DecrementedDisplayColor { get; } = new Color(224f / 255, 55f / 255, 34f / 255);

        #endregion

        #region Properties

        public int Count {
            get {
                if (string.IsNullOrEmpty(_countLabel!.text)) {
                    return 0;
                }
                return int.Parse(_countLabel.text);
            }
            set {
                if (value < 0)
                    Debug.LogError("Count cannot be negative.");
                else if (value > 0)
                    _countLabel!.text = value.ToString();
                else { // value == 0
                    if (_countLabel!.color == DecrementedDisplayColor) {
                        _countLabel.text = value.ToString();  // show red zero
                    }
                    else {
                        _countLabel!.text = string.Empty;
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void Awake()
        {
            if (_countLabel == null) {
                Debug.LogError("Count label is not assigned in the inspector.");
                return;
            }
        }

        public void SetColor(Color color)
        {
            if (_colorCoroutineRunning) {
                // stop the coroutine if we are setting the color to red
                if (color == DecrementedDisplayColor) {
                    _colorCoroutineRunning = false;
                }
                else {
                    _colorToSet = color;
                    return;
                }
            }

            if (_countLabel != null) {
                _countLabel.color = color;
            }

            // if red --> show zero, else dont show zero --> need to refresh
            if (Count == 0) {
                Count = 0;  
            }
        }

        public void SetColorAfterSeconds(Color color, float secondDelay)
        {
            if (secondDelay <= 0) {
                SetColor(color);
                return;
            }

            _colorToSet = color;
            _colorCoroutineRunning = true;
            StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                yield return new WaitForSeconds(secondDelay);
                if (_colorCoroutineRunning) {
                    _colorCoroutineRunning = false;
                    SetColor(_colorToSet);
                }
            }
        }

        #endregion
    }
}
