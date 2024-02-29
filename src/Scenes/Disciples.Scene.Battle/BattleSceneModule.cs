using DryIoc;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Scenes;
using Disciples.Scene.Battle.Controllers;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;

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

        RegisterImplementationTypes<IAttackTypeProcessor>(containerRegistrator, sceneScopeReuse);
        containerRegistrator.RegisterDelegate<IReadOnlyDictionary<UnitAttackType, IAttackTypeProcessor>>(
            resolverContext =>
            {
                return resolverContext
                    .ResolveMany<IAttackTypeProcessor>()
                    .ToDictionary(p => p.AttackType, p => p);
            }, sceneScopeReuse);

        // На каждое действие создаётся новый контроллер.
        RegisterImplementationTypes<IBattleUnitActionController>(containerRegistrator, Reuse.Transient);

        containerRegistrator.Register<BattleProcessor>(sceneScopeReuse);
        containerRegistrator.Register<BattleAiProcessor>(sceneScopeReuse);
        containerRegistrator.Register<BattleInstantProcessor>(sceneScopeReuse);
        containerRegistrator.Register<BattleUnitPortraitPanelController>(sceneScopeReuse);
        containerRegistrator.Register<BattleBottomPanelController>(sceneScopeReuse);
        containerRegistrator.Register<BattleUnitActionFactory>(sceneScopeReuse);
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

    /// <summary>
    /// Зарегистрировать всех наследников указанного типа.
    /// </summary>
    private static void RegisterImplementationTypes<T>(IRegistrator containerRegistrator, IReuse reuse)
    {
        var types = typeof(BattleSceneModule)
            .Assembly
            .GetImplementationTypes(t => t.IsAssignableTo<T>());
        foreach (var type in types)
        {
            containerRegistrator.RegisterMany(new []{ typeof(T), type }, type, reuse);
        }
    }
}