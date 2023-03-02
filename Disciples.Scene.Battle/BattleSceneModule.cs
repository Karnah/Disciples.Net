using Disciples.Engine.Base;
using Disciples.Engine.Common.Providers;
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
        containerRegistrator.Register<BattleContext>(Reuse.Singleton);
        containerRegistrator.Register<BattleProcessor>(Reuse.Singleton);
        containerRegistrator.Register<IBattleResourceProvider, BattleResourceProvider>(Reuse.Singleton);
        containerRegistrator.Register<IBattleInterfaceProvider, BattleInterfaceProvider>(Reuse.Singleton);
        containerRegistrator.Register<IBattleUnitResourceProvider, BattleUnitResourceProvider>(Reuse.Singleton);
        containerRegistrator.Register<IBattleController, BattleController>(Reuse.Singleton);
        containerRegistrator.Register<IBattleInterfaceController, BattleInterfaceController>(Reuse.Singleton);
        containerRegistrator.RegisterMany<BattleScene>(Reuse.Singleton);
    }
}