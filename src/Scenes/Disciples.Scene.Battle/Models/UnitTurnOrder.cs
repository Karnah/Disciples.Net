using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Очередность хода юнита.
/// </summary>
internal class UnitTurnOrder
{
    /// <summary>
    /// Создать объект типа <see cref="UnitTurnOrder" />.
    /// </summary>
    public UnitTurnOrder(Unit unit, int initiative)
    {
        Unit = unit;
        Initiative = initiative;
    }

    /// <summary>
    /// Юнит.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// Его инициатива в этом ходу.
    /// </summary>
    public int Initiative { get; set; }
}