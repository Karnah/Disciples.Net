using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Common.Controllers;

namespace Disciples.Scene.LoadQuest.Controllers;

/// <summary>
/// Контроллер сцены загрузки сейва одиночного квеста.
/// </summary>
internal class LoadQuestInterfaceController : BaseLoadSaveSceneInterfaceController
{
    /// <summary>
    /// Создать объект типа <see cref="LoadQuestInterfaceController" />.
    /// </summary>
    public LoadQuestInterfaceController(
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
    protected override string SceneInterfaceName => "DLG_LOAD_TOURNEMENT";

    /// <inheritdoc />
    protected override MissionType MissionType => MissionType.Quest;
}