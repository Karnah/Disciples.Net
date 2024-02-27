using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Components;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Игровой объект юнита во время сражения.
/// </summary>
internal class BattleUnit : GameObject
{
    /// <summary>
    /// Высота маленького юнита на сцене.
    /// </summary>
    private const int SMALL_BATTLE_UNIT_HEIGHT = 105;
    /// <summary>
    /// Высота большого юнита на сцене.
    /// </summary>
    /// <remarks>
    /// TODO У разных юнитов разная высота. Встречал 150, 120 и 130.
    /// </remarks>
    private const int BIG_BATTLE_UNIT_HEIGHT = 150;
    /// <summary>
    /// Сдвиг по координате Y для больших юнитов.
    /// </summary>
    private const int BIG_BATTLE_UNIT_Y_OFFSET = 45;

    /// <summary>
    /// Сдвиг анимаций маленького юнита.
    /// </summary>
    public static readonly PointD SmallBattleUnitAnimationOffset = new(-365, -410);
    /// <summary>
    /// Сдвиг анимаций большого юнита.
    /// </summary>
    private static readonly PointD BigBattleUnitAnimationOffset = new(SmallBattleUnitAnimationOffset.X, SmallBattleUnitAnimationOffset.Y + BIG_BATTLE_UNIT_Y_OFFSET);
    /// <summary>
    /// Сдвиг анимаций выделения маленького юнита.
    /// </summary>
    private static readonly PointD SmallBattleUnitSelectionAnimationOffset = new(-365, -220);
    /// <summary>
    /// Сдвиг анимаций выделения большого юнита.
    /// </summary>
    private static readonly PointD BigBattleUnitSelectionAnimationOffset = new(SmallBattleUnitSelectionAnimationOffset.X, SmallBattleUnitSelectionAnimationOffset.Y + BIG_BATTLE_UNIT_Y_OFFSET);

    private BattleUnitState _unitState;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnit" />.
    /// </summary>
    public BattleUnit(ISceneObjectContainer sceneObjectContainer,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        Action<BattleUnit> onUnitSelected,
        Action<BattleUnit> onUnitUnselected,
        Action<BattleUnit> onUnitMouseRightButtonClicked,
        Action<BattleUnit> onUnitMouseLeftButtonPressed,
        Unit unit,
        BattleSquadPosition unitSquadPosition,
        RectangleD bounds
        ) : base(bounds)
    {
        Unit = unit;
        IsAttacker = unitSquadPosition == BattleSquadPosition.Attacker;
        SquadPosition = unitSquadPosition;
        UnitState = BattleUnitState.Waiting;

        var unitAnimationOffset = Unit.UnitType.IsSmall
            ? SmallBattleUnitAnimationOffset
            : BigBattleUnitAnimationOffset;
        AnimationComponent = new BattleUnitAnimationComponent(this, sceneObjectContainer, battleUnitResourceProvider, unitAnimationOffset);
        var unitSelectionAnimationOffset = Unit.UnitType.IsSmall
            ? SmallBattleUnitSelectionAnimationOffset
            : BigBattleUnitSelectionAnimationOffset;
        var unitTurnAnimationFrames = Unit.UnitType.IsSmall
            ? battleUnitResourceProvider.SmallUnitTurnAnimationFrames
            : battleUnitResourceProvider.BigUnitTurnAnimationFrames;
        UnitTurnAnimationComponent = new AnimationComponent(this, sceneObjectContainer, unitTurnAnimationFrames,
            BattleLayers.UNIT_SELECTION_ANIMATION_LAYER, unitSelectionAnimationOffset);
        var targetAnimationFrames = Unit.UnitType.IsSmall
            ? battleUnitResourceProvider.SmallUnitTargetAnimationFrames
            : battleUnitResourceProvider.BigUnitTargetAnimationFrames;
        TargetAnimationComponent = new AnimationComponent(this, sceneObjectContainer, targetAnimationFrames,
            BattleLayers.UNIT_SELECTION_ANIMATION_LAYER, unitSelectionAnimationOffset);
        SoundComponent = new BattleUnitSoundComponent(this, battleUnitResourceProvider);
        Components = new IComponent[]
        {
            AnimationComponent,
            UnitTurnAnimationComponent,
            TargetAnimationComponent,
            SoundComponent,
            new SelectionComponent(this,
                () => onUnitSelected.Invoke(this),
                () => onUnitUnselected.Invoke(this)),
            new MouseLeftButtonClickComponent(this, Array.Empty<KeyboardButton>(), onClickedAction: () => onUnitMouseRightButtonClicked.Invoke(this)),
            new MouseRightButtonClickComponent(this, () => onUnitMouseLeftButtonPressed.Invoke(this))
        };

        Height = Unit.UnitType.IsSmall
            ? SMALL_BATTLE_UNIT_HEIGHT
            : BIG_BATTLE_UNIT_HEIGHT;
    }

    /// <summary>
    /// Компонент анимации юнита.
    /// </summary>
    public BattleUnitAnimationComponent AnimationComponent { get; }

    /// <summary>
    /// Компонент анимации выделения текущего юнита.
    /// </summary>
    public AnimationComponent UnitTurnAnimationComponent { get; }

    /// <summary>
    /// Компонент анимации выделении юнита-цели.
    /// </summary>
    public AnimationComponent TargetAnimationComponent { get; }

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
    /// Позиция отряда юнита.
    /// </summary>
    public BattleSquadPosition SquadPosition { get; }

    /// <summary>
    /// Направление, куда смотрит юнит.
    /// </summary>
    public BattleDirection Direction =>
        (IsAttacker && !Unit.Effects.IsRetreating) || (!IsAttacker && Unit.Effects.IsRetreating)
            ? BattleDirection.Face
            : BattleDirection.Back;

    /// <summary>
    /// Признак, что сейчас ход этого юнита.
    /// </summary>
    public bool IsUnitTurn
    {
        get => UnitTurnAnimationComponent.IsEnabled;
        set => UnitTurnAnimationComponent.IsEnabled = value;
    }

    /// <summary>
    /// Признак, что юнит выбран в качестве цели.
    /// </summary>
    public bool IsTarget
    {
        get => TargetAnimationComponent.IsEnabled;
        set => TargetAnimationComponent.IsEnabled = value;
    }

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

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        IsUnitTurn = false;
        IsTarget = false;
    }

    /// <inheritdoc />
    protected override void OnHiddenChanged(bool isHidden)
    {
        base.OnHiddenChanged(isHidden);

        AnimationComponent.IsEnabled = !isHidden;

        if (isHidden)
        {
            IsUnitTurn = false;
            IsTarget = false;
        }
    }
}