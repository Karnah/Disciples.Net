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
    public BattleAiCommand(Unit? target)
    {
        CommandType = BattleCommandType.Attack;
        Target = target;
    }

    /// <summary>
    /// Тип команды.
    /// </summary>
    public BattleCommandType CommandType { get; }

    /// <summary>
    /// Цель команды.
    /// </summary>
    /// <remarks>
    /// NotNull, если <see cref="CommandType" /> равно <see cref="BattleCommandType.Attack" />.
    /// </remarks>
    public Unit? Target { get; }
}