﻿using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие, которое совершается юнитом.
/// </summary>
internal class UnitBattleAction : ComplexBattleAction
{
    /// <summary>
    /// Продолжительность воздействия
    /// </summary>
    private const int TOUCH_UNIT_ACTION_DURATION = 1000;

    public UnitBattleAction(
        BattleUnit targetUnit,
        UnitActionType actionType,
        UnitAttackType? attackType = null,
        int? power = null,
        AnimationBattleAction? animationBattleAction = null,
        UnitAttackSource? attackSource = null
        ) : base(GetBattleActions(animationBattleAction))
    {
        TargetUnit = targetUnit;
        ActionType = actionType;
        AttackType = attackType;
        AttackSource = attackSource;
        Power = power;
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
    /// Тип атаки.
    /// </summary>
    public UnitAttackType? AttackType { get; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource? AttackSource { get; }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Получить данные для отображения на портрете юнита.
    /// </summary>
    public virtual BattleUnitPortraitEventData GetUnitPortraitEventData()
    {
        return new BattleUnitPortraitEventData(ActionType, AttackType, Power);
    }

    /// <summary>
    /// Получить все действия.
    /// </summary>
    private static IReadOnlyList<IBattleAction> GetBattleActions(AnimationBattleAction? animationBattleAction)
    {
        if (animationBattleAction == null)
            return new[] { new DelayBattleAction(TOUCH_UNIT_ACTION_DURATION) };

        return new IBattleAction[]
        {
            new DelayBattleAction(TOUCH_UNIT_ACTION_DURATION),
            animationBattleAction
        };
    }
}