using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Команда для действия.
/// </summary>
internal class BattleAiCommand
{
    /// <summary>
    /// Создать объект типа <see cref="BattleAiCommand" /> с командой выполнить указанное действие.
    /// </summary>
    public BattleAiCommand(BattleCommandType commandType)
    {
        if (commandType == BattleCommandType.Attack)
            throw new ArgumentException("При атаки требуется передать юнита", nameof(commandType));

        CommandType = commandType;
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleAiCommand" /> с командой атаковать указанного юнита.
    /// </summary>
    public BattleAiCommand(Unit target) : this(target.Squad, target.Position)
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="BattleAiCommand" /> с командой атаковать указанную позицию.
    /// </summary>
    public BattleAiCommand(Squad? targetSquad, UnitSquadPosition targetPosition)
    {
        CommandType = BattleCommandType.Attack;
        TargetSquad = targetSquad;
        TargetPosition = targetPosition;
    }

    /// <summary>
    /// Тип команды.
    /// </summary>
    public BattleCommandType CommandType { get; }

    /// <summary>
    /// Целевой отряд.
    /// </summary>
    /// <remarks>
    /// NotNull, если <see cref="CommandType" /> равно <see cref="BattleCommandType.Attack" />.
    /// </remarks>
    public Squad? TargetSquad { get; }

    /// <summary>
    /// Целевая позиция.
    /// </summary>
    /// <remarks>
    /// NotNull, если <see cref="CommandType" /> равно <see cref="BattleCommandType.Attack" />.
    /// </remarks>
    public UnitSquadPosition? TargetPosition { get; }
}