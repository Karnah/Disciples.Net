using Disciples.Engine.Common.Enums;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Позиция юнита.
/// </summary>
internal class BattleUnitPosition
{
    /// <summary>
    /// Создать объект типа <see cref="BattleUnitPosition" />.
    /// </summary>
    public BattleUnitPosition(BattleSquadPosition squadPosition, UnitSquadPosition unitPosition)
    {
        SquadPosition = squadPosition;
        UnitPosition = unitPosition;
    }

    /// <summary>
    /// Позиция отряда юнита.
    /// </summary>
    public BattleSquadPosition SquadPosition { get; }

    /// <summary>
    /// Позиция юнита в отряде.
    /// </summary>
    public UnitSquadPosition UnitPosition { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(SquadPosition)}: {SquadPosition}, {nameof(UnitPosition)}: {UnitPosition}";
    }
}
