#nullable enable

namespace ProjectL.GameScene.Management
{
    using ProjectLCore.GameLogic;
    using System.Collections.Generic;

    public class GameGraphicsSystem : StaticInstance<GameGraphicsSystem>
    {
        #region Fields

        private static int _numTotalManagers = 0;

        private GameCore? _game;

        private List<IGraphicsManager> _managersToRegister = new();

        private int _numRegisteredManagers = 0;

        #endregion

        public interface IGraphicsManager
        {
            #region Methods

            void Init(GameCore game);

            #endregion
        }

        #region Properties

        public bool IsReadyForInitialization => _numTotalManagers == _numRegisteredManagers;

        #endregion

        #region Methods

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
                return;
            }
            _game = game;
            foreach (var manager in _managersToRegister) {
                manager.Init(game);
            }
            _managersToRegister = null!;  // allow garbage collection
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _numTotalManagers = 0;
        }

        #endregion
    }
}
