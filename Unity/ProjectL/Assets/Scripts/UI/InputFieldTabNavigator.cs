using UnityEngine;
using UnityEngine.EventSystems; 
using TMPro; 
using System.Collections.Generic; 
using System.Linq;

#nullable enable

public class InputFieldTabNavigator : MonoBehaviour
{
    [Tooltip("Assign the TMP_InputFields in the desired TAB order.")]
    [SerializeField] private List<TMP_InputField>? inputFields;


    void Start()
    {
        if (inputFields == null || inputFields.Count == 0) {
            Debug.LogWarning("InputFieldTabNavigator: No input fields assigned.", this);
        }
        else if (inputFields.Any(field => field == null)) // Check for null entries
        {
            Debug.LogError("InputFieldTabNavigator: One or more assigned input fields are null! Please check the list in the Inspector.", this);
        }
    }

    void Update()
    {
        // Check if the TAB key was pressed this frame
        if (Input.GetKeyDown(KeyCode.Tab)) {
            // Check if any UI element is currently selected
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null) {
                return;
            }

            // Try to get an InputField component from the selected object
            TMP_InputField currentInputField = currentSelected.GetComponent<TMP_InputField>();

            // If the selected object is not an input field, or our list is invalid, do nothing
            if (currentInputField == null || inputFields == null || inputFields.Count <= 1) {
                return;
            }

            // Find the index of the currently selected input field in our list
            int currentIndex = inputFields.IndexOf(currentInputField);

            // If the selected input field isn't in our tracked list, do nothing
            if (currentIndex == -1) {
                return;
            }

            // Determine if SHIFT is held down for reverse navigation
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            int nextIndex;
            if (shiftHeld) {
                // Navigate backwards
                nextIndex = (currentIndex - 1 + inputFields.Count) % inputFields.Count;
            }
            else {
                // Navigate forwards
                nextIndex = (currentIndex + 1) % inputFields.Count;
            }

            // Select the next input field
            inputFields[nextIndex].ActivateInputField();
        }
    }

}