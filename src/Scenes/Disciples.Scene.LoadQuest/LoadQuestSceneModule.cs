using DryIoc;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Scenes;
using Disciples.Scene.LoadQuest.Controllers;

namespace Disciples.Scene.LoadQuest;

/// <summary>
/// Модуль загрузки сцены сейва из саги <see cref="LoadQuestScene" />.
/// </summary>
public class LoadQuestSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var sceneScopeReuse = new CurrentScopeReuse(nameof(ILoadQuestScene));
        containerRegistrator.Register<ILoadQuestScene, LoadQuestScene>(sceneScopeReuse);
        containerRegistrator.Register<ISceneInterfaceController, SceneInterfaceController>(sceneScopeReuse);
        containerRegistrator.Register<LoadQuestInterfaceController>(sceneScopeReuse);
    }
}