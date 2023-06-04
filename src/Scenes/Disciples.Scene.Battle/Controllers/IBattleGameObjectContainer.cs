using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контейнер для игровых объектов битвы.
/// </summary>
internal interface IBattleGameObjectContainer : IGameObjectContainer
{
    /// <summary>
    /// Добавить юнита на сцену битвы.
    /// </summary>
    /// <param name="unit">Юнит.</param>
    /// <param name="unitSquadPosition">Положение отряда юнита.</param>
    BattleUnit AddBattleUnit(Unit unit, BattleSquadPosition unitSquadPosition);

    /// <summary>
    /// Добавить портрет юнита на сцену.
    /// </summary>
    /// <param name="unit">Юнит, чей портрет необходимо добавить.</param>
    /// <param name="unitSquadPosition">Отряд, в котором располагается юнит.</param>
    /// <param name="portraitBounds">Положение портрета юнита.</param>
    /// <param name="hitPointsBounds">Положение информации здоровья юнита.</param>
    UnitPortraitObject AddUnitPortrait(Unit unit, BattleSquadPosition unitSquadPosition, RectangleD portraitBounds, RectangleD hitPointsBounds);
}