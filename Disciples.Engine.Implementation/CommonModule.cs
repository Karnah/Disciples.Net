using AutoMapper;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Providers;
using Disciples.Resources.Database;
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

        containerRegistrator.RegisterInstance(new Database("Resources"));

        containerRegistrator.Register<ISceneObjectContainer, SceneObjectContainer>(Reuse.Scoped);
        containerRegistrator.Register<IGameObjectContainer, GameObjectContainer>(Reuse.Scoped);


        var mapperConfiguration = new MapperConfiguration(cnf =>
            cnf.AddMaps(typeof(IGameController).GetAssembly()));
        mapperConfiguration.AssertConfigurationIsValid();
        containerRegistrator.RegisterInstance(mapperConfiguration.CreateMapper());
    }
}