﻿using Disciples.Engine;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Platform.Factories;
using Disciples.ResourceProvider;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Providers;

/// <inheritdoc cref="IBattleUnitResourceProvider" />
internal class BattleUnitResourceProvider : BaseSupportLoading, IBattleUnitResourceProvider
{
    private readonly string[] _deathAnimationNames = {
        string.Empty,
        "DEATH_HUMAN_S13",
        "DEATH_HERETIC_S13",
        "DEATH_DWARF_S15",
        "DEATH_UNDEAD_S15",
        "DEATH_NEUTRAL_S10",
        "DEATH_DRAGON_S15",
        "DEATH_GHOST_S15",
        "DEATH_ELF_S15"
    };

    private readonly IBitmapFactory _bitmapFactory;
    private readonly IUnitInfoProvider _unitInfoProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;

    private ImagesExtractor _extractor = null!;
    private SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations = null!;

    /// <inheritdoc />
    public BattleUnitResourceProvider(IBitmapFactory bitmapFactory, IUnitInfoProvider unitInfoProvider, IBattleResourceProvider battleResourceProvider)
    {
        _bitmapFactory = bitmapFactory;
        _unitInfoProvider = unitInfoProvider;
        _battleResourceProvider = battleResourceProvider;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _extractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Resources\\Imgs\\BatUnits.ff");
        _unitsAnimations = new SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation>();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <inheritdoc />
    public BattleUnitAnimation GetBattleUnitAnimation(string unitId, BattleDirection direction)
    {
        if (!_unitsAnimations.ContainsKey((unitId, direction)))
            _unitsAnimations[(unitId, direction)] = ExtractUnitAnimation(unitId, direction);

        return _unitsAnimations[(unitId, direction)];
    }


    private BattleUnitAnimation ExtractUnitAnimation(string unitId, BattleDirection direction)
    {
        var unitType = _unitInfoProvider.GetUnitType(unitId);
        // Анимация после смерти - это просто кости. Они одинаковы для всех юнитов, поэтому извлекаем отдельно.
        var unitFrames = new Dictionary<BattleAction, BattleUnitFrames>
        {
            { BattleAction.Dead, new BattleUnitFrames(null, GetDeadFrames(unitType.SizeSmall), null) }
        };

        foreach (BattleAction action in Enum.GetValues(typeof(BattleAction)))
        {
            if (action == BattleAction.Dead)
                continue;

            unitFrames.Add(action, GetUnitFrames(unitId, direction, action));
        }

        // Методом перебора смотрим, есть ли кадры атаки, применяемые к одному юниту.
        var singleTargetFrames =
            // Анимация зависит от положения.
            GetAnimationFrames($"{unitId.ToUpper()}TUCHA1{GetDirectionResourceKey(direction)}00") ??
            // Анимация симметрична.
            GetAnimationFrames($"{unitId.ToUpper()}TUCHA1B00");
        BattleUnitTargetAnimation? unitTargetAnimation = null;
        if (singleTargetFrames != null)
        {
            unitTargetAnimation = new BattleUnitTargetAnimation(true, singleTargetFrames);
        }
        else
        {
            // Кадры атаки, которые применяется целиком на площадь.
            var areaTargetFrames =
                // Анимация зависит от положения.
                GetAnimationFrames($"{unitId.ToUpper()}HEFFA1{GetDirectionResourceKey(direction)}00") ??
                // Анимация симметрична.
                GetAnimationFrames($"{unitId.ToUpper()}HEFFA1B00");

            if (areaTargetFrames != null)
            {
                unitTargetAnimation = new BattleUnitTargetAnimation(false, areaTargetFrames);
            }
        }

        var deathAnimation = GetDeathFrames(unitType.DeathAnimationId);
        return new BattleUnitAnimation(unitFrames, unitTargetAnimation, deathAnimation);
    }


    // g000uu0015 - ид в верхнем регистре.
    // HHIT - юнита ударили | HMOVE - атакует | IDLE - ждёт | STIL - замер(например, паралич) | TUCH - бьёт 1 врага | HEFF - бьёт площадь
    // A - объект или аура | S - тень
    // 1 - объект | 2 -аура
    // A - атакующий, лицом | D - защищающийся, спиной | B - симметрично
    // 00
    private BattleUnitFrames GetUnitFrames(string unitId, BattleDirection direction, BattleAction action)
    {
        var shadowImagesName = $"{unitId.ToUpper()}{GetBattleActionResourceKey(action)}S1{GetDirectionResourceKey(direction)}00";
        var shadowFrames = GetAnimationFrames(shadowImagesName);

        var unitImagesName = $"{unitId.ToUpper()}{GetBattleActionResourceKey(action)}A1{GetDirectionResourceKey(direction)}00";
        var unitFrames = GetAnimationFrames(unitImagesName);
        if (unitFrames == null)
            throw new ArgumentException($"Отсутствует анимация для юнита: {unitImagesName}");

        var auraImagesName = $"{unitId.ToUpper()}{GetBattleActionResourceKey(action)}A2{GetDirectionResourceKey(direction)}00";
        var auraFrames = GetAnimationFrames(auraImagesName);

        return new BattleUnitFrames(shadowFrames, unitFrames, auraFrames);
    }

    /// <summary>
    /// Получить кадры анимации.
    /// </summary>
    private IReadOnlyList<Frame>? GetAnimationFrames(string fileName)
    {
        var images = _extractor.GetAnimationFrames(fileName);
        if (images == null)
            return null;

        return _bitmapFactory.ConvertToFrames(images);
    }

    /// <summary>
    /// Получить ключ ресурсах, которое соответствует определённому действию.
    /// </summary>
    private static string GetBattleActionResourceKey(BattleAction action)
    {
        return action switch
        {
            BattleAction.Waiting => "IDLE",
            BattleAction.Attacking => "HMOV",
            BattleAction.TakingDamage => "HHIT",
            BattleAction.Paralyzed => "STIL",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };
    }

    /// <summary>
    /// Получить ключ ресурсах, которое соответствует определённому положению.
    /// </summary>
    private static string GetDirectionResourceKey(BattleDirection direction)
    {
        return direction switch
        {
            BattleDirection.Attacker => "A",
            BattleDirection.Defender => "D",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    /// <summary>
    /// Получить анимацию мёртвого юнита.
    /// </summary>
    /// <remarks>
    /// На самом деле это единичная картинка с костями.
    /// </remarks>
    private IReadOnlyList<Frame> GetDeadFrames(bool isSmall)
    {
        var sizeChar = isSmall ? 'S' : 'L';
        var imageIndex = RandomGenerator.Get(2);
        var deadFrame = _battleResourceProvider.GetBattleFrame($"DEAD_HUMAN_{sizeChar}A{imageIndex:00}");

        // Необходимо задать дополнительно смещение, чтобы кости оказались в нужном месте.
        var frame = new Frame(
            deadFrame.Width,
            deadFrame.Height,
            deadFrame.OffsetX - 30,
            deadFrame.OffsetY - 10,
            deadFrame.Bitmap);

        return new []{ frame };
    }

    /// <summary>
    /// Получить анимацию смерти юнита.
    /// </summary>
    private IReadOnlyList<Frame> GetDeathFrames(int deathAnimationId)
    {
        return _battleResourceProvider.GetBattleAnimation(_deathAnimationNames[deathAnimationId]);
    }
}