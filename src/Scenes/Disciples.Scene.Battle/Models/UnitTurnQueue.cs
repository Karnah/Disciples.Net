using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Очередь хода юнитов.
/// </summary>
internal class UnitTurnQueue
{
    /// <summary>
    /// Основная очередь хода юнитов.
    /// </summary>
    private LinkedList<UnitTurnOrder> _turnOrder = new();

    /// <summary>
    /// Очередность хода юнитов, которые выбрали ожидание во время своего хода.
    /// </summary>
    private readonly Stack<Unit> _waitingTurnOrder = new ();

    /// <summary>
    /// Признак, что ходит юнит, который "ждал" в этом раунде.
    /// </summary>
    public bool IsWaitingUnitTurn { get; private set; }

    /// <summary>
    /// Определить очередность ходов в новом раунде.
    /// </summary>
    public Unit NextRound(LinkedList<UnitTurnOrder> turnOrder)
    {
        _turnOrder = turnOrder;
        IsWaitingUnitTurn = false;

        return GetNextUnit()!;
    }

    /// <summary>
    /// Получить юнит, который ходит следующим.
    /// </summary>
    /// <returns>
    /// <see cref="Unit" />, если он ходит следующим.
    /// <see langword="null" />, если все юниты в этом ходу уже сходили.
    /// </returns>
    public Unit? GetNextUnit()
    {
        // Очередь из основных ходов юнитов.
        foreach (var nextUnitTurnOrder in _turnOrder.OrderByDescending(to => to.Initiative))
        {
            if (nextUnitTurnOrder.Unit.IsDeadOrRetreated)
                continue;

            _turnOrder.Remove(nextUnitTurnOrder);
            return nextUnitTurnOrder.Unit;
        }

        // Стек из юнитов, которые "ждали".
        if (_waitingTurnOrder.Count > 0)
        {
            IsWaitingUnitTurn = true;

            do
            {
                if (_waitingTurnOrder.TryPop(out var nextUnit) && !nextUnit.IsDeadOrRetreated)
                    return nextUnit;
            } while (_waitingTurnOrder.Count > 0);
        }

        return null;
    }

    /// <summary>
    /// Обработать ожидание юнита.
    /// </summary>
    public void UnitWait(Unit unit)
    {
        _waitingTurnOrder.Push(unit);
    }

    /// <summary>
    /// Поменять порядок хода юнита.
    /// </summary>
    public void ReorderUnitTurnOrder(Unit unit, int initiative)
    {
        var unitTurnOrder = _turnOrder.FirstOrDefault(to => to.Unit == unit);

        // Юнит в этом ходу уже сходил, ничего ему менять не нужно.
        if (unitTurnOrder == null)
            return;

        unitTurnOrder.Initiative = initiative;
    }
}