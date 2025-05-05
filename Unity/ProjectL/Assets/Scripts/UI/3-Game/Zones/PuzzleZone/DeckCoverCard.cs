#nullable enable

namespace ProjectL
{
    using TMPro;
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using ProjectL.UI.Utils;

    [RequireComponent(typeof(Button))]
    public class DeckCoverCard : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI? _label;
        private Button? _button;

        #endregion

        #region Methods

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button == null || _label == null) {
                Debug.LogError("One or more UI components is not assigned!", this);
                return;
            }

            DisableCard();
        }

        #endregion

        public void SetDeckSize(int n)
        {
            if (_label == null) {
                return;
            }
            _label.text = n.ToString();
        }

        public void DisableCard()
        {
            if (_button != null) {
                _button.interactable = false;
            }
        }

        public void EnableCard()
        {
            if (_button != null) {
                _button.interactable = true;
            }
        }

        public void AddToRadioButtonGroup(string groupName, Action? onSelect = null, Action? onCancel = null)
        {
            if (_button != null) {
                RadioButtonsGroup.RegisterButton(_button, groupName, onSelect, onCancel);
            }
        }
    }
}
