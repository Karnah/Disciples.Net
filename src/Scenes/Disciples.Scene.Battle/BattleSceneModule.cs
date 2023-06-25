using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Scenes;
using Disciples.Scene.Battle.Controllers;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;
using DryIoc;

namespace Disciples.Scene.Battle;

/// <summary>
/// Модуль для регистрации сцены битвы двух отрядов.
/// </summary>
public class BattleSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var sceneScopeReuse = new CurrentScopeReuse(nameof(IBattleScene));
        containerRegistrator.Register<BattleContext>(sceneScopeReuse);
        containerRegistrator.Register<BattleProcessor>(sceneScopeReuse);
        containerRegistrator.Register<BattleAiProcessor>(sceneScopeReuse);
        containerRegistrator.Register<BattleUnitPortraitPanelController>(sceneScopeReuse);
        containerRegistrator.Register<BattleBottomPanelController>(sceneScopeReuse);
        containerRegistrator.Register<BattleUnitActionController>(sceneScopeReuse);
        containerRegistrator.Register<BattleSoundController>(sceneScopeReuse);
        containerRegistrator.Register<BattleDialogController>(sceneScopeReuse);
        containerRegistrator.Register<IBattleResourceProvider, BattleResourceProvider>(sceneScopeReuse);
        containerRegistrator.Register<ISceneInterfaceController, BattleSceneInterfaceController>(sceneScopeReuse);
        containerRegistrator.Register<IBattleInterfaceProvider, BattleInterfaceProvider>(sceneScopeReuse);
        containerRegistrator.Register<IBattleUnitResourceProvider, BattleUnitResourceProvider>(sceneScopeReuse);
        containerRegistrator.Register<IBattleController, BattleController>(sceneScopeReuse);
        containerRegistrator.Register<IBattleInterfaceController, BattleInterfaceController>(sceneScopeReuse);
        containerRegistrator.Register<IBattleGameObjectContainer, BattleGameObjectContainer>(sceneScopeReuse);
        containerRegistrator.Register<IBattleScene, BattleScene>(sceneScopeReuse);
    }
}