using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Юнит, который имеет все характеристики своего <see cref="UnitType" />.
/// </summary>
/// <remarks>
/// Используется при выполнении атак <see cref="UnitAttackType.Doppelganger" /> и <see cref="UnitAttackType.TransformSelf" />
/// </remarks>
internal class FullTransformUnit : Unit, ITransformedUnit
{
    /// <summary>
    /// Создать объект типа <see cref="FullTransformUnit" />.
    /// </summary>
    public FullTransformUnit(Unit originalUnit, UnitType unitType)
        : base(originalUnit.Id, unitType, originalUnit.Player, originalUnit.SquadLinePosition, originalUnit.SquadFlankPosition)
    {
        OriginalUnit = originalUnit is ITransformedUnit transformedUnit
            ? transformedUnit.OriginalUnit
            : originalUnit;

        // Соотношение здоровья сохраняется как у исходного юнита.
        HitPoints = (int) (HitPoints * ((decimal)originalUnit.HitPoints / originalUnit.MaxHitPoints));
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
    public override UnitEffects Effects => OriginalUnit.Effects;
}