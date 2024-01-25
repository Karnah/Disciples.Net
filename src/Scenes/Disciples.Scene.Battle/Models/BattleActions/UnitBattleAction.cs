using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие, которое совершается юнитом.
/// </summary>
internal class UnitBattleAction : ComplexBattleAction
{
    /// <summary>
    /// Продолжительность воздействия.
    /// </summary>
    private const int TOUCH_UNIT_ACTION_DURATION = 1000;

    /// <summary>
    /// Создать объект типа <see cref="UnitBattleAction" />.
    /// </summary>
    public UnitBattleAction(
        BattleUnit targetUnit,
        BattleProcessorAttackResult attackResult,
        bool isEffectTriggered = false,
        AnimationBattleAction? animationBattleAction = null)
        : base(GetBattleActions(animationBattleAction, TOUCH_UNIT_ACTION_DURATION))
    {
        TargetUnit = targetUnit;
        ActionType = GetAttackActionType(attackResult.AttackResult);
        AttackResult = attackResult;
        IsEffectTriggered = isEffectTriggered;
    }

    /// <summary>
    /// Создать объект типа <see cref="UnitBattleAction" />.
    /// </summary>
    public UnitBattleAction(
        BattleUnit targetUnit,
        UnitActionType actionType,
        int? touchUnitActionDuration = null
        ) : base(GetBattleActions(null, touchUnitActionDuration ?? TOUCH_UNIT_ACTION_DURATION))
    {
        TargetUnit = targetUnit;
        ActionType = actionType;
    }

    /// <summary>
    /// Юнит.
    /// </summary>
    public BattleUnit TargetUnit { get; }

    /// <summary>
    /// Действие юнита.
    /// </summary>
    public UnitActionType ActionType { get; }

    /// <summary>
    /// Результат атаки.
    /// </summary>
    public BattleProcessorAttackResult? AttackResult { get; }

    /// <summary>
    /// Признак, что это срабатывание эффекта на ходу юнита.
    /// </summary>
    public bool IsEffectTriggered { get; }

    /// <summary>
    /// Получить данные для отображения на портрете юнита.
    /// </summary>
    public BattleUnitPortraitEventData GetUnitPortraitEventData()
    {
        return new BattleUnitPortraitEventData(
            ActionType,
            AttackResult?.AttackType,
            AttackResult?.Power,
            AttackResult?.CriticalDamage,
            AttackResult?.EffectDuration,
            IsEffectTriggered);
    }

    /// <summary>
    /// Получить все действия.
    /// </summary>
    private static IReadOnlyList<IBattleAction> GetBattleActions(AnimationBattleAction? animationBattleAction, int touchUnitActionDuration)
    {
        if (animationBattleAction == null)
            return new[] { new DelayBattleAction(touchUnitActionDuration) };

        return new IBattleAction[]
        {
            new DelayBattleAction(touchUnitActionDuration),
            animationBattleAction
        };
    }

    /// <summary>
    /// Получить действие в зависимости от результата атаки.
    /// </summary>
    private static UnitActionType GetAttackActionType(AttackResult attackResult)
    {
        return attackResult switch
        {
            Enums.AttackResult.Miss => UnitActionType.Miss,
            Enums.AttackResult.Attack => UnitActionType.Attacked,
            Enums.AttackResult.Ward => UnitActionType.Ward,
            Enums.AttackResult.Immunity => UnitActionType.Immunity,
            _ => throw new ArgumentOutOfRangeException(nameof(attackResult), attackResult, null)
        };
    }
}