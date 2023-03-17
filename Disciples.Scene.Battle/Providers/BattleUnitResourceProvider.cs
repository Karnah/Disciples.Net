using Disciples.Engine;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
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
    private readonly BattleUnitImagesExtractor _extractor;

    private SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations = null!;

    /// <inheritdoc />
    public BattleUnitResourceProvider(
        IBitmapFactory bitmapFactory,
        IUnitInfoProvider unitInfoProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleUnitImagesExtractor extractor)
    {
        _bitmapFactory = bitmapFactory;
        _unitInfoProvider = unitInfoProvider;
        _battleResourceProvider = battleResourceProvider;
        _extractor = extractor;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

    /// <inheritdoc />
    public IBitmap GetUnitFace(UnitType unitType)
    {
        return _unitInfoProvider.GetUnitFace(unitType.Id);
    }

    /// <inheritdoc />
    public IBitmap GetUnitBattleFace(UnitType unitType)
    {
        return _unitInfoProvider.GetUnitBattleFace(unitType.Id);
    }

    /// <inheritdoc />
    public IBitmap GetUnitPortrait(UnitType unitType)
    {
        return _unitInfoProvider.GetUnitPortrait(unitType.Id);
    }

    /// <inheritdoc />
    public BattleUnitAnimation GetBattleUnitAnimation(UnitType unitType, BattleDirection direction)
    {
        var unitTypeId = unitType.Id;
        if (!_unitsAnimations.ContainsKey((unitTypeId, direction)))
            _unitsAnimations[(unitTypeId, direction)] = ExtractUnitAnimation(unitTypeId, direction);

        return _unitsAnimations[(unitTypeId, direction)];
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _unitsAnimations = new SortedDictionary<(string unidId, BattleDirection direction), BattleUnitAnimation>();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Извлечь анимации юнита.
    /// </summary>
    /// <param name="unitId">Идентификатор типа юнита.</param>
    /// <param name="direction">Направление юнита.</param>
    private BattleUnitAnimation ExtractUnitAnimation(string unitId, BattleDirection direction)
    {
        var unitType = _unitInfoProvider.GetUnitType(unitId);
        // Анимация после смерти - это просто кости. Они одинаковы для всех юнитов, поэтому извлекаем отдельно.
        var unitFrames = new Dictionary<BattleUnitState, BattleUnitFrames>
        {
            { BattleUnitState.Dead, new BattleUnitFrames(null, GetDeadFrames(unitType.IsSmall), null) }
        };

        foreach (BattleUnitState action in Enum.GetValues(typeof(BattleUnitState)))
        {
            if (action == BattleUnitState.Dead)
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

        var deathAnimation = GetDeathFrames(unitType.DeathAnimation);
        return new BattleUnitAnimation(unitFrames, unitTargetAnimation, deathAnimation);
    }


    // g000uu0015 - ид в верхнем регистре.
    // HHIT - юнита ударили | HMOVE - атакует | IDLE - ждёт | STIL - замер(например, паралич) | TUCH - бьёт 1 врага | HEFF - бьёт площадь
    // A - объект или аура | S - тень
    // 1 - объект | 2 -аура
    // A - атакующий, лицом | D - защищающийся, спиной | B - симметрично
    // 00
    private BattleUnitFrames GetUnitFrames(string unitId, BattleDirection direction, BattleUnitState unitState)
    {
        var shadowImagesName = $"{unitId.ToUpper()}{GetUnitStateResourceKey(unitState)}S1{GetDirectionResourceKey(direction)}00";
        var shadowFrames = GetAnimationFrames(shadowImagesName);

        var unitImagesName = $"{unitId.ToUpper()}{GetUnitStateResourceKey(unitState)}A1{GetDirectionResourceKey(direction)}00";
        var unitFrames = GetAnimationFrames(unitImagesName);
        if (unitFrames == null)
            throw new ArgumentException($"Отсутствует анимация для юнита: {unitImagesName}");

        var auraImagesName = $"{unitId.ToUpper()}{GetUnitStateResourceKey(unitState)}A2{GetDirectionResourceKey(direction)}00";
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
    private static string GetUnitStateResourceKey(BattleUnitState unitState)
    {
        return unitState switch
        {
            BattleUnitState.Waiting => "IDLE",
            BattleUnitState.Attacking => "HMOV",
            BattleUnitState.TakingDamage => "HHIT",
            BattleUnitState.Paralyzed => "STIL",
            _ => throw new ArgumentOutOfRangeException(nameof(unitState), unitState, null)
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