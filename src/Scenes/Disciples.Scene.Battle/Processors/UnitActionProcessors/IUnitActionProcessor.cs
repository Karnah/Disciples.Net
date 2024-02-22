using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;
using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors;

/// <summary>
/// Интерфейс для обработки действий юнитов.
/// </summary>
/// <remarks>
/// В отличие от <see cref="IBattleUnitActionController" />, действия выполняются с <see cref="Unit" />.
/// </remarks>
internal interface IUnitActionProcessor
{
    /// <summary>
    /// Тип действия.
    /// </summary>
    UnitActionType ActionType { get; }

    /// <summary>
    /// Юнит, который является целью действия.
    /// </summary>
    Unit TargetUnit { get; }

    /// <summary>
    /// Обработать начало действия.
    /// </summary>
    void ProcessBeginAction();

    /// <summary>
    /// Обработать завершение действия.
    /// </summary>
    void ProcessCompletedAction();
}