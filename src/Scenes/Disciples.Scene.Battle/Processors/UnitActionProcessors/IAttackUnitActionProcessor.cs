using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Интерфейс для обработчиков атаки.
/// </summary>
internal interface IAttackUnitActionProcessor : IUnitActionProcessor
{
    /// <summary>
    /// Юниты, которые должны быть атакованы второй атакой.
    /// </summary>
    /// <remarks>
    /// Список не пуст, если это обработчик первой атаки + у атакующего юнита есть вторая атака.
    /// </remarks>
    IReadOnlyList<Unit> SecondaryAttackUnits { get; }
}