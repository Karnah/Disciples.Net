using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Команда для действия.
/// </summary>
internal class BattleAiCommand
{
    /// <summary>
    /// Тип команды.
    /// </summary>
    public BattleCommandType CommandType { get; init; }

    /// <summary>
    /// Цель команды.
    /// </summary>
    /// <remarks>
    /// NotNull, если <see cref="CommandType" /> равно <see cref="BattleCommandType.Attack" />.
    /// </remarks>
    public Unit? Target { get; init; }
}