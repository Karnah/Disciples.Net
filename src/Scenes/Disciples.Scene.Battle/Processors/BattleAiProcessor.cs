﻿using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Extensions;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Processors;

/// <summary>
/// Управление компьютером для битвы.
/// </summary>
internal class BattleAiProcessor
{
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="BattleAiProcessor" />.
    /// </summary>
    public BattleAiProcessor(BattleProcessor battleProcessor)
    {
        _battleProcessor = battleProcessor;
    }

    /// <summary>
    /// Получить команду следующего хода от компьютера.
    /// </summary>
    /// <returns>Команда для юнита.</returns>
    public BattleAiCommand GetAiCommand()
    {
        var attackingUnit = _battleProcessor.CurrentUnit;
        var mainAttack = attackingUnit.MainAttack;

        // Данные атаки имеют особую логику, поэтому обрабатываем их отдельно.
        switch (mainAttack.AttackType)
        {
            case UnitAttackType.Summon:
            {
                var summonCommand = GetSummonnerCommand(attackingUnit);
                if (summonCommand != null)
                    return summonCommand;

                break;
            }

            case UnitAttackType.Doppelganger:
            {
                var doppelgangerCommand = GetDoppelgangerCommand(attackingUnit);
                if (doppelgangerCommand != null)
                    return doppelgangerCommand;

                break;
            }
        }

        // Альтернативные атаки имеет Доппельгангер и Повелитель Волков.
        // Атака первого проверена выше, если там null, значит он не может ни в кого превратиться и должен атаковать врукопашную.
        // Повелитель Волков основной атакой превращается в Духа Фенрира. И это бесполезное превращение, так как он теряет ход.
        // Поэтому всегда используем альтернативную атаку, если такая имеется.
        var targetMainAttack = attackingUnit.AlternativeAttack ?? mainAttack;
        var isAllyAttack = targetMainAttack.AttackType.IsAllyAttack();
        var targetSquad = isAllyAttack
            ? _battleProcessor.GetUnitSquad(attackingUnit)
            : _battleProcessor.GetUnitEnemySquad(attackingUnit);
        var targetUnits = GetTargetUnits(attackingUnit, targetSquad);
        if (targetUnits.Count == 0)
            return new BattleAiCommand(BattleCommandType.Defend);

        // Если бьёт по площади, не важно какой юнит выбран целью.
        if (targetMainAttack.Reach == UnitAttackReach.All)
            return new BattleAiCommand(targetUnits[0].Unit);

        return isAllyAttack
            ? GetOwnSquadCommand(attackingUnit, targetUnits)
            : GetEnemySquadCommand(attackingUnit, targetUnits);
    }

    /// <summary>
    /// Получить команду для использования на союзниках.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="targetUnits">Доступные юниты-цели.</param>
    /// <returns>Команда для юнита.</returns>
    private static BattleAiCommand GetOwnSquadCommand(Unit attackingUnit, IReadOnlyList<AiTargetUnit> targetUnits)
    {
        // Если усиливается урон, то выбираем самого сильного юнита.
        if (attackingUnit.UnitType.MainAttack.AttackType is UnitAttackType.IncreaseDamage ||
            attackingUnit.UnitType.SecondaryAttack?.AttackType is UnitAttackType.IncreaseDamage)
        {
            var targetUnit = targetUnits
                .OrderByPower()
                // BUG: неправильно работает уменьшение приоритета.
                // Если на юнита наложен уже эффект усиления, то понижаем ему приоритет в зависимости от силы эффекта.
                .ThenBy(tu =>
                {
                    if (tu.Unit.Effects.TryGetBattleEffect(UnitAttackType.IncreaseDamage, out var increaseDamageEffect))
                        return increaseDamageEffect.Power ?? 0;

                    return 0;
                })
                .First();
            return new BattleAiCommand(targetUnit.Unit);
        }

        // Для дополнительной атаки аналогично выбираем самого сильно юнита.
        // Но чисто теоретически, можно было бы проверять лекарей и других полезных юнитов.
        if (attackingUnit.UnitType.MainAttack.AttackType is UnitAttackType.GiveAdditionalAttack ||
            attackingUnit.UnitType.SecondaryAttack?.AttackType is UnitAttackType.GiveAdditionalAttack)
        {
            var targetUnit = targetUnits
                .OrderByPower()
                .First();
            return new BattleAiCommand(targetUnit.Unit);
        }

        // Воскрешаем самого сильного юнита.
        if (attackingUnit.UnitType.MainAttack.AttackType is UnitAttackType.Revive ||
            attackingUnit.UnitType.SecondaryAttack?.AttackType is UnitAttackType.Revive)
        {
            var targetUnit = targetUnits
                .OrderByPower()
                .FirstOrDefault(u => u.Unit.IsDead && !u.Unit.IsRevived);
            if (targetUnit != null)
                return new BattleAiCommand(targetUnit.Unit);
        }

        // Если юнит лечит, то выбираем самого слабого юнита.
        // Также, возможно стоит отдавать приоритет тем юнитам, которым будет восстановлено большее количество здоровья.
        if (attackingUnit.UnitType.MainAttack.AttackType is UnitAttackType.Heal ||
            attackingUnit.UnitType.SecondaryAttack?.AttackType is UnitAttackType.Heal)
        {
            var target = targetUnits
                .Where(tu => tu.Unit.HitPoints < tu.Unit.MaxHitPoints && !tu.Unit.IsInactive)
                .OrderByWeakness()
                .FirstOrDefault()
                ?.Unit;
            if (target != null)
                return new BattleAiCommand(target);
        }

        return new BattleAiCommand(BattleCommandType.Defend);
    }

