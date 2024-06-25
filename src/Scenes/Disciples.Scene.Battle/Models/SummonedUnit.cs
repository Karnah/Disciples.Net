using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Вызванный юнит.
/// </summary>
internal class SummonedUnit : Unit
{
    /// <summary>
    /// Создать объект типа <see cref="SummonedUnit" />.
    /// </summary>
    public SummonedUnit(
        UnitType unitType,
        Player player,
        Squad squad,
        UnitSquadPosition position,
        IReadOnlyList<Unit> hiddenUnits
        ) : base(Guid.NewGuid().ToString(), unitType, player, squad, position)
    {
        HiddenUnits = hiddenUnits;
    }

    /// <summary>
    /// Юниты в отряде, которые были перекрыты вызванным.
    /// </summary>
    public IReadOnlyList<Unit> HiddenUnits { get; }
}