#nullable enable

namespace ProjectL.UI.GameScene
{
    using UnityEngine;
    using ProjectLCore.GameLogic;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.GameScene.Zones.PuzzleZone;
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.GameScene.Zones.PieceZone;
    using System.Collections.Generic;

    public abstract class GraphicsManager<TSelf> : StaticInstance<TSelf>, GameGraphicsSystem.IGraphicsManager
        where TSelf : GraphicsManager<TSelf>
    {

        protected override void Awake()
        {
            base.Awake();
            GameGraphicsSystem.ReportNewManagerCreated();
        }

        protected virtual void Start()
        {
            if (GameGraphicsSystem.Instance == null) {
                Debug.LogError("GameGraphicsSystem is not initialized.", this);
                return;
            }
            // register in Start so that components of this class can be initialized in Awake
            GameGraphicsSystem.Instance.Register(this);
        }

        public abstract void Init(GameCore game);
    }

    public class GameGraphicsSystem : StaticInstance<GameGraphicsSystem>
    {
        public static Color ActiveColor { get; } = Color.white;
        public static Color InactiveColor { get; } = new Color(0.27f, 0.27f, 0.27f);

        private GameCore? _game;

        private List<IGraphicsManager> _managersToRegister = new();

        private static int _numTotalManagers = 0;
        private int _numRegisteredManagers = 0;

        public bool IsReadyForInitialization => _numTotalManagers == _numRegisteredManagers;

        public static void ReportNewManagerCreated()
        {
            _numTotalManagers++;
        }

        public void Register(IGraphicsManager manager)
        {
            _numRegisteredManagers++;
            if (_game != null) {
                manager.Init(_game);
            }
            else {
                _managersToRegister.Add(manager);
            }
        }

        public void Init(GameCore game)
        {
            if (_game != null) {
                Debug.LogError("GameGraphicsSystem is already initialized.", this);
                return;
            }
            _game = game;
            foreach (var manager in _managersToRegister) {
                manager.Init(game);
            }
            _managersToRegister = null!;  // allow garbage collection
        }

        public interface IGraphicsManager
        {
            void Init(GameCore game);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _numTotalManagers = 0;
        }
    }
}
