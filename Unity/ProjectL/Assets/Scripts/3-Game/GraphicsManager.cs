#nullable enable

namespace ProjectL.GameScene
{
    using ProjectL.GameScene.Management;
    using ProjectLCore.GameLogic;
    using UnityEngine;

    public abstract class GraphicsManager<TSelf> : StaticInstance<TSelf>, GameGraphicsSystem.IGraphicsManager
        where TSelf : GraphicsManager<TSelf>
    {
        #region Methods

        public abstract void Init(GameCore game);

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

        #endregion
    }
}
