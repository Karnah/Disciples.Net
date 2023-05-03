using Disciples.Engine.Base;
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
        var loadingScopeReuse = new CurrentScopeReuse(nameof(ILoadingSaveScene));
        containerRegistrator.Register<ILoadingSaveScene, LoadingSaveScene>(loadingScopeReuse);
        containerRegistrator.RegisterDelegate<IReadOnlyList<BaseResourceExtractor>>(context => new BaseResourceExtractor[]
        {
            context.Resolve<BattleImagesExtractor>(),
            context.Resolve<BattleSoundsExtractor>(),
            context.Resolve<BattleSoundsMappingExtractor>(),
            context.Resolve<BattleUnitImagesExtractor>(),
            context.Resolve<UnitFaceImagesExtractor>(),
            context.Resolve<UnitPortraitImagesExtractor>(),
        }, loadingScopeReuse);
    }
}