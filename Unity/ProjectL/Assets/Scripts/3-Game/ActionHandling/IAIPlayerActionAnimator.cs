#nullable enable

namespace ProjectL.GameScene.ActionHandling
{
    using ProjectLCore.GameActions;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAIPlayerActionAnimator<T> where T : GameAction
    {
        #region Methods

        Task Animate(T action, CancellationToken cancellationToken);

        #endregion
    }
}
