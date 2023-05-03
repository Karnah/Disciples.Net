using Disciples.Engine.Base;
using Disciples.Engine.Models;

namespace Disciples.Scene.LoadingGame;

/// <summary>
/// Сцена загрузки игры.
/// </summary>
public interface ILoadingGameScene : IScene, ISupportLoadingWithParameters<SceneParameters>
{
}