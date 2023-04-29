using Disciples.Engine.Common.Components;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Components;

/// <summary>
/// Компонент для звуков юнита.
/// </summary>
internal class BattleUnitSoundComponent : BaseComponent
{
    private readonly BattleUnit _battleUnit;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitSoundComponent" />.
    /// </summary>
    public BattleUnitSoundComponent(BattleUnit battleUnit, IBattleUnitResourceProvider unitResourceProvider) : base(battleUnit)
    {
        _battleUnit = battleUnit;
        _unitResourceProvider = unitResourceProvider;
    }

    /// <summary>
    /// Звуки юнита.
    /// </summary>
    public BattleUnitSounds Sounds { get; private set; } = null!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        Sounds = _unitResourceProvider.GetBattleUnitSounds(_battleUnit.Unit.UnitType);
    }
}