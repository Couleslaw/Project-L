#nullable enable

namespace ProjectL.UI.GameScene.Zones.PieceZone
{
    using ProjectL.UI.GameScene.Actions;
    using ProjectL.UI.Sound;
    using ProjectLCore.GamePieces;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public enum SelectionEffect
    {
        None,
        GiveToPlayer,
        RemoveFromPlayer,
    }

    public interface ITetrominoSpawnerListener
    {
        #region Methods

        void OnTetrominoSpawned(TetrominoShape tetromino);

        void OnTetrominoReturned(TetrominoShape tetromino);

        #endregion
    }

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class TetrominoSpawner : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Fields

        [SerializeField] private DraggableTetromino? draggableTetrominoPrefab;

        private Camera? _mainCamera;

        private DraggableTetromino? _currentTetromino = null;

        private Image? _image;

        private Button? _button;

        private PieceZoneMode _mode = PieceZoneMode.Disabled;

        private bool _isGrayedOut = false;

        #endregion

        #region Events

        private event Action<TetrominoShape>? TetrominoSpawnedEventHandler;

        private event Action<TetrominoShape>? TetrominoReturnedEventHandler;

        #endregion

        #region Properties


        public TetrominoShape Shape => draggableTetrominoPrefab!.Shape;

        public bool IsGrayedOut {
            get => _isGrayedOut;
            set {
                _isGrayedOut = value;
                if (_image != null) {
                    _image.color = value ? GameGraphicsSystem.InactiveColor : Color.white;
                }
                _button!.interactable = !value && _mode != PieceZoneMode.Disabled;
            }
        }

        private bool CanBeUsed => _mode != PieceZoneMode.Disabled && !IsGrayedOut;
        private bool CanSpawn => CanBeUsed  && _mode == PieceZoneMode.Spawning;

        #endregion

        #region Methods

        // Called by EventSystem when pointer presses down ON THIS BUTTON
        public void OnPointerDown(PointerEventData eventData)
        {
            if (CanSpawn == false) {
                return;
            }

            // --- Instantiate the Draggable Object ---
            // Convert mouse screen position to world position
            Vector3 spawnPosition = _mainCamera!.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, _mainCamera.nearClipPlane + 10f)); // Adjust Z as needed
            spawnPosition.z = 0; // Ensure Z is appropriate for 2D

            _currentTetromino = SpawnTetromino();
            _currentTetromino.StartDragging();
        }

        public DraggableTetromino SpawnTetromino()
        {
            if (draggableTetrominoPrefab == null) {
                throw new InvalidOperationException("DraggableTetromino prefab is not assigned!");
            }
            SoundManager.Instance?.PlaySliderSound();
            DraggableTetromino tetromino = Instantiate(draggableTetrominoPrefab, transform.position, Quaternion.identity);
            tetromino.Init(_mainCamera!, TetrominoReturnedEventHandler);
            TetrominoSpawnedEventHandler?.Invoke(Shape);
            if (_mode != PieceZoneMode.Disabled) {
                HumanPlayerActionCreator.Instance?.OnPlacePieceActionRequested();
            }
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
            TetrominoSpawnedEventHandler += listener.OnTetrominoSpawned;
            TetrominoReturnedEventHandler += listener.OnTetrominoReturned;
        }

        public void RemoveListener(ITetrominoSpawnerListener listener)
        {
            TetrominoSpawnedEventHandler -= listener.OnTetrominoSpawned;
            TetrominoReturnedEventHandler -= listener.OnTetrominoReturned;
        }


        public void SetMode(PieceZoneMode mode)
        {
            _mode = mode;
            switch (mode) {
                case PieceZoneMode.Disabled:
                    _button!.interactable = CanBeUsed;
                    break;
                case PieceZoneMode.Spawning:
                    break;
                case PieceZoneMode.TakeBasicTetromino:
                    break;
                case PieceZoneMode.ChangeTetromino:
                    break;
                case PieceZoneMode.SelectReward:
                    break;
                default:
                    break;
            }
        }

        public TemporaryButtonSelector CreateTemporaryButtonSelector(SelectionEffect effect = SelectionEffect.None)
        {
            return new TemporaryButtonSelector(this, effect);
        }

        private void Awake()
        {
            if (draggableTetrominoPrefab == null) {
                Debug.LogError("DraggableTetromino prefab is not assigned!", this);
                return;
            }
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
            _mainCamera = Camera.main; // Cache the camera
        }

        #endregion

        public class TemporaryButtonSelector : IDisposable
        {
            #region Constants

            private const float _temporaryScaleIncrease = 1.3f;

            #endregion

            #region Fields

            internal RectTransform _spawnerRectTransform;

            internal TetrominoSpawner _spawner;

            internal SelectionEffect _effect;

            #endregion

            #region Constructors

            public TemporaryButtonSelector(TetrominoSpawner spawner, SelectionEffect effect)
            {
                SoundManager.Instance?.PlaySoftTapSoundEffect();
                _spawnerRectTransform = spawner.GetComponent<RectTransform>();
                _spawnerRectTransform.localScale *= _temporaryScaleIncrease;

                _effect = effect;
                _spawner = spawner;
                if (_effect == SelectionEffect.GiveToPlayer) {
                    _spawner.TetrominoReturnedEventHandler?.Invoke(_spawner.Shape);
                }
                if (_effect == SelectionEffect.RemoveFromPlayer) {
                    _spawner.TetrominoSpawnedEventHandler?.Invoke(_spawner.Shape);
                }
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                _spawnerRectTransform.localScale /= _temporaryScaleIncrease;

                if (_effect == SelectionEffect.GiveToPlayer) {
                    _spawner.TetrominoSpawnedEventHandler?.Invoke(_spawner.Shape);
                }
                if (_effect == SelectionEffect.RemoveFromPlayer) {
                    _spawner.TetrominoReturnedEventHandler?.Invoke(_spawner.Shape);
                }
            }

            #endregion
        }
    }
}
