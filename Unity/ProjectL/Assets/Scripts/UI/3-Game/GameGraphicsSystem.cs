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

        protected virtual void Start()
        {
            if (GameGraphicsSystem.Instance != null) {
                GameGraphicsSystem.Instance.Register(this);
            }
            else {
                Debug.LogError($"GameGraphicsSystem instance is null.", this);
            }
        }

        public abstract void Init(GameCore game);
    }

    public class GameGraphicsSystem : StaticInstance<GameGraphicsSystem>
    {
        public static Color ActivePlayerColor { get; } = Color.white;
        public static Color InactivePlayerColor { get; } = Color.gray;

        private List<IGraphicsManager> _graphicsManagers = new();

        private GameCore? _game;

        public void Register(IGraphicsManager manager)
        {
            if (_game != null) {
                manager.Init(_game);
            }
            else {
                _graphicsManagers.Add(manager);
            }
        }

        public void Init(GameCore game)
        {
            if (_game != null) {
                Debug.LogError("GameGraphicsSystem is already initialized.", this);
                return;
            }
            _game = game;
            foreach (var manager in _graphicsManagers) {
                manager.Init(game);
            }
            _graphicsManagers = null!;  // allow garbage collection
        }

        public interface IGraphicsManager
        {
            void Init(GameCore game);
        }
    }
}
