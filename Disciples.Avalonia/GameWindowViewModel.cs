using Disciples.Engine.Common.Controllers;

namespace Disciples.Avalonia
{
    /// <summary>
    /// ViewModel для окна игры.
    /// </summary>
    public class GameWindowViewModel
    {
        /// <inheritdoc />
        public GameWindowViewModel(IScene scene)
        {
            Scene = scene;
        }

        /// <summary>
        /// Все объекты, которые отрисовываются на сцене.
        /// </summary>
        public IScene Scene { get; }
    }
}