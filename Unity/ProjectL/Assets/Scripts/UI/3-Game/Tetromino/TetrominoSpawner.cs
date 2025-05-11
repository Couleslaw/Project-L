#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectLCore.GameActions;
    using ProjectLCore.GamePieces;
    using System;
    using System.Threading;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public interface ITetrominoSpawnerListener
    {
        #region Methods

        void OnTetrominoSpawned(TetrominoShape tetromino);

        void OnTetrominoReturned(TetrominoShape tetromino);

        #endregion
    }

    [RequireComponent(typeof(Image))]
    public class TetrominoSpawner : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Fields

        [SerializeField] private DraggableTetromino? draggableTetrominoPrefab;

        private Camera? mainCamera;

        private DraggableTetromino? _currentTetromino = null;
        private Image? _image;
        private bool _spawningEnabled = false;

        #endregion

        #region Events

        private event Action<TetrominoShape>? TetrominoSpawned;

        private event Action<TetrominoShape>? TetrominoReturned;

        #endregion

        #region Properties

        public TetrominoShape Shape => draggableTetrominoPrefab!.Shape;

        #endregion

        #region Methods

        // Called by EventSystem when pointer presses down ON THIS BUTTON
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_spawningEnabled == false) {
                return;
            }

            // --- Instantiate the Draggable Object ---
            // Convert mouse screen position to world position
            Vector3 spawnPosition = mainCamera!.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, mainCamera.nearClipPlane + 10f)); // Adjust Z as needed
            spawnPosition.z = 0; // Ensure Z is appropriate for 2D

            _currentTetromino = SpawnTetromino();
            _currentTetromino.StartDragging();
        }

        public DraggableTetromino SpawnTetromino()
        {
            if (draggableTetrominoPrefab == null) {
                throw new InvalidOperationException("DraggableTetromino prefab is not assigned!");
            }
            DraggableTetromino tetromino = Instantiate(draggableTetrominoPrefab, transform.position, Quaternion.identity);
            tetromino.Init(mainCamera!, () => TetrominoReturned?.Invoke(Shape));
            TetrominoSpawned?.Invoke(Shape);
            return tetromino;
        }

        // Called by EventSystem when pointer is released ANYWHERE after pressing down on this button
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_currentTetromino != null) {
                _currentTetromino.StopDragging();
                _currentTetromino = null; // Release the reference
            }
        }

        public void AddListener(ITetrominoSpawnerListener listener)
        {
            TetrominoSpawned += listener.OnTetrominoSpawned;
            TetrominoReturned += listener.OnTetrominoReturned;
        }

        public void RemoveListener(ITetrominoSpawnerListener listener)
        {
            TetrominoSpawned -= listener.OnTetrominoSpawned;
            TetrominoReturned -= listener.OnTetrominoReturned;
        }

        internal void Start()
        {
            if (draggableTetrominoPrefab == null) {
                Debug.LogError("DraggableTetromino prefab is not assigned!", this);
                return;
            }
            _image = GetComponent<Image>();
            mainCamera = Camera.main; // Cache the camera
        }

        public void GrayOut(bool gray)
        {
            if (_image == null) {
                return;
            }
            _image.color = gray ? Color.gray : Color.white;
        }

        public void EnableSpawner(bool enable)
        {
            _spawningEnabled = enable;
        }

        #endregion
    }
}
