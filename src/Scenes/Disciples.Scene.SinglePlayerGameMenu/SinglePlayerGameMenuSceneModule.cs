using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Scenes;
using Disciples.Scene.SinglePlayerGameMenu.Controllers;
using DryIoc;

namespace Disciples.Scene.SinglePlayerGameMenu;

/// <summary>
/// Модуль меню одиночной игры <see cref="SinglePlayerGameMenuScene" />.
/// </summary>
public class SinglePlayerGameMenuSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var loadingScopeReuse = new CurrentScopeReuse(nameof(ISinglePlayerGameMenuScene));
        containerRegistrator.Register<ISinglePlayerGameMenuScene, SinglePlayerGameMenuScene>(loadingScopeReuse);
        containerRegistrator.Register<ISceneInterfaceController, SceneInterfaceController>(loadingScopeReuse);
        containerRegistrator.Register<SinglePlayerGameMenuInterfaceController>(loadingScopeReuse);
    }
}