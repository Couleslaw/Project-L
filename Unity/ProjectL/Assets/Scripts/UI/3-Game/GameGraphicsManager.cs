#nullable enable

namespace ProjectL.UI.GameScene
{
    using UnityEngine;
    using ProjectLCore.GameLogic;
    using ProjectL.UI.GameScene.Zones.PlayerZone;
    using ProjectL.UI.GameScene.Zones.PuzzleZone;
    using ProjectL.UI.GameScene.Zones.ActionZones;
    using ProjectL.UI.GameScene.Zones.PieceZone;

    public abstract class GameZoneManager<TSelf> : StaticInstance<TSelf> where TSelf : GameZoneManager<TSelf>
    {
        public abstract void Init(GameCore game);
    }

    public class GameGraphicsManager : GameZoneManager<GameGraphicsManager>
    {
        public override void Init(GameCore game)
        {
            PuzzleZoneManager.Instance.Init(game);
            PlayerZoneManager.Instance.Init(game);
            ActionZoneManager.Instance.Init(game);
            PieceZoneManager.Instance.Init(game);
        }
    }
}
