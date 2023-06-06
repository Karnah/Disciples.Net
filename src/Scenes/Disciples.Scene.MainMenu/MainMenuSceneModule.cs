using DryIoc;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Scenes;
using Disciples.Scene.MainMenu.Controllers;

namespace Disciples.Scene.MainMenu;

/// <summary>
/// Модуль главного меню <see cref="MainMenuScene" />.
/// </summary>
public class MainMenuSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var loadingScopeReuse = new CurrentScopeReuse(nameof(IMainMenuScene));
        containerRegistrator.Register<IMainMenuScene, MainMenuScene>(loadingScopeReuse);
        containerRegistrator.Register<ISceneInterfaceController, SceneInterfaceController>(loadingScopeReuse);
        containerRegistrator.Register<MainMenuInterfaceController>(loadingScopeReuse);
    }
}