using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Превращённый атакой <see cref="UnitAttackType.TransformEnemy" /> юнит.
/// </summary>
internal class TransformedEnemyUnit : Unit
{
    /// <summary>
    /// Создать объект типа <see cref="TransformedEnemyUnit" />.
    /// </summary>
    public TransformedEnemyUnit(Unit originalUnit, UnitType transformedUnitType)
        : base(originalUnit.Id, transformedUnitType, originalUnit.Player, originalUnit.SquadLinePosition, originalUnit.SquadFlankPosition)
    {
        OriginalUnit = originalUnit;

        HitPoints = originalUnit.HitPoints;
    }

    /// <summary>
    /// Юнит, который был превращён.
    /// </summary>
    public Unit OriginalUnit { get; }

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
    public override int MaxHitPoints => OriginalUnit.MaxHitPoints;

    /// <inheritdoc />
    public override UnitEffects Effects => OriginalUnit.Effects;
}