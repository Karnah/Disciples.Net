using System;
using System.IO;
using AutoMapper;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Implementation.Common.Providers;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
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

        containerRegistrator.Register<BattleImagesExtractor>(Reuse.Singleton);
        containerRegistrator.Register<BattleUnitImagesExtractor>(Reuse.Singleton);
        containerRegistrator.Register<InterfaceImagesExtractor>(Reuse.Singleton);
        containerRegistrator.Register<UnitFaceImagesExtractor>(Reuse.Singleton);
        containerRegistrator.Register<UnitPortraitImagesExtractor>(Reuse.Singleton);
        containerRegistrator.Register<SceneInterfaceExtractor>(Reuse.Singleton);

        containerRegistrator.Register<BattleSoundsMappingExtractor>(Reuse.Singleton);
        containerRegistrator.Register<BattleSoundsExtractor>(Reuse.Singleton);
        containerRegistrator.Register<ISoundController, BassSoundController>(Reuse.Singleton);
        //containerRegistrator.Register<ISoundController, NullSoundController>(Reuse.Singleton);
        containerRegistrator.Register<ISoundProvider, SoundProvider>(Reuse.Singleton);
        containerRegistrator.Register<MenuSoundController>(Reuse.Singleton);

        containerRegistrator.Register<IVideoProvider, VideoProvider>(Reuse.Singleton);

        containerRegistrator.Register<ITextProvider, TextProvider>(Reuse.Singleton);
        containerRegistrator.Register<IInterfaceProvider, InterfaceProvider>(Reuse.Singleton);
        containerRegistrator.Register<IUnitInfoProvider, UnitInfoProvider>(Reuse.Singleton);

        containerRegistrator.RegisterDelegate(
            context => new GameDataContextFactory(context.Resolve<GameSettings>().DatabaseConnection),
            Reuse.Singleton);

        containerRegistrator.Register<ISceneObjectContainer, SceneObjectContainer>(Reuse.Scoped);
        containerRegistrator.Register<IGameObjectContainer, GameObjectContainer>(Reuse.Scoped);
        containerRegistrator.Register<IDialogController, DialogController>(Reuse.Scoped);

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