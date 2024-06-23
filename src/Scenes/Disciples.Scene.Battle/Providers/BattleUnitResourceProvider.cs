using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Common.Exceptions;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Resources;
using Disciples.Scene.Battle.Resources.Enum;
using Disciples.Scene.Battle.Resources.ImageKeys;
using Disciples.Scene.Battle.Resources.ImageKeys.Extensions;

namespace Disciples.Scene.Battle.Providers;

/// <inheritdoc cref="IBattleUnitResourceProvider" />
internal class BattleUnitResourceProvider : BaseSupportLoading, IBattleUnitResourceProvider
{
    private readonly IBitmapFactory _bitmapFactory;
    private readonly IUnitInfoProvider _unitInfoProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;
    private readonly BattleUnitImagesExtractor _imagesExtractor;
    private readonly BattleSoundsMappingExtractor _soundMappingExtractor;

    private readonly Dictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations = new();
    private readonly Dictionary<(UnitAttackType effectAttackType, bool isSmall), AnimationFrames> _effectsAnimations = new();
    private readonly Dictionary<(RaceType RaceType, bool isSmall), AnimationFrames> _unitLevelUpAnimations = new();
    private readonly Dictionary<string, BattleUnitSounds> _unitSounds;

    /// <inheritdoc />
    public BattleUnitResourceProvider(
        IBitmapFactory bitmapFactory,
        IUnitInfoProvider unitInfoProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleUnitImagesExtractor imagesExtractor,
        BattleSoundsMappingExtractor soundMappingExtractor)
    {
        _bitmapFactory = bitmapFactory;
        _unitInfoProvider = unitInfoProvider;
        _battleResourceProvider = battleResourceProvider;
        _imagesExtractor = imagesExtractor;
        _soundMappingExtractor = soundMappingExtractor;

        _unitSounds = new Dictionary<string, BattleUnitSounds>();
    }

    /// <inheritdoc />
    public AnimationFrames SmallUnitTurnAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public AnimationFrames BigUnitTurnAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public AnimationFrames SmallUnitTargetAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public AnimationFrames BigUnitTargetAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public AnimationFrames SmallUnitUnsummonAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public AnimationFrames BigUnitUnsummonAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public AnimationFrames SummonPlaceholderAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public AnimationFrames DrainLifeHealAnimationFrames { get; private set; } = null!;

    /// <inheritdoc />
    public IBitmap GetUnitFace(UnitType unitType)
    {
        return _unitInfoProvider.GetUnitFace(unitType.LeaderBaseUnitType?.Id ?? unitType.Id);
    }

    /// <inheritdoc />
    public IBitmap GetUnitBattleFace(UnitType unitType)
    {
        return _unitInfoProvider.GetUnitBattleFace(unitType.LeaderBaseUnitType?.Id ?? unitType.Id);
    }

    /// <inheritdoc />
    public IBitmap GetUnitPortrait(UnitType unitType)
    {
        return _unitInfoProvider.GetUnitPortrait(unitType.LeaderBaseUnitType?.Id ?? unitType.Id);
    }

    /// <inheritdoc />
    public BattleUnitAnimation GetBattleUnitAnimation(UnitType unitType, BattleDirection direction)
    {
        var unitTypeId = unitType.LeaderBaseUnitType?.Id ?? unitType.Id;
        var animationKey = (unitTypeId, direction);
        if (!_unitsAnimations.ContainsKey(animationKey))
            _unitsAnimations[animationKey] = ExtractUnitAnimation(unitTypeId, direction);

        return _unitsAnimations[animationKey];
    }

    /// <inheritdoc />
    public AnimationFrames? GetAttackTypeAnimation(UnitAttackType effectAttackType, bool isSmallUnit)
    {
        if (!effectAttackType.HasResourceKey())
            return null;

        var animationKey = (effectAttackType, isSmall: isSmallUnit);
        if (!_effectsAnimations.ContainsKey(animationKey))
            _effectsAnimations[animationKey] = _battleResourceProvider.GetBattleAnimation(new UnitBattleEffectAnimationResourceKey(effectAttackType, isSmallUnit).Key);

        return _effectsAnimations[animationKey];
    }

    /// <inheritdoc />
    public AnimationFrames GetUnitLevelUpAnimation(UnitType unitType)
    {
        var animationKey = (unitType.RaceType, isSmall: unitType.IsSmall);
        if (!_unitLevelUpAnimations.ContainsKey(animationKey))
            _unitLevelUpAnimations[animationKey] = _battleResourceProvider.GetBattleAnimation(new UnitLevelUpAnimationResourceKey(unitType.RaceType, unitType.IsSmall).Key);

        return _unitLevelUpAnimations[animationKey];
    }

