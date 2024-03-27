using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Контекст обработчика битвы.
/// </summary>
internal class BattleProcessorContext
{
    /// <summary>
    /// Создать объект типа <see cref="BattleProcessorContext" />.
    /// </summary>
    public BattleProcessorContext(Unit currentUnit, Squad attackingSquad, Squad defendingSquad, UnitTurnQueue unitTurnQueue, int roundNumber)
    {
        CurrentUnit = currentUnit;
        AttackingSquad = attackingSquad;
        DefendingSquad = defendingSquad;
        UnitTurnQueue = unitTurnQueue;
        RoundNumber = roundNumber;

        CurrentUnitSquad = GetUnitSquad(currentUnit);
    }

    /// <summary>
    /// Юнит, чей ход сейчас.
    /// </summary>
    public Unit CurrentUnit { get; }

    /// <summary>
    /// Отряд текущего юнита.
    /// </summary>
    public Squad CurrentUnitSquad { get; }

    /// <summary>
    /// Атакующий отряд.
    /// </summary>
    public Squad AttackingSquad { get; }

    /// <summary>
    /// Защищающийся отряд.
    /// </summary>
    public Squad DefendingSquad { get; }

    /// <summary>
    /// Очередность хода юнитов.
    /// </summary>
    public UnitTurnQueue UnitTurnQueue { get; }

    /// <summary>
    /// Номер раунда битвы.
    /// </summary>
    public int RoundNumber { get; }

    /// <summary>
    /// Получить отряд указанного юнита.
    /// </summary>
    public Squad GetUnitSquad(Unit unit)
    {
        return unit.Player == AttackingSquad.Player
            ? AttackingSquad
            : DefendingSquad;
    }
}