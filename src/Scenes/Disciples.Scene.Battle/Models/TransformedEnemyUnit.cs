using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Превращённый атакой <see cref="UnitAttackType.TransformEnemy" /> юнит.
/// </summary>
internal class TransformedEnemyUnit : Unit, ITransformedUnit
{
    /// <summary>
    /// Базовый юнит, от которого берётся количество хп.
    /// </summary>
    private readonly Unit _baseUnit;

    /// <summary>
    /// Создать объект типа <see cref="TransformedEnemyUnit" />.
    /// </summary>
    public TransformedEnemyUnit(Unit originalUnit, UnitType transformedUnitType)
        : base(originalUnit.Id, transformedUnitType, originalUnit.Player, originalUnit.Squad, originalUnit.Position)
    {
        _baseUnit = originalUnit;

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
    public override int MaxHitPoints => _baseUnit.MaxHitPoints;

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