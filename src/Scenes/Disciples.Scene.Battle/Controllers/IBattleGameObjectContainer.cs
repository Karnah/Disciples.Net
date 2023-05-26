using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
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
    /// <param name="isAttacker">Является ли юнит атакующим.</param>
    BattleUnit AddBattleUnit(Unit unit, bool isAttacker);

    /// <summary>
    /// Добавить текстовую информацию о юните на сцену битвы.
    /// </summary>
    /// <param name="x">Положение текста, координата X.</param>
    /// <param name="y">Положение текста, координата Y.</param>
    /// <param name="layer">Слой, на котором необходимо отображать текст.</param>
    BattleUnitInfoGameObject AddBattleUnitInfo(int x, int y, int layer);

    /// <summary>
    /// Добавить портрет юнита на сцену.
    /// </summary>
    /// <param name="unit">Юнит, чей портрет необходимо добавить.</param>
    /// <param name="rightToLeft">Указатель того, что юнит смотрит справа налево.</param>
    /// <param name="x">Положение портрета, координата X.</param>
    /// <param name="y">Положение портрета, координата Y.</param>
    UnitPortraitObject AddUnitPortrait(Unit unit, bool rightToLeft, double x, double y);
}