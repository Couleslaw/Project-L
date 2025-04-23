using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#nullable enable

/// <summary>
/// Implements the TAB-navigation logic for <see cref="TMP_InputField"/>s.
/// </summary>
public class InputFieldTabNavigator : MonoBehaviour
{
    #region Fields

    [Tooltip("Assign the TMP_InputFields in the desired TAB order.")]
    [SerializeField] private List<TMP_InputField>? inputFields;

    #endregion

    #region Methods

    private void Start()
    {
        // check that inputFields is not null and contains valid TMP_InputField references
        if (inputFields == null || inputFields.Count == 0) {
            Debug.LogWarning("InputFieldTabNavigator: No input fields assigned.", this);
        }
        else if (inputFields.Any(field => field == null)) {
            Debug.LogError("InputFieldTabNavigator: One or more assigned input fields are null! Please check the list in the Inspector.", this);
        }
    }

    private void Update()
    {
        // Check if the TAB key was pressed this frame
        if (Input.GetKeyDown(KeyCode.Tab)) {
            // Check if any UI element is currently selected
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null)
                return;

            // Try to get an InputField component from the selected object
            TMP_InputField currentInputField = currentSelected.GetComponent<TMP_InputField>();

            // If the selected object is not an input field, or our list is invalid, do nothing
            if (currentInputField == null || inputFields == null || inputFields.Count <= 1)
                return;

            // Find the index of the currently selected input field in our list
            int currentIndex = inputFields.IndexOf(currentInputField);

            // If the selected input field isn't in our tracked list, do nothing
            if (currentIndex == -1)
                return;

            // Determine if SHIFT is held down for reverse navigation
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            int nextIndex;
            // Navigate backwards
            if (shiftHeld)
                nextIndex = (currentIndex - 1 + inputFields.Count) % inputFields.Count;
            // Navigate forwards
            else
                nextIndex = (currentIndex + 1) % inputFields.Count;

            // Select the next input field
            inputFields[nextIndex].ActivateInputField();
        }
    }

    #endregion
}
