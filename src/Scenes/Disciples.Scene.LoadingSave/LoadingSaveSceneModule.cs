using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Scenes;
using Disciples.Resources.Common;
using DryIoc;

namespace Disciples.Scene.LoadingSave;

/// <summary>
/// Модуль регистрации для сцены сейва.
/// </summary>
public class LoadingSaveSceneModule : IGameModule
{
    /// <inheritdoc />
    public void Initialize(IRegistrator containerRegistrator)
    {
        var sceneScopeReuse = new CurrentScopeReuse(nameof(ILoadingSaveScene));
        containerRegistrator.Register<ILoadingSaveScene, LoadingSaveScene>(sceneScopeReuse);
        containerRegistrator.Register<ISceneInterfaceController, SceneInterfaceController>(sceneScopeReuse);
        containerRegistrator.RegisterDelegate<IReadOnlyList<BaseMqdbResourceExtractor>>(context => new BaseMqdbResourceExtractor[]
        {
            context.Resolve<BattleImagesExtractor>(),
            context.Resolve<BattleSoundsExtractor>(),
            context.Resolve<BattleSoundsMappingExtractor>(),
            context.Resolve<BattleUnitImagesExtractor>(),
            context.Resolve<UnitFaceImagesExtractor>(),
            context.Resolve<UnitPortraitImagesExtractor>(),
        }, sceneScopeReuse);
    }
}