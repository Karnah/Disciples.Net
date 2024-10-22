﻿using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Engine.Settings;
using Disciples.Scene.LoadSaga.Controllers;

namespace Disciples.Scene.LoadSaga;

/// <inheritdoc cref="ILoadSagaScene" />
internal class LoadSagaScene : BaseMenuScene, ILoadSagaScene
{
    private readonly LoadSagaInterfaceController _interfaceController;
    private readonly MenuSoundController _soundController;
    private readonly GameSettings _settings;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaScene" />.
    /// </summary>
    public LoadSagaScene(
        IGameObjectContainer gameObjectContainer,
        ISceneObjectContainer sceneObjectContainer,
        IDialogController dialogController,
        IInterfaceProvider interfaceProvider,
        LoadSagaInterfaceController interfaceController,
        MenuSoundController soundController,
        GameSettings settings
        ) : base(gameObjectContainer, sceneObjectContainer, dialogController, interfaceProvider)
    {
        _interfaceController = interfaceController;
        _soundController = soundController;
        _settings = settings;
    }

    /// <inheritdoc />
    protected override string? TransitionAnimationName => _settings.IsBrokenTransitionAnimationsDisabled
        ? null
        : "TRANS_SINGLE2NEW";

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        base.LoadInternal();

        _soundController.Load();
        _interfaceController.Load();
    }

    protected override void UnloadInternal()
    {
        base.UnloadInternal();

        _soundController.Unload();
    }

    /// <inheritdoc />
    protected override void BeforeSceneUpdate(UpdateSceneData data)
    {
        base.BeforeSceneUpdate(data);

        _soundController.BeforeSceneUpdate();
        _interfaceController.BeforeSceneUpdate();
    }

    /// <inheritdoc />
    protected override void AfterSceneUpdate()
    {
        base.AfterSceneUpdate();

        _soundController.AfterSceneUpdate();
        _interfaceController.AfterSceneUpdate();
    }
}