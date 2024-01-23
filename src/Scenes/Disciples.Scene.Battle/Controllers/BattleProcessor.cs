using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Обработчик битвы.
/// </summary>
internal class BattleProcessor
{
    /// <summary>
    /// Разброс инициативы при вычислении очередности.
    /// </summary>
    private const int INITIATIVE_RANGE = 5;

    /// <summary>
    /// Разброс атаки при ударе.
    /// </summary>
    private const int ATTACK_RANGE = 5;

    #region Очерёдность ходов

    /// <summary>
    /// Получить очередность ходов юнитов.
    /// </summary>
    /// <param name="attackingSquad">Атакующий отряд.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    public LinkedList<UnitTurnOrder> GetTurnOrder(Squad attackingSquad, Squad defendingSquad)
    {
        return new LinkedList<UnitTurnOrder>(
            attackingSquad.Units
                .Concat(defendingSquad.Units)
                .Where(u => !u.IsDeadOrRetreated)
                .Select(u => new UnitTurnOrder(u,u.Initiative + RandomGenerator.Get(0, INITIATIVE_RANGE) )));
    }

    #endregion

    #region Проверка возможности атаковать

    /// <summary>
    /// Проверить, может ли атаковать один юнит другого.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="attackingSquad">Отряд атакующего юнита.</param>
    /// <param name="targetUnit">Цель.</param>
    /// <param name="targetSquad">Отряд цели.</param>
    public bool CanAttack(Unit attackingUnit, Squad attackingSquad, Unit targetUnit, Squad targetSquad)
    {
        if (attackingUnit.IsRetreated)
            return false;

        // Лекарь не может атаковать врага, а воин не может атаковать союзника.
        if (attackingUnit.Player == targetUnit.Player && attackingUnit.HasAllyAbility() == false ||
            attackingUnit.Player != targetUnit.Player && attackingUnit.HasEnemyAbility() == false)
        {
            return false;
        }

        // Для юнитов бьющих по всей площади, разрешаем кликнуть на любого юнита (даже на мёртвого).
        if (attackingUnit.UnitType.MainAttack.Reach == UnitAttackReach.All)
            return true;

        // Если юнит может атаковать только ближайшего, то проверяем препятствия.
        if (attackingUnit.UnitType.MainAttack.Reach == UnitAttackReach.Adjacent)
        {
            // Если атакующий юнит находится сзади и есть линия союзников впереди.
            if (attackingUnit.SquadLinePosition == 0 && IsFrontLineEmpty(attackingSquad) == false)
                return false;

            // Если враг находится сзади, то проверяем, что нет первой вражеской линии.
            if (targetUnit.SquadLinePosition == 0 && IsFrontLineEmpty(targetSquad) == false)
                return false;

            // Проверка, может ли юнит дотянуться до врага.
            if (CanAttackOnFlank(
                    attackingUnit.SquadFlankPosition,
                    targetUnit.SquadFlankPosition,
                    targetUnit.SquadLinePosition,
                    targetSquad) == false)
            {
                return false;
            }
        }

        var mainAttackType = attackingUnit.UnitType.MainAttack.AttackType;
        var canAttackMainAttack = CanAttack(attackingUnit.UnitType.MainAttack.AttackType, attackingUnit.MainAttackPower, targetUnit);
        if (canAttackMainAttack)
            return true;

        // Для атак на союзников есть особые условия. Первая атака может быть невозможна, но вторая да, и она должна быть выполнена.
        // Например, Иерофант: он не может лечить мёртвого (первая атака), но может воскресить второй.
        // Также архидруидресса: она может не иметь возможности усилить, но может второй снять дебаффы.
        if (mainAttackType.IsAllyAttack() && attackingUnit.UnitType.SecondaryAttack != null)
        {
            var canAttackSecondaryAttack = CanAttack(attackingUnit.UnitType.SecondaryAttack.AttackType, attackingUnit.SecondaryAttackPower, targetUnit);
            if (canAttackSecondaryAttack)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Проверить, можно ли атаковать юнита типом атаки.
    /// </summary>
    private static bool CanAttack(UnitAttackType attackType, int? power, Unit targetUnit)
    {
        if (targetUnit.IsDead && attackType != UnitAttackType.Revive)
            return false;

        switch (attackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.Paralyze:
            case UnitAttackType.Petrify:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.DrainLevel:
            case UnitAttackType.DrainLifeOverflow:
            case UnitAttackType.Blister:
            case UnitAttackType.Shatter:
                return true;

            case UnitAttackType.Heal:
                return targetUnit.HitPoints < targetUnit.MaxHitPoints;

            case UnitAttackType.Fear:
                return !targetUnit.Effects.IsRetreating;

            case UnitAttackType.BoostDamage:
            {
                // Усилять можно только юнитов с прямым уроном от первой атаки.
                if (!targetUnit.UnitType.MainAttack.AttackType.IsDirectDamage())
                    return false;

                // Усилить юнита можно только большим эффектом.
                if (targetUnit.Effects.TryGetBattleEffect(UnitAttackType.BoostDamage, out var boostEffect) &&
                    power <= boostEffect.Power)
                {
                    return false;
                }

                return true;
            }

            case UnitAttackType.Revive:
                return targetUnit.IsDead && !targetUnit.IsRevived;

            case UnitAttackType.Cure:
                return targetUnit.Effects.HasCurableEffects();

            case UnitAttackType.Summon:
                return false;

            case UnitAttackType.GiveAdditionalAttack:
                return targetUnit.UnitType.MainAttack.AttackType != UnitAttackType.GiveAdditionalAttack;

            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.TransformOther:
            case UnitAttackType.BestowWards:
                return false;

            default:
                throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
        }
    }

    /// <summary>
    /// Проверить, свободна ли первая линия в отряде.
    /// </summary>
    private static bool IsFrontLineEmpty(Squad squad)
    {
        return !squad.Units.Any(u => u.SquadLinePosition == UnitSquadLinePosition.Front && !u.IsDeadOrRetreated);
    }

    /// <summary>
    /// Проверить, можно ли атаковать цель в зависимости от расположения на фланге.
    /// </summary>
    private static bool CanAttackOnFlank(
        UnitSquadFlankPosition currentUnitFlankPosition,
        UnitSquadFlankPosition targetUnitFlankPosition,
        UnitSquadLinePosition targetUnitLinePosition,
        Squad targetSquad)
    {
        // Если юниты находятся по разные стороны флагов и занят вражеский центр или соседняя с атакующим клетка, то атаковать нельзя.
        if (Math.Abs(currentUnitFlankPosition - targetUnitFlankPosition) > 1 &&
            (!IsPlaceEmpty(targetSquad, targetUnitLinePosition, UnitSquadFlankPosition.Center)
             || !IsPlaceEmpty(targetSquad, targetUnitLinePosition, currentUnitFlankPosition)))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверить, свободна ли клетка на арене.
    /// </summary>
    private static bool IsPlaceEmpty(Squad squad, UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition)
    {
        return squad.Units.Any(u => u.SquadLinePosition == linePosition &&
                                    u.SquadFlankPosition == flankPosition &&
                                    u.IsDeadOrRetreated == false) == false;
    }

    #endregion

    #region Рассчет атаки

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью основной атаки.
    /// </summary>
    public BattleProcessorAttackResult? ProcessMainAttack(Unit attackingUnit, Unit targetUnit)
    {
        var power = attackingUnit.MainAttackPower;
        var attack = attackingUnit.UnitType.MainAttack;
        var accuracy = attackingUnit.MainAttackAccuracy;
        return ProcessAttack(attackingUnit, targetUnit, attack, power, accuracy);
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью второстепенной атаки.
    /// </summary>
    public BattleProcessorAttackResult? ProcessSecondaryAttack(Unit attackingUnit, Unit targetUnit)
    {
        var power = attackingUnit.SecondaryAttackPower;
        var attack = attackingUnit.UnitType.SecondaryAttack!;
        var accuracy = attackingUnit.SecondaryAttackAccuracy!.Value;
        return ProcessAttack(attackingUnit, targetUnit, attack, power, accuracy);
    }

    /// <summary>
    /// Обработать лечение вампиризмом.
    /// </summary>
    public IReadOnlyList<DrainLifeHealUnit> ProcessDrainLifeHeal(Unit vampireUnit, Squad vampireUnitSquad, int totalDamage)
    {
        var attackType = vampireUnit.UnitType.MainAttack.AttackType;
        if (attackType is not UnitAttackType.DrainLife and not UnitAttackType.DrainLifeOverflow)
        {
            throw new ArgumentException(
                $"Вампиризм может быть обработан только для атак типа {UnitAttackType.DrainLife} и {UnitAttackType.DrainLifeOverflow}",
                nameof(attackType));
        }

        var totalHeal = totalDamage / 2;
        if (totalHeal == 0)
            return Array.Empty<DrainLifeHealUnit>();

        // DrainLife может лечить только себя, если его здоровье меньше максимального.
        // DrainLifeOverflow может лечить любого у кого здоровье меньше максимального.
        var canHealByDrain = attackType is UnitAttackType.DrainLife &&
                             vampireUnit.HitPoints < vampireUnit.MaxHitPoints;
        var canHealByDrainOverFlow = attackType is UnitAttackType.DrainLifeOverflow &&
                                     vampireUnitSquad.Units.Any(u => !u.IsDeadOrRetreated && u.HitPoints < u.MaxHitPoints);
        if (!canHealByDrain && !canHealByDrainOverFlow)
            return Array.Empty<DrainLifeHealUnit>();

        var result = new List<DrainLifeHealUnit>();

        var vampireHealPower = Math.Min(vampireUnit.MaxHitPoints - vampireUnit.HitPoints, totalHeal);
        if (vampireHealPower > 0)
        {
            totalHeal -= vampireHealPower;
            result.Add(new DrainLifeHealUnit(vampireUnit, vampireHealPower));
        }

        if (canHealByDrain || totalHeal == 0)
            return result;

        var damagedUnits = vampireUnitSquad
            .Units
            .Where(u => u != vampireUnit && !u.IsDeadOrRetreated && u.HitPoints < u.MaxHitPoints)
            .OrderBy(u => u.MaxHitPoints - u.HitPoints)
            .ToArray();
        for (int unitIndex = 0; unitIndex < damagedUnits.Length; unitIndex++)
        {
            // Лечение юнитов делится поровну между всеми.
            // Но если есть слабо раненые юниты (нужно восстановить меньше, чем среднее), то остальные получат больше.
            var targetUnit = damagedUnits[unitIndex];
            var unitHealPower = Math.Min(
                targetUnit.MaxHitPoints - targetUnit.HitPoints,
                totalHeal / (damagedUnits.Length - unitIndex));
            totalHeal -= unitHealPower;
            result.Add(new DrainLifeHealUnit(targetUnit, unitHealPower));
        }

        return result;
    }

    /// <summary>
    /// Обработать действие эффекта.
    /// </summary>
    public BattleProcessorAttackResult? ProcessEffect(Unit turnUnit, Unit targetUnit, UnitBattleEffect effect, int roundNumber)
    {
        // Эффект будет сбрасываться на ходе другого юнита.
        if (effect.DurationControlUnit != turnUnit)
            return null;

        int? power = null;

        if (effect.AttackType.IsDamageEffect())
        {
            // Если этот эффект уже срабатывал в этом ходу, то второго срабатывания не будет.
            if (effect.RoundTriggered == roundNumber)
                return null;

            power = Math.Min(targetUnit.HitPoints, effect.Power!.Value);
        }

        effect.RoundTriggered = roundNumber;
        effect.Duration.DecreaseTurn();

        return new BattleProcessorAttackResult(AttackResult.Attack, power, effect.Duration, effect.DurationControlUnit, effect.AttackType, effect.AttackSource);
    }

    /// <summary>
    /// Обработать действие одного юнита на другого.
    /// </summary>
    /// <param name="attackingUnit">Атакующий юнит.</param>
    /// <param name="targetUnit">Юнит, на которого воздействует.</param>
    /// <param name="attack">Тип атаки.</param>
    /// <param name="power">Сила атаки.</param>
    /// <param name="accuracy">Точность атаки.</param>
    private static BattleProcessorAttackResult? ProcessAttack(Unit attackingUnit, Unit targetUnit, UnitAttack attack, int? power, int accuracy)
    {
        if (targetUnit.IsRetreated)
            return null;

        // Единственная атаки, которая действует на мёртвых юнитов - воскрешение и призыв.
        if (targetUnit.IsDead
            && attack.AttackType is not UnitAttackType.Revive or UnitAttackType.Summon)
        {
            // Если вторая атака у юнита воскрешает, то нужно будет перейти сразу к ней.
            if (attackingUnit.UnitType.SecondaryAttack?.AttackType is UnitAttackType.Revive)
                return new BattleProcessorAttackResult(AttackResult.Skip);

            return null;
        }

        // BUG Иммунитет проверяется до меткость, но защита - после.
        var attackTypeProtection = targetUnit
            .AttackTypeProtections
            .FirstOrDefault(atp => atp.UnitAttackType == attack.AttackType);
        if (attackTypeProtection != null)
        {
            var attackResult = attackTypeProtection.ProtectionCategory == ProtectionCategory.Ward
                ? AttackResult.Ward
                : AttackResult.Immunity;
            return new BattleProcessorAttackResult(attackResult, attack.AttackType, attack.AttackSource);
        }

        var attackSourceProtection = targetUnit
            .AttackSourceProtections
            .FirstOrDefault(asp => asp.UnitAttackSource == attack.AttackSource);
        if (attackSourceProtection != null)
        {
            var attackResult = attackSourceProtection.ProtectionCategory == ProtectionCategory.Ward
                ? AttackResult.Ward
                : AttackResult.Immunity;
            return new BattleProcessorAttackResult(attackResult, attack.AttackType, attack.AttackSource);
        }

        // Проверяем меткость юнита.
        var chanceOfAttack = RandomGenerator.Get(0, 100);
        if (chanceOfAttack > accuracy)
            return new BattleProcessorAttackResult(AttackResult.Miss);

        if (!CanAttack(attack.AttackType, power, targetUnit))
            return new BattleProcessorAttackResult(AttackResult.Skip);

        switch (attack.AttackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.DrainLifeOverflow:
                // todo Максимальное значение атаки - 250/300/400.
                var attackPower = power!.Value + RandomGenerator.Get(ATTACK_RANGE);

                // Уменьшаем входящий урон в зависимости от защиты.
                attackPower = (int)(attackPower * (1 - targetUnit.Armor / 100.0));

                // Если юнит защитился, то урон уменьшается в два раза.
                // BUG Механизм хитрее и зависит от брони юнита. Кроме того есть параметр в GVar.
                if (targetUnit.Effects.IsDefended)
                    attackPower /= 2;

                // Мы не можем нанести урон больше, чем осталось очков здоровья.
                attackPower = Math.Min(attackPower, targetUnit.HitPoints);
                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    attackPower,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.Paralyze:
            case UnitAttackType.Petrify:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.Blister:
                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    power,
                    GetEffectDuration(attack),
                    targetUnit,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.Heal:
                var healPower = Math.Min(power!.Value, targetUnit.MaxHitPoints - targetUnit.HitPoints);
                if (healPower != 0)
                {
                    return new BattleProcessorAttackResult(
                        AttackResult.Attack,
                        healPower,
                        attack.AttackType,
                        attack.AttackSource);
                }

                break;

            case UnitAttackType.Fear:
                // TODO Если нельзя отступить (например, отряд в городе),
                // То страх действует как паралич.
                return new BattleProcessorAttackResult(AttackResult.Attack,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.BoostDamage:
                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    power,
                    GetEffectDuration(attack),
                    attackingUnit,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.Revive:
                if (!targetUnit.IsDead || targetUnit.IsRevived)
                    return null;

                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.GiveAdditionalAttack:
                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.Cure:
                return targetUnit.Effects.HasCurableEffects()
                    ? new BattleProcessorAttackResult(
                        AttackResult.Attack,
                        attack.AttackType,
                        attack.AttackSource)
                    : null;

            case UnitAttackType.Summon:
            case UnitAttackType.DrainLevel:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.TransformOther:
            case UnitAttackType.BestowWards:
            case UnitAttackType.Shatter:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    /// <summary>
    /// Получить длительность эффекта в раундах.
    /// </summary>
    private static EffectDuration GetEffectDuration(UnitAttack attack)
    {
        switch (attack.AttackType)
        {
            case UnitAttackType.Paralyze:
            case UnitAttackType.Petrify:
            case UnitAttackType.TransformOther:
                return attack.IsInfinitive
                    ? EffectDuration.CreateRandom(1, 3)
                    : EffectDuration.Create(1);

            case UnitAttackType.BoostDamage:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.Summon:
            case UnitAttackType.DrainLevel:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.BestowWards:
            case UnitAttackType.Shatter:
                return attack.IsInfinitive
                    ? EffectDuration.CreateInfinitive()
                    : EffectDuration.Create(1);

            case UnitAttackType.ReduceInitiative:
                return attack.IsInfinitive
                    ? EffectDuration.CreateInfinitive()
                    : EffectDuration.CreateRandom(2, 4);

            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.Blister:
                return attack.IsInfinitive
                    ? EffectDuration.CreateRandom(2, 4)
                    : EffectDuration.Create(1);

            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.Heal:
            case UnitAttackType.Fear:
            case UnitAttackType.Revive:
            case UnitAttackType.DrainLifeOverflow:
            case UnitAttackType.Cure:
            case UnitAttackType.GiveAdditionalAttack:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Проверка окончания битвы

    // TODO Если в отряде есть целитель, то перед завершением битвы ему даётся ход.

    /// <summary>
    /// Получить победителя битвы.
    /// <see langword="null" />, если битва еще не завершена.
    /// </summary>
    /// <param name="attackingSquad">Атакующий отряд.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    public Squad? GetBattleWinnerSquad(Squad attackingSquad, Squad defendingSquad)
    {
        if (attackingSquad.Units.All(u => u.IsDeadOrRetreated))
            return defendingSquad;

        if (defendingSquad.Units.All(u => u.IsDeadOrRetreated))
            return attackingSquad;

        return null;
    }

    #endregion
}