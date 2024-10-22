﻿using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Common.Controllers;

namespace Disciples.Scene.LoadSaga.Controllers;

/// <summary>
/// Контроллер сцены загрузки сейва сценария.
/// </summary>
internal class LoadSagaInterfaceController : BaseLoadSaveSceneInterfaceController
{
    /// <summary>
    /// Создать объект типа <see cref="LoadSagaInterfaceController" />.
    /// </summary>
    public LoadSagaInterfaceController(
        ISaveProvider saveProvider,
        IGameController gameController,
        ITextProvider textProvider,
        ISceneInterfaceController sceneInterfaceController,
        IRaceProvider raceProvider,
        IInterfaceProvider interfaceProvider
        ) : base(saveProvider, gameController, textProvider, sceneInterfaceController, raceProvider, interfaceProvider)
    {
    }

    /// <inheritdoc />
    protected override string SceneInterfaceName => "DLG_LOAD";

    /// <inheritdoc />
    protected override MissionType MissionType => MissionType.Saga;
}