    /// <summary>
    /// Получить команду для использования на врагах.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="targetUnits">Доступные юниты-цели.</param>
    /// <returns>Команда для юнита.</returns>
    private static BattleAiCommand GetEnemySquadCommand(Unit attackingUnit, IReadOnlyList<AiTargetUnit> targetUnits)
    {
        // Если одна из атак выводит из строя, то выбираем самого сильного юнита.
        var isMainAttackDisabled = IsDisableAttack(attackingUnit.UnitType.MainAttack);
        var isSecondaryAttackDisabled = IsDisableAttack(attackingUnit.UnitType.SecondaryAttack);
        if (isMainAttackDisabled || isSecondaryAttackDisabled)
        {
            var targetUnit = targetUnits
                .OrderByPower()
                .FirstOrDefault(tu => !tu.Unit.Effects.IsDisabled &&
                                      tu.MainAttackResult != UnitActionType.Ward &&
                                      (!isSecondaryAttackDisabled || tu.SecondaryAttackResult != UnitActionType.Ward));
            if (targetUnit != null)
                return new BattleAiCommand(targetUnit.Unit);
        }

        // Превращаем самого сильного юнита.
        if (attackingUnit.UnitType.MainAttack.AttackType is UnitAttackType.TransformEnemy ||
            attackingUnit.UnitType.SecondaryAttack?.AttackType is UnitAttackType.TransformEnemy)
        {
            var targetUnit = targetUnits
                .OrderByPower()
                .FirstOrDefault(tu => !tu.Unit.Effects.IsDisabled &&
                                      tu.MainAttackResult != UnitActionType.Ward &&
                                      (!isSecondaryAttackDisabled || tu.SecondaryAttackResult != UnitActionType.Ward));
            if (targetUnit != null)
                return new BattleAiCommand(targetUnit.Unit);
        }

        var target = targetUnits
            // Если у юнита есть защита от атаки, то снижаем ему приоритет.
            .OrderBy(unit => unit.MainAttackResult == UnitActionType.Ward
                ? 1
                : 0)
            .ThenByWeakness()
            .First()
            .Unit;
        return new BattleAiCommand(target);
    }

    /// <summary>
    /// Получить команду для превращения Доппельгангера.
    /// </summary>
    private BattleAiCommand? GetDoppelgangerCommand(Unit attackingUnit)
    {
        var targetUnit = GetTargetUnits(attackingUnit, _battleProcessor.AttackingSquad)
            .Concat(GetTargetUnits(attackingUnit, _battleProcessor.DefendingSquad))
            .Where(tu => tu.MainAttackProcessor is UnitSuccessAttackProcessor unitSuccessAttackProcessor &&
                         unitSuccessAttackProcessor.AttackTypeProcessor.AttackType == UnitAttackType.Doppelganger)
            // TODO Если цель - самый сильный юнит, то нужно сортировать по базовой силе типа юнита
            // (именно типа, а не юнита, т.к. Доппельгангер превращается в юнита того уровне, что задан в типе).
            // Также приоритет превращения в лучника/мага/целителя должен быть меньше, если Доппельгангер в первой линии.
            // И наоборот.
            .OrderByPower()
            .FirstOrDefault();
        if (targetUnit != null)
            return new BattleAiCommand(targetUnit.Unit);

        return null;
    }

    /// <summary>
    /// Получить команду для вызова юнитов.
    /// </summary>
    private BattleAiCommand? GetSummonnerCommand(Unit attackingUnit)
    {
        var summonPositions = _battleProcessor.GetSummonPositions();
        if (summonPositions.Count == 0)
            return null;

        // Приоритет для вызова - передняя линия, так как она защищает призывателя.
        var summonPosition = summonPositions.MaxBy(sp => sp.Line == UnitSquadLinePosition.Front);
        return new BattleAiCommand(_battleProcessor.GetUnitSquad(attackingUnit), summonPosition);
    }

    /// <summary>
    /// Получить список юнитов для атаки.
    /// </summary>
    private IReadOnlyList<AiTargetUnit> GetTargetUnits(Unit attackingUnit, Squad targetSquad)
    {
        return targetSquad
            .Units
            .Select(targetUnit =>
            {
                var canAttack = _battleProcessor.CanAttack(targetUnit);
                var mainAttackResult = canAttack
                    ? _battleProcessor.ProcessSingleMainAttack(targetUnit)
                    : null;
                var secondaryAttackResult = canAttack && attackingUnit.UnitType.SecondaryAttack != null
                    ? _battleProcessor.ProcessSecondaryAttack(targetUnit)
                    : null;

                return new AiTargetUnit(targetUnit, mainAttackResult, secondaryAttackResult);
            })
            .Where(target => target.MainAttackResult is not null and not UnitActionType.Immunity)
            .ToArray();
    }

    /// <summary>
    /// Признак, что атака выводит юнита из строя.
    /// </summary>
    private static bool IsDisableAttack(UnitAttack? unitAttack)
    {
        return unitAttack?.AttackType is UnitAttackType.Paralyze or UnitAttackType.Petrify or UnitAttackType.Fear;
    }
}