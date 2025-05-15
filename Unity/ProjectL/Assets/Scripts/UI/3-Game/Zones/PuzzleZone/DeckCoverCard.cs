#nullable enable

namespace ProjectL.UI.GameScene.Zones.PuzzleZone
{
    using TMPro;
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using ProjectLCore.GameLogic;
    using ProjectLCore.GamePieces;
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.Sound;
    using ProjectLCore.GameActions;

    [RequireComponent(typeof(Button))]
    public class DeckCoverCard : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI? _label;
        private Button? _button;

        private int _deckSize;
        private bool _isBlack;

        #endregion

        #region Methods


        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button == null || _label == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }
            _button.onClick.AddListener(SoundManager.Instance!.PlaySoftTapSoundEffect);
        }

        public void Init(bool isBlack)
        {
            _isBlack = isBlack;
        }

        #endregion

        public void SetDeckSize(int n)
        {
            if (n < 0) {
                throw new ArgumentOutOfRangeException(nameof(n), "Deck size cannot be negative.");
            }
            _deckSize = n;
            if (_label == null) {
                return;
            }
            _label.text = n.ToString();
        }

        public void SetMode(PuzzleZoneMode mode, TurnInfo turnInfo)
        {
            _button!.interactable = mode != PuzzleZoneMode.Disabled;

            if (mode == PuzzleZoneMode.TakePuzzle && CanTakePuzzle()) {
                Action onSelect = () => {
                    TakePuzzleAction.Options option = _isBlack ? TakePuzzleAction.Options.TopBlack : TakePuzzleAction.Options.TopWhite;
                    var action = new TakePuzzleAction(option);
                    PuzzleZoneManager.Instance!.ReportTakePuzzleChange(new(action));
                };
                Action onCancel = () => {
                    PuzzleZoneManager.Instance!.ReportTakePuzzleChange(new(null));
                };
                PuzzleZoneManager.AddToRadioButtonGroup(_button, onSelect, onCancel);
            }
            else {
                PuzzleZoneManager.RemoveFromRadioButtonGroup(_button);
            }

            bool CanTakePuzzle()
            {
                if (_deckSize == 0)
                    return false;

                if (!_isBlack)
                    return true;

                if (turnInfo.GamePhase == GamePhase.EndOfTheGame && turnInfo.TookBlackPuzzle)
                    return false;

                return true;
            }
        }
    }
}
