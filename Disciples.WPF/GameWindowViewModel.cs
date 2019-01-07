using Disciples.Engine.Common.Controllers;

namespace Disciples.WPF
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
        /// Информация о сцене.
        /// </summary>
        public IScene Scene { get; }
    }
}