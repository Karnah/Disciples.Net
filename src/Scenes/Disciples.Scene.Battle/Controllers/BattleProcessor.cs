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

        // BUG Патриарх может воскресить юнита, так что эта проверка не совсем корректна.
        // Если юнит бьёт по площади, то разрешаем кликнуть на мертвого юнита.
        if (targetUnit.IsDead && attackingUnit.UnitType.MainAttack.Reach != UnitAttackReach.All)
            return false;

        // Лекарь по одиночной цели без второй атаки может лечить только тех,
        // у кого меньше максимального значения здоровья.
        if (attackingUnit.UnitType.MainAttack.AttackType == UnitAttackType.Heal &&
            attackingUnit.UnitType.MainAttack.Reach == UnitAttackReach.Any &&
            attackingUnit.UnitType.SecondaryAttack == null &&
            targetUnit.HitPoints == targetUnit.MaxHitPoints)
        {
            return false;
        }

        // TODO При обработке эффекта "Cure" изменить проверку. Даже если нельзя баффать, эффект снимать будет можно.
        if (attackingUnit.UnitType.MainAttack.AttackType == UnitAttackType.BoostDamage &&
            attackingUnit.UnitType.MainAttack.Reach == UnitAttackReach.Any)
        {
            // Усилять можно только юнитов с прямым уроном от первой атаки.
            if (targetUnit.UnitType.MainAttack.AttackType != UnitAttackType.Damage)
                return false;

            // Усилить юнита можно только большим эффектом.
            if (targetUnit.Effects.TryGetBattleEffect(UnitAttackType.BoostDamage, out var boostEffect) &&
                attackingUnit.MainAttackPower <= boostEffect.Power)
            {
                return false;
            }
        }

        // Нельзя давать дополнительную атаку для юнита, который сам даёт дополнительную атаку.
        if (attackingUnit.UnitType.MainAttack.AttackType is UnitAttackType.GiveAdditionalAttack &&
            targetUnit.UnitType.MainAttack.AttackType is UnitAttackType.GiveAdditionalAttack)
        {
            return false;
        }

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

        return true;
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
    public BattleProcessorAttackResult? ProcessSecondaryAttack(Unit attackingUnit, Unit targetUnit, int? externalPower)
    {
        var power = externalPower ?? attackingUnit.SecondaryAttackPower;
        var attack = attackingUnit.UnitType.SecondaryAttack!;
        var accuracy = attackingUnit.SecondaryAttackAccuracy!.Value;
        return ProcessAttack(attackingUnit, targetUnit, attack, power, accuracy);
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

        // Яд, заморозка и ожег единственный эффекты, которые наносят урон.
        if (effect.AttackType is UnitAttackType.Poison or
            UnitAttackType.Frostbite or
            UnitAttackType.Blister)
        {
            // Если этот эффект уже срабатывал в этом ходу, то второго срабатывания не будет.
            if (effect.RoundTriggered == roundNumber)
                return null;

            power = Math.Min(targetUnit.HitPoints, effect.Power!.Value);
        }

        effect.RoundTriggered = roundNumber;
        effect.Duration.DecreaseTurn();

        return new BattleProcessorAttackResult(AttackResult.Effect, power, effect.Duration, effect.DurationControlUnit, effect.AttackType, effect.AttackSource);
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
            return null;
        }

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

        switch (attack.AttackType)
        {
            case UnitAttackType.Damage:
                // todo Максимальное значение атаки - 250/300/400.
                var attackPower = power!.Value + RandomGenerator.Get(ATTACK_RANGE);

                // Уменьшаем входящий урон в зависимости от защиты.
                attackPower = (int)(attackPower * (1 - targetUnit.Armor / 100.0));

                // Если юнит защитился, то урон уменьшается в два раза.
                if (targetUnit.Effects.IsDefended)
                    attackPower /= 2;

                // Мы не можем нанести урон больше, чем осталось очков здоровья.
                attackPower = Math.Min(attackPower, targetUnit.HitPoints);
                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    attackPower,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.Drain:
                break;

            case UnitAttackType.Paralyze:
            case UnitAttackType.Petrify:
                return new BattleProcessorAttackResult(
                    AttackResult.Effect,
                    null,
                    GetEffectDuration(attack),
                    targetUnit,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.Heal:
                var healPower = Math.Min(power!.Value, targetUnit.MaxHitPoints - targetUnit.HitPoints);
                if (healPower != 0)
                {
                    return new BattleProcessorAttackResult(
                        AttackResult.Heal,
                        healPower,
                        attack.AttackType,
                        attack.AttackSource);
                }

                break;

            case UnitAttackType.Fear:
                // TODO Если нельзя отступить (например, отряд в городе),
                // То страх действует как паралич.
                return new BattleProcessorAttackResult(AttackResult.Fear,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.BoostDamage:
                return new BattleProcessorAttackResult(
                    AttackResult.Effect,
                    power,
                    GetEffectDuration(attack),
                    attackingUnit,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.Blister:
                return new BattleProcessorAttackResult(
                    AttackResult.Effect,
                    power,
                    GetEffectDuration(attack),
                    targetUnit,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.GiveAdditionalAttack:
                return new BattleProcessorAttackResult(
                    AttackResult.AdditionalAttack,
                    attack.AttackType,
                    attack.AttackSource);

            case UnitAttackType.Revive:
            case UnitAttackType.DrainOverflow:
            case UnitAttackType.Cure:
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
            case UnitAttackType.Drain:
            case UnitAttackType.Heal:
            case UnitAttackType.Fear:
            case UnitAttackType.Revive:
            case UnitAttackType.DrainOverflow:
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