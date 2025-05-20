#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using ProjectL.Animation;
    using System.Collections;
    using TMPro;
    using UnityEngine;

    public class PieceCounter : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI? _countLabel;

        private Color _colorToSet;

        private bool _colorCoroutineRunning = false;

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
                    if (_countLabel!.color == ColorManager.red) {
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

        public void SetColor(Color color)
        {
            if (_colorCoroutineRunning) {
                // stop the coroutine if we are setting the color to red
                if (color == ColorManager.red) {
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

            // if red --> show zero, else don't show zero --> need to refresh
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

        private void Awake()
        {
            if (_countLabel == null) {
                Debug.LogError("Count label is not assigned in the inspector.");
                return;
            }
        }

        #endregion
    }
}
