using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Database.Sqlite;
using Microsoft.EntityFrameworkCore;
using UnitType = Disciples.Engine.Common.Models.UnitType;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="IUnitInfoProvider" />
public class UnitInfoProvider : BaseSupportLoading, IUnitInfoProvider
{
    private readonly IBitmapFactory _bitmapFactory;
    private readonly IMapper _mapper;
    private readonly GameDataContextFactory _gameDataContextFactory;
    private readonly UnitFaceImagesExtractor _unitFaceExtractor;
    private readonly UnitPortraitImagesExtractor _unitPortraitExtractor;

    private readonly Dictionary<string, UnitType> _unitTypes = new();
    private readonly Dictionary<string, IBitmap> _unitFaces = new();
    private readonly Dictionary<string, IBitmap> _unitBattleFaces = new();
    private readonly Dictionary<string, IBitmap> _unitPortraits = new ();

    /// <summary>
    /// Создать объект типа <see cref="UnitInfoProvider" />.
    /// </summary>
    public UnitInfoProvider(IBitmapFactory bitmapFactory,
        IMapper mapper,
        GameDataContextFactory gameDataContextFactory,
        UnitFaceImagesExtractor unitFaceExtractor,
        UnitPortraitImagesExtractor unitPortraitExtractor)
    {
        _bitmapFactory = bitmapFactory;
        _mapper = mapper;
        _gameDataContextFactory = gameDataContextFactory;
        _unitFaceExtractor = unitFaceExtractor;
        _unitPortraitExtractor = unitPortraitExtractor;
    }

    /// <inheritdoc />
    public UnitType GetUnitType(string unitTypeId)
    {
        if (!_unitTypes.TryGetValue(unitTypeId, out var unitType))
        {
            unitType = GetUnitTypeInternal(unitTypeId);
            _unitTypes.Add(unitTypeId, unitType);
        }

        return unitType;
    }

    /// <inheritdoc />
    public IBitmap GetUnitFace(string unitTypeId)
    {
        if (!_unitFaces.TryGetValue(unitTypeId, out var unitFace))
        {
            unitFace = _bitmapFactory.FromByteArray(_unitFaceExtractor.GetFileContent($"{unitTypeId}FACE"));
            _unitFaces.Add(unitTypeId, unitFace);
        }

        return unitFace;
    }

    /// <inheritdoc />
    public IBitmap GetUnitBattleFace(string unitTypeId)
    {
        if (!_unitBattleFaces.TryGetValue(unitTypeId, out var unitBattleFace))
        {
            unitBattleFace = _bitmapFactory.FromRawToBitmap(_unitFaceExtractor.GetImage($"{unitTypeId}FACEB"));
            _unitBattleFaces.Add(unitTypeId, unitBattleFace);
        }

        return unitBattleFace;
    }

    /// <inheritdoc />
    public IBitmap GetUnitPortrait(string unitTypeId)
    {
        if (!_unitPortraits.TryGetValue(unitTypeId, out var unitPortrait))
        {
            unitPortrait = _bitmapFactory.FromRawToBitmap(_unitPortraitExtractor.GetImage(unitTypeId));
            _unitPortraits.Add(unitTypeId, unitPortrait);
        }

        return unitPortrait;
    }


    /// <inheritdoc />
    protected override void LoadInternal()
    {
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Получить тип юнита.
    /// </summary>
    /// <param name="unitTypeId">Идентификатор типа юнита.</param>
    private UnitType GetUnitTypeInternal(string unitTypeId)
    {
        using (var context = _gameDataContextFactory.Create())
        {
            var unitType = context
                .UnitTypes
                .AsNoTracking()
                .Include(ut => ut.LeaderBaseUnit)
                .Include(ut => ut.PreviousUnitType)
                .Include(ut => ut.Name)
                .Include(ut => ut.Description)
                .Include(ut => ut.AbilityDescription)
                .Include(ut => ut.MainAttack.Name)
                .Include(ut => ut.MainAttack.Description)
                .Include(ut => ut.MainAttack.AlternativeAttack!.Name)
                .Include(ut => ut.MainAttack.AlternativeAttack!.Description)
                .Include(ut => ut.SecondaryAttack!.Name)
                .Include(ut => ut.SecondaryAttack!.Description)
                .Include(ut => ut.LowLevelUpgrade)
                .Include(ut => ut.HighLevelUpgrade)
                .FirstOrDefault(ut => ut.Id == unitTypeId);
            if (unitType == null)
                throw new ArgumentException($"Тип юнита {unitTypeId} не найден", nameof(unitTypeId));

            // Грузим их отдельными запросами, чтобы не увеличить количество строк в предыдущем запросе.
            context.Entry(unitType)
                .Collection(ut => ut.AttackSourceProtections)
                .Load();
            context.Entry(unitType)
                .Collection(ut => ut.AttackTypeProtections)
                .Load();

            return _mapper.Map<UnitType>(unitType);
        }
    }
}