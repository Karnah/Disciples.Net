using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Превращённый атакой <see cref="UnitAttackType.TransformOther" /> юнит.
/// </summary>
internal class TransformedOtherUnit : Unit
{
    /// <summary>
    /// Создать объект типа <see cref="TransformedOtherUnit" />.
    /// </summary>
    public TransformedOtherUnit(Unit originalUnit, UnitType unitType)
        : base(originalUnit.Id, unitType, originalUnit.Player, originalUnit.SquadLinePosition, originalUnit.SquadFlankPosition)
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