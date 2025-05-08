#nullable enable

namespace ProjectL.UI.GameScene
{
    using UnityEngine;
    using ProjectLCore.GameLogic;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.GameScene.Zones.PuzzleZone;
    using ProjectL.UI.GameScene.Zones.ActionZones;


    public interface IGameZoneManager
    {
        void Init(GameCore game);
    }

    public class GameGraphicsManager : StaticInstance<GameGraphicsManager>, IGameZoneManager
    {
        public void Init(GameCore game)
        {
            PuzzleZoneManager.Instance.Init(game);
            PlayerZoneManager.Instance.Init(game);
            ActionZoneManager.Instance.Init(game);
        }
    }
}
