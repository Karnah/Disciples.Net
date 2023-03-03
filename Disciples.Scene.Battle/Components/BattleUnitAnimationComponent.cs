using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;
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
    private readonly BattleUnit _battleUnit;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;

    /// <summary>
    /// Анимация какого действия отображается в данный момент.
    /// </summary>
    private BattleUnitState _unitState;

    /// <summary>
    /// Кадры анимации тени юнита.
    /// </summary>
    private IReadOnlyList<Frame>? _shadowFrames;
    /// <summary>
    /// Кадры анимации юнита.
    /// </summary>
    private IReadOnlyList<Frame> _unitFrames = Array.Empty<Frame>();
    /// <summary>
    /// Кадры анимации ауры юнита.
    /// </summary>
    private IReadOnlyList<Frame>? _auraFrames;

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
        IBattleUnitResourceProvider battleUnitResourceProvider
    ) : base(battleUnit, sceneObjectContainer, GetLayer(battleUnit))
    {
        _battleUnit = battleUnit;
        _battleUnitResourceProvider = battleUnitResourceProvider;
    }

    /// <summary>
    /// Вся информация об анимации юнита.
    /// </summary>
    public BattleUnitAnimation BattleUnitAnimation { get; private set; } = null!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        BattleUnitAnimation = _battleUnitResourceProvider.GetBattleUnitAnimation(_battleUnit.Unit.UnitType.UnitTypeId, _battleUnit.Direction);

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
        return new[] {
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
        var battleLine = battleUnit.IsAttacker
            ? battleUnit.Unit.SquadLinePosition
            : 3 - battleUnit.Unit.SquadLinePosition;
        var flankPosition = 2 - battleUnit.Unit.SquadFlankPosition;

        return battleLine * 100 + flankPosition * 10 + 5;
    }

    /// <summary>
    /// Обновить анимацию юнита.
    /// </summary>
    private void UpdateSource(int startFrameIndex = 0)
    {
        _unitState = _battleUnit.UnitState;

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