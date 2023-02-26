using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Loading
{
    /// <summary>
    /// Сцена загрузки приложения.
    /// </summary>
    public interface ILoadingSceneController : ISceneController, IScene, ISupportLoadingWithParameters<SceneParameters>
    {
    }
}