using Disciples.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Components;

/// <summary>
/// Компонент для создания анимации юнита.
/// </summary>
internal class BattleUnitAnimationComponent : BaseAnimationComponent
{
    private static readonly PointD SmallUnitDeadOffset = new(-10, -10);
    private static readonly PointD BigUnitDeadOffset = new(-10, 30);

    private readonly BattleUnit _battleUnit;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly PointD _animationOffset;

    /// <summary>
    /// Анимация какого действия отображается в данный момент.
    /// </summary>
    private BattleUnitState _unitState;

    /// <summary>
    /// Анимация какого направления отображается в данный момент.
    /// </summary>
    private BattleDirection _unitDirection;

    /// <summary>
    /// Кадры анимации тени юнита.
    /// </summary>
    private AnimationFrames? _shadowFrames;
    /// <summary>
    /// Кадры анимации юнита.
    /// </summary>
    private AnimationFrames _unitFrames = null!;
    /// <summary>
    /// Кадры анимации ауры юнита.
    /// </summary>
    private AnimationFrames? _auraFrames;

    /// <summary>
    /// Изображение, которые отрисовывает кадры анимации тени юнита.
    /// </summary>
    private IImageSceneObject? _shadowAnimationHost;
    /// <summary>
    /// Изображение, которые отрисовывает кадры анимации юнита.
    /// </summary>
    private IImageSceneObject? _unitAnimationHost;
    /// <summary>
    /// Изображение, которые отрисовывает кадры анимации ауры юнита.
    /// </summary>
    private IImageSceneObject? _auraAnimationHost;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitAnimationComponent" />.
    /// </summary>
    public BattleUnitAnimationComponent(
        BattleUnit battleUnit,
        ISceneObjectContainer sceneObjectContainer,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        PointD animationOffset
    ) : base(battleUnit, sceneObjectContainer, GetLayer(battleUnit), animationOffset)
    {
        _battleUnit = battleUnit;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _animationOffset = animationOffset;
    }

    /// <summary>
    /// Вся информация об анимации юнита.
    /// </summary>
    public BattleUnitAnimation BattleUnitAnimation { get; private set; } = null!;

    /// <inheritdoc />
    protected override PointD AnimationOffset => _battleUnit.UnitState == BattleUnitState.Dead
        ? _battleUnit.Unit.UnitType.IsSmall
            ? SmallUnitDeadOffset
            : BigUnitDeadOffset
        : base.AnimationOffset;

    /// <summary>
    /// Позиция вывода анимации.
    /// </summary>
    public PointD AnimationPoint => new(GameObject.X + _animationOffset.X, GameObject.Y + _animationOffset.Y);

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        BattleUnitAnimation = _battleUnitResourceProvider.GetBattleUnitAnimation(_battleUnit.Unit.UnitType, _battleUnit.Direction);

        // Чтобы юниты не двигались синхронно в начале боя, первый кадр выбирается случайно.
        var frameIndex = RandomGenerator.Get(BattleUnitAnimation.BattleUnitFrames[_battleUnit.UnitState].UnitFrames.Count);
        UpdateSource(frameIndex);
    }

    /// <inheritdoc />
    public override void Update(long tickCount)
    {
        if (_battleUnit.UnitState != _unitState)
        {
            UpdateSource();
            return;
        }

        // Если юнит изменил направление (готовится к побегу),
        // То заново запрашиваем анимации.
        if (_battleUnit.Direction != _unitDirection)
        {
            BattleUnitAnimation = _battleUnitResourceProvider.GetBattleUnitAnimation(_battleUnit.Unit.UnitType, _battleUnit.Direction);
            UpdateSource();
            return;
        }

        base.Update(tickCount);
    }

    /// <inheritdoc />
    protected override void OnAnimationFrameChange()
    {
        UpdateFrame(_shadowAnimationHost, _shadowFrames);
        UpdateFrame(_unitAnimationHost, _unitFrames);
        UpdateFrame(_auraAnimationHost, _auraFrames);
    }

    /// <inheritdoc />
    protected override IReadOnlyList<IImageSceneObject?> GetAnimationHosts()
    {
        return new[]
        {
            _shadowAnimationHost,
            _unitAnimationHost,
            _auraAnimationHost
        };
    }

    /// <summary>
    /// Вычислить на каком слое располагается юнит визуально.
    /// </summary>
    private static int GetLayer(BattleUnit battleUnit)
    {
        var unitBaseLayer = battleUnit.IsAttacker
            ? BattleLayers.ATTACKER_UNIT_BASE_LAYER
            : BattleLayers.DEFENDER_UNIT_BASE_LAYER;
        var linePosition = battleUnit.IsAttacker
            ? battleUnit.Unit.Position.Line.ToIndex()
            : battleUnit.Unit.Position.Line.ToReverseIndex();
        var flankPosition = battleUnit.Unit.Position.Flank.ToReverseIndex();

        return unitBaseLayer + flankPosition * 100 + linePosition * 10;
    }

    /// <summary>
    /// Обновить анимацию юнита.
    /// </summary>
    private void UpdateSource(int startFrameIndex = 0)
    {
        _unitState = _battleUnit.UnitState;
        _unitDirection = _battleUnit.Direction;

        var frames = BattleUnitAnimation.BattleUnitFrames[_unitState];
        _shadowFrames = frames.ShadowFrames;
        _unitFrames = frames.UnitFrames;
        _auraFrames = frames.AuraFrames;

        FrameIndex = startFrameIndex;
        FramesCount = _unitFrames.Count;

        UpdatePosition(ref _shadowAnimationHost, _shadowFrames, Layer - 1);
        UpdatePosition(ref _unitAnimationHost, _unitFrames, Layer);
        UpdatePosition(ref _auraAnimationHost, _auraFrames, Layer + 1);
    }
}