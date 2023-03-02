using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Components;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Игровой объект юнита во время сражения.
/// </summary>
internal class BattleUnit : GameObject
{
    /// <summary>
    /// Ширина юнита на сцене.
    /// </summary>
    private const int BATTLE_UNIT_WIDTH = 75;
    /// <summary>
    /// Высота юнита на сцене.
    /// </summary>
    private const int BATTLE_UNIT_HEIGHT = 100;

    public BattleUnit(
        ISceneObjectContainer sceneObjectContainer,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        Unit unit,
        bool isAttacker
    ) : base(GetSceneUnitPosition(isAttacker, unit.SquadLinePosition, unit.SquadFlankPosition))
    {
        Unit = unit;
        IsAttacker = isAttacker;
        Direction = isAttacker
            ? BattleDirection.Attacker
            : BattleDirection.Defender;
        Action = BattleAction.Waiting;

        AnimationComponent = new BattleUnitAnimationComponent(this, sceneObjectContainer, battleUnitResourceProvider);
        this.Components = new IComponent[] { AnimationComponent };

        Width = BATTLE_UNIT_WIDTH;
        Height = BATTLE_UNIT_HEIGHT;
    }


    /// <inheritdoc />
    public override bool IsInteractive => true;

    /// <summary>
    /// Компонент анимации юнита.
    /// </summary>
    public BattleUnitAnimationComponent AnimationComponent { get; }


    /// <summary>
    /// Информация о юните.
    /// </summary>
    public Unit Unit { get; }

    /// <summary>
    /// Принадлежит ли юнит атакующему отряду.
    /// </summary>
    public bool IsAttacker { get; }

    /// <summary>
    /// Направление, куда смотрит юнит.
    /// </summary>
    public BattleDirection Direction { get; set; }

    /// <summary>
    /// Действие, которое выполняет юнит в данный момент.
    /// </summary>
    public BattleAction Action { get; set; }


    /// <summary>
    /// Рассчитать позицию юнита на сцене.
    /// </summary>
    /// <param name="isAttacker">Находится ли юнит в атакующем отряде.</param>
    /// <param name="line">Линия, на которой располагается юнит.</param>
    /// <param name="flank">Позиция на которой находится юнит (центр, правый и левый фланги).</param>
    public static (double X, double Y) GetSceneUnitPosition(bool isAttacker, double line, double flank)
    {
        // Если смотреть на поле, то фронт защищающегося отряда (линия 0) - это 2 линия
        // Тыл же (линия 1) будет на 3 линии. Поэтому пересчитываем положение
        var gameLine = isAttacker
            ? line
            : 3 - line;

        var x = 60 + 95 * gameLine + 123 * flank;
        var y = 200 + 60 * gameLine - 43 * flank;

        return (x, y);
    }
}