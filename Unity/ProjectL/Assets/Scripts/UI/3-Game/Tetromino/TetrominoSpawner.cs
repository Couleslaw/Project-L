using UnityEngine;
using UnityEngine.EventSystems;

#nullable enable

public class TetrominoSpawner : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private DraggableTetromino? draggableTetrominoPrefab;
    private Camera? mainCamera;
    private DraggableTetromino? currentTetromino = null;

    void Start()
    {
        if (draggableTetrominoPrefab == null) {
            Debug.LogError("DraggableTetromino prefab is not assigned!", this);
            return;
        }
        mainCamera = Camera.main; // Cache the camera
    }

    // Called by EventSystem when pointer presses down ON THIS BUTTON
    public void OnPointerDown(PointerEventData eventData)
    {

        // --- Instantiate the Draggable Object ---
        // Convert mouse screen position to world position
        Vector3 spawnPosition = mainCamera!.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, mainCamera.nearClipPlane + 10f)); // Adjust Z as needed
        spawnPosition.z = 0; // Ensure Z is appropriate for 2D

        currentTetromino = Instantiate(draggableTetrominoPrefab, spawnPosition, Quaternion.identity);
        currentTetromino!.Init(mainCamera, null);
        currentTetromino.StartDragging(); 
    }

    // Called by EventSystem when pointer is released ANYWHERE after pressing down on this button
    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentTetromino != null) {
            currentTetromino.StopDragging();
            currentTetromino = null; // Release the reference
        }
    }
}
