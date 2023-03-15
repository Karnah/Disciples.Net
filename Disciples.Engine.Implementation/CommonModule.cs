using System;
using System.IO;
using AutoMapper;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Providers;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Settings;
using Disciples.Resources.Database.Sqlite;
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

        containerRegistrator.RegisterDelegate(
            context => new GameDataContextFactory(context.Resolve<GameSettings>().DatabaseConnection),
            Reuse.Singleton);

        containerRegistrator.Register<ISceneObjectContainer, SceneObjectContainer>(Reuse.Scoped);
        containerRegistrator.Register<IGameObjectContainer, GameObjectContainer>(Reuse.Scoped);


        var mapperConfiguration = new MapperConfiguration(cnf =>
            cnf.AddMaps(typeof(IGameController).GetAssembly()));
        mapperConfiguration.AssertConfigurationIsValid();
        containerRegistrator.RegisterInstance(mapperConfiguration.CreateMapper());

        containerRegistrator.RegisterInstance(LoadSettings());
    }

    /// <summary>
    /// Загрузить настройки игры.
    /// </summary>
    private static GameSettings LoadSettings()
    {
        const string settingsFileName = "settings.json";
        try
        {
            return File
                .ReadAllText(settingsFileName)
                .DeserializeFromJson<GameSettings>();
        }
        catch (Exception e)
        {
            throw new Exception($"Не удалось загрузить файл с настройками {settingsFileName}", e);
        }
    }
}