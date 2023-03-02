using Disciples.Engine.Base;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Providers;
using DryIoc;

namespace Disciples.Engine.Implementation;

/// <summary>
/// Модуль для регистрации общих зависимостей.
/// </summary>
public class CommonModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        containerRegistrator.Register<ILogger, Logger>(Reuse.Singleton);

        containerRegistrator.RegisterMany<GameController>(Reuse.Singleton);

        containerRegistrator.Register<ITextProvider, TextProvider>(Reuse.Singleton);
        containerRegistrator.Register<IInterfaceProvider, InterfaceProvider>(Reuse.Singleton);
        containerRegistrator.Register<IUnitInfoProvider, UnitInfoProvider>(Reuse.Singleton);


        containerRegistrator.Register<ISceneObjectContainer, SceneObjectContainer>(Reuse.Scoped);
        containerRegistrator.Register<IGameObjectContainer, GameObjectContainer>(Reuse.Scoped);
    }
}