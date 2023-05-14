using Disciples.Engine.Base;
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
        var battleScopeReuse = new CurrentScopeReuse(nameof(IBattleScene));
        containerRegistrator.Register<BattleContext>(battleScopeReuse);
        containerRegistrator.Register<BattleProcessor>(battleScopeReuse);
        containerRegistrator.Register<BattleAiProcessor>(battleScopeReuse);
        containerRegistrator.Register<BattleUnitPortraitPanelController>(battleScopeReuse);
        containerRegistrator.Register<BattleBottomPanelController>(battleScopeReuse);
        containerRegistrator.Register<BattleUnitActionController>(battleScopeReuse);
        containerRegistrator.Register<BattleSoundController>(battleScopeReuse);
        containerRegistrator.Register<BattleDialogController>(battleScopeReuse);
        containerRegistrator.Register<IBattleResourceProvider, BattleResourceProvider>(battleScopeReuse);
        containerRegistrator.Register<IBattleInterfaceProvider, BattleInterfaceProvider>(battleScopeReuse);
        containerRegistrator.Register<IBattleUnitResourceProvider, BattleUnitResourceProvider>(battleScopeReuse);
        containerRegistrator.Register<IBattleController, BattleController>(battleScopeReuse);
        containerRegistrator.Register<IBattleInterfaceController, BattleInterfaceController>(battleScopeReuse);
        containerRegistrator.Register<IBattleGameObjectContainer, BattleGameObjectContainer>(battleScopeReuse);
        containerRegistrator.Register<IBattleScene, BattleScene>(battleScopeReuse);
    }
}