﻿using Disciples.Engine.Base;
using Disciples.Engine.Scenes;
using Disciples.Scene.LoadSaga.Controllers;
using Disciples.Scene.LoadSaga.Providers;
using DryIoc;

namespace Disciples.Scene.LoadSaga;

/// <summary>
/// Модуль загрузки сцены сейва из саги <see cref="LoadSagaScene" />.
/// </summary>
public class LoadSagaSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var loadingScopeReuse = new CurrentScopeReuse(nameof(ILoadSagaScene));
        containerRegistrator.Register<ILoadSagaScene, LoadSagaScene>(loadingScopeReuse);
        containerRegistrator.Register<LoadSagaInterfaceProvider>(loadingScopeReuse);
        containerRegistrator.Register<LoadSagaInterfaceController>(loadingScopeReuse);
        containerRegistrator.Register<LoadSagaSoundController>(loadingScopeReuse);
        containerRegistrator.Register<LoadSagaGameObjectContainer>(loadingScopeReuse);
        containerRegistrator.Register<SaveProvider>(loadingScopeReuse);
    }
}