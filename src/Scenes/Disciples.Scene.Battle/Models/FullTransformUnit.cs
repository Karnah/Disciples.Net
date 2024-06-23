using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Юнит, который имеет все характеристики своего <see cref="UnitType" />.
/// </summary>
/// <remarks>
/// Используется при выполнении атак <see cref="UnitAttackType.ReduceLevel" />, <see cref="UnitAttackType.Doppelganger" />
/// И <see cref="UnitAttackType.TransformSelf" />
/// </remarks>
internal class FullTransformUnit : Unit, ITransformedUnit
{
    /// <summary>
    /// Создать объект типа <see cref="FullTransformUnit" />.
    /// </summary>
    public FullTransformUnit(Unit originalUnit, UnitType unitType)
        : base(originalUnit.Id, unitType, originalUnit.Player, originalUnit.Squad, originalUnit.SquadLinePosition, originalUnit.SquadFlankPosition)
    {
        OriginalUnit = originalUnit is ITransformedUnit transformedUnit
            ? transformedUnit.OriginalUnit
            : originalUnit;
    }

    /// <inheritdoc />
    public Unit OriginalUnit { get; }

    /// <inheritdoc />
    public Unit Unit => this;

    /// <inheritdoc />
    public override bool IsDead
    {
        get => OriginalUnit.IsDead;
        set => OriginalUnit.IsDead = value;
    }

    /// <inheritdoc />
    public override bool IsRetreated
    {
        get => OriginalUnit.IsRetreated;
        set => OriginalUnit.IsRetreated = value;
    }

    /// <inheritdoc />
    public override bool IsUnsummoned
    {
        get => OriginalUnit.IsUnsummoned;
        set => OriginalUnit.IsUnsummoned = value;
    }

    /// <inheritdoc />
    public override int DeathExperience => OriginalUnit.DeathExperience;

    /// <inheritdoc />
    public override int BattleExperience
    {
        get => OriginalUnit.BattleExperience;
        set => OriginalUnit.BattleExperience = value;
    }

    /// <inheritdoc />
    public override UnitEffects Effects => OriginalUnit.Effects;
}