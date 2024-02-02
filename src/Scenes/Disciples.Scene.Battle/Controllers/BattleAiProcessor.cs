using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Extensions;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

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
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="attackingSquad">Отряд атакующего юнита.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    /// <returns>Команда для юнита.</returns>
    public BattleAiCommand GetAiCommand(Unit attackingUnit, Squad attackingSquad, Squad defendingSquad)
    {
        var hasAllyAbility = attackingUnit.HasAllyAbility();
        var targetSquad = hasAllyAbility
            ? attackingSquad
            : defendingSquad;
        var targetUnits = GetTargetUnits(attackingUnit, attackingSquad, targetSquad);
        if (targetUnits.Count == 0)
            return new BattleAiCommand(BattleCommandType.Defend);

        // Если бьёт по площади, не важно какой юнит выбран целью.
        if (attackingUnit.UnitType.MainAttack.Reach == UnitAttackReach.All)
            return new BattleAiCommand(targetUnits[0].Unit);

        return hasAllyAbility
            ? GetOwnSquadCommand(attackingUnit, targetUnits)
            : GetEnemySquadCommand(attackingUnit, targetUnits);
    }

    /// <summary>
    /// Быстрое завершение битвы
    /// </summary>
    /// <remarks>
    /// TODO Просто оставляем всех атакующих юнитов с 1 ХП.
    /// Нужно использовать последовательные выводы GetAiCommand и обработки атак/результатов.
    /// Плюс получать очередность ходов в качестве параметра.
    ///
    /// TODO Также не очень хорошо, что меняется Unit напрямую. Хотя, может и норм.
    /// </remarks>
    /// <returns>Победивший отряд в битве.</returns>
    public Squad ProcessInstantBattle(Squad attackingSquad, Squad defendingSquad)
    {
        if (attackingSquad.Units.All(u => u.IsDeadOrRetreated))
            return defendingSquad;

        if (defendingSquad.Units.All(u => u.IsDeadOrRetreated))
            return attackingSquad;

        foreach (var attackingSquadUnit in attackingSquad.Units)
        {
            if (attackingSquadUnit.IsDeadOrRetreated)
                continue;

            attackingSquadUnit.HitPoints = 1;
        }

        foreach (var defendingSquadUnit in defendingSquad.Units)
        {
            if (defendingSquadUnit.IsDeadOrRetreated)
                continue;

            defendingSquadUnit.HitPoints = 0;
            defendingSquadUnit.IsDead = true;
        }

        return attackingSquad;
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
                // BUG неправильно работает уменьшение приоритета.
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
                .Where(tu => tu.Unit.HitPoints < tu.Unit.MaxHitPoints && !tu.Unit.IsDeadOrRetreated)
                .OrderByWeakness()
                .FirstOrDefault()
                ?.Unit;
            if (target != null)
                return new BattleAiCommand (target);
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
                                      tu.MainAttackResult != AttackResult.Ward &&
                                      (!isSecondaryAttackDisabled || tu.SecondaryAttackResult != AttackResult.Ward));
            if (targetUnit != null)
                return new BattleAiCommand(targetUnit.Unit);
        }

        // Превращаем самого сильного юнита.
        if (attackingUnit.UnitType.MainAttack.AttackType is UnitAttackType.TransformOther ||
            attackingUnit.UnitType.SecondaryAttack?.AttackType is UnitAttackType.TransformOther)
        {
            var targetUnit = targetUnits
                .OrderByPower()
                .FirstOrDefault(tu => !tu.Unit.Effects.IsDisabled &&
                                      tu.MainAttackResult != AttackResult.Ward &&
                                      (!isSecondaryAttackDisabled || tu.SecondaryAttackResult != AttackResult.Ward));
            if (targetUnit != null)
                return new BattleAiCommand(targetUnit.Unit);
        }

        var target = targetUnits
            // Если у юнита есть защита от атаки, то снижаем ему приоритет.
            .OrderBy(unit => unit.MainAttackResult == AttackResult.Ward
                ? 1
                : 0)
            .ThenByWeakness()
            .First()
            .Unit;
        return new BattleAiCommand(target);
    }

    /// <summary>
    /// Получить список юнитов для атаки.
    /// </summary>
    private IReadOnlyList<AiTargetUnit> GetTargetUnits(Unit attackingUnit, Squad attackingSquad, Squad targetSquad)
    {
        return targetSquad
            .Units
            .Where(u => _battleProcessor.CanAttack(attackingUnit, attackingSquad, u, targetSquad))
            .Select(u => new AiTargetUnit(u,
                _battleProcessor.ProcessMainAttack(attackingUnit, u)?.AttackResult,
                attackingUnit.UnitType.SecondaryAttack != null
                    ? _battleProcessor.ProcessSecondaryAttack(attackingUnit, u)?.AttackResult
                    : null))
            .Where(r => r.MainAttackResult is not null and not AttackResult.Immunity)
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