    /// <inheritdoc />
    public BattleUnitSounds GetBattleUnitSounds(UnitType unitType)
    {
        var unitTypeId = unitType.LeaderBaseUnitType?.Id ?? unitType.Id;
        if (!_unitSounds.TryGetValue(unitTypeId, out var battleUnitSounds))
        {
            battleUnitSounds = ExtractUnitSounds(unitTypeId);
            _unitSounds[unitTypeId] = battleUnitSounds;
        }

        return battleUnitSounds;
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        SmallUnitTurnAnimationFrames = _battleResourceProvider.GetBattleAnimation("MRKCURSMALLA");
        BigUnitTurnAnimationFrames = _battleResourceProvider.GetBattleAnimation("MRKCURLARGEA");
        SmallUnitTargetAnimationFrames = _battleResourceProvider.GetBattleAnimation("MRKSMALLA");
        BigUnitTargetAnimationFrames = _battleResourceProvider.GetBattleAnimation("MRKLARGEA");
        SummonPlaceholderAnimationFrames = _battleResourceProvider.GetBattleAnimation("MRKEMPTYSMALLA");
        SmallUnitUnsummonAnimationFrames = _battleResourceProvider.GetBattleAnimation("USUMMONANIMS");
        BigUnitUnsummonAnimationFrames = _battleResourceProvider.GetBattleAnimation("USUMMONANIML");
        DrainLifeHealAnimationFrames = GetAnimationFrames(new StaticResourceKey("HEALTUCHA1B00"));
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Извлечь анимации юнита.
    /// </summary>
    /// <param name="unitTypeId">Идентификатор типа юнита.</param>
    /// <param name="direction">Направление юнита.</param>
    private BattleUnitAnimation ExtractUnitAnimation(string unitTypeId, BattleDirection direction)
    {
        var unitType = _unitInfoProvider.GetUnitType(unitTypeId);
        var unitFrames = new Dictionary<BattleUnitState, BattleUnitFrames>
        {
            // Анимация после смерти - это просто кости. Они одинаковы для всех юнитов, поэтому извлекаем отдельно.
            { BattleUnitState.Dead, new BattleUnitFrames(null, GetDeadFrames(unitType.IsSmall), null) },

            // Если юнит сбежал, то там пустое место.
            { BattleUnitState.Retreated, new BattleUnitFrames(null, new AnimationFrames(), null )}
        };

        foreach (BattleUnitState action in Enum.GetValues(typeof(BattleUnitState)))
        {
            if (unitFrames.ContainsKey(action))
                continue;

            unitFrames.Add(action, GetUnitFrames(unitTypeId, direction, action));
        }

        var unitTargetAnimation = GetUnitTargetAnimation(unitTypeId);
        var deathAnimation = GetDeathAnimation(unitType.DeathAnimationType);
        return new BattleUnitAnimation(unitFrames, unitTargetAnimation, deathAnimation);
    }

    /// <summary>
    /// Получить анимацию юнита.
    /// </summary>
    private BattleUnitFrames GetUnitFrames(string unitTypeId, BattleDirection direction, BattleUnitState unitState)
    {
        var bodyKey = new UnitAnimationResourceKey(unitTypeId, unitState, direction, UnitAnimationType.Body);
        var unitFrames = TryGetAnimationFrames(bodyKey);

        // Баг ресурсов Disciples.
        // Для некоторых юнитов (например, умертвие) не задана анимация тела получения урона.
        // В этом случае игра использует анимацию ожидания.
        if (unitFrames == null)
        {
            if (unitState == BattleUnitState.Waiting)
                throw new ResourceNotFoundException(bodyKey.Key);

            unitState = BattleUnitState.Waiting;
            bodyKey = new UnitAnimationResourceKey(unitTypeId, unitState, direction, UnitAnimationType.Body);
            unitFrames = GetAnimationFrames(bodyKey);
        }

        var shadowKey = new UnitAnimationResourceKey(unitTypeId, unitState, direction, UnitAnimationType.Shadow);
        var shadowFrames = TryGetAnimationFrames(shadowKey);

        var auraKey = new UnitAnimationResourceKey(unitTypeId, unitState, direction, UnitAnimationType.Aura);
        var auraFrames = TryGetAnimationFrames(auraKey);

        return new BattleUnitFrames(shadowFrames, unitFrames, auraFrames);
    }

    /// <summary>
    /// Получить анимацию, которая применяется к атакуемым юнитам.
    /// </summary>
    /// <remarks>
    /// TODO Может быть больше одной анимации. Порядковый индекс - последние две цифры в ключе.
    /// </remarks>
    private BattleUnitTargetAnimation? GetUnitTargetAnimation(string unitTypeId)
    {
        var targetSymmetricAnimationFrames = TryGetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, UnitTargetAnimationType.Single, null));
        var targetAttackingFrames = targetSymmetricAnimationFrames ?? TryGetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, UnitTargetAnimationType.Single, BattleSquadPosition.Attacker));
        var targetDefenderFrames = targetSymmetricAnimationFrames ?? TryGetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, UnitTargetAnimationType.Single, BattleSquadPosition.Defender));

        var areaSymmetricDirectionFrames = TryGetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, UnitTargetAnimationType.Area, null));
        var areaAttackingFrames = areaSymmetricDirectionFrames ?? TryGetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, UnitTargetAnimationType.Area, BattleSquadPosition.Attacker));
        var areaDefenderFrames = areaSymmetricDirectionFrames ?? TryGetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, UnitTargetAnimationType.Area, BattleSquadPosition.Defender));
        if (areaSymmetricDirectionFrames == null && targetAttackingFrames == null && areaAttackingFrames == null && areaDefenderFrames == null)
            return null;

        return new BattleUnitTargetAnimation(targetAttackingFrames, targetDefenderFrames, areaAttackingFrames, areaDefenderFrames);
    }

    /// <summary>
    /// Получить кадры анимации.
    /// </summary>
    private AnimationFrames? TryGetAnimationFrames(BaseResourceKey key)
    {
        var images = _imagesExtractor.TryGetAnimationFrames(key.Key);
        if (images == null)
            return null;

        return _bitmapFactory.ConvertToFrames(images);
    }

    /// <summary>
    /// Получить кадры анимации.
    /// </summary>
    private AnimationFrames GetAnimationFrames(BaseResourceKey key)
    {
        return TryGetAnimationFrames(key) ?? throw new ResourceNotFoundException(key.Key);
    }

    /// <summary>
    /// Получить анимацию мёртвого юнита.
    /// </summary>
    /// <remarks>
    /// На самом деле это единичная картинка с костями.
    /// </remarks>
    private AnimationFrames GetDeadFrames(bool isSmall)
    {
        var imageIndex = RandomGenerator.Get(2);
        var resourceKey = new UnitDeadBodyResourceKey(isSmall, imageIndex);
        var deadFrame = _battleResourceProvider.GetBattleBitmap(resourceKey.Key);
        return new AnimationFrames(new[] { deadFrame });
    }

    /// <summary>
    /// Получить анимацию смерти юнита.
    /// </summary>
    private AnimationFrames GetDeathAnimation(UnitDeathAnimationType animationType)
    {
        return _battleResourceProvider.GetBattleAnimation(new UnitDeathAnimationResourceKey(animationType).Key);
    }

    /// <summary>
    /// Извлечь звуки для юнита.
    /// </summary>
    private BattleUnitSounds ExtractUnitSounds(string unitTypeId)
    {
        var unitTypeSounds = _soundMappingExtractor.GetUnitTypeSounds(unitTypeId);
        return new BattleUnitSounds
        {
            AttackSounds = ExtractRawSounds(unitTypeSounds.AttackSounds),
            BeginAttackSoundFrameIndex = unitTypeSounds.BeginAttackSoundFrameIndex,
            EndAttackSoundFrameIndex = unitTypeSounds.EndAttackSoundFrameIndex,
            HitTargetSounds = ExtractRawSounds(unitTypeSounds.HitTargetSounds),
            BeginAttackHitSoundFrameIndex = unitTypeSounds.BeginAttackHitSoundFrameIndex,
            EndAttackHitSoundFrameIndex = unitTypeSounds.EndAttackHitSoundFrameIndex,
            DamagedSounds = ExtractRawSounds(unitTypeSounds.DamagedSounds)
        };
    }

    /// <summary>
    /// Извлечь звуки.
    /// </summary>
    private IReadOnlyList<RawSound> ExtractRawSounds(IReadOnlyList<string> soundNames)
    {
        return soundNames
            .Select(_battleResourceProvider.GetSound)
            .Where(rs => rs != null)
            .ToArray()!;
    }
}