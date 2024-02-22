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
    private readonly LinkedList<Unit> _waitingTurnOrder = new ();

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
        foreach (var nextUnitTurnOrder in _turnOrder.OrderByDescending(to => to.Initiative).ToArray())
        {
            _turnOrder.Remove(nextUnitTurnOrder);

            if (nextUnitTurnOrder.Unit.IsDeadOrRetreated)
                continue;

            return nextUnitTurnOrder.Unit;
        }

        // Стек из юнитов, которые "ждали".
        if (_waitingTurnOrder.Count > 0)
        {
            IsWaitingUnitTurn = true;

            while (_waitingTurnOrder.Last != null)
            {
                var nextUnit = _waitingTurnOrder.Last.Value;
                _waitingTurnOrder.RemoveLast();

                if (nextUnit.IsDeadOrRetreated)
                    continue;

                return nextUnit;
            }
        }

        return null;
    }

    /// <summary>
    /// Обработать ожидание юнита.
    /// </summary>
    public void UnitWait(Unit unit)
    {
        _waitingTurnOrder.AddLast(unit);
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

    /// <summary>
    /// Изменить очередность из-за превращения юнита.
    /// </summary>
    public void ReorderTransformedUnitTurn(Unit oldUnit, Unit newUnit, int initiative)
    {
        var unitTurnOrder = _turnOrder.FirstOrDefault(to => to.Unit == oldUnit);
        if (unitTurnOrder != null)
        {
            _turnOrder.Remove(unitTurnOrder);
            _turnOrder.AddLast(new UnitTurnOrder(newUnit, initiative));
        }

        var unitWaitingTurnOrder = _waitingTurnOrder.Find(oldUnit);
        if (unitWaitingTurnOrder != null)
        {
            _waitingTurnOrder.AddAfter(unitWaitingTurnOrder, newUnit);
            _waitingTurnOrder.Remove(unitWaitingTurnOrder);
        }
    }

    /// <summary>
    /// Добавить юнита с дополнительной атакой в очередь.
    /// </summary>
    public void AddUnitAdditionalAttack(Unit unit)
    {
        _turnOrder.AddLast(new UnitTurnOrder(unit, int.MaxValue));
    }
}