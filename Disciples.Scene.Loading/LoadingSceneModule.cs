using Disciples.Engine.Base;
using DryIoc;

namespace Disciples.Scene.Loading;

/// <summary>
/// Модуль регистрации для сцены загрузки.
/// </summary>
public class LoadingSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        containerRegistrator.Register<LoadingScene>();
    }
}