using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Resources.Database.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="IRaceProvider" />
internal class RaceProvider : BaseSupportLoading, IRaceProvider
{
    private readonly IInterfaceProvider _interfaceProvider;
    private readonly IMapper _mapper;
    private readonly GameDataContextFactory _gameDataContextFactory;

    private Dictionary<RaceType, Race> _races = null!;
    private Dictionary<RaceType, IBitmap> _raceImages = null!;

    /// <summary>
    /// Создать объект типа <see cref="RaceProvider" />.
    /// </summary>
    public RaceProvider(IInterfaceProvider interfaceProvider, IMapper mapper, GameDataContextFactory gameDataContextFactory)
    {
        _interfaceProvider = interfaceProvider;
        _mapper = mapper;
        _gameDataContextFactory = gameDataContextFactory;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        using var context = _gameDataContextFactory.Create();
        _races = context
            .Races
            .Include(r => r.Name)
            .Select(r => _mapper.Map<Race>(r))
            .ToDictionary(r => r.RaceType, r => r);
        _raceImages = LoadRaceImages();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        _races.Clear();
        _raceImages.Clear();
    }

    /// <inheritdoc />
    public Race GetRace(RaceType raceType)
    {
        return _races[raceType];
    }

    /// <inheritdoc />
    public IBitmap GetRaceImage(RaceType raceType)
    {
        return _raceImages[raceType];
    }

    /// <summary>
    /// Загрузить расу из ресурсов.
    /// </summary>
    /// <remarks>
    /// Для нейтралов нет изображения расы.
    /// </remarks>
    private Dictionary<RaceType, IBitmap> LoadRaceImages()
    {
        return new Dictionary<RaceType, IBitmap>
        {
            { RaceType.Human, _interfaceProvider.GetImage("GODHU") },
            { RaceType.Undead, _interfaceProvider.GetImage("GODUN") },
            { RaceType.Heretic, _interfaceProvider.GetImage("GODHE") },
            { RaceType.Dwarf, _interfaceProvider.GetImage("GODDW") },
            { RaceType.Elf, _interfaceProvider.GetImage("GODEL") },
        };
    }
}