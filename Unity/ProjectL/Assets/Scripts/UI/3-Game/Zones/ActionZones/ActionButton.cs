#nullable enable

namespace ProjectL.UI.GameScene.Zones.ActionZones
{
    using ProjectL.UI.Utils;
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class ActionButton : MonoBehaviour
    {

        private Button? _button;


        #region Properties

        public static event Action? CancelAction;

        public event Action? SelectAction;

        #endregion

        #region Methods

        public void DisableButton()
        {
            if (_button == null) {
                return;
            }
            RadioButtonsGroup.ForceDeselectButton(_button, nameof(ActionButton));
            _button.interactable = false;
        }

        public void EnableButton()
        {
            if (_button == null) {
                return;
            }
            _button.interactable = true;
        }

        public static void DeselectCurrentButton()
        {
            RadioButtonsGroup.ForceDeselectButton(nameof(ActionButton));
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button == null) {
                Debug.LogError("Button component is missing!", this);
                return;
            }

            if (_button.transition != Selectable.Transition.SpriteSwap) {
                Debug.LogError("Button transition is not set to SpriteSwap!", this);
                return;
            }

            RadioButtonsGroup.RegisterButton(_button, nameof(ActionButton), SelectAction, CancelAction);
        }

        #endregion
    }
}
