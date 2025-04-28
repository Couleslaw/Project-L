using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SyncTriggerToUI : MonoBehaviour
{
    private RectTransform uiElementToTrack;

    [Tooltip("The main camera rendering the world (used for coordinate conversion). Usually Camera.main.")]
    private Camera worldCamera;

    private BoxCollider2D triggerCollider;
    private Vector3[] worldCorners = new Vector3[4]; // Cache array to avoid GC alloc

    // Optional: Optimization - only update if screen size changes
    private int lastScreenWidth = 0;
    private int lastScreenHeight = 0;
    private bool needsUpdate = true; // Force update on first frame

    void Awake()
    {
        worldCamera = Camera.main; 
        triggerCollider = GetComponent<BoxCollider2D>();
    }

    public void Initialize(RectTransform elementToTrack)
    {
        uiElementToTrack = elementToTrack;
        UpdateTriggerBounds();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }


    void LateUpdate() // Use LateUpdate to ensure UI layout is finalized for the frame
    {
        // Optimization check
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
            needsUpdate = true;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }

        // Only update if necessary
        if (needsUpdate || uiElementToTrack.hasChanged) // hasChanged detects RectTransform changes
        {
            UpdateTriggerBounds();
            needsUpdate = false; // Reset flag until next screen resize
            uiElementToTrack.hasChanged = false; // Reset UI change flag
        }
    }

    void UpdateTriggerBounds()
    {
        if (uiElementToTrack == null || triggerCollider == null || worldCamera == null) {
            return; // Safety check
        }

        // Get the world corners of the UI element's RectTransform
        uiElementToTrack.GetWorldCorners(worldCorners);

        // Corners are: [0] Bottom Left, [1] Top Left, [2] Top Right, [3] Bottom Right

        // Calculate the center in world space
        Vector3 center = (worldCorners[0] + worldCorners[2]) / 2f;

        // Calculate the size in world space
        // Note: This assumes the UI element isn't rotated significantly in world space
        // For simple screen-aligned UI, this works.
        float worldWidth = Vector3.Distance(worldCorners[0], worldCorners[3]);
        float worldHeight = Vector3.Distance(worldCorners[0], worldCorners[1]);
        Vector2 worldSize = new Vector2(worldWidth, worldHeight);

        // --- Apply to the Trigger Zone ---

        // Set position (ensure Z is appropriate for your 2D setup, often 0)
        transform.position = new Vector3(center.x, center.y, transform.position.z);

        // Set collider size
        triggerCollider.size = worldSize;

        // Reset collider offset if it shouldn't be used
        triggerCollider.offset = Vector2.zero;

        // Debugging (Optional): Draw the bounds in Scene view
#if UNITY_EDITOR
        Debug.DrawLine(worldCorners[0], worldCorners[1], Color.cyan);
        Debug.DrawLine(worldCorners[1], worldCorners[2], Color.cyan);
        Debug.DrawLine(worldCorners[2], worldCorners[3], Color.cyan);
        Debug.DrawLine(worldCorners[3], worldCorners[0], Color.cyan);
#endif
    }
}