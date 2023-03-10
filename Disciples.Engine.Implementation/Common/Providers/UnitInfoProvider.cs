using System;
using System.Collections.Generic;
using System.IO;
using AutoMapper;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Platform.Factories;
using Disciples.ResourceProvider;
using Disciples.Resources.Database;
using UnitType = Disciples.Engine.Common.Models.UnitType;

namespace Disciples.Engine.Implementation.Common.Providers;

/// <inheritdoc cref="IUnitInfoProvider" />
public class UnitInfoProvider : BaseSupportLoading, IUnitInfoProvider
{
    private readonly IBitmapFactory _bitmapFactory;
    private readonly IMapper _mapper;
    private readonly Database _database;
    private readonly ImagesExtractor _facesExtractor;
    private readonly ImagesExtractor _portraitExtractor;

    private readonly Dictionary<string, UnitType> _unitTypes = new();
    private readonly Dictionary<string, IBitmap> _unitFaces = new();
    private readonly Dictionary<string, IBitmap> _unitBattleFaces = new();
    private readonly Dictionary<string, IBitmap> _unitPortraits = new ();

    /// <summary>
    /// Создать объект типа <see cref="UnitInfoProvider" />.
    /// </summary>
    public UnitInfoProvider(IBitmapFactory bitmapFactory, IMapper mapper, Database database)
    {
        _bitmapFactory = bitmapFactory;
        _mapper = mapper;
        _database = database;

        _facesExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Faces.ff");
        _portraitExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\Events.ff");
    }


    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => true;


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
            unitFace = _bitmapFactory.FromByteArray(_facesExtractor.GetFileContent($"{unitTypeId}FACE"));
            _unitFaces.Add(unitTypeId, unitFace);
        }

        return unitFace;
    }

    /// <inheritdoc />
    public IBitmap GetUnitBattleFace(string unitTypeId)
    {
        if (!_unitBattleFaces.TryGetValue(unitTypeId, out var unitBattleFace))
        {
            unitBattleFace = _bitmapFactory.FromRawToBitmap(_facesExtractor.GetImage($"{unitTypeId}FACEB"));
            _unitBattleFaces.Add(unitTypeId, unitBattleFace);
        }

        return unitBattleFace;
    }

    /// <inheritdoc />
    public IBitmap GetUnitPortrait(string unitTypeId)
    {
        if (!_unitPortraits.TryGetValue(unitTypeId, out var unitPortrait))
        {
            unitPortrait = _bitmapFactory.FromRawToOriginalBitmap(_portraitExtractor.GetImage(unitTypeId.ToUpper()));
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
        if (!_database.UnitTypes.TryGetValue(unitTypeId, out var dbUnitType))
            throw new ArgumentException($"Тип юнита {unitTypeId} не найден", nameof(unitTypeId));

        var unitType = new UnitType
        {
            PreviousUnitType = dbUnitType.PreviousUnitTypeId == null ? null : GetUnitType(dbUnitType.PreviousUnitTypeId),
            RaceId = string.Empty, // todo
            RecruitBuildingId = null, // todo,
            Name = GetText(dbUnitType.NameTextId),
            Description = GetText(dbUnitType.DescriptionTextId),
            Ability = GetText(dbUnitType.AbilityTextId),
            MainAttack = GetUnitAttack(dbUnitType.MainUserAttackId),
            SecondaryAttack = dbUnitType.SecondaryUserAttackId == null ? null : GetUnitAttack(dbUnitType.SecondaryUserAttackId),
            LeaderBaseUnit = dbUnitType.LeaderBaseUnitId == null ? null : GetUnitType(dbUnitType.LeaderBaseUnitId),
            UpgradeBuildingId = null, // todo
            LowLevelUpgradeId = string.Empty, // todo
            HighLevelUpgradeId = string.Empty, // todo
            AttackSourceProtections = _database.UnitAttackSourceProtections.TryGetValue(dbUnitType.Id, out var unitAttackSourceProtections)
                    ? _mapper.Map<List<UnitAttackSourceProtection>>(unitAttackSourceProtections)
                    : Array.Empty<UnitAttackSourceProtection>(),
            AttackTypeProtections = _database.UnitAttackTypeProtections.TryGetValue(dbUnitType.Id, out var unitAttackTypeProtections)
                ? _mapper.Map<List<UnitAttackTypeProtection>>(unitAttackTypeProtections)
                : Array.Empty<UnitAttackTypeProtection>(),
        };

        unitType = _mapper.Map(dbUnitType, unitType);
        return unitType;
    }

    /// <summary>
    /// Получить данные атаки юнита.
    /// </summary>
    /// <param name="unitAttackId">Идентификатор атаки юнита.</param>
    private UnitAttack GetUnitAttack(string unitAttackId)
    {
        if (!_database.UnitAttacks.TryGetValue(unitAttackId, out var dbUnitAttack))
            throw new ArgumentException($"Тип атаки юнита {unitAttackId} не найден", nameof(unitAttackId));

        var unitAttack = new UnitAttack
        {
            Name = GetText(dbUnitAttack.NameTextId),
            Description = GetText(dbUnitAttack.DescriptionTextId),
            AlternativeUnitAttack = dbUnitAttack.AlternativeUnitAttackId == null ? null : GetUnitAttack(dbUnitAttack.AlternativeUnitAttackId),
            Ward1 = null, // todo ссылки.
            Ward2 = null,
            Ward3 = null,
            Ward4 = null
        };

        unitAttack = _mapper.Map(dbUnitAttack, unitAttack);
        return unitAttack;
    }

    /// <summary>
    /// Получить текст по его идентификатору.
    /// </summary>
    /// <param name="textId">Идентификатор текстовой записи.</param>
    private string GetText(string textId)
    {
        if (!_database.GlobalTextResources.TryGetValue(textId, out var textResource))
            throw new ArgumentException($"Текстовый идентификатор {textId} не найден", nameof(textId));

        return textResource.Text;
    }
}