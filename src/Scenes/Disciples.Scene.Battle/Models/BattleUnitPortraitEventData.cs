using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Данные для события, которое должно отображаться на портрете юнита.
/// </summary>
internal readonly ref struct BattleUnitPortraitEventData
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitPortraitEventData" />.
    /// </summary>
    public BattleUnitPortraitEventData(UnitActionType unitActionType, UnitAttackType? attackType, int? power)
    {
        UnitActionType = unitActionType;
        AttackType = attackType;
        Power = power;
    }

    /// <summary>
    /// Действие юнита.
    /// </summary>
    public UnitActionType UnitActionType { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType? AttackType { get; }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int? Power { get; }
}