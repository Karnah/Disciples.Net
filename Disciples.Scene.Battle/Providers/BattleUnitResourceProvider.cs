using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Resources;
using Disciples.Scene.Battle.Resources.Enum;

namespace Disciples.Scene.Battle.Providers;

/// <inheritdoc cref="IBattleUnitResourceProvider" />
internal class BattleUnitResourceProvider : BaseSupportLoading, IBattleUnitResourceProvider
{
    private readonly IBitmapFactory _bitmapFactory;
    private readonly IUnitInfoProvider _unitInfoProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;
    private readonly BattleUnitImagesExtractor _extractor;

    private Dictionary<(string unidId, BattleDirection direction), BattleUnitAnimation> _unitsAnimations = null!;
    private Dictionary<(UnitBattleEffectType effectType, bool isSmall), IReadOnlyList<Frame>> _effectsAnimation = null!;

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
        var animationKey = (unitTypeId, direction);
        if (!_unitsAnimations.ContainsKey(animationKey))
            _unitsAnimations[animationKey] = ExtractUnitAnimation(unitTypeId, direction);

        return _unitsAnimations[animationKey];
    }

    /// <inheritdoc />
    public IReadOnlyList<Frame> GetEffectAnimation(UnitBattleEffectType effectType, bool isSmall)
    {
        var animationKey = (effectType, isSmall);
        if (!_effectsAnimation.ContainsKey(animationKey))
            _effectsAnimation[animationKey] = _battleResourceProvider.GetBattleAnimation(new UnitBattleEffectAnimationResourceKey(effectType, isSmall).Key);

        return _effectsAnimation[animationKey];
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _unitsAnimations = new Dictionary<(string unidId, BattleDirection direction), BattleUnitAnimation>();
        _effectsAnimation = new Dictionary<(UnitBattleEffectType effectType, bool isSmall), IReadOnlyList<Frame>>();
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
        // Анимация после смерти - это просто кости. Они одинаковы для всех юнитов, поэтому извлекаем отдельно.
        var unitFrames = new Dictionary<BattleUnitState, BattleUnitFrames>
        {
            { BattleUnitState.Dead, new BattleUnitFrames(null, GetDeadFrames(unitType.IsSmall), null) }
        };

        foreach (BattleUnitState action in Enum.GetValues(typeof(BattleUnitState)))
        {
            if (action == BattleUnitState.Dead)
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
        var shadowKey = new UnitAnimationResourceKey(unitTypeId, unitState, direction, UnitAnimationType.Shadow);
        var shadowFrames = GetAnimationFrames(shadowKey);

        var bodyKey = new UnitAnimationResourceKey(unitTypeId, unitState, direction, UnitAnimationType.Body);
        var unitFrames = GetAnimationFrames(bodyKey);
        if (unitFrames == null)
            throw new ArgumentException($"Отсутствует анимация для юнита: {bodyKey.Key}");

        var auraKey = new UnitAnimationResourceKey(unitTypeId, unitState, direction, UnitAnimationType.Aura);
        var auraFrames = GetAnimationFrames(auraKey);

        return new BattleUnitFrames(shadowFrames, unitFrames, auraFrames);
    }

    /// <summary>
    /// Получить анимацию, которая применяется к атакуемым юнитам.
    /// </summary>
    private BattleUnitTargetAnimation? GetUnitTargetAnimation(string unitTypeId)
    {
        // В начале пытаемся достать анимацию для одного юнита.
        // Если её нет, то для площади.
        // Если и для площади нет, то просто вернётся null.
        return GetUnitTargetAnimation(unitTypeId, UnitTargetAnimationType.Single)
               ?? GetUnitTargetAnimation(unitTypeId, UnitTargetAnimationType.Area);
    }

    /// <summary>
    /// Получить анимацию, которая применяется к атакуемым юнитам.
    /// </summary>
    private BattleUnitTargetAnimation? GetUnitTargetAnimation(string unitTypeId, UnitTargetAnimationType animationType)
    {
        var isSingle = animationType == UnitTargetAnimationType.Single;

        // Проверяем анимацию на одну цель, которая не зависит от направления.
        var targetWithoutDirectionFrames = GetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, animationType, null));
        if (targetWithoutDirectionFrames != null)
            return new BattleUnitTargetAnimation(isSingle, targetWithoutDirectionFrames, targetWithoutDirectionFrames);

        var targetAttackingFrames = GetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, animationType, BattleSquadPosition.Attacker));
        var targetDefenderFrames = GetAnimationFrames(new UnitAttackAnimationResourceKey(unitTypeId, animationType, BattleSquadPosition.Defender));
        if (targetAttackingFrames != null && targetDefenderFrames != null)
            return new BattleUnitTargetAnimation(isSingle, targetAttackingFrames, targetDefenderFrames);

        return null;
    }

    /// <summary>
    /// Получить кадры анимации.
    /// </summary>
    private IReadOnlyList<Frame>? GetAnimationFrames(BaseImageKey key)
    {
        var images = _extractor.GetAnimationFrames(key.Key);
        if (images == null)
            return null;

        return _bitmapFactory.ConvertToFrames(images);
    }

    /// <summary>
    /// Получить анимацию мёртвого юнита.
    /// </summary>
    /// <remarks>
    /// На самом деле это единичная картинка с костями.
    /// </remarks>
    private IReadOnlyList<Frame> GetDeadFrames(bool isSmall)
    {
        var imageIndex = RandomGenerator.Get(2);
        var resourceKey = new UnitDeadBodyResourceKey(isSmall, imageIndex);
        var deadFrame = _battleResourceProvider.GetBattleFrame(resourceKey.Key);

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
    private IReadOnlyList<Frame> GetDeathAnimation(UnitDeathAnimationType animationType)
    {
        return _battleResourceProvider.GetBattleAnimation(new UnitDeathAnimationResourceKey(animationType).Key);
    }
}