using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Scene.Loading;

/// <summary>
/// Сцена, которая отображается при переходе между двумя "тяжелыми" сценами.
/// </summary>
public interface ILoadingScene : IScene, ISupportLoadingWithParameters<SceneParameters>
{
}