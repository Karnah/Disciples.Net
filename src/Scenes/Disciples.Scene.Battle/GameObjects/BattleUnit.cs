using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
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

    private BattleUnitState _unitState;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnit" />.
    /// </summary>
    public BattleUnit(
        ISceneObjectContainer sceneObjectContainer,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        Action<BattleUnit> onUnitSelected,
        Action<BattleUnit> onUnitUnselected,
        Action<BattleUnit> onUnitMouseRightButtonClicked,
        Action<BattleUnit> onUnitMouseLeftButtonPressed,
        Unit unit,
        bool isAttacker
    ) : base(GetSceneUnitPosition(isAttacker, (int)unit.SquadLinePosition, (int)unit.SquadFlankPosition))
    {
        Unit = unit;
        IsAttacker = isAttacker;
        SquadPosition = isAttacker
            ? BattleSquadPosition.Attacker
            : BattleSquadPosition.Defender;
        Direction = isAttacker
            ? BattleDirection.Face
            : BattleDirection.Back;
        UnitState = BattleUnitState.Waiting;

        AnimationComponent = new BattleUnitAnimationComponent(this, sceneObjectContainer, battleUnitResourceProvider);
        SoundComponent = new BattleUnitSoundComponent(this, battleUnitResourceProvider);
        this.Components = new IComponent[]
        {
            AnimationComponent,
            SoundComponent,
            new SelectionComponent(this,
                () => onUnitSelected.Invoke(this),
                () => onUnitUnselected.Invoke(this)),
            new MouseLeftButtonClickComponent(this, Array.Empty<KeyboardButton>(), onClickedAction: () => onUnitMouseRightButtonClicked.Invoke(this)),
            new MouseRightButtonClickComponent(this, () => onUnitMouseLeftButtonPressed.Invoke(this))
        };

        Width = BATTLE_UNIT_WIDTH;
        Height = BATTLE_UNIT_HEIGHT;
    }


    /// <summary>
    /// Компонент анимации юнита.
    /// </summary>
    public BattleUnitAnimationComponent AnimationComponent { get; }

    /// <summary>
    /// Компонент звуков юнита.
    /// </summary>
    public BattleUnitSoundComponent SoundComponent { get; }

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
    public BattleSquadPosition SquadPosition { get; set; }

    /// <summary>
    /// Направление, куда смотрит юнит.
    /// </summary>
    public BattleDirection Direction { get; set; }

    /// <summary>
    /// Действие, которое выполняет юнит в данный момент.
    /// </summary>
    public BattleUnitState UnitState
    {
        get => _unitState;
        set
        {
            if (_unitState == value)
                return;

            _unitState = value;

            // Сразу после смены состояния нужно обновить состояние анимаций.
            AnimationComponent.Update(0);
        }
    }


    /// <summary>
    /// Рассчитать позицию юнита на сцене.
    /// </summary>
    /// <param name="isAttacker">Находится ли юнит в атакующем отряде.</param>
    /// <param name="linePosition">Линия, на которой располагается юнит.</param>
    /// <param name="flankPosition">Позиция на которой находится юнит (центр, правый и левый фланги).</param>
    /// <remarks>
    /// Этот метод используется в том числе для расчета позиций анимаций, которые располагаются между юнитами.
    /// Поэтому используем double для позиций.
    /// </remarks>
    public static (double X, double Y) GetSceneUnitPosition(bool isAttacker, double linePosition, double flankPosition)
    {
        // Если смотреть на поле, то фронт защищающегося отряда (линия 0) - это 2 линия
        // Тыл же (линия 1) будет на 3 линии. Поэтому пересчитываем положение
        var gameLine = isAttacker
            ? linePosition
            : 3 - linePosition;

        var x = 60 + 95 * gameLine + 123 * flankPosition;
        var y = 200 + 60 * gameLine - 43 * flankPosition;

        return (x, y);
    }
}