using Disciples.Engine;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;

namespace Disciples.Scene.LoadSaga.Providers;

/// <summary>
/// Провайдер для интерфейса сцены.
/// </summary>
internal class LoadSagaInterfaceProvider : BaseSupportLoading
{
    private readonly IInterfaceProvider _interfaceProvider;

    /// <summary>
    /// Создать объект типа <see cref="LoadSagaInterfaceProvider" />.
    /// </summary>
    public LoadSagaInterfaceProvider(IInterfaceProvider interfaceProvider)
    {
        _interfaceProvider = interfaceProvider;
    }

    /// <summary>
    /// Интерфейс сцены.
    /// </summary>
    public SceneInterface SceneInterface { get; private set; } = null!;

    /// <summary>
    /// Картинки рас.
    /// </summary>
    public IReadOnlyDictionary<RaceType, IBitmap> Races { get; private set; } = null!;

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        SceneInterface = _interfaceProvider.GetSceneInterface("DLG_LOAD");

        var races = _interfaceProvider.GetImageParts("GODFACES.PNG");
        Races = new Dictionary<RaceType, IBitmap>
        {
            { RaceType.Human, races["GODHU"] },
            { RaceType.Undead, races["GODUN"] },
            { RaceType.Heretic, races["GODHE"] },
            { RaceType.Dwarf, races["GODDW"] },
            { RaceType.Elf, races["GODEL"] },
        };
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }
}