using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Engine.Scenes;

/// <summary>
/// Меню однопользовательской игры.
/// </summary>
public interface ISinglePlayerGameMenuScene : IScene, ISupportLoadingWithParameters<SceneParameters>
{
}