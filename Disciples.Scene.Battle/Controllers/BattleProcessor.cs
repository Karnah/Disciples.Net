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
    public Queue<Unit> GetTurnOrder(Squad attackingSquad, Squad defendingSquad)
    {
        return new Queue<Unit>(
            attackingSquad.Units
                .Concat(defendingSquad.Units)
                .Where(u => !u.IsDead)
                .OrderByDescending(u => u.Initiative + RandomGenerator.Get(0, INITIATIVE_RANGE)));
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
        if (attackingUnit.IsDead)
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
        if (attackingUnit.Player == targetUnit.Player &&
            attackingUnit.UnitType.MainAttack.AttackType == UnitAttackType.Heal &&
            attackingUnit.UnitType.MainAttack.Reach == UnitAttackReach.Any &&
            attackingUnit.UnitType.SecondaryAttack == null &&
            targetUnit.HitPoints == targetUnit.UnitType.HitPoints)
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
        return !squad.Units.Any(u => u.SquadLinePosition == UnitSquadLinePosition.Front && !u.IsDead);
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
                                    u.IsDead == false) == false;
    }

    #endregion

    #region Рассчет атаки

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью основной атаки.
    /// </summary>
    public BattleProcessorAttackResult? ProcessMainAttack(Unit attackingUnit, Unit targetUnit)
    {
        var power = attackingUnit.FirstAttackPower;
        var attack = attackingUnit.UnitType.MainAttack;
        var accuracy = attackingUnit.MainAttackAccuracy;
        return ProcessAttack(targetUnit, attack, power, accuracy);
    }

    /// <summary>
    /// Выполнить одну атаку юнита на другого с помощью второстепенной атаки.
    /// </summary>
    public BattleProcessorAttackResult? ProcessSecondaryAttack(Unit attackingUnit, Unit targetUnit, int? externalPower)
    {
        var power = externalPower ?? attackingUnit.SecondAttackPower;
        var attack = attackingUnit.UnitType.SecondaryAttack!;
        var accuracy = attackingUnit.SecondaryAttackAccuracy!.Value;
        return ProcessAttack(targetUnit, attack, power, accuracy);
    }

    /// <summary>
    /// Обработать действие эффекта.
    /// </summary>
    public BattleProcessorAttackResult? ProcessEffect(Unit targetUnit, UnitBattleEffect effect)
    {
        switch (effect.EffectType)
        {
            case UnitBattleEffectType.Poison:
            case UnitBattleEffectType.Frostbite:
            case UnitBattleEffectType.Blister:
                var damage = Math.Min(targetUnit.HitPoints, effect.Power!.Value);
                return new BattleProcessorAttackResult(AttackResult.Effect, damage, 1, (UnitAttackType)effect.EffectType);

            default:
                return null;
        }
    }

    /// <summary>
    /// Обработать действие одного юнита на другого.
    /// </summary>
    /// <param name="targetUnit">Юнит, на которого воздействует.</param>
    /// <param name="attack">Тип атаки.</param>
    /// <param name="power">Сила атаки.</param>
    /// <param name="accuracy">Точность атаки.</param>
    private static BattleProcessorAttackResult? ProcessAttack(Unit targetUnit, UnitAttack attack, int? power, int accuracy)
    {
        // Единственная атаки, которая действует на мёртвых юнитов - воскрешение и призыв.
        if (targetUnit.IsDead
            && attack.AttackType is not UnitAttackType.Revive or UnitAttackType.Summon)
        {
            return null;
        }

        // Проверяем меткость юнита.
        var chanceOfFirstAttack = RandomGenerator.Get(0, 100);
        if (chanceOfFirstAttack > accuracy)
            return new BattleProcessorAttackResult(AttackResult.Miss);

        // todo Сразу обработать иммунитет + сопротивления. Также вернуть результат.
        // Вторая атака не будет действовать, если первая упёрлась в иммунитет.

        switch (attack.AttackType)
        {
            case UnitAttackType.Damage:
                // todo Максимальное значение атаки - 250/300/400.
                var attackPower = power!.Value + RandomGenerator.Get(ATTACK_RANGE);

                // Уменьшаем входящий урон в зависимости от защиты.
                attackPower = (int)(attackPower * (1 - targetUnit.Armor / 100.0));

                // Если юнит защитился, то урон уменьшается в два раза.
                if (targetUnit.Effects.ExistsBattleEffect(UnitBattleEffectType.Defend))
                {
                    attackPower /= 2;
                }

                // Мы не можем нанести урон больше, чем осталось очков здоровья.
                attackPower = Math.Min(attackPower, targetUnit.HitPoints);
                return new BattleProcessorAttackResult(
                    AttackResult.Attack,
                    attackPower,
                    attack.AttackType);

            case UnitAttackType.Drain:
            case UnitAttackType.Paralyze:
                break;

            case UnitAttackType.Heal:
                var healPower = Math.Min(power!.Value, targetUnit.MaxHitPoints - targetUnit.HitPoints);
                if (healPower != 0)
                {
                    return new BattleProcessorAttackResult(
                        AttackResult.Heal,
                        healPower,
                        attack.AttackType);
                }

                break;

            case UnitAttackType.Fear:
            case UnitAttackType.BoostDamage:
            case UnitAttackType.Petrify:
            case UnitAttackType.LowerDamage:
            case UnitAttackType.LowerInitiative:
                break;

            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.Blister:
                return new BattleProcessorAttackResult(
                    AttackResult.Effect,
                    power,
                    2,
                    attack.AttackType);

            case UnitAttackType.Revive:
            case UnitAttackType.DrainOverflow:
            case UnitAttackType.Cure:
            case UnitAttackType.Summon:
            case UnitAttackType.DrainLevel:
            case UnitAttackType.GiveAttack:
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

    #endregion

    #region Проверка окончания битвы

    // TODO Если в отряде есть целитель, то перед завершением битвы ему даётся ход.

    /// <summary>
    /// Проверить битва завершилась победой одной из сторон.
    /// </summary>
    /// <param name="attackingSquad">Атакующий отряд.</param>
    /// <param name="defendingSquad">Защищающийся отряд.</param>
    public bool IsBattleCompleted(Squad attackingSquad, Squad defendingSquad)
    {
        return
            attackingSquad.Units.All(u => u.IsDead)
            || defendingSquad.Units.All(u => u.IsDead);
    }

    #endregion
}