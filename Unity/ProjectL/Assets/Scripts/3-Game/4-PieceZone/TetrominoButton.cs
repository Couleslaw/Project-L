#nullable enable

namespace ProjectL.GameScene.PieceZone
{
    using ProjectL.Animation;
    using ProjectL.GameScene.ActionHandling;
    using ProjectL.Sound;
    using ProjectLCore.GamePieces;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public enum SelectionSideEffect
    {
        None,
        GiveToPlayer,
        RemoveFromPlayer,
    }

    public enum SelectionButtonEffect
    {
        None,
        MakeBigger,
        MakeSmaller
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
    public class TetrominoButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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
                    _image.color = value ? ColorManager.gray : Color.white;
                }
                if (_button != null) {
                    _button.interactable = !value && _mode != PieceZoneMode.Disabled;
                }
            }
        }

        private bool CanBeUsed => _mode != PieceZoneMode.Disabled && !IsGrayedOut;

        private bool CanSpawn => _mode == PieceZoneMode.Spawning && !IsGrayedOut;

        #endregion

        #region Methods

        public DraggableTetromino SpawnTetromino(bool isAnimation = false)
        {
            if (draggableTetrominoPrefab == null) {
                throw new InvalidOperationException("DraggableTetromino prefab is not assigned!");
            }

            // play sound effect
            SoundManager.Instance.PlaySliderSound();

            // instantiate the tetromino prefab and initialize it
            DraggableTetromino tetromino = Instantiate(draggableTetrominoPrefab, transform.position, Quaternion.identity);
            tetromino.Init(this, isAnimation);
            tetromino.RemovedFromSceneEventHandler += () => TetrominoReturnedEventHandler?.Invoke(Shape);

            // notify listeners that a tetromino has been spawned
            TetrominoSpawnedEventHandler?.Invoke(Shape);

            // notify the HumanPlayerActionCreator to handle the action
            if (_mode != PieceZoneMode.Disabled) {
                HumanPlayerActionCreationManager.Instance.OnPlacePieceActionRequested();
            }

            return tetromino;
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
            _button!.interactable = CanBeUsed;
        }

        public DisposableButtonSelector GetDisposableButtonSelector(SelectionSideEffect sideEffect = SelectionSideEffect.None, SelectionButtonEffect buttonEffect = SelectionButtonEffect.MakeBigger)
        {
            return new DisposableButtonSelector(this, sideEffect, buttonEffect);
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

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            if (_mode == PieceZoneMode.Disabled) {
                return;
            }

            // if we are in spawning mode, spawn a new tetromino at the clicked position
            if (CanSpawn) {
                _currentTetromino = SpawnTetromino();
                return;
            }

            // if we are not spawning --> report click
            if (CanBeUsed) {
                PieceZoneManager.Instance.ReportButtonClick(this);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            // Called by EventSystem when pointer is released ANYWHERE after pressing down on this button
            if (eventData.button != PointerEventData.InputButton.Left) {
                return;
            }

            // notify the last spawned tetromino to stop dragging
            if (_currentTetromino != null) {
                _currentTetromino.StopDragging();
                _currentTetromino = null;
            }
        }

        #endregion

        public class DisposableButtonSelector : IDisposable
        {
            #region Constants

            private const float _temporaryScaleIncrease = 1.3f;

            #endregion

            #region Fields

            private RectTransform _spawnerRectTransform;

            private SelectionButtonEffect _buttonEffect;

            private SelectionSideEffect _effect;

            private List<IDisposable> _temporaryEffects = new();

            #endregion

            #region Constructors

            public DisposableButtonSelector(TetrominoButton spawner, SelectionSideEffect sideEffect, SelectionButtonEffect buttonEffect)
            {
                SoundManager.Instance?.PlaySoftTapSoundEffect();
                _spawnerRectTransform = spawner.GetComponent<RectTransform>();

                _buttonEffect = buttonEffect;
                if (buttonEffect == SelectionButtonEffect.MakeBigger) {
                    _spawnerRectTransform.localScale *= _temporaryScaleIncrease;
                }
                if (buttonEffect == SelectionButtonEffect.MakeSmaller) {
                    _spawnerRectTransform.localScale /= _temporaryScaleIncrease;
                }

                _effect = sideEffect;
                if (_effect == SelectionSideEffect.GiveToPlayer) {
                    _temporaryEffects.Add(PlayerStatsManager.Instance.CurrentPieceColumn!.CreateTemporaryCountIncreaser(spawner.Shape));
                    _temporaryEffects.Add(SharedReserveManager.Instance.PieceColumn.CreateTemporaryCountDecreaser(spawner.Shape));
                }
                if (_effect == SelectionSideEffect.RemoveFromPlayer) {
                    _temporaryEffects.Add(PlayerStatsManager.Instance.CurrentPieceColumn!.CreateTemporaryCountDecreaser(spawner.Shape));
                    _temporaryEffects.Add(SharedReserveManager.Instance.PieceColumn.CreateTemporaryCountIncreaser(spawner.Shape));

                }
            }

            #endregion

            #region Methods

            public void Dispose()
            {
                if (_buttonEffect == SelectionButtonEffect.MakeBigger) {
                    _spawnerRectTransform.localScale /= _temporaryScaleIncrease;
                }
                if (_buttonEffect == SelectionButtonEffect.MakeSmaller) {
                    _spawnerRectTransform.localScale *= _temporaryScaleIncrease;
                }

                foreach (var effect in _temporaryEffects) {
                    effect.Dispose();
                }
                _temporaryEffects.Clear();
            }

            #endregion
        }
    }
}
